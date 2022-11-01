using Dendrite.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dendrite
{
    public class Conv2d : NeuralItem
    {
        public Conv2d(int inChannels, int outChannels, int kSize, int stride, int padding, bool bias = false, int dilation = 1)
        {
            this.inChannels = inChannels;
            this.outChannels = outChannels;
            Weight = new InternalArray(new int[] { outChannels, inChannels, kSize, kSize });

            this.padding = new int[] { padding, padding };
            this.stride = new[] { stride, stride };
            this.kernelSize = new[] { kSize, kSize };
            this.dilation = new[] { dilation, dilation };
        }

        public InternalArray Weight;
        int[] kernelSize;
        int[] padding;
        int[] stride;
        int[] dilation;

        int inChannels;
        int outChannels;

        public override int SetData(List<InternalArray> arrays)
        {
            //if (!arrays[0].Name.Contains("conv")) throw new ArgumentException("not conv weight detected");
            Weight = arrays[0];
            return 1;
        }

        public static bool AllowOptimized = true;
        public static bool AllowOptimizedViaDot = false;


        public InternalArray ProcessImageViaDot(InternalArray ar)
        {
            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];

            var hout = ((hin + 2 * padding[0] - dilation[0] * (kernelSize[0] - 1) - 1) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - dilation[1] * (kernelSize[1] - 1) - 1) / stride[1]) + 1;

            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });
            //im2col
            int patches = hout * wout;//stride:1,padding:1,kSize:3
            int k = kernelSize[0] * kernelSize[1] * c;

            double[][] data1 = new double[hout * wout][];
            for (int i = 0; i < data1.Length; i++)
            {
                data1[i] = new double[k];
                //1 val to 9 position
            }

            double[][] data2 = new double[outChannels][];
            for (int i = 0; i < data2.Length; i++)
            {
                data2[i] = new double[k];
                var shift = Weight.offsets[0] * i;
                Array.Copy(Weight.Data, shift, data2[i], 0, k);
            }

            double[,] result = new double[patches, outChannels];


            for (int i = 0; i < data1.GetLength(0); i++)
            {
                for (int j = 0; j < data2.Length; j++)
                {
                    double val = 0;

                    for (int t = 0; t < data1[i].Length; t++)
                    {
                        val += data1[i][t] * data2[j][t];
                    }

                    result[i, j] = val;
                }
            }

            double sum = 0;
            int cnt = 0;
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    //sum += result[i, j];
                    ret.Data[cnt] = result[i, j];
                    cnt++;
                }
            }

            //ret.Data[0] = sum;
            return ret;
        }
        double[] a = new double[100];
        double[] b = new double[100];

        /// <summary>
        /// stride=1, kernel=1,padding=0
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="hout"></param>
        /// <param name="wout"></param>
        /// <param name="c"></param>
        /// <param name="hin"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        public InternalArray ProcessImageOptimizedNoPaddingKernel1(InternalArray ar, int hout, int wout, int c, int hin, int win)
        {

            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }


            Parallel.For(0, hout, (i) =>
            {

                for (int j = 0; j < wout; j++)
                {

                    var index2 = i * ret.offsets[1] + j;
                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;
                        var index = i * ar.offsets[1] + j;
                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];
                            val += kernel.Data[0] * ar.Data[index];
                            index += ar.offsets[0];
                        }
                        //ret.Data[ch * ret.offsets[0] + i * ret.offsets[1] + j] = val;
                        ret.Data[index2] = val;
                        index2 += ret.offsets[0];
                    }
                }
            });


            return ret;
        }

        public InternalArray ProcessImageOptimizedNoPadding(InternalArray ar, int hout, int wout, int c, int hin, int win)
        {
            //throw new Exception("22");
            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }

            //int shiftx = padding[0] - kernelSize[0] / 2;
            //int shifty = padding[1] - kernelSize[1] / 2;
            int shiftx = -kernelSize[0] / 2;
            int shifty = -kernelSize[1] / 2;


            Parallel.For(0, hout, (i) =>
            {
                var imul = (i) * stride[0] - kernelSize[0] / 2 - shiftx;

                for (int j = 0; j < wout; j++)
                {
                    var jmul = (j) * stride[1] - kernelSize[1] / 2 - shifty;

                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;

                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];
                            var offset1 = zz * ar.offsets[0];

                            var index = offset1 + imul * ar.offsets[1] + jmul;

                            /* b[0] = ar.Data[index];
                             b[1] = ar.Data[index + 1];
                             b[2] = ar.Data[index + 2];
                             index += ar.offsets[1];
                             b[3] = ar.Data[index];

                             var aSimd = new System.Numerics.Vector<double>(kernel.Data, 0);
                             var bSimd = new System.Numerics.Vector<double>(b, 0);
                             var r = System.Numerics.Vector.Dot<double>(aSimd, bSimd);
                             val += r;


                             b[0] = ar.Data[index + 1];
                             b[1] = ar.Data[index + 2];
                             index += ar.offsets[1];
                             b[2] = ar.Data[index];
                             b[3] = ar.Data[index + 1];

                             aSimd = new System.Numerics.Vector<double>(kernel.Data, 4);
                             bSimd = new System.Numerics.Vector<double>(b, 0);
                             r = System.Numerics.Vector.Dot<double>(aSimd, bSimd);
                             val += r;


                             val += kernel.Data[8] * ar.Data[index + 2];
                             */
                            #region old
                            val += kernel.Data[0] * ar.Data[index];
                            val += kernel.Data[1] * ar.Data[index + 1];
                            val += kernel.Data[2] * ar.Data[index + 2];

                            index += ar.offsets[1];

                            val += kernel.Data[3] * ar.Data[index];


                            val += kernel.Data[4] * ar.Data[index + 1];
                            val += kernel.Data[5] * ar.Data[index + 2];


                            index += ar.offsets[1];

                            val += kernel.Data[6] * ar.Data[index];
                            val += kernel.Data[7] * ar.Data[index + 1];
                            val += kernel.Data[8] * ar.Data[index + 2];
                            #endregion

                        }
                        ret.Data[ch * ret.offsets[0] + i * ret.offsets[1] + j] = val;
                    }
                }
            });
            // for (int i = 0; i < hout; i++)
            {

            }

            return ret;
        }

        public List<int> indexes1 = new List<int>();
        public List<int> indexes2 = new List<int>();
        public List<int> indexes3 = new List<int>();

        public InternalArray ProcessImageOptimized2(InternalArray ar, int hout, int wout, int c, int hin, int win)
        {


            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            int pos0 = 0;
            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.GetNext2dImageFrom4dArray(ref pos0);
                    var kernel2 = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }

            int shiftx = padding[0] - kernelSize[0] / 2;
            int shifty = padding[1] - kernelSize[1] / 2;

            Parallel.For(0, hout, (i) =>
            {
                var imul = (i) * stride[0] - kernelSize[0] / 2 - shiftx;
                var maxi1 = Math.Min((ar.Shape[1] - imul) / dilation[0], kernelSize[0]);
                var mini1 = Math.Max((int)Math.Ceiling(-(double)imul / dilation[0]), 0);
                Parallel.For(0, wout, (j) =>
                {
                    var jmul = (j) * stride[1] - kernelSize[1] / 2 - shifty;
                    var minj1 = Math.Max((int)Math.Ceiling(-(double)jmul / dilation[1]), 0);
                    var maxj1 = Math.Min((ar.Shape[2] - jmul) / dilation[1], kernelSize[1]);

                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;

                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];
                            var offset1 = zz * ar.offsets[0];
                            int kindex = 0;

                            for (int i1 = mini1; i1 < maxi1; i1++)
                            {
                                var x = imul + i1 * dilation[0];

                                for (int j1 = minj1; j1 < maxj1; j1++)
                                {
                                    var y = jmul + j1 * dilation[1];
                                    var index = offset1 + x * ar.offsets[1] + y;



                                    val += kernel.Data[kindex] * ar.Data[index];
                                    kindex++;
                                }
                            }
                        }
                        ret.Set3D(ch, i, j, val);
                    }
                });
            });
            //for (int i = 0; i < hout; i++)
            {

            }

            return ret;
        }

        public void CalcRegionsHashes(InternalArray ar)
        {
            int ww = RegionSize;//region size
            int rw = ar.Shape[1] / ww;
            int rh = ar.Shape[2] / ww;

            for (int i = 0; i < rw; i++)
            {
                for (int j = 0; j < rh; j++)
                {
                    int pointer = 0;
                    pointer = 0;//zero channel
                    var xx = i * ww;
                    var yy = j * ww;
                    pointer = xx + yy * ar.offsets[1];
                    int hash = 0;
                    for (int i2 = 0; i2 < ww; i2++)
                    {
                        for (int j2 = 0; j2 < ww; j2++)
                        {
                            hash ^= (int)(ar.Data[pointer] * 1000);
                            pointer++;
                        }
                        pointer += ar.offsets[1];
                        pointer -= ww;
                    }
                }
            }
        }

        public int CalcSubRegionHash(InternalArray ar, int sx, int sy, int ww, int hout, int wout)
        {
            int pointer = 0;
            int hash = 0;
            for (int ch = 0; ch < ar.Shape[0]; ch++)
            {
                var xx = sx * ww;
                var yy = sy * ww;
                pointer = xx + yy * ar.offsets[1];

                for (int i2 = 0; i2 < ww; i2++)
                {
                    for (int j2 = 0; j2 < ww; j2++)
                    {
                        hash ^= (int)(ar.Data[pointer] * 1000);
                        pointer++;
                    }
                    pointer += ar.offsets[1];
                    pointer -= ww;
                }
                pointer += ar.offsets[0];
            }
            return hash;
        }

        public void ProcessSubRegionOptimized(InternalArray ar, InternalArray[,] filters, InternalArray ret, int sx, int sy, int ww, int hout, int wout)
        {
            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];


            int shiftx = padding[0] - kernelSize[0] / 2;
            int shifty = padding[1] - kernelSize[1] / 2;

            var maxw = Math.Min(wout, (sy + 1) * ww);
            var maxh = Math.Min(hout, (sx + 1) * ww);
            var starth = sx * ww;
            var startw = sy * ww;

            for (int i = starth; i < maxh; i++)
            {
                var imul = (i) * stride[0] - kernelSize[0] / 2 - shiftx;
                var maxi1 = Math.Min((ar.Shape[1] - imul) / dilation[0], kernelSize[0]);
                var mini1 = Math.Max((int)Math.Ceiling(-(double)imul / dilation[0]), 0);

                for (int j = startw; j < maxw; j++)
                {
                    var jmul = (j) * stride[1] - kernelSize[1] / 2 - shifty;
                    var minj1 = Math.Max((int)Math.Ceiling(-(double)jmul / dilation[1]), 0);
                    var maxj1 = Math.Min((ar.Shape[2] - jmul) / dilation[1], kernelSize[1]);

                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;

                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];
                            var offset1 = zz * ar.offsets[0];
                            int kindex = 0;

                            for (int i1 = mini1; i1 < maxi1; i1++)
                            {
                                var x = imul + i1 * dilation[0];

                                for (int j1 = minj1; j1 < maxj1; j1++)
                                {
                                    var y = jmul + j1 * dilation[1];
                                    var index = offset1 + x * ar.offsets[1] + y;
                                    val += kernel.Data[kindex] * ar.Data[index];
                                    kindex++;
                                }
                            }
                        }
                        ret.Set3D(ch, i, j, val);
                    }
                }
            }
        }

        public void ProcessSubRegion(InternalArray ar, InternalArray[,] filters, InternalArray ret, int sx, int sy, int ww, int hout, int wout)
        {
            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];


            int shiftx = padding[0] - kernelSize[0] / 2;
            int shifty = padding[1] - kernelSize[1] / 2;

            var maxw = Math.Min(wout, (sy + 1) * ww);
            var maxh = Math.Min(hout, (sx + 1) * ww);
            var starth = sx * ww;
            var startw = sy * ww;

            for (int i = starth; i < maxh; i++)
            {
                for (int j = startw; j < maxw; j++)
                {
                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;
                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];

                            for (int i1 = 0; i1 < kernelSize[0]; i1++)
                            {
                                for (int j1 = 0; j1 < kernelSize[0]; j1++)
                                {
                                    //var x = i * stride[0] - kernelSize[0] / 2 + i1;
                                    //var y = j * stride[1] - kernelSize[1] / 2 + j1;

                                    //outspace
                                    var xout = (i) * stride[0] - kernelSize[0] / 2 + i1 * dilation[0];
                                    var yout = (j) * stride[1] - kernelSize[1] / 2 + j1 * dilation[1];
                                    //inspace
                                    var xin = xout - shiftx;
                                    var yin = yout - shifty;
                                    if (!ar.WithIn(zz, xin, yin)) continue;
                                    //var y=jmul+j1

                                    var ii1 = i1 * kernel.Shape[1] + j1;
                                    var ii2 = zz * ar.offsets[0] + xin * ar.offsets[1] + yin;
                                    val += kernel.Get2D(i1, j1) * ar.Get3D(zz, xin, yin);
                                    var ii3 = j + ch * ret.offsets[0] + i * ret.offsets[1];



                                }
                            }
                        }
                        ret.Set3D(ch, i, j, val);
                    }
                }
            }

        }

        public int RegionSize = 20;

        public InternalArray ProcessImageBySubRegions(InternalArray ar)
        {
#if PROFILER
            method = "sub.regions";
#endif

            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];

            var hout = ((hin + 2 * padding[0] - dilation[0] * (kernelSize[0] - 1) - 1) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - dilation[1] * (kernelSize[1] - 1) - 1) / stride[1]) + 1;

            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }

            int shiftx = padding[0] - kernelSize[0] / 2;
            int shifty = padding[1] - kernelSize[1] / 2;
            int ww = RegionSize;
            var maxh = (hout / ww) + 1;
            var maxw = (wout / ww) + 1;
            bool useParallel = true;
            if (useParallel)
            {
                Parallel.For(0, maxh, (i) =>
                {
                    Parallel.For(0, maxw, (j) =>
                    {
                        var hash1 = CalcSubRegionHash(ar, i, j, ww, hout, wout);
                        if (RegionHash.ContainsKey(hash1))
                        {
                            var r = RegionHash[hash1];
                            CopySubRegion(r, ret, i, j, ww);
                        }
                        else
                        {
                            //ProcessSubRegion(ar, filters, ret, i, j, ww, hout, wout);
                            ProcessSubRegionOptimized(ar, filters, ret, i, j, ww, hout, wout);
                        }
                    });
                });
            }
            else
            {
                for (int i = 0; i < maxh; i++)
                {
                    for (int j = 0; j < maxw; j++)
                    {
                        var hash1 = CalcSubRegionHash(ar, i, j, ww, hout, wout);
                        if (RegionHash.ContainsKey(hash1))
                        {
                            var r = RegionHash[hash1];
                            CopySubRegion(r, ret, i, j, ww);
                        }
                        else
                        {
                            //ProcessSubRegion(ar, filters, ret, i, j, ww, hout, wout);
                            ProcessSubRegionOptimized(ar, filters, ret, i, j, ww, hout, wout);
                        }
                    }
                }
            }
            return ret;
        }

        public void CopySubRegion(InternalArray src, InternalArray target, int sx, int sy, int ww)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, InternalArray> RegionHash = new Dictionary<int, InternalArray>();

        public InternalArray ProcessImage(InternalArray ar)
        {
            //CalcRegionsHashes(ar);

            indexes1.Clear();
            indexes2.Clear();
            indexes3.Clear();

            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];

            var hout = ((hin + 2 * padding[0] - dilation[0] * (kernelSize[0] - 1) - 1) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - dilation[1] * (kernelSize[1] - 1) - 1) / stride[1]) + 1;

            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }

            int shiftx = padding[0] - kernelSize[0] / 2;
            int shifty = padding[1] - kernelSize[1] / 2;
            for (int i = 0; i < hout; i++)
            {
                for (int j = 0; j < wout; j++)
                {
                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;
                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];

                            for (int i1 = 0; i1 < kernelSize[0]; i1++)
                            {
                                for (int j1 = 0; j1 < kernelSize[0]; j1++)
                                {
                                    //var x = i * stride[0] - kernelSize[0] / 2 + i1;
                                    //var y = j * stride[1] - kernelSize[1] / 2 + j1;

                                    //outspace
                                    var xout = (i) * stride[0] - kernelSize[0] / 2 + i1 * dilation[0];
                                    var yout = (j) * stride[1] - kernelSize[1] / 2 + j1 * dilation[1];
                                    //inspace
                                    var xin = xout - shiftx;
                                    var yin = yout - shifty;
                                    if (!ar.WithIn(zz, xin, yin)) continue;
                                    //var y=jmul+j1

                                    var ii1 = i1 * kernel.Shape[1] + j1;
                                    var ii2 = zz * ar.offsets[0] + xin * ar.offsets[1] + yin;
                                    val += kernel.Get2D(i1, j1) * ar.Get3D(zz, xin, yin);
                                    var ii3 = j + ch * ret.offsets[0] + i * ret.offsets[1];
                                    //if (i == 0)
                                    {
                                        /*indexes1.Add(ii1);
                                        indexes2.Add(ii2);
                                        indexes3.Add(ii3);*/
                                    }


                                }
                            }
                        }
                        ret.Set3D(ch, i, j, val);
                    }
                }
            }

            return ret;
        }

        public InternalArray ProcessImageOptimized(InternalArray ar)
        {
            var hin = ar.Shape[1];
            var win = ar.Shape[2];

            var c = ar.Shape[0];

            var hout = ((hin + 2 * padding[0] - dilation[0] * (kernelSize[0] - 1) - 1) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - dilation[1] * (kernelSize[1] - 1) - 1) / stride[1]) + 1;

            InternalArray ret = new InternalArray(new int[] { outChannels, hout, wout });

            InternalArray[,] filters = new InternalArray[outChannels, c];

            for (int ch = 0; ch < outChannels; ch++)
            {
                for (int zz = 0; zz < c; zz++)
                {
                    var kernel = Weight.Get2DImageFrom4DArray(ch, zz);
                    filters[ch, zz] = kernel;
                }
            }

            for (int i = 0; i < hout; i++)
            {
                var imul = i * stride[0] - kernelSize[0] / 2;
                for (int j = 0; j < wout; j++)
                {
                    var jmul = j * stride[1] - kernelSize[1] / 2;
                    var sindex = j + i * ret.offsets[1];
                    for (int ch = 0; ch < outChannels; ch++)
                    {
                        double val = 0;
                        var mini1 = Math.Max(-imul, 0);
                        var minj1 = Math.Max(-jmul, 0);
                        var maxi1 = Math.Min(kernelSize[0], hin - imul);
                        var maxj1 = Math.Min(kernelSize[1], win - jmul);
                        for (int zz = 0; zz < c; zz++)
                        {
                            var kernel = filters[ch, zz];
                            int kindex = 0;
                            int offset1 = zz * ar.offsets[0];

                            int offset = offset1 + imul * ar.offsets[1];

                            for (int i1 = mini1; i1 < maxi1; i1++)
                            {
                                var x = imul + i1;
                                for (int j1 = minj1; j1 < maxj1; j1++)
                                {
                                    //var y=jmul+j1
                                    val += kernel.Data[kindex] * ar.Data[offset + jmul + j1];
                                    kindex++;
                                }
                                offset += ar.offsets[1];
                            }
                        }

                        //ret.Set3D(ch,i,j,val)
                        ret.Data[sindex] = val;
                        sindex += ret.offsets[0];
                    }

                }
            }

            return ret;
        }


        public string method = "";

        public override InternalArray Forward(InternalArray ar)
        {
            //Profiler.PushCurrent(new CalcLogItem(this, "conv2d"));
#if PROFILER
            LogItem = new CalcLogItem(this, "conv2d");
            Profiler.AddLog(LogItem);
            var sw = Stopwatch.StartNew();
#endif

            var hin = ar.Shape[2];
            var win = ar.Shape[3];
            var n = ar.Shape[0];
            var c = ar.Shape[1];

            var hout = ((hin + 2 * padding[0] - dilation[0] * (kernelSize[0] - 1) - 1) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - dilation[1] * (kernelSize[1] - 1) - 1) / stride[1]) + 1;



            var cout = Weight.Shape[0];
            InternalArray ret = new InternalArray(new int[] { n, cout, hout, wout });

            //get all 3d images            
            int pos = 0;
            int pos0 = 0;
            for (int i = 0; i < n; i++)
            {
                var img = ar.GetNext3DImageFrom4DArray(ref pos0);
                InternalArray img2 = null;
                if (AllowOptimized)
                {
                    if (AllowOptimizedViaDot)
                    {
                        img2 = ProcessImageViaDot(img);
#if PROFILER
                        method = "dot";
#endif
                    }
                    else
                    {
                        /*if (dilation[0] == 1 && kernelSize[0] == 3 && padding[0] > 0)
                        {
#if PROFILER

                            method = "+pad";
#endif
                            img2 = ProcessImageOptimized2(img, hout, wout, c, hin, win);
                            var img4 = Helpers.Pad3d(img, padding[0]);
                            var img3 = ProcessImageOptimizedNoPadding(img4, hout + padding[0] * 2, wout + padding[0] * 2, c, img.Shape[1], img.Shape[2]);
                            var img5 = Helpers.Unpad3d(img, padding[0]);

                        }
                        else*/
                        {

                            if (padding[0] == 0 && dilation[1] == 1 && kernelSize[0] == 1 && stride[0] == 1)
                            {
#if PROFILER
                                method = "nopad.k1";
#endif
                                img2 = ProcessImageOptimizedNoPaddingKernel1(img, hout, wout, c, hin, win);
                                /*var img3 = ProcessImageOptimized2(img, hout, wout, c, hin, win);
                                if (!img2.IsEqual(img3))
                                {
                                    throw new Exception("11");
                                }*/


                            }
                            else


                            if (padding[0] == 0 && dilation[0] == 1 && kernelSize[0] == 3)
                            {
#if PROFILER
                                method = "no pad";
                                //Helpers.Pad2d()
#endif
                                img2 = ProcessImageOptimizedNoPadding(img, hout, wout, c, hin, win);
                                /*var img3 = ProcessImageOptimized2(img, hout, wout, c, hin, win);
                                if (!img2.IsEqual(img3))
                                {
                                    throw new Exception("11");
                                }
                                */
                            }
                            else
                            {
#if PROFILER
                                method = "opt2";
#endif
                                img2 = ProcessImageOptimized2(img, hout, wout, c, hin, win);
                            }
                        }


                    }
                }
                else
                {
#if PROFILER
                    method = "native";
#endif
                    //img2 = ProcessImage(img);
                    img2 = ProcessImageBySubRegions(img);
                    /*var res1 = ProcessImageBySubRegions(img);
                    if (!img2.IsEqual(res1))
                    {
                        throw new Exception("11");
                    }*/

                }

                Array.Copy(img2.Data, 0, ret.Data, pos, img2.Data.Length);
                pos += img2.Data.Length;
            }
            //sw.Stop();

            //  LastMs = sw.ElapsedMilliseconds;
#if PROFILER

            Profiler.AddLog(LogItem, this, method + "; hin: " + hin + "; win: ;" + win + "; padding: " + padding[0] + "; dilation: " + dilation[0] + "; ksize: " + kernelSize[0] + "; stride: " + stride[0] + "; nIn: " + inChannels + "; nOut: " + outChannels, LastMs);

            Profiler.AddLog(LogItem, this, $"ar: {ar}; w: {Weight}");
            Profiler.AddLog(LogItem, this, "out shape: " + ret.Shape.Aggregate("", (x, y) => x + y + "; "));
            //Profiler.PopCurrent();
            if (Parent != null)
            {
                Profiler.AddLog(Parent.LogItem, LogItem);
            }

#endif
            return ret;
        }

    }
}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class InternalArray
    {
        public override string ToString()
        {
            if (shp == null)
            {
                string shp1 = "(";
                for (int i = 0; i < Shape.Length; i++)
                {
                    if (i > 0) { shp1 += ", "; }
                    shp1 += Shape[i];
                }
                shp1 += ")";
                shp = $"{Name}: {shp1}";
            }
            return shp;
        }
        string shp = null;
        #region ctors
        public InternalArray(int[] dims)
        {
            Shape = (int[])dims.Clone();
            long l = 1;
            for (int i = 0; i < dims.Length; i++)
            {
                l *= dims[i];
            }
            Data = new double[l];


            offsets = new int[dims.Length - 1];
            for (int i = 0; i < dims.Length - 1; i++)
            {
                int val = 1;

                for (int j = 0; j < (i + 1); j++)
                {
                    val *= Shape[Shape.Length - 1 - j];
                }
                offsets[offsets.Length - 1 - i] = val;
            }
            //offsets = offsets.Reverse().ToArray();
            //= new int[] { Shape.Dims[1] * Shape.Dims[2] * Shape.Dims[3], Shape.Dims[2] * Shape.Dims[3], Shape.Dims[3] };
            //= new int[] { Shape.Dims[1] * Shape.Dims[2], Shape.Dims[2] };
        }

        internal void Mult(double v)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] *= v;
            }
        }
        #endregion



        #region fields

        public string Name { get; set; }
        public readonly int[] offsets = null;
        public double[] Data;
        public int[] Shape;

        #endregion



        public static InternalArray FromXml(string path)
        {
            XDocument doc = XDocument.Load(path);

            var item = doc.Descendants("data").First().Value;
            var sz = doc.Descendants("size").First().Value;

            var dims = sz.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            InternalArray ret = new InternalArray(dims.Select(int.Parse).ToArray());

            var data = item.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            ret.Data = data.Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();

            return ret;
        }

        public void Add(InternalArray ar)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] += ar.Data[i];
            }
        }

        public void Add(double ar)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] += ar;
            }
        }

        public static InternalArray Add(InternalArray a1, InternalArray a2)
        {
            if (a1.Shape.Length != a2.Shape.Length) throw new ArgumentException("dim1.len!=dim2.len");
            int[] outs = new int[a1.Shape.Length];
            bool rightBigger = false;
            for (int i = 0; i < a1.Shape.Length; i++)
            {
                outs[i] = Math.Max(a1.Shape[i], a2.Shape[i]);
                if (a2.Shape[i] > a1.Shape[i])
                {
                    rightBigger = true;
                }
            }
            if (rightBigger)
            {
                var temp = a2;
                a2 = a1;
                a1 = temp;
            }
            InternalArray ret = new InternalArray(outs);
            if (outs.Length == 4)
            {
                //iterate over all values of bigger one and add smallest one.
                int index = 0;
                int index2 = 0;
                for (int i = 0; i < a1.Shape[0]; i++)
                {
                    if (!a2.WithIn(i, 0, 0, 0)) continue;
                    for (int i1 = 0; i1 < a1.Shape[1]; i1++)
                    {
                        if (!a2.WithIn(i, i1, 0, 0)) continue;
                        for (int i2 = 0; i2 < a1.Shape[2]; i2++)
                        {
                            if (!a2.WithIn(i, i1, i2, 0)) continue;
                            for (int i3 = 0; i3 < a1.Shape[3]; i3++)
                            {
                                if (!a2.WithIn(i, i1, i2, i3)) continue;
                                //a1.Data[index++] = a2.Get4D(i, i1, i2, i3);
                                ret.Data[index] = a1.Data[index] + a2.Data[index2];
                                index++;
                                index2++;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return ret;
        }
        public double Get4D(int v1, int v2, int v3, int v4)
        {
            //int[] ar = new int[] { Shape.Dims[1] * Shape.Dims[2] * Shape.Dims[3], Shape.Dims[2] * Shape.Dims[3], Shape.Dims[3] };

            int[] dat = new int[] { v1, v2, v3 };

            int pos = v4;
            for (int i = 0; i < 3; i++)
            {
                pos += dat[i] * offsets[i];
            }
            return Data[pos];
        }

        public void Set4D(int v1, int v2, int v3, int v4, double val)
        {
            //int[] ar = new int[] { Shape.Dims[1] * Shape.Dims[2] * Shape.Dims[3], Shape.Dims[2] * Shape.Dims[3], Shape.Dims[3] };
            int[] dat = new int[] { v1, v2, v3 };

            int pos = v4;
            for (int i = 0; i < 3; i++)
            {
                pos += dat[i] * offsets[i];
            }
            Data[pos] = val;
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Set3D(int v1, int v2, int v3, double val)
        {
            Data[v3 + v1 * offsets[0] + v2 * offsets[1]] = val;
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public double Get3D(int v1, int v2, int v3)
        {
            return Data[v1 * offsets[0] + v2 * offsets[1] + v3];
        }

        public void Sub(InternalArray ar)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] -= ar.Data[i];
            }
        }

        public void Sub(double bias)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] -= bias;
            }
        }

        public double GetItem(int[] index)
        {
            int pos = 0;
            for (int i = 0; i < index.Length; i++)
            {
                pos += index[i] * Shape[i];
            }
            return Data[pos];
        }

        public InternalArray Clone()
        {
            InternalArray ret = new InternalArray(Shape);
            ret.Shape = (int[])ret.Shape.Clone();
            ret.Data = new double[Data.Length];
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }

        internal InternalArray Unsqueeze(int v)
        {
            InternalArray ret = new InternalArray(new int[] { 1, Shape[0], Shape[1], Shape[2] });
            ret.Data = Data.ToArray();
            return ret;
        }

        internal InternalArray Transpose(int[] v)
        {
            //3d only!
            InternalArray ret = new InternalArray(v.Select(z => Shape[z]).ToArray());

            for (int i = 0; i < Shape[0]; i++)
            {
                for (int j = 0; j < Shape[1]; j++)
                {
                    for (int k = 0; k < Shape[2]; k++)
                    {
                        var val = Get3D(i, j, k);
                        var ar1 = new int[] { i, j, k };
                        ret.Set3D(ar1[v[0]], ar1[v[1]], ar1[v[2]], val);
                    }
                }
            }

            return ret;

        }
        internal void Set(int[] inds, double val)
        {
            switch (inds.Length)
            {
                case 2:
                    Set2D(inds[0], inds[1], val);
                    break;
                case 3:
                    Set3D(inds[0], inds[1], inds[2], val);
                    break;
                case 4:
                    Set4D(inds[0], inds[1], inds[2], inds[3], val);
                    break;
                default:
                    throw new Exception($"set value: unsupported dim len: {inds.Length}");
            }            
        }
        internal void Set2D(int i, int j, double val)
        {
            int pos = i * Shape[1] + j;
            Data[pos] = val;
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal bool WithIn(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Shape[0] && y < Shape[1];
        }
#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal bool WithIn(int x, int y, int z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < Shape[0] && y < Shape[1] && z < Shape[2];
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal bool WithIn(int x, int y, int z, int k)
        {
            return x >= 0 && y >= 0 && z >= 0 && k >= 0 && x < Shape[0] && y < Shape[1] && z < Shape[2] && k < Shape[3];
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public double Get2D(int i, int j)
        {
            int pos = i * Shape[1] + j;
            return Data[pos];
        }

        public static double tolerance = 10e-6;

        public bool IsEqual(InternalArray resss)
        {
            if (Shape.Length != resss.Shape.Length) return false;
            for (int i = 0; i < Shape.Length; i++)
            {
                if (Shape[i] != resss.Shape[i]) return false;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                if (Math.Abs(Data[i] - resss.Data[i]) > tolerance) return false;
            }
            return true;
        }
    }
}

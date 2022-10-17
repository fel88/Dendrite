using Dendrite.Preprocessors;
using Onnx;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public static class Helpers
    {
        public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            var size = new System.Drawing.Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static GraphicsPath HalfRoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            var size = new System.Drawing.Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom   
            arc.Y = bounds.Bottom - diameter;
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);


            path.CloseFigure();
            return path;
        }

        public static Mat DrawBoxes(Mat mat1, Rect[] detections, float[] oscores, float visTresh, int[] classes = null)
        {

            Mat mat = mat1.Clone();


            for (int i = 0; i < detections.Length; i++)
            {
                if (oscores[i] < visTresh) continue;
                mat.Rectangle(detections[i], new OpenCvSharp.Scalar(255, 0, 0), 2);

                var text = Math.Round(oscores[i], 4).ToString();
                if (classes != null)
                {
                    int cls = classes[i];
                    text += $"(cls: {cls})";
                }
                var cx = detections[i].X;
                var cy = detections[i].Y + 12;
                mat.Rectangle(new OpenCvSharp.Point(cx, cy + 5), new OpenCvSharp.Point(cx + 120, cy - 15), new Scalar(0, 0, 0), -1);
                mat.PutText(text, new OpenCvSharp.Point(cx, cy),
                            HersheyFonts.HersheyDuplex, 0.5, new Scalar(255, 255, 255));
            }
            return mat;
        }
        public static Mat DrawBoxes(Mat mat1, ObjectDetectionInfo[] detections, float visTresh, bool printLabels = true)
        {
            Mat mat = mat1.Clone();
            if (mat.Type() != MatType.CV_8UC3)
                mat.ConvertTo(mat, MatType.CV_8UC3);

            for (int i = 0; i < detections.Length; i++)
            {
                if (detections[i].Conf < visTresh) continue;
                mat.Rectangle(detections[i].Rect, new OpenCvSharp.Scalar(255, 0, 0), 2);

                var text = Math.Round(detections[i].Conf, 4).ToString();
                if (detections[i].Class != null)
                {
                    int cls = detections[i].Class.Value;
                    text += $"(cls: {cls} {detections[i].Label})";
                }
                var cx = detections[i].Rect.X;
                var cy = detections[i].Rect.Y + 12;
                if (printLabels)
                {
                    mat.Rectangle(new OpenCvSharp.Point(cx, cy + 5), new OpenCvSharp.Point(cx + 250, cy - 15), new Scalar(0, 0, 0), -1);
                    mat.PutText(text, new OpenCvSharp.Point(cx, cy),
                                HersheyFonts.HersheyDuplex, 0.5, new Scalar(255, 255, 255));
                }
            }
            return mat;
        }

        public static double ParseDouble(this string str)
        {
            return double.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }
        public static float ParseFloat(this string str)
        {
            return float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }
        public static int ParseInt(this string str)
        {
            return int.Parse(str);
        }
        public static string ToDoubleInvariantString(this float str)
        {
            return str.ToString().Replace(",", ".");
        }

        public static Mat drawKeypoints(Mat mat1, KeypointsDetectionInfo[] detections, float visTresh)
        {
            Mat mat = mat1.Clone();

            for (int i = 0; i < detections.Length; i++)
            {
                for (int j = 0; j < detections[i].Points.Length; j++)
                {
                    mat.Circle((int)detections[i].Points[j].X, (int)detections[i].Points[j].Y, 5, Scalar.AliceBlue, 1);
                }

                List<Scalar> clrs = new List<Scalar>();
                clrs.Add(Scalar.Red);
                clrs.Add(Scalar.Yellow);
                clrs.Add(Scalar.Green);
                clrs.Add(Scalar.Blue);
                clrs.Add(Scalar.Black);
                clrs.Add(Scalar.White);
                clrs.Add(Scalar.LightGray);
                clrs.Add(Scalar.LightBlue);
                clrs.Add(Scalar.MediumVioletRed);
                clrs.Add(Scalar.Violet);
                clrs.Add(Scalar.BlueViolet);
                clrs.Add(Scalar.OrangeRed);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);
                clrs.Add(Scalar.Orange);

                for (int j = 0; j < DrawKeypointsPostProcessor.Edges.Length; j += 2)
                {
                    var i0 = DrawKeypointsPostProcessor.Edges[j];
                    var i1 = DrawKeypointsPostProcessor.Edges[j + 1];
                    var p0 = detections[i].Points[i0];
                    var p1 = detections[i].Points[i1];
                    mat.Line(new OpenCvSharp.Point(p0.X, p0.Y), new OpenCvSharp.Point(p1.X, p1.Y), clrs[j / 2], 2);
                }
                /* if (detections[i].Conf < visTresh) continue;
                 mat.Rectangle(detections[i].Rect, new OpenCvSharp.Scalar(255, 0, 0), 2);

                 var text = Math.Round(detections[i].Conf, 4).ToString();
                 if (detections[i].Class != null)
                 {
                     int cls = detections[i].Class.Value;
                     text += $"(cls: {cls} {detections[i].Label})";
                 }
                 var cx = detections[i].Rect.X;
                 var cy = detections[i].Rect.Y + 12;
                 mat.Rectangle(new OpenCvSharp.Point(cx, cy + 5), new OpenCvSharp.Point(cx + 250, cy - 15), new Scalar(0, 0, 0), -1);
                 mat.PutText(text, new OpenCvSharp.Point(cx, cy),
                             HersheyFonts.HersheyDuplex, 0.5, new Scalar(255, 255, 255));*/
            }
            return mat;
        }

        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"

            var resourcePath = assembly.GetManifestResourceNames()
                 .Single(str => str.Contains(name));


            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        public static void ShowError(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowQuestion(string msg, string caption, MessageBoxButtons btn = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(msg, caption, btn, MessageBoxIcon.Question);
        }

        public static void ShowInfo(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static string AsString(this AttributeProto item)
        {
            string val = string.Empty;
            if (item.HasF)
            {
                val = item.F.ToString();
            }
            if (item.HasI)
            {
                val = item.I.ToString();
            }
            if (item.HasS)
            {
                val = item.S.ToStringUtf8();
            }
            if (item.Strings != null && item.Strings.Any())
            {
                val = string.Join("; ", item.Strings.Select(z => z.ToStringUtf8()));
            }
            if (item.Floats != null && item.Floats.Any())
            {
                val = string.Join("; ", item.Floats);
            }
            if (item.Ints != null && item.Ints.Any())
            {
                val = string.Join("; ", item.Ints);
            }
            return val;
        }
        public static InternalArray Pad2d(InternalArray ar)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[0] + 2, ar.Shape[1] + 2 });
            int pos = 0;
            int target = ret.Shape[1];
            for (int i = 0; i < ar.Shape[0]; i++)
            {
                //copy row                    
                target++;
                Array.Copy(ar.Data, pos, ret.Data, target, ar.Shape[1]);
                target += ar.Shape[1];
                pos += ar.Shape[1];
                target++;
            }

            return ret;
        }

        public static InternalArray Unpad2d(InternalArray ar)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[0] - 2, ar.Shape[1] - 2 });
            int pos = 0;
            int target = ar.Shape[1];
            for (int i = 0; i < ret.Shape[0]; i++)
            {
                //copy row                    
                target++;
                Array.Copy(ar.Data, target, ret.Data, pos, ret.Shape[1]);
                target += ret.Shape[1];
                pos += ret.Shape[1];
                target++;
            }

            return ret;
        }

        public static InternalArray Unpad3d(InternalArray ar, int padCnt = 1)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[0], ar.Shape[1] - padCnt * 2, ar.Shape[2] - padCnt * 2 });
            int pos = 0;
            int target = 0;
            for (int j = 0; j < ret.Shape[0]; j++)
            {
                target += ar.Shape[1] * padCnt;
                for (int i = 0; i < ret.Shape[1]; i++)
                {
                    //copy row                    
                    target += padCnt;
                    Array.Copy(ar.Data, target, ret.Data, pos, ret.Shape[2]);
                    target += ret.Shape[2];
                    pos += ret.Shape[2];
                    target += padCnt;
                }
                target += ar.Shape[1] * padCnt;
            }

            return ret;
        }

        public static InternalArray Pad3d(InternalArray ar, int padCnt = 1)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[0], ar.Shape[1] + padCnt * 2, ar.Shape[2] + padCnt * 2 });
            int pos = 0;
            int target = 0;
            for (int j = 0; j < ar.Shape[0]; j++)
            {
                target += ret.Shape[1] * padCnt;
                for (int i = 0; i < ar.Shape[1]; i++)
                {
                    //copy row                    
                    target += padCnt;
                    Array.Copy(ar.Data, pos, ret.Data, target, ar.Shape[2]);
                    target += ar.Shape[2];
                    pos += ar.Shape[2];
                    target += padCnt;
                }
                target += ret.Shape[1] * padCnt;
            }

            return ret;
        }

        public static bool AllowParallelProcessing = false;
        public static InternalArray[] ParallelProcess(NeuralItem[] items, InternalArray input)
        {
            InternalArray[] res = new InternalArray[items.Length];
            if (!AllowParallelProcessing)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    res[i] = items[i].Forward(input);
                }
            }
            else
            {
                Parallel.For(0, items.Length, (i) =>
                {
                    res[i] = items[i].Forward(input);
                });
            }
            return res;
        }

        public static InternalArray Cat(InternalArray[] items, int dim = 0)
        {
            var n = items[0].Shape[0];
            var sumch = items.Sum(z => z.Shape[1]);
            var h = items[0].Shape[2];
            var w = items[0].Shape[3];

            InternalArray ret = new InternalArray(new int[] { n, sumch, h, w });

            List<double> data = new List<double>();
            foreach (var item in items)
            {
                for (int i = 0; i < n; i++)
                {
                    var img = Get3DImageFrom4DArray(item, i);
                    data.AddRange(img.Data);
                }
            }

            ret.Data = data.ToArray();
            return ret;
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static InternalArray Get3DImageFrom4DArray(this InternalArray array, int ind1)
        {
            int pos = ind1 * array.offsets[0];
            InternalArray ret = new InternalArray(new int[] { array.Shape[1], array.Shape[2], array.Shape[3] });
            Array.Copy(array.Data, pos, ret.Data, 0, ret.Data.Length);
            return ret;
        }

#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static InternalArray GetNext3DImageFrom4DArray(this InternalArray array, ref int pos)
        {

            InternalArray ret = new InternalArray(new int[] { array.Shape[1], array.Shape[2], array.Shape[3] });
            Array.Copy(array.Data, pos, ret.Data, 0, ret.Data.Length);
            pos += array.offsets[0];
            return ret;
        }
        public static List<NeuralItem> GetAllChilds(NeuralItem item, List<NeuralItem> ret = null)
        {
            if (ret == null)
            {
                ret = new List<NeuralItem>();
            }
            if (item.Childs == null)
            {
                ret.Add(item);
            }
            else
            {
                foreach (var citem in item.Childs)
                {
                    GetWeightedAllChilds(citem, ret);
                }
            }
            return ret;
        }
        public static List<NeuralItem> GetWeightedAllChilds(NeuralItem item, List<NeuralItem> ret = null)
        {
            if (ret == null)
            {
                ret = new List<NeuralItem>();
            }
            if (item.Childs == null)
            {
                ret.Add(item);
            }
            else
            {
                foreach (var citem in item.Childs)
                {
                    if (citem is AvgPool2d) continue;
                    GetWeightedAllChilds(citem, ret);
                }
            }
            return ret;
        }

        public static InternalArray ParseFromString(string str)
        {
            List<int> dims = new List<int>();

            int dim = 0;
            int maxdim = 0;
            StringBuilder sb = new StringBuilder();
            List<int> cnt = new List<int>();
            List<double> data = new List<double>();
            str = str.Substring(str.IndexOf('['));
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '[')
                {

                    dim++; maxdim = Math.Max(dim, maxdim);

                    if (dims.Count < maxdim)
                    {
                        dims.Add(0);
                        cnt.Add(0);
                    }
                    cnt[dim - 1] = 1;
                    sb.Clear();
                    continue;
                }
                if (str[i] == ']')
                {
                    if (dim == maxdim)
                    {
                        data.Add(double.Parse(sb.ToString()));
                    }
                    dims[dim - 1] = Math.Max(dims[dim - 1], cnt[dim - 1]);
                    dim--;
                    sb.Clear();
                    if (dim == 0) { break; }
                    continue;
                }
                if (str[i] == ',')
                {
                    if (dim == maxdim)
                    {
                        data.Add(double.Parse(sb.ToString()));
                    }
                    sb.Clear();
                    cnt[dim - 1]++;
                    continue;
                }
                sb.Append(str[i]);
            }

            InternalArray ret = new InternalArray(dims.ToArray());
            ret.Data = data.ToArray();
            return ret;
        }
        public static InternalArray ParseFromString2(string str)
        {

            var ar = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var a1 = ar[0].Split(new char[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var dims = a1.Select(int.Parse).ToArray();
            InternalArray ret = new InternalArray(dims.ToArray());
            foreach (var ss in ar.Skip(1))
            {
                var s1 = ss.Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                int[] index = new int[dims.Length];
                for (int i = 0; i < dims.Length; i++)
                {
                    index[i] = int.Parse(s1[i]);
                }
                var val = double.Parse(s1.Last());
                ret.Set4D(index[0], index[1], index[2], index[3], val);
            }



            return ret;
        }
#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static InternalArray GetNext2dImageFrom4dArray(this InternalArray array, ref int pos0)
        {

            InternalArray ret = new InternalArray(new int[] { array.Shape[2], array.Shape[3] });
            Array.Copy(array.Data, pos0, ret.Data, 0, ret.Data.Length);
            pos0 += array.offsets[1];
            return ret;
        }
#if NET461
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static InternalArray Get2DImageFrom4DArray(this InternalArray array, int ind1, int ind2)
        {
            var pos = ind1 * array.offsets[0] + ind2 * array.offsets[1];
            InternalArray ret = new InternalArray(new int[] { array.Shape[2], array.Shape[3] });
            Array.Copy(array.Data, pos, ret.Data, 0, ret.Data.Length);
            return ret;
        }
        public static InternalArray Get2DImageFrom3DArray(this InternalArray array, int ind1)
        {
            var pos = ind1 * array.offsets[0];
            InternalArray ret = new InternalArray(new int[] { array.Shape[1], array.Shape[2] });
            Array.Copy(array.Data, pos, ret.Data, 0, ret.Data.Length);
            return ret;
        }
        public static InternalArray Get1DImageFrom3DArray(this InternalArray array, int ind1, int ind2)
        {
            var pos = ind1 * array.offsets[0] + ind2 * array.offsets[1];
            InternalArray ret = new InternalArray(new int[] { array.Shape[2] });
            Array.Copy(array.Data, pos, ret.Data, 0, ret.Data.Length);
            return ret;
        }


        public static InternalArray Randn(int[] dims)
        {
            Random r = new Random();
            var ar = new InternalArray(dims);
            for (int i = 0; i < ar.Data.Length; i++)
            {
                ar.Data[i] = r.NextGaussian();
            }
            return ar;
        }

        public static InternalArray Randn(this Random r, int[] dims)
        {
            var ar = new InternalArray(dims);
            for (int i = 0; i < ar.Data.Length; i++)
            {
                ar.Data[i] = r.NextGaussian();
            }
            return ar;
        }

        public static double NextGaussian(this Random rand, double mean = 0, double stdDev = 1)
        {
            //Random rand = new Random(); //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }

        public static long[] VectorsTest(int k = 1000)
        {

            float[] data = new float[k];
            float[] data2 = new float[k];

            Random r = new Random();
            for (int i = 0; i < k; i++)
            {
                data[i] = ((float)r.NextDouble());
                data2[i] = ((float)r.NextDouble());
            }

            float[] res = new float[data.Length];
            float[] res2 = new float[data.Length];
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < data.Length; i++)
            {
                res2[i] = data[i] * data2[i];
            }

            sw1.Stop();
            var e1 = sw1.ElapsedMilliseconds;

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < data.Length; i += 4)
            {
                /*Vector4 v1 = new Vector4((float)data[i], (float)data[i + 1], (float)data[i + 2], data[i + 3]);
                Vector4 v2 = new Vector4((float)data2[i], (float)data2[i + 1], (float)data2[i + 2], data2[i + 3]);

                var rr = Vector4.Multiply(v1, v2);

                res[i] = rr.X;
                res[i + 1] = rr.Y;
                res[i + 2] = rr.Z;
                res[i + 3] = rr.W;*/
            }

            sw2.Stop();
            var e2 = sw2.ElapsedMilliseconds;

            for (int i = 0; i < res2.Length; i++)
            {
                if (res2[i] != res[i])
                {
                    throw new ArgumentException();
                }
            }
            return new[] { e1, e2 };
        }
    }
}

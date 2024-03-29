﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dendrite
{
    public class Decoders
    {
        public static int[] sort_indexes(List<float> scores)
        {
            var order = scores.Select((z, i) => new Tuple<int, float>(i, z)).OrderByDescending(z => z.Item2).Select(z => z.Item1).ToArray();
            return order;
        }
        public static Dictionary<string, float[][]> allPriorBoxes = new Dictionary<string, float[][]>();
        public static float[][] decode(List<float[]> loc, float[][] priors, float[] variances)
        {
            List<float[]> ret = new List<float[]>();

            for (var i = 0; i < loc.Count; i++)
            {

                float z0 = priors[i][0] + loc[i][0] * variances[0] * priors[i][2];
                float z1 = priors[i][1] + loc[i][1] * variances[0] * priors[i][3];
                float z2 = (float)(priors[i][2] * Math.Exp(loc[i][2] * variances[1]));
                float z3 = (float)(priors[i][3] * Math.Exp(loc[i][3] * variances[1]));

                z0 -= z2 / 2;
                z1 -= z3 / 2;
                z2 += z0;
                z3 += z1;
                ret.Add(new float[] { z0, z1, z2, z3 });
            }


            return ret.ToArray();
        }

        public static int[] nms(List<float[]> dets, float thresh)
        {
            List<float> x1 = new List<float>();
            List<float> y1 = new List<float>();
            List<float> x2 = new List<float>();
            List<float> y2 = new List<float>();
            List<float> scores = new List<float>();

            List<float> areas = new List<float>();
            for (var i = 0; i < dets.Count; i++)
            {
                x1.Add(dets[i][0]);
                y1.Add(dets[i][1]);
                x2.Add(dets[i][2]);
                y2.Add(dets[i][3]);
                scores.Add(dets[i][4]);
                areas.Add((x2[i] - x1[i] + 1) * (y2[i] - y1[i] + 1));
            }


            var order = scores.Select((z, i) => new Tuple<int, float>(i, z)).OrderByDescending(z => z.Item2).Select(z => z.Item1).ToArray();


            List<int> keep = new List<int>();
            while (order.Count() > 0)
            {
                int i = order[0];
                keep.Add(i);
                List<float> xx1 = new List<float>();
                List<float> yy1 = new List<float>();
                List<float> xx2 = new List<float>();
                List<float> yy2 = new List<float>();
                for (var j = 1; j < order.Count(); j++)
                {
                    xx1.Add(Math.Max(x1[i], x1[order[j]]));
                    yy1.Add(Math.Max(y1[i], y1[order[j]]));
                    xx2.Add(Math.Min(x2[i], x2[order[j]]));
                    yy2.Add(Math.Min(y2[i], y2[order[j]]));
                }
                List<float> w = new List<float>();
                List<float> h = new List<float>();
                List<float> inter = new List<float>();

                for (var j = 0; j < xx2.Count(); j++)
                {
                    w.Add(Math.Max(0.0f, xx2[j] - xx1[j] + 1));
                    h.Add(Math.Max(0.0f, yy2[j] - yy1[j] + 1));
                    inter.Add(w[j] * h[j]);
                }



                List<float> ovr = new List<float>();
                for (var j = 0; j < inter.Count(); j++)
                {
                    ovr.Add(inter[j] / (areas[i] + areas[order[j + 1]] - inter[j]));
                }



                List<int> inds = new List<int>();
                for (var j = 0; j < ovr.Count(); j++)
                {
                    if (ovr[j] > thresh) continue;
                    inds.Add(j);
                }


                List<int> order2 = new List<int>();
                for (var j = 0; j < inds.Count(); j++)
                {
                    order2.Add(order[inds[j] + 1]);
                }
                order = order2.ToArray();
            }

            return keep.ToArray();
        }

        public static int[][] min_sizes = new int[][] { new int[] { 32, 64, 128 }, new int[] { 256 }, new int[] { 512 } };
        public static int[] steps = new int[] { 32, 64, 128 };

        public static float[][] PriorBoxes(int img_w, int img_h)
        {

            List<float[]> prior_data = new List<float[]>();

            List<int[]> feature_maps = new List<int[]>();

            foreach (var step in steps)
            {
                int w1 = (int)Math.Ceiling(img_w / (float)step);
                int h1 = (int)Math.Ceiling(img_h / (float)step);
                feature_maps.Add(new int[] { h1, w1 });
            }


            for (var k = 0; k < feature_maps.Count; k++)
            {
                var f = feature_maps[k];

                var _min_sizes = min_sizes[k];
                for (var i = 0u; i < (uint)f[0]; i++)
                {
                    for (int j = 0; j < f[1]; j++)
                    {
                        for (var jj = 0u; jj < 2; jj++)
                        {
                            int min_size = _min_sizes[jj];
                            float s_kx = min_size / (float)img_w;
                            float s_ky = min_size / (float)img_h;

                            float x = j + 0.5f;
                            var cx = (x * steps[k] / img_w);
                            float y = i + 0.5f;
                            var cy = (y * steps[k] / img_h);
                            prior_data.Add(new float[] { cx, cy, s_kx, s_ky });
                        }
                    }
                }
            }
            return prior_data.ToArray();
        }
        public static Tuple<Rect[], float[], int[]> BoxesDecode(OpenCvSharp.Size matSize, float[] confd, float[] locd, System.Drawing.Size sz, float[][] prior_data, float vis_thresh = 0.5f)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (confd == null || locd == null)
            {
                return null;
            }




            List<float[]> loc = new List<float[]>();
            List<float> scores = new List<float>();
            List<int> winners = new List<int>();

            float[] variances = new float[] { 0.1f, 0.2f };
            var nnInputWidth = sz.Width;
            var nnInputHeight = sz.Height;
            float wz1 = nnInputWidth;
            float hz1 = nnInputHeight;
            float[] scale = new float[] { (float)nnInputWidth, (float)nnInputHeight, (float)nnInputWidth, (float)nnInputHeight };
            float koef = wz1 / (float)(matSize.Width);
            float koef2 = hz1 / (float)(matSize.Height);


            float[] resize = new float[] { koef, koef2 };

            var rets3 = confd;
            var rets1 = locd;


            for (var i = 0; i < rets1.Length; i += 4)
            {
                loc.Add(new float[] { rets1[i + 0], rets1[i + 1], rets1[i + 2], rets1[i + 3] });
            }
            int numClasses = rets3.Length / (rets1.Length / 4);

            for (var i = 0; i < rets3.Length; i += numClasses)
            {
                if (numClasses > 2)
                {
                    //first class - background usually
                    float maxj = rets3[i + 1];
                    int mind = 1;
                    for (int j = 2; j < numClasses; j++)
                    {
                        if (rets3[i + j] > maxj)
                        {
                            maxj = rets3[i + j];
                            mind = j;
                        }
                    }

                    //var sub = rets3.Skip(i + 1).Take(numClasses - 1).Select((v, ii) => new Tuple<int, float>(ii, v)).OrderByDescending(z => z.Item2).First();
                    winners.Add(mind - 1);
                    scores.Add(maxj);
                }
                else
                {
                    scores.Add(rets3[i + 1]);
                }
            }

            var boxes = Decoders.decode(loc, prior_data, variances);
            for (var i = 0; i < boxes.Count(); i++)
            {
                boxes[i][0] = (boxes[i][0] * scale[0]) / resize[0];
                boxes[i][1] = (boxes[i][1] * scale[1]) / resize[1];
                boxes[i][2] = (boxes[i][2] * scale[2]) / resize[0];
                boxes[i][3] = (boxes[i][3] * scale[3]) / resize[1];
            }

            float[] scale1 = new float[] { wz1, hz1, wz1, hz1, wz1, hz1, wz1, hz1, wz1, hz1 };

            float confidence_threshold = 0.2f;
            List<int> inds = new List<int>();

            for (var i = 0; i < scores.Count(); i++)
            {
                if (scores[i] > confidence_threshold)
                {
                    inds.Add(i);
                }
            }

            List<float[]> boxes2 = new List<float[]>();
            for (var i = 0; i < inds.Count(); i++)
            {
                boxes2.Add(boxes[inds[i]]);
            }
            boxes = boxes2.ToArray();

            List<float> scores2 = new List<float>();
            for (var i = 0; i < inds.Count(); i++)
            {
                scores2.Add(scores[inds[i]]);
            }
            scores = scores2;

            List<int> winners2 = new List<int>();
            if (numClasses > 2)
            {
                for (var i = 0; i < inds.Count(); i++)
                {
                    winners2.Add(winners[inds[i]]);
                }
                winners = winners2;
            }

            var order = Decoders.sort_indexes(scores);
            List<float[]> boxes3 = new List<float[]>();
            for (var i = 0; i < order.Count(); i++)
            {
                boxes3.Add(boxes[order[i]]);

            }

            boxes = boxes3.ToArray();

            List<float> scores3 = new List<float>();
            for (var i = 0u; i < order.Count(); i++)
            {
                scores3.Add(scores[order[i]]);

            }
            scores = scores3;

            if (numClasses > 2)
            {
                List<int> winners3 = new List<int>();

                for (var i = 0; i < inds.Count(); i++)
                {
                    winners3.Add(winners[order[i]]);
                }
                winners = winners3;
            }
            //2. nms
            List<float[]> dets = new List<float[]>();
            for (var i = 0; i < boxes.Count(); i++)
            {
                if (numClasses > 2)
                {
                    dets.Add(new float[] { boxes[i][0], boxes[i][1], boxes[i][2], boxes[i][3], scores[i], winners[i] });
                }
                else
                {
                    dets.Add(new float[] { boxes[i][0], boxes[i][1], boxes[i][2], boxes[i][3], scores[i] });
                }
            }
            var keep = Decoders.nms(dets, 0.4f);

            List<float[]> dets2 = new List<float[]>();

            for (var i = 0u; i < keep.Count(); i++)
            {
                dets2.Add(dets[keep[i]]);
            }
            dets = dets2;

            List<Rect> detections = new List<Rect>();

            //float vis_thresh = 0.2f;

            List<int> indexMap = new List<int>();

            //List<float[]> odets = new List<float[]>();
            List<float> oscores = new List<float>();
            List<int> owin = new List<int>();

            for (var i = 0; i < dets.Count(); i++)
            {
                var aa = dets[i];
                if (aa[4] < vis_thresh) continue;
                detections.Add(new Rect((int)(aa[0]), (int)(aa[1]), (int)(aa[2] - aa[0]), (int)(aa[3] - aa[1])));
                indexMap.Add(i);

                //oscores.Add(scores3[i]);
                oscores.Add(aa[4]);
                if (numClasses > 2)
                {
                    owin.Add((int)aa[5]);
                }
            }

            /* for (var i = 0; i < dets.Count(); i++)
             {
                 odets.Add(dets[i]);
             }*/
            sw.Stop();

            var ret = new Tuple<Rect[], float[], int[]>(detections.ToArray(), oscores.ToArray(), numClasses > 2 ? owin.ToArray() : null);
            return ret;
        }

        public static float[][] PriorBoxes2(int img_w, int img_h)
        {
            List<float[]> prior_data = new List<float[]>();

            List<int[]> feature_maps = new List<int[]>();

            foreach (var step in steps)
            {
                int w1 = (int)Math.Ceiling(img_w / (float)step);
                int h1 = (int)Math.Ceiling(img_h / (float)step);
                feature_maps.Add(new int[] { h1, w1 });
            }


            for (var k = 0; k < feature_maps.Count; k++)
            {
                var f = feature_maps[k];

                var _min_sizes = min_sizes[k];
                for (var i = 0u; i < (uint)f[0]; i++)
                {
                    for (int j = 0; j < f[1]; j++)
                    {

                        for (var jj = 0u; jj < _min_sizes.Length; jj++)
                        {
                            int min_size = _min_sizes[jj];
                            float s_kx = min_size / (float)img_w;
                            float s_ky = min_size / (float)img_h;

                            float[] dx = null;
                            float[] dy = null;
                            float[] xar = null;
                            float[] yar = null;

                            if (min_size == 32)
                            {
                                xar = new float[] { j, j + 0.25f, j + 0.5f, j + 0.75f };
                                yar = new float[] { i, i + 0.25f, i + 0.5f, i + 0.75f };
                            }
                            else if (min_size == 64)
                            {
                                xar = new float[] { j, j + 0.5f };
                                yar = new float[] { i, i + 0.5f };
                            }
                            else
                            {
                                xar = new float[] { j + 0.5f };
                                yar = new float[] { i + 0.5f };
                            }

                            dx = (xar.Select(z => z * steps[k] / img_w)).ToArray();
                            dy = (yar.Select(z => z * steps[k] / img_h)).ToArray();
                            foreach (var cy in dy)
                                foreach (var cx in dx)
                                    prior_data.Add(new float[] { cx, cy, s_kx, s_ky });
                        }
                    }
                }
            }
            return prior_data.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
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
                            List<float> dense_cx = new List<float>();
                            List<float> dense_cy = new List<float>();
                            float x = j + 0.5f;
                            dense_cx.Add(x * steps[k] / img_w);
                            float y = i + 0.5f;
                            dense_cy.Add(y * steps[k] / img_h);


                            foreach (var cy in dense_cy)
                            {
                                foreach (var cx in dense_cx)
                                {
                                    prior_data.Add(new float[]{
                                        cx,cy,s_kx,s_ky
            });

                                }
                            }
                        }
                    }
                }
            }
            return prior_data.ToArray();
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

                            if (min_size == 32)
                            {                              
                                float[] xar = new float[] { j + 0, j + 0.25f, j + 0.5f, j + 0.75f };
                                var dx = (xar.Select(z => z * steps[k] / img_w));
                                float[] yar = new float[] { i + 0, i + 0.25f, i + 0.5f, i + 0.75f };
                                var dy = (yar.Select(z => z * steps[k] / img_h));


                                foreach (var cy in dy)
                                    foreach (var cx in dx)
                                        prior_data.Add(new float[] { cx, cy, s_kx, s_ky });

                            }
                            else if (min_size == 64)
                            {                               
                                float[] xar = new float[] { j + 0, j + 0.5f };
                                var dx = (xar.Select(z => z * steps[k] / img_w));
                                float[] yar = new float[] { i + 0, i + 0.5f };
                                var dy = (yar.Select(z => z * steps[k] / img_h));


                                foreach (var cy in dy)
                                    foreach (var cx in dx)
                                        prior_data.Add(new float[] { cx, cy, s_kx, s_ky });

                            }
                            else
                            {
                                float cx = (j + 0.5f) * steps[k] / img_w;
                                float cy = (i + 0.5f) * steps[k] / img_h;
                                prior_data.Add(new float[] { cx, cy, s_kx, s_ky });
                            }

                        }
                    }
                }
            }
            return prior_data.ToArray();
        }
    }
}
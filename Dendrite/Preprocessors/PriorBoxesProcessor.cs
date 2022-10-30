using Dendrite.Preprocessors.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "priorBoxes")]
    public class PriorBoxesProcessor : AbstractPreprocessor, IPriorBoxesGenerator
    {
        public float[] Variances = new float[] { 0.1f, 0.2f };
        public int[][] MinSizes = new int[][] { new int[] { 32, 64, 128 }, new int[] { 256 }, new int[] { 512 } };
        public int[] Steps = new int[] { 32, 64, 128 };
        public override string Name => "prior boxes";

        //retinaFace:
        //int[][] min_sizes = new int[][] { new int[] { 16, 32 }, new int[] { 64, 128 }, new int[] { 256, 512 } };
        //int[] steps = new int[] { 8, 16, 32 };
        //PriorBoxes2Mode=false

        public bool UseCache { get; set; }
        public bool PriorBoxes2Mode { get; set; } = true;
        public float[][] Generate(int w, int h)
        {
            //use cache?
            if (UseCache)
            {
                string key = $"{w}x{h}";
                if (!allPriorBoxes.ContainsKey(key))
                {
                    allPriorBoxes[key] = PriorBoxes(w, h);

                }
                return allPriorBoxes[key];
            }
            return PriorBoxes(w, h);
        }

        public override Type ConfigControl => typeof(PriorBoxesConfigControl);

        Dictionary<string, float[][]> allPriorBoxes = new Dictionary<string, float[][]>();

        public float[][] PriorBoxes(int img_w, int img_h)
        {

            List<float[]> prior_data = new List<float[]>();

            List<int[]> feature_maps = new List<int[]>();

            foreach (var step in Steps)
            {
                int w1 = (int)Math.Ceiling(img_w / (float)step);
                int h1 = (int)Math.Ceiling(img_h / (float)step);
                feature_maps.Add(new int[] { h1, w1 });
            }


            for (var k = 0; k < feature_maps.Count; k++)
            {
                var f = feature_maps[k];

                var _min_sizes = MinSizes[k];
                for (var i = 0u; i < (uint)f[0]; i++)
                {
                    for (int j = 0; j < f[1]; j++)
                    {

                        if (PriorBoxes2Mode)
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

                                dx = (xar.Select(z => z * Steps[k] / img_w)).ToArray();
                                dy = (yar.Select(z => z * Steps[k] / img_h)).ToArray();
                                foreach (var cy in dy)
                                    foreach (var cx in dx)
                                        prior_data.Add(new float[] { cx, cy, s_kx, s_ky });
                            }
                        }
                        else
                        {
                            for (var jj = 0u; jj < 2; jj++)
                            {
                                int min_size = _min_sizes[jj];
                                float s_kx = min_size / (float)img_w;
                                float s_ky = min_size / (float)img_h;

                                float x = j + 0.5f;
                                var cx = (x * Steps[k] / img_w);
                                float y = i + 0.5f;
                                var cy = (y * Steps[k] / img_h);

                                prior_data.Add(new float[] { cx, cy, s_kx, s_ky });
                            }
                        }
                    }
                }
            }
            return prior_data.ToArray();
        }

        public override object Process(object input)
        {
            OutputSlots[0].Data = this;
            return null;
        }

        public override void ParseXml(XElement sb)
        {
            Steps = sb.Attribute("steps").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseInt).ToArray();
            Variances = sb.Attribute("variances").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseFloat).ToArray();
            var msz = sb.Attribute("minSizes").Value.Split(new char[] { ',' }).ToArray();
            MinSizes = msz.Select(z => z.Split(new char[] { '(', ')', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseInt).ToArray()).ToArray();
            //parse min_sizes

            UseCache = (bool.Parse(sb.Attribute("useCache").Value));
            PriorBoxes2Mode = (bool.Parse(sb.Attribute("priorBoxes2Mode").Value));
        }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<priorBoxes priorBoxes2Mode=\"{PriorBoxes2Mode}\" useCache=\"{UseCache}\" steps=\"{string.Join(";", Steps)}\" minSizes=\"{string.Join(",", MinSizes.Select(z => $"({string.Join(";", z)})"))}\" variances=\"{string.Join(";", Variances.Select(Helpers.ToDoubleInvariantString))}\"/>");
        }
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dendrite.Preprocessors
{
    /// <summary>
    /// FaceBoxes decoder
    /// </summary>
    [XmlName(XmlKey = "boxesDecoder")]
    public class BoxesDecodePostProcessor : AbstractPreprocessor
    {

        public BoxesDecodePostProcessor()
        {
            InputSlots = new DataSlot[4];
            InputSlots[0] = (new DataSlot() { Name = "conf" });
            InputSlots[1] = (new DataSlot() { Name = "loc" });
            InputSlots[2] = (new DataSlot() { Name = "img_size" });
            InputSlots[3] = (new DataSlot() { Name = "prior_boxes" });
        }

        public override string Name => "boxes decoder";
        public float NmsThreshold { get; set; } = 0.8f;
        public double Threshold { get; set; } = 0.4;
        public float VisThreshold { get; set; } = 0.5f;

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<boxesDecoder/>");
        }

        public List<string> AllowedClasses = new List<string>();
        public ObjectDetectionInfo[] BoxesDecode(int w, int h)
        {


            var rets1 = InputSlots[0].Data as float[];
            var rets3 = InputSlots[1].Data as float[];

            /*  var dims = net.Nodes.First(z => z.IsInput).Dims;
              var sz = new System.Drawing.Size(dims[3], dims[2]);
              if (dims[2] == -1)
              {*/
            var sz = new System.Drawing.Size();
            sz.Height = h;
            sz.Width = w;
            //  }
            string key = $"{sz.Width}x{sz.Height}";
            var pg = InputSlots[3].Data as IPriorBoxesGenerator;

           /* if (!Decoders.allPriorBoxes.ContainsKey(key))
            {
                var pd = Decoders.PriorBoxes2(sz.Width, sz.Height);
                Decoders.allPriorBoxes.Add(key, pd);
            }*/
            
            //var prior_data = Decoders.allPriorBoxes[key];
            var prior_data = pg.Generate(sz.Width, sz.Height);

            var ret = Decoders.BoxesDecode(new Size(w, h), rets1, rets3, sz, prior_data, VisThreshold);

            List<ObjectDetectionInfo> ret2 = new List<ObjectDetectionInfo>();
            for (int i = 0; i < ret.Item1.Length; i++)
            {
                var rect = ret.Item1[i];
                var odi = new ObjectDetectionInfo()
                {
                    Rect = rect,
                    Conf = ret.Item2[i],
                    
                };
                if (ret.Item3 != null)
                {
                    odi.Class = ret.Item3[i];
                }
                ret2.Add(odi);
            }
            return ret2.ToArray();

        }
        public override Type ConfigControl => null;
        public override object Process(object inp)
        {
            //var list = inp as object[];
            var list = InputSlots[0].Data as object[];
            // var net = list.First(z => z is Nnet) as Nnet;
            if (InputSlots[0].FetchData != null)
            {
                foreach (var item in InputSlots)
                {
                    item.FetchData(item);
                }
            }
            else
            {
                /* var f1 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("conf"));
                 var f2 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("loc"));
                 if (f1 == null || f2 == null)
                 {
                     f2 = net.Nodes.FirstOrDefault(z => z.IsOutput && z.Dims.Last() == 4);
                     f1 = net.Nodes.FirstOrDefault(z => z != f2 && z.IsOutput);
                     if (f1 == null || f2 == null)
                         return null;
                 }*/

                //InputSlots[0].Data = net.OutputDatas[f2.Name];
                // InputSlots[1].Data = net.OutputDatas[f1.Name];
            }
            var sz = InputSlots[2].Data as int[];
            int ww = 0;
            int hh = 0;
            if (sz.Length == 4) // NCHW format
            {
                ww = sz[3];
                hh = sz[2];
            }
            else
            if (sz.Length == 2)//WH format
            {
                ww = sz[0];
                hh = sz[1];
            }
            var ret = BoxesDecode(ww, hh);
            ObjectDetectionContext ctx = new ObjectDetectionContext() { Infos = ret, Size = new Size(ww, hh) };
            OutputSlots[0].Data = ctx;
            return ret;
        }
    }
}

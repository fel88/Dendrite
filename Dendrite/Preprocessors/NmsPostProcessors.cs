using Dendrite.Preprocessors.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class NmsPostProcessors : AbstractPreprocessor
    {
        public override string Name => "nms";
        public override Type ConfigControl => typeof(NmsConfigControl);

        public float NmsThreshold = 0.2f;
        public override object Process(object input)
        {
            var list = input as object[];
            var dets = list.First(z => z is DetectionInfo[]) as DetectionInfo[];

            var bb = dets.Select(z => new float[] { z.Rect.X, z.Rect.Y, z.Rect.X + z.Rect.Width, z.Rect.Y + z.Rect.Height, z.Conf }).ToArray();

            var ret = Decoders.nms(bb.ToList(), NmsThreshold);
            List<DetectionInfo> rr = new List<DetectionInfo>();
            for (int i = 0; i < ret.Length; i++)
            {
                rr.Add(dets[ret[i]]);
            }

            return rr.ToArray();
        }
    }
}

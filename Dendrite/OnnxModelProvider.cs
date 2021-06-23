using Google.Protobuf;
using Onnx;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Dendrite
{
    public class OnnxModelProvider : ModelProvider
    {
        public override string SaveDialogFilter => "Onnx files (*.onnx)|*.onnx";

        public override void AppendToOutput(GraphModel model, GraphNode node)
        {
            var proto = (model as OnnxGraphModel).ProtoModel;
            var fr2 = (model as OnnxGraphModel).ProtoModel.Graph.Node.First(z => z.Name.ToLower().Contains(node.Name.ToLower()));

            proto.Graph.Output.Add(new ValueInfoProto() { Name = fr2.Output[0] });
        }

        public override bool IsSuitableFile(string path)
        {
            return path.EndsWith(".onnx");
        }

        public override GraphModel LoadFromFile(string path)
        {
            var bb = File.ReadAllBytes(path);
            var res2 = Onnx.ModelProto.Parser.ParseFrom(bb);



            var nodes = new List<GraphNode>();
            nodes.AddRange(res2.Graph.Node.Select(z =>
            new GraphNode(z.OpType)
            {
                Name = z.Name,
                Tag = z

            }).ToArray());

            Dictionary<string, GraphNode> outs = new Dictionary<string, GraphNode>();

            foreach (var item in res2.Graph.Input)
            {
                var gn = new GraphNode() { Name = item.Name, LayerType = LayerType.Input };
                outs.Add(item.Name, gn);
                gn.Shape = item.Type.TensorType.Shape.Dim.Select(z => z.DimValue).ToArray();

            }

            for (int i = 0; i < res2.Graph.Node.Count; i++)
            {
                Onnx.NodeProto item = res2.Graph.Node[i];
                foreach (var item2 in item.Output)
                {
                    outs.Add(item2, nodes[i]);
                }
            }

            Dictionary<string, TensorProto> inits = new Dictionary<string, TensorProto>();
            foreach (var iitem in res2.Graph.Initializer)
            {
                inits.Add(iitem.Name, iitem);
            }
            for (int i = 0; i < res2.Graph.Node.Count; i++)
            {
                Onnx.NodeProto item = res2.Graph.Node[i];

                string ss = "";
                foreach (var aitem in item.Attribute)
                {
                    var atr1 = new AttributeInfo() { Name = aitem.Name, Tag = aitem, Owner = nodes[i] };
                    nodes[i].Attributes.Add(atr1);
                    List<int[]> dd = new List<int[]>();
                    switch (aitem.Type)
                    {
                        case Onnx.AttributeProto.Types.AttributeType.Ints:
                            atr1.Type = AttributeInfoDataType.Ints;
                            atr1.Ints = aitem.Ints.ToList();
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Floats:
                            atr1.Type = AttributeInfoDataType.Floats;
                            atr1.Floats = aitem.Floats.ToList();
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Float:
                            atr1.Type = AttributeInfoDataType.Float32;
                            atr1.FloatData = aitem.F;
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Int:
                            atr1.Type = AttributeInfoDataType.Int;
                            atr1.IntData = aitem.I;
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.String:
                            atr1.Type = AttributeInfoDataType.String;
                            atr1.StringData = aitem.S.ToStringUtf8();
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Strings:
                            atr1.Type = AttributeInfoDataType.Strings;
                            atr1.Strings = aitem.Strings.Select(z => z.ToStringUtf8()).ToList();
                            break;
                        case AttributeProto.Types.AttributeType.Tensor:
                            var dims = aitem.T.Dims;
                            var bar = aitem.T.RawData.ToByteArray();
                            List<float> fff = new List<float>();
                            for (int j = 0; j < bar.Length; j += 4)
                            {
                                /*var bar1 = bar.Skip(j).Take(4).ToArray();
                                bar1 = bar1.Reverse().ToArray();
                                fff.Add(BitConverter.ToSingle(bar1, 0));*/
                                fff.Add(BitConverter.ToSingle(bar, j));
                            }
                            atr1.Floats = fff.ToList();
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                }

                foreach (var iitem in item.Input)
                {
                    if (inits.ContainsKey(iitem))
                    {
                        var d = inits[iitem];
                        var id = new InputData() { Name = iitem, Parent = nodes[i] };
                        nodes[i].Data.Add(id);
                        var initer = inits[iitem];

                        if (initer.HasRawData)
                        {
                            var bts = initer.RawData.ToByteArray();
                            var b64 = initer.RawData.ToBase64();

                            TensorProto p = new TensorProto();
                            p.RawData = Google.Protobuf.ByteString.FromBase64(b64);

                            List<float> ret = new List<float>();
                            for (int j = 0; j < bts.Length; j += 4)
                            {
                                ret.Add(BitConverter.ToSingle(bts, j));
                            }
                            id.Weights = ret.ToArray();
                        }
                        else if (initer.Int64Data != null && initer.Int64Data.Count > 0)
                        {
                            var arr = initer.Int64Data.ToArray();
                            id.LWeights = arr.ToArray();
                        }
                        else if (initer.FloatData != null && initer.FloatData.Count > 0)
                        {
                            id.Weights = initer.FloatData.ToArray();
                        }
                        else
                        {

                        }
                        id.Dims = initer.Dims.ToArray();
                        continue;
                    }
                    else
                    if (!outs.ContainsKey(iitem))
                    {
                        var id = new InputData() { Name = iitem, Parent = nodes[i] };
                        nodes[i].Data.Add(id);
                        if (inits.ContainsKey(iitem))
                        {
                            var initer = inits[iitem];

                            var bts = initer.RawData.ToByteArray();
                            var b64 = initer.RawData.ToBase64();

                            TensorProto p = new TensorProto();
                            p.RawData = Google.Protobuf.ByteString.FromBase64(b64);

                            List<float> ret = new List<float>();
                            for (int j = 0; j < bts.Length; j += 4)
                            {
                                ret.Add(BitConverter.ToSingle(bts, j));
                            }
                            id.Weights = ret.ToArray();
                            id.Dims = initer.Dims.ToArray();
                            continue;
                        }
                    }
                    //nodes[i].Input = iitem;
                    if (outs.ContainsKey(iitem))
                    {
                        nodes[i].Parents.Add(outs[iitem]);
                        if (nodes[i].Parents.Count > 1)
                        {

                        }
                        //nodes[i].Parent = outs[iitem];
                        outs[iitem].Childs.Add(nodes[i]);
                    }
                }
                if (item.Input.Any())
                {
                    ss = item.Input[0];

                }
                else
                {

                }

                if (!item.Output.Any())
                {

                }
            }


            foreach (var item in res2.Graph.Output)
            {
                var gn = new GraphNode() { Name = item.Name };
                gn.Tag = item;
                nodes.Add(gn);
                var pp = outs[gn.Name];
                gn.Parents.Add(pp);
                pp.Childs.Add(gn);
                //  outs.Add(gn.Name, gn);
            }

            var cnt2 = res2.Graph.Output[0].Name;
            nodes.InsertRange(0, res2.Graph.Input.Select(z => outs[z.Name]));
            var ret2 = new OnnxGraphModel() { Name = Path.GetFileName(path), Path = path };
            ret2.Provider = this;
            ret2.ProtoModel = res2;
            ret2.Nodes = nodes.ToArray();
            return ret2;
        }

        public override void SaveModel(GraphModel model, string path)
        {
            using (var output = File.Create(path))
            {
                using (Google.Protobuf.CodedOutputStream ff = new Google.Protobuf.CodedOutputStream(output))
                {
                    (model as OnnxGraphModel).ProtoModel.WriteTo(ff);
                }
            }
            return;
        }
        public override void UpdateIntAttributeValue(GraphModel model, GraphNode parentNode, string name, int val)
        {
            var ff = (model as OnnxGraphModel).ProtoModel.Graph.Node.First(z => z.Name == parentNode.Name);
            var attr = ff.Attribute.First(z => z.Name == name);
            attr.I = val;
        }

        public override void UpdateFloatTensor(GraphModel model, GraphNode parentNode, string name, float[] data, long[] dims)
        {

            Dictionary<string, TensorProto> inits = new Dictionary<string, TensorProto>();
            var gm = model as OnnxGraphModel;
            foreach (var iitem in gm.ProtoModel.Graph.Initializer)
            {
                inits.Add(iitem.Name, iitem);
            }


            List<float> ret = new List<float>();

            var aa = inits[inits.Keys.First(z => z.Contains(name))];
            var bts = aa.RawData.ToByteArray();
            for (int j = 0; j < bts.Length; j += 4)
            {
                ret.Add(BitConverter.ToSingle(bts, j));
            }
            MemoryStream ms = new MemoryStream();

            //var res = ParseTensorFromString(Clipboard.GetText());

            foreach (var item in data)
            {
                var ar = BitConverter.GetBytes(item);
                ms.Write(ar, 0, ar.Length);
            }
            ms.Seek(0, SeekOrigin.Begin);
            aa.RawData = Google.Protobuf.ByteString.FromStream(ms);

            var bts2 = aa.RawData.ToByteArray();
            List<float> ret2 = new List<float>();

            for (int j = 0; j < bts2.Length; j += 4)
            {
                ret2.Add(BitConverter.ToSingle(bts2, j));
            }
        }
    }
}

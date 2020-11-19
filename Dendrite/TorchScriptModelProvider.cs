using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Dendrite
{
    public class TorchScriptModelProvider : ModelProvider
    {
        public override bool IsSuitableFile(string path)
        {
            if (!(path.EndsWith(".pt") || path.EndsWith(".pth"))) return false;
            //check zip signature here

            try
            {
                bool modelExist = false;
                bool inferExist = false;
                using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.Name == "model.json")
                        {
                            modelExist = true;
                        }
                        if (entry.Name == "infer.py")
                        {
                            inferExist = true;
                        }
                    }
                return modelExist && inferExist;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        public override GraphModel LoadFromFile(string path)
        {
            Torch.ModelDef res2 = null;
            List<TensorInfo> tensors = new List<TensorInfo>();
            string topology = string.Empty;
            using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name == "model.json")
                    {
                        using (var stream = entry.Open())
                        {
                            using (var srd = new StreamReader(stream))
                            {
                                var str = srd.ReadToEnd();
                                res2 = Torch.ModelDef.Parser.ParseJson(str);
                            }
                        }
                    }
                    if (entry.Name == "infer.py")
                    {
                        using (var stream = entry.Open())
                        {
                            using (var srd = new StreamReader(stream))
                            {
                                topology = srd.ReadToEnd();
                            }
                        }
                    }
                    if (entry.FullName.Contains("tensors"))
                    {
                        using (var stream = entry.Open())
                        {
                            var bb = stream.ReadFully();
                            tensors.Add(new TensorInfo() { RawData = bb });
                        }
                    }
                }

            for (int i = 0; i < res2.Tensors.Count; i++)
            {
                Torch.TensorDef item = (Torch.TensorDef)res2.Tensors[i];
                tensors[i].Dims = item.Dims.ToArray();
                switch (item.DataType)
                {
                    case Caffe2.TensorProto.Types.DataType.Float:
                        {
                            List<float> ret = new List<float>();
                            for (int j = 0; j < tensors[i].RawData.Length; j += 4)
                            {
                                ret.Add(BitConverter.ToSingle(tensors[i].RawData, j));
                            }
                            tensors[i].Data = ret.ToArray();
                        }
                        break;
                    case Caffe2.TensorProto.Types.DataType.Int64:
                        {
                            List<Int64> ret = new List<Int64>();
                            for (int j = 0; j < tensors[i].RawData.Length; j += 8)
                            {
                                ret.Add(BitConverter.ToInt64(tensors[i].RawData, j));
                            }
                            tensors[i].Ints64 = ret.ToArray();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                        break;

                }
            }
            GraphNode gr = new GraphNode();
            List<GraphNode> gn = new List<GraphNode>();
            RecTorchBuilder(gn, res2.MainModule, tensors, string.Empty);
            TorchParseTopology(gn, topology, tensors);


            TorchScriptGraphModel rett = new TorchScriptGraphModel() { Name = Path.GetFileName(path), Path = path }; 
            rett.Provider = this;
            rett.ProtoModel = res2;
            rett.Nodes = gn.ToArray();
            return rett;

        }

        public class TorchScriptGraphModel : GraphModel
        {
            public Torch.ModelDef ProtoModel;
        }
        public class TensorInfo
        {
            public string Name;
            public byte[] RawData;
            public float[] Data;
            public Int64[] Ints64;
            public long[] Dims;
        }

        void RecTorchBuilder(List<GraphNode> gr, Torch.ModuleDef module, List<TensorInfo> tensors, string prefix)
        {
            var ngr = new GraphNode();
            if (module.Submodules.Count == 0)
            {
                gr.Add(ngr);
            }

            if (string.IsNullOrEmpty(prefix))
            {
                //ngr.Name = module.Name;
                ngr.Name = "self";
            }
            else
            {
                ngr.Name = prefix + "." + module.Name;
            }
            foreach (var item in module.Parameters)
            {
                var nm = item.Name;
                var tid = (int)item.TensorId;

                ngr.Data.Add(new InputData() { Name = nm, Dims = tensors[tid].Dims, Weights = tensors[tid].Data });

            }
            foreach (var item in module.Submodules)
            {
                RecTorchBuilder(gr, item, tensors, ngr.Name);
            }
        }

        string[] Tokenize(string input)
        {
            List<string> ret = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool insideString = false;
            List<Predicate<char>[]> pools = new List<Predicate<char>[]>();
            pools.Add(new Predicate<char>[] { char.IsLetterOrDigit, (x) => x == '_' || x == '.' });
            pools.Add(new Predicate<char>[] { char.IsWhiteSpace });
            for (int i = 0; i < input.Length; i++)
            {
                if (sb.Length == 0)
                {
                    sb.Append(input[i]);
                    continue;
                }
                if (input[i] == '\"')
                {
                    if (insideString)
                    {
                        insideString = false;

                        sb.Append(input[i]);
                        ret.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    insideString = true;
                }
                if (insideString)
                {
                    sb.Append(input[i]);
                    continue;
                }

                if (pools.Any(z => new char[] { sb[sb.Length - 1], input[i] }.All(t => z.Any(y => y(t)))))
                {
                    sb.Append(input[i]);
                }
                else
                {
                    ret.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(input[i]);
                }
            }
            if (sb.Length > 0)
            {
                ret.Add(sb.ToString());
            }
            ret = ret.Where(z => !z.All(char.IsWhiteSpace)).ToList();
            return ret.ToArray();
        }
        private void TorchParseTopology(List<GraphNode> gn, string topology, List<TensorInfo> tensors)
        {
            //tokenize
            var lines = topology.Split(new char[] { '\r', '\n' }).ToArray();
            List<string> inputs = new List<string>();

            foreach (var item in lines)
            {
                if (!item.Contains("=")) continue;
                var arr = item.Split(new char[] { '=' }).ToArray();
                var nm = arr[0].Trim();
                if (!inputs.Contains(nm)) inputs.Add(nm);
                if (arr[1].Contains("convolution"))
                {
                    var spl1 = arr[1].Split(new char[] { ',', ' ', '(', ')' }).ToArray();

                }
            }

            foreach (var item in lines)
            {
                var res = Tokenize(item);
                if (!item.Contains("=")) continue;
                var arr = item.Split(new char[] { '=' }).ToArray();
                var nm = arr[0].Trim();
                var aa1 = arr[1];

                var spl1 = aa1.Split(new char[] { ',', ' ', '(', ')', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                //todo: build AST here
                List<string> tokens = new List<string>();
                for (int i = 0; i < res.Length; i++)
                {
                    if (res[i] == "getattr")
                    {

                        List<string> left = new List<string>();
                        List<string> right = new List<string>();
                        bool isR = false;
                        while (res[i] != ")")
                        {
                            if (res[i] == ",") { i++; isR = true; continue; }
                            if (!isR)
                                left.Add(res[i]);
                            else
                                right.Add(res[i]);
                            i++;
                        }
                        for (int j = 0; j < right.Count; j++)
                        {
                            right[j] = right[j].Trim(new char[] { '\"' });
                        }
                        var result = $"{string.Join("", left.ToArray())}.{string.Join("", right.ToArray())}";

                        tokens.AddRange(Tokenize(result));

                        continue;
                    }
                    tokens.Add(res[i]);
                }

                //collapse points
                List<string> tokens2 = new List<string>();
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].StartsWith("."))
                    {
                        tokens2[tokens2.Count - 1] += tokens[i];
                        continue;
                    }
                    tokens2.Add(tokens[i]);
                }

                tokens = tokens2;
                var inp1 = inputs.Where(z => spl1.Contains(z)).ToArray();

                var ww2 = tensors.Where(z => spl1.Any(u => u == z.Name));
                var ww1 = gn.Where(z => tokens.Any(u => u.StartsWith(z.Name))).ToArray();
                if (arr[1].Contains("convolution") && !ww1.Any())
                {
                }
                if (ww1.Any())
                {
                    if (arr[1].Contains("convolution"))
                    {
                        ww1[0].LayerType = LayerType.Conv;
                    }
                    if (arr[1].Contains("batch_norm"))
                    {
                        ww1[0].LayerType = LayerType.Batch;
                    }
                    if (arr[1].Contains("prelu"))
                    {
                        ww1[0].LayerType = LayerType.Relu;
                    }
                    if (arr[1].Contains("cat"))
                    {
                        ww1[0].LayerType = LayerType.Concat;
                    }
                    if (arr[1].Contains("add"))
                    {
                        ww1[0].LayerType = LayerType.MathOperation;
                        ww1[0].OpType = "add";
                    }
                }
            }
        }

        public override void SaveModel(GraphModel model, string path)
        {
            throw new NotImplementedException();
        }

        public override void UpdateFloatTensor(GraphModel model, GraphNode node, float[] data, int[] dims)
        {
            throw new NotImplementedException();
        }
    }
}

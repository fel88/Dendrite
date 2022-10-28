using Dendrite.Preprocessors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class InferenceEnvironment
    {
        public string Name;
        public string Path;
        public Nnet Net = new Nnet();
        public PipelineGraph Pipeline = new PipelineGraph();
        public StringBuilder GetConfigXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            Pipeline.StoreXml(sb);
            sb.AppendLine("</root>");
            return sb;
        }

        public void Load(string epath)
        {
            Path = epath;
            Net = new Nnet();
            //Net.Init(epath);
            using (ZipArchive zip = ZipFile.Open(epath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.EndsWith("config.xml"))
                    {
                        using (var stream2 = entry.Open())
                        {
                            using (var reader = new StreamReader(stream2))
                            {
                                var config = reader.ReadToEnd();
                                var fs = new ZipFilesystem(epath);
                                LoadConfig(fs, config);
                            }
                        }
                    }
                    //if (entry.Name.EndsWith(".onnx")) {  }
                }
            }
        }

        public static Node GenerateNodeFromProcessor(IInputPreprocessor pp)
        {
            var node = new Node();
            node.Attach(pp);            

            return node;
        }
        public static Node GenerateNodeFromNet(Nnet net)
        {
            var node = new NetNode() { Name = net.ModelName, Tag = net, ModelPath = net.NetPath };
            foreach (var nitem in net.Nodes.Where(z => z.IsInput))
            {
                var ds = new DataSlot() { Name = nitem.Name };
                node.Inputs.Add(new NodePin(node, ds) { Name = nitem.Name });

                if (nitem.SourceDims.Contains(-1))//check dynamic axes
                {
                    node.Inputs.Add(new NodePin(node, new DataSlot() { Name = nitem.Name + "_dims" }) { Name = nitem.Name + "_dims" });
                }
                if (!net.InputDatas.ContainsKey(nitem.Name)) continue;
                var item = net.InputDatas[nitem.Name];
            }

            foreach (var item in net.Nodes.Where(z => z.IsOutput))
            {
                var ds = new DataSlot() { Name = item.Name };
                node.Outputs.Add(new NodePin(node, ds) { Name = item.Name });
            }

            return node;
        }

        internal void Process()
        {
            var nodes = Pipeline.Toposort();
            foreach (var item in nodes)
            {
                try
                {
                    item.Process();
                }
                catch (Exception ex)
                {
                    item.LastException = ex;
                }
                if (item.LastException != null) break;
            }
            //Net.Run();
        }


        private void LoadConfig(IFilesystem fs, string config)
        {
            var doc = XDocument.Parse(config);

            var pln = doc.Descendants("pipeline").First();
            Pipeline.RestoreXml(fs, pln);

        }
    }
}



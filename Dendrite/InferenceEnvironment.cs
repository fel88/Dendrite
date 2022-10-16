using Dendrite.Preprocessors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Dendrite
{
    public class InferenceEnvironment
    {
        public string Name;
        public string Path;
        public Nnet Net = new Nnet();
        public PipelineGraph Pipeline = new PipelineGraph();

        public void Load(string epath)
        {
            Path = epath;
            using (ZipArchive zip = ZipFile.Open(epath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.EndsWith(".xml"))
                    {
                        using (var stream2 = entry.Open())
                        {
                            using (var reader = new StreamReader(stream2))
                            {
                                var config = reader.ReadToEnd();
                                LoadConfig(config);
                            }
                        }
                    }
                    if (entry.Name.EndsWith(".onnx"))
                    {
                        Net = new Nnet();
                        using (var stream1 = entry.Open())
                        {
                            var model = stream1.ReadFully();
                            Net.Init(epath, model);
                        }
                    }
                }
            }

            InitPipeline();
        }

        private void InitPipeline()
        {
            var netNode = new Node() { Name = "net", Tag = Net };
            Pipeline.Nodes.Add(netNode);

            foreach (var nitem in Net.Nodes.Where(z => z.IsInput))
            {
                netNode.Inputs.Add(new NodePin(netNode) { Name = nitem.Name });
                if (!Net.InputDatas.ContainsKey(nitem.Name)) continue;
                var item = Net.InputDatas[nitem.Name];

             
                List<Node> prepnodes = new List<Node>();
                foreach (var pp in item.Preprocessors)
                {
                    var node = new Node() { Name = pp.Name, Tag = pp };
                    node.Inputs.Add(new NodePin(node) { });
                    node.Outputs.Add(new NodePin(node) { });
                    if(prepnodes.Count>0)
                    {
                        PinLink pl = new PinLink();                        
                        prepnodes.Last().Outputs[0].OutputLinks.Add(pl);
                        pl.Input = prepnodes.Last().Outputs[0];
                        pl.Output = node.Inputs[0];
                        node.Inputs[0].InputLinks.Add(pl);
                    }                    
                    prepnodes.Add(node);

                    Pipeline.Nodes.Add(node);
                }
                if (prepnodes.Any())
                {
                    PinLink pl = new PinLink();
                    pl.Input = prepnodes.Last().Outputs[0];
                    pl.Output = netNode.Inputs.Last();
                    prepnodes.Last().Outputs[0].OutputLinks.Add(pl);
                    netNode.Inputs.Last().InputLinks.Add(pl);
                }
            }
            
            foreach (var item in Net.Nodes.Where(z=>z.IsOutput))
            {
                netNode.Outputs.Add(new NodePin(netNode) { Name = item.Name });
            }
            
        }

        internal void Process()
        {
            Net.run();
        }

        private void LoadConfig(string config)
        {
            var doc = XDocument.Parse(config);
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlNameAttribute)) != null).ToArray();

            foreach (var item in doc.Descendants("inputNode"))
            {
                var key = item.Attribute("key").Value;
                foreach (var pitem in item.Descendants("preprocessors"))
                {
                    foreach (var eitem in pitem.Elements())
                    {
                        var fr = types.FirstOrDefault(z => ((XmlNameAttribute)z.GetCustomAttribute(typeof(XmlNameAttribute))).XmlKey == eitem.Name.LocalName);
                        if (fr != null)
                        {
                            if (!Net.InputDatas.ContainsKey(key))
                                Net.InputDatas.Add(key, new InputInfo());

                            var proc = Activator.CreateInstance(fr) as IInputPreprocessor;
                            Net.InputDatas[key].Preprocessors.Add(proc);
                            proc.ParseXml(eitem);
                        }
                    }
                }
            }
        }
    }
}



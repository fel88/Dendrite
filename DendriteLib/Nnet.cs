using Dendrite.Preprocessors;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Dendrite
{
    public class Nnet
    {
        public string NetPath => _netPath;
        public string ModelName => new FileInfo(NetPath).Name;
        string _netPath;
        public NodeInfo[] Nodes => _nodes.ToArray();

        List<NodeInfo> _nodes = new List<NodeInfo>();

        InferenceSession session;
        public void Init(IFilesystem fs, string path)
        {
            _netPath = path;
            /*if (path.EndsWith(".den"))
                session = new InferenceSession(GetModelBytes());
            else*/
            //session = new InferenceSession(_netPath);
            session = new InferenceSession(fs.ReadAllBytes(path));

            Prepare(session);
        }

        void Prepare(InferenceSession session1)
        {
            _nodes.Clear();
            foreach (var item in session1.OutputMetadata.Keys)
            {
                var dims = session1.OutputMetadata[item].Dimensions;
                _nodes.Add(new NodeInfo() { Name = item, Dims = dims, ElementType = session1.OutputMetadata[item].ElementType });
            }

            foreach (var name in session1.InputMetadata.Keys)
            {
                var dims = session1.InputMetadata[name].Dimensions;
                _nodes.Add(new NodeInfo()
                {
                    Name = name,
                    SourceDims = (int[])dims.Clone(),
                    Dims = dims,
                    IsInput = true,
                    ElementType = session1.InputMetadata[name].ElementType
                });
            }
        }

        public Dictionary<string, InputInfo> InputDatas = new Dictionary<string, InputInfo>();
        public Dictionary<string, object> OutputDatas = new Dictionary<string, object>();

        public bool FetchNextFrame = true;
        public float[] inputData;

        public void SetDims(string inputKey, int[] dims)
        {
            var inputMeta = session.InputMetadata;
            for (int i = 0; i < inputMeta[inputKey].Dimensions.Length; i++)
            {
                inputMeta[inputKey].Dimensions[i] = dims[i];
            }
        }


        public void SetInputArray(string name, float[] data)
        {
            var inputMeta = session.InputMetadata;
            var tensor = new DenseTensor<float>(data, inputMeta[name].Dimensions);
            container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
        }
        public Mat prepareData(List<NamedOnnxValue> container, InferenceSession session1)
        {
            var inputMeta = session1.InputMetadata;

            Mat mat2 = null;
            foreach (var name in inputMeta.Keys)
            {
                var node = Nodes.First(z => z.Name == name);
                var data = InputDatas[name];
                if (data.Data is InternalArray intar)
                {
                    for (int i = 0; i < inputMeta[name].Dimensions.Length; i++)
                    {
                        if (inputMeta[name].Dimensions[i] == -1)
                        {
                            inputMeta[name].Dimensions[i] = intar.Shape[i];
                        }
                    }

                    for (int i = 0; i < node.SourceDims.Length; i++)
                    {
                        if (node.SourceDims[i] == -1)
                        {
                            inputMeta[name].Dimensions[i] = intar.Shape[i];
                        }
                    }

                    inputData = intar.Data.Select(z => (float)z).ToArray();
                    var tensor = new DenseTensor<float>(inputData, inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
                if (data.Data is Mat matOrig)
                {

                    var mat = matOrig.Clone();
                    lastReadedMat = mat.Clone();

                    if (inputMeta[name].Dimensions[2] == -1 && inputMeta[name].Dimensions[3] == -1)
                    {
                        inputMeta[name].Dimensions[2] = mat.Height;
                        inputMeta[name].Dimensions[3] = mat.Width;
                    }
                    if (node.SourceDims[2] == -1 && node.SourceDims[3] == -1)
                    {
                        inputMeta[name].Dimensions[2] = mat.Height;
                        inputMeta[name].Dimensions[3] = mat.Width;
                    }

                    mat2 = mat.Clone();
                    mat.ConvertTo(mat, MatType.CV_32F);
                    object param = mat;
                    /*foreach (var pitem in data.Preprocessors)
                    {
                        param = pitem.Process(param);
                        if (pitem is ZeroImagePreprocessor && param is Mat mt2)
                        {

                            inputMeta[name].Dimensions[3] = mt2.Width;
                            inputMeta[name].Dimensions[2] = mt2.Height;
                            mt2.ConvertTo(mt2, MatType.CV_32F);


                        }
                        if (pitem is AspectResizePreprocessor asp && param is Mat mt)
                        {
                            if (asp.ForceH)
                            {
                                inputMeta[name].Dimensions[3] = mt.Width;
                                inputMeta[name].Dimensions[2] = mt.Height;

                            }
                            if (inputMeta[name].Dimensions[2] != -1 && inputMeta[name].Dimensions[3] == -1)//keep aspect required
                            {
                                inputMeta[name].Dimensions[3] = mt.Width;
                            }
                        }
                    }*/

                    inputData = param as float[];
                    var tensor = new DenseTensor<float>(param as float[], inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
                if (data.Data is VideoCapture cap)
                {
                    Mat mat = new Mat();
                    bool w = false;
                    try
                    {
                        if (FetchNextFrame)
                        {
                            cap.Read(mat);
                            lastReadedMat = mat.Clone();
                        }
                        else
                        {
                            mat = lastReadedMat.Clone();
                        }
                        w = true;
                        if (inputMeta[name].Dimensions[2] == -1)
                        {
                            inputMeta[name].Dimensions[2] = mat.Height;
                            inputMeta[name].Dimensions[3] = mat.Width;
                        }
                        if (node.SourceDims[2] == -1 && node.SourceDims[3] == -1)
                        {
                            inputMeta[name].Dimensions[2] = mat.Height;
                            inputMeta[name].Dimensions[3] = mat.Width;
                        }
                        //  pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                        mat2 = mat.Clone();
                        mat.ConvertTo(mat, MatType.CV_32F);
                        object param = mat;
                        /*foreach (var pitem in data.Preprocessors)
                        {
                            param = pitem.Process(param);
                        }
                        */
                        inputData = param as float[];
                        var tensor = new DenseTensor<float>(param as float[], inputMeta[name].Dimensions);

                        container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                    }
                    catch (Exception ex)
                    {
                        throw new PrepareDataException() { IsVideo = true, SourceMat = w ? lastReadedMat : null };
                    }
                }
                if (data.Data is float[] fl)
                {
                    var tensor = new DenseTensor<float>(fl, inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));

                }
            }
            return mat2;
        }
        List<NamedOnnxValue> container = new List<NamedOnnxValue>();

        public void ResetContainer()
        {
            container = new List<NamedOnnxValue>();
        }
        public void Run(InferenceSession session1 = null)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (session1 == null)
            {
                session1 = session;
            }
            if (session1 == null)
            {
                session1 = new InferenceSession(_netPath);
            }

            //var container = new List<NamedOnnxValue>();

            //var mat2 = prepareData(container, session1);
            OutputDatas.Clear();
            using (var results = session1.Run(container))
            {
                // Get the results
                foreach (var result in results)
                {
                    var tnm = result.ElementType.ToString().ToLower();
                    if (tnm.Contains("float"))
                    {
                        var data = result.AsTensor<float>();
                        InternalArray arr = new InternalArray(data.Dimensions.ToArray());

                        var rets = data.ToArray();
                        arr.Data = rets.Select(z => (double)z).ToArray();
                        OutputDatas.Add(result.Name, arr);
                    }
                    else if (tnm.Contains("int64"))
                    {
                        var data = result.AsTensor<long>();
                        //var dims = data.Dimensions;
                        var rets = data.ToArray();
                        OutputDatas.Add(result.Name, rets);
                    }
                    else if (tnm.Contains("uint8"))
                    {
                        var data = result.AsTensor<byte>();
                        //var dims = data.Dimensions;
                        var rets = data.ToArray();
                        OutputDatas.Add(result.Name, rets);
                    }
                }
            }

            object inp = this;
            List<object> objs = new List<object>();
            objs.Add(inp);
            sw.Stop();

        }


        public Mat lastReadedMat;

        public byte[] GetModelBytes()
        {
            using (ZipArchive zip = ZipFile.Open(NetPath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {

                    if (entry.Name.EndsWith(".onnx"))
                    {

                        using (var stream1 = entry.Open())
                        {
                            var model = stream1.ReadFully();
                            return model;
                        }
                    }
                }
            }
            return null;

        }
        public string GetModelName()
        {
            using (ZipArchive zip = ZipFile.Open(NetPath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {

                    if (entry.Name.EndsWith(".onnx"))
                    {
                        return entry.Name;

                    }
                }
            }
            return null;
        }
    }

    public class PrepareDataException : Exception
    {
        public bool IsVideo;
        public Mat SourceMat;
    }
}

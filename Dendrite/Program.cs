using Dendrite.Dagre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Mdi();
            //test1();
            Application.Run(MainForm);
        }

        static void test1()
        {
            DagreGraph dg = new DagreGraph();
            int ii = 0;

            dg._nodes2.Add("0", new DagreNode() { id = "node-Conv_0", width = 100, height = 67 });
            dg._nodes2.Add("1", new DagreNode() { id = "node-Conv_1", width = 100, height = 67 });
            dg._nodes2.Add("2", new DagreNode() { id = "node-Concat_2", width = 48.171875, height = 23 });
            dg._nodes2.Add("3", new DagreNode() { id = "graph-input", width = 39.59375, height = 23 });
            dg._nodes2.Add("4", new DagreNode() { width = 47.09375, height = 23, _class = "graph_input" }); ;

            /*foreach (var item in Model.Nodes)
            {
                dg._nodes2.Add(ii.ToString(), new DagreNode() { });
                ii++;
            }
            */
           
            dg._edgeObjs.Add(DagreGraph.edgeArgsToId(dg.directed, "0", "2", null), new DagreEdgeIndex() { v = "0", w = "2" });
            dg._edgeObjs.Add(DagreGraph.edgeArgsToId(dg.directed, "1", "2", null), new DagreEdgeIndex() { v = "1", w = "2" });
            dg._edgeObjs.Add(DagreGraph.edgeArgsToId(dg.directed, "3", "0", null), new DagreEdgeIndex() { v = "3", w = "0" });
            dg._edgeObjs.Add(DagreGraph.edgeArgsToId(dg.directed, "3", "1", null), new DagreEdgeIndex() { v = "3", w = "1" });
            dg._edgeObjs.Add(DagreGraph.edgeArgsToId(dg.directed, "2", "4", null), new DagreEdgeIndex() { v = "2", w = "4" });



            dg._edgeLabels.Add($"0{DagreGraph.EDGE_KEY_DELIM}2{DagreGraph.EDGE_KEY_DELIM}{DagreGraph.DEFAULT_EDGE_NAME}", new DagreEdge() { label = "", id = "edge-5", arrowhead = "vee" });
            dg._edgeLabels.Add($"1{DagreGraph.EDGE_KEY_DELIM}2{DagreGraph.EDGE_KEY_DELIM}{DagreGraph.DEFAULT_EDGE_NAME}", new DagreEdge() { label = "", id = "edge-6", arrowhead = "vee" });
            dg._edgeLabels.Add($"2{DagreGraph.EDGE_KEY_DELIM}4{DagreGraph.EDGE_KEY_DELIM}{DagreGraph.DEFAULT_EDGE_NAME}", new DagreEdge() { label = "", id = "bs-5", arrowhead = "vee" });
            dg._edgeLabels.Add($"3{DagreGraph.EDGE_KEY_DELIM}0{DagreGraph.EDGE_KEY_DELIM}{DagreGraph.DEFAULT_EDGE_NAME}", new DagreEdge() { label = "", id = "bs-5", arrowhead = "vee" });
            dg._edgeLabels.Add($"3{DagreGraph.EDGE_KEY_DELIM}1{DagreGraph.EDGE_KEY_DELIM}{DagreGraph.DEFAULT_EDGE_NAME}", new DagreEdge() { label = "", id = "bs-5", arrowhead = "vee" });

            DagreLayout dl = new DagreLayout();
            dl.layout(dg);
        }

        public static Mdi MainForm;
    }
}

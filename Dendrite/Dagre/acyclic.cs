﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dendrite.Dagre
{

    public class acyclic
    {

        public static void undo(DagreGraph g)
        {
            foreach (dynamic e in g.edges())
            {
                var label = g.edge(e);
                if (label.reversed != null)
                {
                    g.removeEdge(e);

                    var forwardName = label.forwardName;
                    label.reversed = null;
                    label.forwardName = null;


                    g.setEdgeRaw(new object[] { e["w"], e["v"], label, forwardName });
                }
            }

        }

        public static Func<string, int> weightFn(DagreGraph g)
        {
            return (Func<string, int>)((e) => { return g.edge(e)["weight"]; });
        }
        public static void run(DagreGraph g)
        {
            string cyclicer = "";
            if (g.graph().ContainsKey("acyclicer"))
            {
                cyclicer = g.graph()["acyclicer"];
            }
            var fas = (cyclicer == "greedy"
   ? greedyFAS(g, weightFn(g))
   : dfsFAS(g));
            foreach (dynamic e in fas)
            {
                var label = g.edge(e);
                g.removeEdge(e);
                label.forwardName = e.name;
                label.reversed = true;

                g.setEdgeRaw(new object[] { e["w"], e["v"], label, util.uniqueId("rev") });

            }


        }
        public static DagreEdgeIndex[] greedyFAS(DagreGraph g, Func<string, int> wf)
        {
            throw new NotImplementedException();
        }
        public static DagreEdgeIndex[] dfsFAS(DagreGraph g)
        {
            HashSet<string> visited = new HashSet<string>();
            List<DagreEdgeIndex> fas = new List<DagreEdgeIndex>();
            HashSet<string> stack = new HashSet<string>();
            Action<string> dfs = null;
            dfs = (v) =>
            {
                if (visited.Contains(v))
                {
                    return;
                }
                visited.Add(v);
                stack.Add(v);
                foreach (dynamic e in g.outEdges(v))
                {
                    if (stack.Contains(e["w"]))
                    {
                        fas.Add(e);
                    }
                    else
                    {
                        dfs(e["w"]);
                    }
                }
                stack.Remove(v);

            };
            foreach (var item in g.nodesRaw())
            {
                dfs(item);
            }
            return fas.ToArray();
        }

    }
}

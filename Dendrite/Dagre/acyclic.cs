using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dendrite.Dagre
{

    public class acyclic
    {

        public static Func<string, int> weightFn(DagreGraph g)
        {
            return (Func<string, int>)((e) => { return g.edge(e).weight; });
        }
        public static void run(DagreGraph g)
        {
            var fas = (g.graph().acyclicer == "greedy"
   ? greedyFAS(g, weightFn(g))
   : dfsFAS(g));
            foreach (var e in fas)
            {
                var label = g.edge(e);
                g.removeEdge(e);
                label.forwardName = e.name;
                label.reversed = true;

                //g.setEdge(e.w, e.v, label, _.uniqueId("rev"));
                g.setEdge(e.w, e.v, label, Guid.NewGuid());

            }


        }
        public static DagreEdgeIndex[] greedyFAS(DagreGraph g, Func<string, int> wf)
        {
            throw new NotImplementedException();
        }
        public static DagreEdgeIndex[] dfsFAS(DagreGraph g)
        {
            List<string> visited = new List<string>();
            List<DagreEdgeIndex> fas = new List<DagreEdgeIndex>();
            List<string> stack = new List<string>();
            Action<string> dfs = null;
            dfs = (v) =>
            {
                if (visited.Contains(v))
                {
                    return;
                }
                visited.Add(v);
                stack.Add(v);
                foreach (var e in g.outEdges(v))
                {
                    if (stack.Contains(e.w))
                    {
                        fas.Add(e);
                    }
                    else
                    {
                        dfs(e.w);
                    }
                }
                stack.Remove(v);

            };
            foreach (var item in g.nodes)
            {
                dfs(item.key);
            }
            return fas.ToArray();
        }

    }
}

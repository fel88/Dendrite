using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dendrite.Layouts
{
    public class SimpleGraphLayout : GraphLayout
    {
        /*public void dfs(GraphNode node, List<GraphNode> outp, List<GraphNode> visited)
        {
            visited.Add(node);
            foreach (var item in node.Childs)
            {
                if (visited.Contains(item)) continue;
                dfs(item, outp, visited);
            }
            outp.Add(node);
        }*/
        public void dfs(GraphModel model, GraphNode node, List<GraphNode> used, List<GraphNode> answer)
        {
            used.Add(node);
            foreach (var to in node.Childs)
            {
                if (!used.Contains(to))
                    dfs(model, to, used, answer);
            }

            answer.Add(node);
        }
        GraphNode[] topoSort(GraphModel model)
        {
            List<GraphNode> used = new List<GraphNode>();

            List<GraphNode> answer = new List<GraphNode>();
            for (int i = 0; i < model.Nodes.Length; ++i)
                if (!used.Contains(model.Nodes[i]))
                    dfs(model, model.Nodes[i], used, answer);
            answer.Reverse();

            return answer.ToArray();
        }
        public override void Layout(GraphModel model)
        {

            var topo = topoSort(model);
          //  List<GraphNode> topo = new List<GraphNode>();
            List<GraphNode> visited = new List<GraphNode>();

            foreach (var item in model.Nodes)
            {
                if (!visited.Contains(item))
                {
                    //dfs(item, topo, visited);
                }
            }

            //topo.Reverse();
            


            int yy = 100;
            int line = 0;
            /*   var inps = Graph.Where(z => z.Parent == null).ToArray();
               GraphNode crnt = inps[0];
               Queue<GraphNode> q = new Queue<GraphNode>();
               q.Enqueue(crnt);
               List<GraphNode> visited = new List<GraphNode>();
               while (q.Any())
               {
                   var deq = q.Dequeue();
                   if (visited.Contains(deq)) continue;
                   if (deq.DrawTag != null) continue;
                   if (deq.Parent == null)
                   {
                       deq.DrawTag = new GraphNodeDrawInfo() { Text = deq.Name, Rect = new Rectangle(100, 100, 300, 100) };
                   }
                   else
                   {
                       var dtag = deq.Parent.DrawTag as GraphNodeDrawInfo;
                       var ind = deq.Parent.Childs.IndexOf(deq);
                       var rect = dtag.Rect;
                       deq.DrawTag = new GraphNodeDrawInfo() { Text = deq.Name, Rect = new Rectangle(rect.X + ind * 350, rect.Bottom+50, rect.Width, rect.Height) };
                   }

                   foreach (var item in deq.Childs)
                   {                    
                       q.Enqueue(item);
                   }
               }*/
            /*while (true)
            {
                crnt.DrawTag = new GraphNodeDrawInfo() { Text = crnt.Name, Rect = new Rectangle(100, yy, 300, 100) };
                if (crnt.Childs.Count == 0) break;

                crnt = crnt.Childs[0];

                yy += 150;
            }*/
            /*return;
             * */
            foreach (var item in topo)
            {
                int shift = 0;
                int xx = 100;
                if (item.Parent != null)
                {
                    shift += item.Parent.Childs.IndexOf(item) * 350;
                    if (item.Parent.Childs.IndexOf(item) != 0)
                    {
                        line++;
                        shift = line * 350;
                    }
                    var info = (item.Parent.DrawTag as GraphNodeDrawInfo);
                    xx = info.Rect.Left + shift;
                    yy = info.Rect.Bottom + 50;
                }
                item.DrawTag = new GraphNodeDrawInfo() { Text = item.Name, Rect = new Rectangle(xx, yy, 300, 100) };
                yy += 150;
            }
        }

    }
}

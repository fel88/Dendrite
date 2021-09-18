using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace Dendrite.Layouts
{
    public class TableGraphLayout : GraphLayout
    {
        public void dfs(GraphNode node, List<GraphNode> outp, List<GraphNode> visited)
        {
            visited.Add(node);
            foreach (var item in node.Childs)
            {
                if (visited.Contains(item)) continue;
                dfs(item, outp, visited);
            }
            outp.Add(node);
        }
        public override void Layout(GraphModel model)
        {

            var www = model.Nodes.OrderBy(z => z.Parents.Count).Reverse().ToArray();
            www = model.Nodes.ToArray();
            List<GraphNode> topo = new List<GraphNode>();
            List<GraphNode> visited = new List<GraphNode>();

            foreach (var item in www)
            {
                if (visited.Contains(item)) continue;
                dfs(item, topo, visited);
            }
            topo.Reverse();

            int cntr = 0;
            var s1 = (int)Math.Ceiling(Math.Sqrt(topo.Count));
            for (int i = 0; i < s1; i++)
            {
                for (int j = 0; j < s1; j++)
                {
                    if (cntr >= topo.Count) break;
                    //topo[cntr].DrawTag = new GraphNodeDrawInfo() { Text = topo[cntr].Name, Rect = new Rectangle(i * 350, j * 150, 300, 100) };
                    
                    var tag = (topo[cntr].DrawTag as GraphNodeDrawInfo);
                    tag.X = i * 350;
                    tag.Y = j * 150;
                    tag.Width = 300;
                    tag.Height = 100;
                    cntr++;
                }
                if (cntr >= topo.Count) break;
            }

        }
    }
}

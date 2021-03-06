﻿using System;
using System.Collections.Generic;

namespace Dendrite.Dagre
{
    public class addBorderSegments
    {

        public static void   addBorderNode(DagreGraph g, string prop, string  prefix, object sg, DagreNode sgNode, int rank)
        {
            /*var label = { width: 0, height: 0, rank: rank, borderType: prop };
        var prev = sgNode[prop][rank - 1];
        var curr = util.addDummyNode(g, "border", label, prefix);
        sgNode[prop][rank] = curr;
  g.setParent(curr, sg);
  if (prev) {
    g.setEdge(prev, curr, { weight: 1 });
  }*/
}


        public static void _addBorderSegments(DagreGraph g)
        {
            Action<string> dfs = null;
            dfs = (v) =>
           {
               var children = g.children(v);
               var node = g.node(v);
               if (children != null && children.Length > 0)
               {
                   foreach (var item in children)
                   {
                       dfs(item);
                   }
               }

               if (node.minRank != null)
               {
                   node.borderLeft = new List<object>();
                   node.borderRight = new List<object>();
                   for (int rank = node.minRank.Value, maxRank = node.maxRank.Value + 1;
                     rank < maxRank;
                     ++rank)
                   {
                       addBorderNode(g, "borderLeft", "_bl", v, node, rank);
                       addBorderNode(g, "borderRight", "_br", v, node, rank);
                   }
               }
           };
            
            foreach (var item in g.children())
            {
                dfs(item);
            }
        }
    }
}

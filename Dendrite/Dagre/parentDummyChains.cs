using System;
using System.Collections.Generic;

namespace Dendrite.Dagre
{
    public static class parentDummyChains
    {
        public static void _parentDummyChains(DagreGraph g)
        {
            var postorderNums = postorder(g);


        }

        // Find a path from v to w through the lowest common ancestor (LCA). Return the
        // full path and the LCA.
        public static void findPath(DagreGraph g, Dictionary<string, Dto1> postorderNums, string v, string w)
        {
        }

        public static Dictionary<string, Dto1> postorder(DagreGraph g)
        {
            Dictionary<string, Dto1> result = new Dictionary<string, Dto1>();

            var lim = 0;
            Action<string> dfs = null;
            dfs = (v) =>
            {
                var low = lim;
                foreach (var item in g.children(v))
                {
                    dfs(item);
                }
                result.Add(v, new Dto1 { low = low, lim = lim++ });
            };
            foreach (var item in g.children())
            {
                dfs(item);
            }


            return result;
        }

        public class Dto1
        {
            public int low;
            public int lim;
        }
    }

}

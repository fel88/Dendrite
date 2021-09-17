using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class bk
    {
        /*
 * This module provides coordinate assignment based on Brandes and Köpf, "Fast
 * and Simple Horizontal Coordinate Assignment."
 */

        public static Tuple<double, string>[] positionX(DagreGraph g)
        {
            var layering = util.buildLayerMatrix(g);

            var conflicts = findType1Conflicts(g, layering).Union(
                findType2Conflicts(g, layering)
                );

            /*
             var layering = util.buildLayerMatrix(g);
  var conflicts = _.merge(
    findType1Conflicts(g, layering),
    findType2Conflicts(g, layering));

  var xss = {};
  var adjustedLayering;
  _.forEach(["u", "d"], function(vert) {
    adjustedLayering = vert === "u" ? layering : _.values(layering).reverse();
    _.forEach(["l", "r"], function(horiz) {
      if (horiz === "r") {
        adjustedLayering = _.map(adjustedLayering, function(inner) {
          return _.values(inner).reverse();
        });
      }

      var neighborFn = (vert === "u" ? g.predecessors : g.successors).bind(g);
      var align = verticalAlignment(g, adjustedLayering, conflicts, neighborFn);
      var xs = horizontalCompaction(g, adjustedLayering,
        align.root, align.align, horiz === "r");
      if (horiz === "r") {
        xs = _.mapValues(xs, function(x) { return -x; });
      }
      xss[vert + horiz] = xs;
    });
  });

  var smallestWidth = findSmallestWidthAlignment(g, xss);
  alignCoordinates(xss, smallestWidth);
  return balance(xss, g.graph().align);*/
            return null;
        }




        public static void addConflict(Dictionary<string, HashSet<string>> conflicts, string v, string w)
        {
            if (int.Parse(v) > int.Parse(w))
            {
                var tmp = v;
                v = w;
                w = tmp;
            }
            if (!conflicts.ContainsKey(v))
            {
                conflicts.Add(v, new HashSet<string>());
            }
            conflicts[v].Add(w);


        }
        public static string findOtherInnerSegmentNode(DagreGraph g, string v)
        {
            if (g.nodeRaw(v).dummy != null)
            {
                return g.predecessors(v).FirstOrDefault(u => g.nodeRaw(u).dummy != null);

            }
            return null;
        }
        /*
         * Marks all edges in the graph with a type-1 conflict with the "type1Conflict"
         * property. A type-1 conflict is one where a non-inner segment crosses an
         * inner segment. An inner segment is an edge with both incident nodes marked
         * with the "dummy" property.
         *
         * This algorithm scans layer by layer, starting with the second, for type-1
         * conflicts between the current layer and the previous layer. For each layer
         * it scans the nodes from left to right until it reaches one that is incident
         * on an inner segment. It then scans predecessors to determine if they have
         * edges that cross that inner segment. At the end a final scan is done for all
         * nodes on the current rank to see if they cross the last visited inner
         * segment.
         *
         * This algorithm (safely) assumes that a dummy node will only be incident on a
         * single node in the layers being scanned.
         */
        public static object[] findType1Conflicts(DagreGraph g, dynamic layering)
        {
            Dictionary<string, HashSet<string>> conflicts = new Dictionary<string, HashSet<string>>();

            Func<dynamic, dynamic, dynamic> visitLayer = (prevLayer, layer) =>
               {
                   // last visited node in the previous layer that is incident on an inner
                   // segment.
                   int?

                 k0 = 0;
                   // Tracks the last node in this layer scanned for crossings with a type-1
                   // segment.
                   var scanPos = 0;
                   var prevLayerLength = prevLayer.Length;
                   var lastNode = layer.Last();

                   foreach (var v in layer)
                   {
                       var w = findOtherInnerSegmentNode(g, v);
                       var k1 = w != null ? g.node(w).order : prevLayerLength;

                       int i = 0;
                       if (w != null || v == lastNode)
                       {
                           foreach (var scanNode in layer.Skip(scanPos).Take(i + 1))
                           {
                               foreach (var u in g.predecessors(scanNode))
                               {
                                   var uLabel = g.node(u);
                                   var uPos = uLabel["order"];
                                   if ((uPos < k0 || k1 < uPos) &&
                                   !(uLabel.ContainsKey("dummy") && g.node(scanNode).ContainsKey("dummy") != null))
                                   {
                                       addConflict(conflicts, u, scanNode);
                                   }
                               }
                               scanPos = i + 1;
                               k0 = k1;
                           }
                       }
                   }


                   return layer;
               };

            string[] prev = null;
            foreach (var item in layering)
            {
                visitLayer(prev, item);
                prev = item;

            }
            return null;
            //return conflicts.ToArray();
        }


        public static object[] findType2Conflicts(DagreGraph g, dynamic layering)
        {
            //var conflicts = { };
            /*
                        function scan(south, southPos, southEnd, prevNorthBorder, nextNorthBorder)
                        {
                            var v;
                            _.forEach(_.range(southPos, southEnd), function(i) {
                                v = south[i];
                                if (g.node(v).dummy)
                                {
                                    _.forEach(g.predecessors(v), function(u) {
                                        var uNode = g.node(u);
                                        if (uNode.dummy &&
                                            (uNode.order < prevNorthBorder || uNode.order > nextNorthBorder))
                                        {
                                            addConflict(conflicts, u, v);
                                        }
                                    });
                    }
                });
              }


            function visitLayer(north, south)
            {
                var prevNorthPos = -1,
                  nextNorthPos,
                  southPos = 0;

                _.forEach(south, function(v, southLookahead) {
                    if (g.node(v).dummy === "border")
                    {
                        var predecessors = g.predecessors(v);
                        if (predecessors.length)
                        {
                            nextNorthPos = g.node(predecessors[0]).order;
                            scan(south, southPos, southLookahead, prevNorthPos, nextNorthPos);
                            southPos = southLookahead;
                            prevNorthPos = nextNorthPos;
                        }
                    }
                    scan(south, southPos, south.length, nextNorthPos, north.length);
                });

            return south;
        }

        _.reduce(layering, visitLayer);
  return conflicts;*/
            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class sortSubGraphModule
    {
        public static barycenterDto[] barycenter(DagreGraph g, string[] movable)
        {
            return movable.Select(v =>
            {
                var inV = g.inEdges(v);
                //if (!inV.length)
                {
                    return new barycenterDto { v = v };
                    //   return { v: v };
                }
                //else
                {
                    /*var result = _.reduce(inV, function(acc, e) {
                        var edge = g.edge(e),
                          nodeU = g.node(e.v);
                        return {
                        sum: acc.sum + (edge.weight * nodeU.order),
              weight: acc.weight + edge.weight
                            };
                    }, { sum: 0, weight: 0 });

                return {
                v: v,
            barycenter: result.sum / result.weight,
            weight: result.weight
                        };*/

                }
            }).ToArray();

        }
        public static object sortSubraph(DagreGraph g, string v, DagreGraph cg, bool biasRight)
        {
            var movable = g.children(v);
            var node = g.node(v);
            var bl = node != null ? node.borderLeft : null;
            var br = node != null ? node.borderRight : null;
            var subgraphs = new List<object>();

            if (bl != null)
            {
                /*movable.Where(z => z != bl && z != br);
                movable = _.filter(movable, function(w) {
                    return w !== bl && w !== br;
                });*/
            }

            var barycenters = barycenter(g, movable);







            
            var entries = resolveConflictsModule.resolveConflicts(barycenters, cg);
            //expandSubgraphs(entries, subgraphs);

            //var result = sort(entries, biasRight);


            return new object();
        }


   


        /*
         * 
function sortSubgraph(g, v, cg, biasRight) {
  var movable = g.children(v);
  var node = g.node(v);
  var bl = node ? node.borderLeft : undefined;
  var br = node ? node.borderRight: undefined;
  var subgraphs = {};

  if (bl) {
    movable = _.filter(movable, function(w) {
      return w !== bl && w !== br;
    });
  }

  var barycenters = barycenter(g, movable);
  _.forEach(barycenters, function(entry) {
    if (g.children(entry.v).length) {
      var subgraphResult = sortSubgraph(g, entry.v, cg, biasRight);
      subgraphs[entry.v] = subgraphResult;
      if (_.has(subgraphResult, "barycenter")) {
        mergeBarycenters(entry, subgraphResult);
      }
    }
  });

  var entries = resolveConflicts(barycenters, cg);
  expandSubgraphs(entries, subgraphs);

  var result = sort(entries, biasRight);

  if (bl) {
    result.vs = _.flatten([bl, result.vs, br], true);
    if (g.predecessors(bl).length) {
      var blPred = g.node(g.predecessors(bl)[0]),
        brPred = g.node(g.predecessors(br)[0]);
      if (!_.has(result, "barycenter")) {
        result.barycenter = 0;
        result.weight = 0;
      }
      result.barycenter = (result.barycenter * result.weight +
                           blPred.order + brPred.order) / (result.weight + 2);
      result.weight += 2;
    }
  }

  return result;
}

function expandSubgraphs(entries, subgraphs) {
  _.forEach(entries, function(entry) {
    entry.vs = _.flatten(entry.vs.map(function(v) {
      if (subgraphs[v]) {
        return subgraphs[v].vs;
      }
      return v;
    }), true);
  });
}

function mergeBarycenters(target, other) {
  if (!_.isUndefined(target.barycenter)) {
    target.barycenter = (target.barycenter * target.weight +
                         other.barycenter * other.weight) /
                        (target.weight + other.weight);
    target.weight += other.weight;
  } else {
    target.barycenter = other.barycenter;
    target.weight = other.weight;
  }
}

         */
    }
}

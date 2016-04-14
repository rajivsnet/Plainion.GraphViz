﻿using System.IO;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    // Hint: also for rendering we pass label to trigger dot.exe to create proper size of node bounding box
    public class DotWriter
    {
        private readonly string myPath;

        public DotWriter(string path)
        {
            Contract.RequiresNotNull(path, "path");

            myPath = path;
        }

        internal int? FastRenderingNodeCountLimit { get; set; }

        internal bool IgnoreStyle { get; set; }

        // http://www.graphviz.org/Gallery/directed/cluster.html
        // returns written nodes
        public int Write(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            return new WriteAction(this, presentation).Execute();
        }

        private class WriteAction
        {
            private readonly DotWriter myOwner;
            private readonly IGraphPresentation myPresentation;
            private readonly IPropertySetModule<Caption> myCaptions;
            private readonly IPropertySetModule<NodeStyle> myNodeStyles;
            private readonly IPropertySetModule<EdgeStyle> myEdgeStyles;
            private TextWriter myWriter;

            public WriteAction(DotWriter owner, IGraphPresentation presentation)
            {
                myOwner = owner;
                myPresentation = presentation;

                myCaptions = presentation.GetPropertySetFor<Caption>();
                myNodeStyles = presentation.GetPropertySetFor<NodeStyle>();
                myEdgeStyles = presentation.GetPropertySetFor<EdgeStyle>();
            }

            public int Execute()
            {
                var transformationModule = myPresentation.GetModule<ITransformationModule>();

                using (myWriter = new StreamWriter(myOwner.myPath))
                {
                    myWriter.WriteLine("digraph {");

                    myWriter.WriteLine("  ratio=\"compress\"");
                    myWriter.WriteLine("  rankdir=BT");
                    myWriter.WriteLine("  ranksep=\"2.0 equally\"");

                    var visibleNodes = transformationModule.Graph.Nodes
                        .Where(n => myPresentation.Picking.Pick(n))
                        .ToList();

                    if (myOwner.FastRenderingNodeCountLimit.HasValue && visibleNodes.Count > myOwner.FastRenderingNodeCountLimit.Value)
                    {
                        // http://www.graphviz.org/content/attrs#dnslimit
                        myWriter.WriteLine("  nslimit=0.2");
                        myWriter.WriteLine("  nslimit1=0.2");
                        myWriter.WriteLine("  splines=line");
                        myWriter.WriteLine("  mclimit=0.5");
                    }

                    foreach (var cluster in transformationModule.Graph.Clusters)
                    {
                        var visibleClusterNodes = cluster.Nodes
                            .Where(visibleNodes.Contains)
                            .ToList();

                        if (visibleClusterNodes.Count == 0)
                        {
                            continue;
                        }

                        var clusterId = cluster.Id.StartsWith("cluster_") ? cluster.Id : "cluster_" + cluster.Id;
                        myWriter.WriteLine("  subgraph \"" + clusterId + "\" {");

                        myWriter.WriteLine("    label = \"{0}\"", myCaptions.Get(cluster.Id).DisplayText);

                        foreach (var node in visibleClusterNodes)
                        {
                            Write(node, "    ");
                        }

                        myWriter.WriteLine("  }");
                    }

                    foreach (var node in visibleNodes)
                    {
                        Write(node, "  ");
                    }

                    foreach (var edge in transformationModule.Graph.Edges.Where(e => myPresentation.Picking.Pick(e)))
                    {
                        Write(edge, "  ");
                    }

                    myWriter.WriteLine("}");

                    return visibleNodes.Count;
                }
            }

            private void Write(Node node, string indent)
            {
                myWriter.Write(indent);

                var label = myCaptions.Get(node.Id).DisplayText;
                myWriter.Write("\"{0}\" [label=\"{1}\"", node.Id, label);

                if (!myOwner.IgnoreStyle)
                {
                    var fillColor = myNodeStyles.Get(node.Id).FillColor;
                    myWriter.Write(", color={0}", fillColor);
                }

                myWriter.WriteLine("]");
            }

            private void Write(Edge edge, string indent)
            {
                var label = myCaptions.Get(edge.Id);

                // Hint (rendering): always pass label otherwise parser will fail :(
                myWriter.Write(indent);

                myWriter.Write("\"{0}\" -> \"{1}\" [label=\"{2}\"",
                    edge.Source.Id,
                    edge.Target.Id,
                    label.DisplayText != label.OwnerId ? label.DisplayText : ".");

                if (!myOwner.IgnoreStyle)
                {
                    var color = myEdgeStyles.Get(edge.Id).Color;
                    myWriter.Write(", color={0}", color);
                }

                myWriter.WriteLine("]");
            }
        }
    }
}
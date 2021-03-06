﻿using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class RemoveNodesWithoutEdges
    {
        private readonly IGraphPresentation myPresentation;
        private Mode myMode;

        public enum Mode
        {
            /// <summary>
            /// Remove nodes without any edges
            /// </summary>
            All,
            /// <summary>
            /// Remove nodes without incoming edges
            /// </summary>
            Incomings,
            /// <summary>
            /// Remove nodes without outgoing edges
            /// </summary>
            Outgoings
        }

        public RemoveNodesWithoutEdges(IGraphPresentation presentation)
            : this(presentation, Mode.All)
        {
        }

        public RemoveNodesWithoutEdges(IGraphPresentation presentation, Mode mode)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
            myMode = mode;
        }

        public void Execute()
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            Execute(transformationModule.Graph.Nodes);
        }

        private void Execute(IEnumerable<Node> nodes)
        {
            var nodesToHide = nodes
                .Where(n => HideNode(n));

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);
            mask.Label = "Nodes without ";

            if (myMode == Mode.Incomings)
            {
                mask.Label += "incomings";
            }
            else if (myMode == Mode.Outgoings)
            {
                mask.Label += "outgoings";
            }
            else
            {
                mask.Label += "edges";
            }
            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        public void Execute(Cluster cluster)
        {
            Execute(cluster.Nodes);
        }

        private bool HideNode(Node node)
        {
            var noIncomings = !node.In.Any(e => myPresentation.Picking.Pick(e));
            var noOutgoings = !node.Out.Any(e => myPresentation.Picking.Pick(e));

            if (myMode == Mode.All && noIncomings && noOutgoings)
            {
                return true;
            }
            else if (myMode == Mode.Incomings && noIncomings)
            {
                return true;
            }
            else if (myMode == Mode.Outgoings && noOutgoings)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

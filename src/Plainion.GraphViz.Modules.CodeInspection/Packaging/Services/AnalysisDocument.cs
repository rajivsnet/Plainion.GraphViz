﻿using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    // we use private properties here to support Json serialiation
    public class AnalysisDocument
    {
        private HashSet<string> myNodes { get; set; }
        private HashSet<Tuple<string, string>> myEdges { get; set; }
        private Dictionary<string, IEnumerable<string>> myClusters { get; set; }
        private List<Caption> myCaptions { get; set; }
        private List<NodeStyle> myNodeStyles { get; set; }

        public AnalysisDocument()
        {
            myNodes = new HashSet<string>();
            myEdges = new HashSet<Tuple<string, string>>();
            myClusters = new Dictionary<string, IEnumerable<string>>();

            myCaptions = new List<Caption>();
            myNodeStyles = new List<NodeStyle>();
        }

        public IEnumerable<string> Nodes { get { return myNodes; } }

        public IEnumerable<Tuple<string, string>> Edges { get { return myEdges; } }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get { return myClusters; } }

        public IEnumerable<Caption> Captions { get { return myCaptions; } }

        public IEnumerable<NodeStyle> NodeStyles { get { return myNodeStyles; } }

        public void AddNode(string nodeId)
        {
            myNodes.Add(nodeId);
        }

        public void AddEdge(string sourceNodeId, string targetNodeId)
        {
            myEdges.Add(Tuple.Create(sourceNodeId, targetNodeId));
        }

        public void AddToCluster(string clusterId, string nodeId)
        {
            IEnumerable<string> existing;
            if (!myClusters.TryGetValue(clusterId, out existing))
            {
                existing = new HashSet<string>();
                myClusters.Add(clusterId, existing);
            }

            ((HashSet<string>)existing).Add(nodeId);
        }

        public void Add(Caption caption)
        {
            if (!myCaptions.Any(c => c.OwnerId == caption.OwnerId))
            {
                myCaptions.Add(caption);
            }
        }

        public void Add(NodeStyle nodeStyle)
        {
            if (!myNodeStyles.Any(n => n.OwnerId == nodeStyle.OwnerId))
            {
                myNodeStyles.Add(nodeStyle);
            }
        }
    }
}
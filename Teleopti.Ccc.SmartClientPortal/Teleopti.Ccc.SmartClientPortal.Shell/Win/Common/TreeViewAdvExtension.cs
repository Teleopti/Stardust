using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Contains all the extension methods which belongs to TreeViewAdv.
    /// </summary>
    /// <remarks>
    /// Created By: kosalanp
    /// Created Date: 17-04-2008
    /// </remarks>
    public static class TreeViewAdvExtension
    {
        public static IEnumerable<TreeNodeAdv> FindNodesWithTagObject(this TreeViewAdv tree, object value)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            var foundTreeNodes = new List<TreeNodeAdv>();
            int totalNodes = tree.GetNodeCount(true); // All node count including child nodes.
            if (totalNodes == 0) return foundTreeNodes;
            
            foreach (TreeNodeAdv currentNode in tree.Nodes)
            {
                AddNodesWithTagObjectToList(foundTreeNodes, currentNode, value);
            }

            // Nothing found.
            return foundTreeNodes;
        }

        private static void AddNodesWithTagObjectToList(IList<TreeNodeAdv> foundTreeNodes, TreeNodeAdv currentNode, object value)
        {
            if (currentNode != null)
            {
                if (currentNode.TagObject != null &&
                    currentNode.TagObject.Equals(value))
                {
                    foundTreeNodes.Add(currentNode);
                }
                foreach (TreeNodeAdv node in currentNode.Nodes)
                {
                    AddNodesWithTagObjectToList(foundTreeNodes,node,value);
                }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public class ClipboardHandler
    {
        /// <summary>
        /// Contains cloneable items that are copied.
        /// </summary>
        private readonly List<ICloneable> _clips;

        /// <summary>
        /// Gets the clips.
        /// </summary>
        /// <value>The clips.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-11
        /// </remarks>
        public IList<ICloneable> Clips
        {
            get
            {
                return _clips;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ClipboardHandler()
        {
            _clips = new List<ICloneable>();
        }

        /// <summary>
        /// Instance property variable.
        /// </summary>
        private static ClipboardHandler _instance;
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static ClipboardHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ClipboardHandler(); return _instance;
            }
        }

        /// <summary>
        /// Prepares the env'nt for copy.
        /// </summary>
        protected void BeginCopy()
        {
            _clips.Clear();
        }
        /// <summary>
        /// Adds selected tree nodes to clipboard container.
        /// </summary>
        /// <param name="tree">Tree view to copy.</param>
        public void CopySelection(TreeViewAdv tree)
        {
            BeginCopy();

            foreach (TreeNodeAdv node in tree.SelectedNodes)
            {
                _clips.Add(((ICloneable)node.TagObject));
            }
        }
        /// <summary>
        /// Verifies whether paste action can be performed.
        /// </summary>
        /// <returns>True if can be pasted, otherwise False.</returns>
        public bool CanPaste()
        {
            return (_clips.Count > 0);
        }

        /// <summary>
        /// Adds selected cells to clipboard container.
        /// </summary>
        /// <param name="grid">Grid to be copied.</param>
        public void CopyGridData(GridControl grid)
        {
            BeginCopy();

            if (!grid.CutPaste.CanCopy()) return;
            if (grid.Model.SelectedRanges == null || grid.Model.SelectedRanges.Count == 0) return;

            grid.CutPaste.Copy();
        }
    }
}

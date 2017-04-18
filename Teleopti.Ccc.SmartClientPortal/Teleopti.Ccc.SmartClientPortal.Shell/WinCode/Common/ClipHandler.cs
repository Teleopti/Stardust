using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary>
    /// handle clips
    /// </summary>
    [Serializable]
    public class ClipHandler
    {
        private List<Clip> _clipList;   //list with clips         
        private int _anchorRow;         //row anchor (first clip in list)
        private int _anchorColumn;         //col anchor (first clip in list)
        private bool _isInCutMode;

        /// <summary>
        /// constructor
        /// </summary>
        public ClipHandler()
        {
            _clipList = new List<Clip>();
        }

        /// <summary>
        /// reset cliplist
        /// </summary>
        public void Clear()
        {
            _clipList.Clear();
            _anchorRow = 0;
            _anchorColumn = 0;
            IsInCutMode = false;
        }

        /// <summary>
        /// get cliplist
        /// </summary>
        public ReadOnlyCollection<Clip> ClipList
        {
            get { return new ReadOnlyCollection<Clip>(_clipList); }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is in cut mode.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in cut mode; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-17
        /// </remarks>
        public bool IsInCutMode
        {
            get { return _isInCutMode; }
            set { _isInCutMode = value; }
        }

        /// <summary>
        /// Gets the anchor row.
        /// </summary>
        /// <value>The anchor row.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-17
        /// </remarks>
        public int AnchorRow
        {
            get { return _anchorRow; }
        }

        /// <summary>
        /// Gets the anchor column.
        /// </summary>
        /// <value>The anchor column.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-17
        /// </remarks>
        public int AnchorColumn
        {
            get { return _anchorColumn; }
        }


        /// <summary>
        /// add clip to list
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        //public void AddClip(int row, int col, ClipboardAssignment value)
        public void AddClip(int row, int col, object value)
        {
            Clip clip;

            if (_clipList.Count == 0)
            {
                clip = new Clip(0, 0, value);
                _anchorRow = row;
                _anchorColumn = col;
            }
            else
            {
                clip = new Clip(row - AnchorRow, col - AnchorColumn, value);
            }

            _clipList.Add(clip);
        }

        /// <summary>
        /// get how many rows the clips in list span over
        /// </summary>
        /// <returns></returns>
        public int RowSpan()
        {
            int rowSpan = BottomRow() - TopRow();

            return rowSpan + 1;
        }

        /// <summary>
        /// get how many columns the clips in list span over
        /// </summary>
        /// <returns></returns>
        public int ColSpan()
        {
            int colSpan = BottomCol() - TopCol();

            return colSpan + 1;
        }

        /// <summary>
        /// get top row in clip list
        /// </summary>
        /// <returns></returns>
        private int TopRow()
        {
            if (_clipList.Count == 0)
                return 0;
            else
                return _clipList.Min(c => c.RowOffset);
        }

        /// <summary>
        /// get top left column in clip list
        /// </summary>
        /// <returns></returns>
        private int TopCol()
        {
            if (_clipList.Count == 0)
                return 0;
            else
                return _clipList.Min(c => c.ColOffset);
        }

        /// <summary>
        /// get bottom row in cliplist
        /// </summary>
        /// <returns></returns>
        private int BottomRow()
        {
            if (_clipList.Count == 0)
                return 0;
            else
                return _clipList.Max(c => c.RowOffset);
        }

        /// <summary>
        /// get bottom right column in clip list
        /// </summary>
        /// <returns></returns>
        private int BottomCol()
        {
            if (_clipList.Count == 0)
                return 0;
            else
                return _clipList.Max(c => c.ColOffset);
        }
    }
}
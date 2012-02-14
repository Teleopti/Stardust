using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WindowsFormsApplication1.Helper
{
    /// <summary>
    /// handle clips
    /// </summary>
    public class ClipHandler<T>
    {
        private readonly List<Clip<T>> _clipList; 
        private int _anchorRow;  
        private int _anchorColumn;
        private bool _isInCutMode;

        /// <summary>
        /// constructor
        /// </summary>
        public ClipHandler()
        {
            _clipList = new List<Clip<T>>();
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
        public ReadOnlyCollection<Clip<T>> ClipList
        {
            get { return new ReadOnlyCollection<Clip<T>>(_clipList); }
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
        public void AddClip(int row, int col, T value)
        {
            Clip<T> clip;

            if (_clipList.Count == 0)
            {
                clip = new Clip<T>(0, 0, value);
                _anchorRow = row;
                _anchorColumn = col;
            }
            else
            {
                clip = new Clip<T>(row - AnchorRow, col - AnchorColumn, value);
            }

            _clipList.Add(clip);
        }

    }
}

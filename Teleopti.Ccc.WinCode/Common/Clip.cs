using System;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// clip to be handled in ClipHandler
    /// </summary>
    [Serializable]
    public class Clip
    {
        //private ClipboardAssignment _object;     //object in clip
        private object _object;

        private int _colOffset;     //col offset from anchor clip(first)
        private int _rowOffset;     //row offset from anchor clip(first)

        /// <summary>
        /// constructor, create clip
        /// </summary>
        /// <param name="rowOffset"></param>
        /// <param name="colOffset"></param>
        /// <param name="value"></param>
        //public Clip(int rowOffset, int colOffset, ClipboardAssignment value)
        public Clip(int rowOffset, int colOffset, object value)
        {
            _colOffset = colOffset;
            _rowOffset = rowOffset;
            _object = value;
        }

        /// <summary>
        /// get column offset
        /// </summary>
        public int ColOffset
        {
            get { return _colOffset; }
        }

        /// <summary>
        /// get row offset
        /// </summary>
        public int RowOffset
        {
            get { return _rowOffset; }
        }

        /// <summary>
        /// get object in clip
        /// </summary>
        //public ClipboardAssignment ClipObject
        public object ClipObject
        {
            get { return _object; }
        }
    }
}
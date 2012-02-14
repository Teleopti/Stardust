namespace WindowsFormsApplication1.Helper
{
    /// <summary>
    /// clip to be handled in ClipHandler
    /// </summary>
    public struct Clip<T>
    {
        private readonly T _value;
        private readonly int _colOffset;     //col offset from anchor
        private readonly int _rowOffset;     //row offset from anchor

        /// <summary>
        /// constructor, create clip
        /// </summary>
        /// <param name="rowOffset"></param>
        /// <param name="colOffset"></param>
        /// <param name="value"></param>
        public Clip(int rowOffset, int colOffset, T value)
        {
            _colOffset = colOffset;
            _rowOffset = rowOffset;
            _value = value;
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
        /// get value in clip
        /// </summary>
        public T ClipValue
        {
            get { return _value; }
        }
    }
}

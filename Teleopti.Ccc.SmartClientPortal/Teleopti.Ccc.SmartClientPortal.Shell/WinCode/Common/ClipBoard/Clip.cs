using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard
{
    /// <summary>
    /// clip to be handled in ClipHandler
    /// </summary>
    [Serializable]
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
        public int ColOffset => _colOffset;

		/// <summary>
        /// get row offset
        /// </summary>
        public int RowOffset => _rowOffset;

		/// <summary>
        /// get value in clip
        /// </summary>
        public T ClipValue => _value;

		/// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
        	if (obj == null || !(obj is Clip<T>))
                return false;
        	return Equals((Clip<T>)obj);
        }

    	/// <summary>
        /// Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Clip<T> other)
		{
			return other.RowOffset == RowOffset && other.ColOffset == ColOffset;
		}

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode() ^ _rowOffset.GetHashCode() ^ _colOffset.GetHashCode();
        }

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="clip1"></param>
        /// <param name="clip2"></param>
        /// <returns></returns>
        public static bool operator ==(Clip<T> clip1, Clip<T> clip2)
        {
            return clip1.Equals(clip2);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="clip1"></param>
        /// <param name="clip2"></param>
        /// <returns></returns>
        public static bool operator !=(Clip<T> clip1, Clip<T> clip2)
        {
            return !clip1.Equals(clip2);
        }
    }
}

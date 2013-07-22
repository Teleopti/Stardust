using System;

namespace Teleopti.Ccc.AgentPortalCode.Common.Clipboard
{
    public class Clip<T>
    {
        private readonly T _clipValue;
        private readonly DateTime _date;

        public Clip(T clipValue, DateTime date)
        {
            _clipValue = clipValue;
            _date = date;
        }

        public T ClipValue
        {
            get { return _clipValue; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public override bool Equals(object obj)
        {
            Clip<T> clip = obj as Clip<T>;

            if (clip == null)
            {
                return false;
            }
        	return Equals(clip);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Clip<T> other)
        {
            if (other.Date  == _date && other.ClipValue.Equals(other))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _clipValue.GetHashCode() ^ _date.GetHashCode() ;
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

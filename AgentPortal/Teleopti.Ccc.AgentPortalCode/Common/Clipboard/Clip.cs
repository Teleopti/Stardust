#region Imports

using System;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common.Clipboard
{

    /// <summary>
    /// Represents a clip to be handle in clipboard copy and paste.
    /// </summary>
    public class Clip<T>
    {

        #region Fields - Instance Member

        private readonly T _clipValue;
        private readonly DateTime _date;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - Clip Members

        public T ClipValue
        {
            get { return _clipValue; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public ScheduleAppointmentTypes ClipValueType
        {
            get { return _clipValueType; }
            set { _clipValueType = value; }
        }

        private ScheduleAppointmentTypes _clipValueType;

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - Clip Members

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Clip<T> clip = obj as Clip<T>;

            if (clip == null)
            {
                return false;
            }
            else
            {
                return Equals(clip);
            }
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
            else
            {
                return false;
            }
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

        #endregion

        #region Methods - Instance Member - Clip Members - (constructors)

        public Clip(T clipValue, ScheduleAppointmentTypes clipValueType,DateTime date)
        {
            _clipValue = clipValue;
            _clipValueType = clipValueType;
            _date = date;
        }

        #endregion

        #endregion


    }

}

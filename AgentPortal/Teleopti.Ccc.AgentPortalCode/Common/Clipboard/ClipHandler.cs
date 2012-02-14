#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common.Clipboard
{

    /// <summary>
    /// Represents a class that handles the clips in copy/paste operations .
    /// </summary>
    public class ClipHandler<T>
    {

        #region Fields - Instance Member

        private readonly List<Clip<T>> _clipList;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ClipHandler Members

        public ReadOnlyCollection<Clip<T>> ClipList
        {
            get { return new ReadOnlyCollection<Clip<T>>(_clipList); }
        }

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - ClipHandler Members

        /// <summary>
        /// add clip to list
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="clipValueType">Type of the clip value.</param>
        /// <param name="date">The date.</param>
        public void AddClip(T value,ScheduleAppointmentTypes clipValueType,DateTime date)
        {
            Clip<T> clip;

            clip = new Clip<T>(value, clipValueType,date);
            _clipList.Add(clip);
        }

        /// <summary>
        /// reset cliplist
        /// </summary>
        public void Clear()
        {
            _clipList.Clear();
        }

        #endregion

        #region Methods - Instance Member - ClipHandler Members -  (constructors)

        /// <summary>
        /// constructor
        /// </summary>
        public ClipHandler()
        {
            _clipList = new List<Clip<T>>();
        }

        #endregion
        
        #endregion
    }
}

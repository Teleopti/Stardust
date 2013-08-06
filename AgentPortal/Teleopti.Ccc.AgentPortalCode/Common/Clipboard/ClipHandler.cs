using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.AgentPortalCode.Common.Clipboard
{
    public class ClipHandler<T>
    {
        private readonly List<Clip<T>> _clipList;

        public ClipHandler()
        {
            _clipList = new List<Clip<T>>();
        }

        public ReadOnlyCollection<Clip<T>> ClipList
        {
            get { return new ReadOnlyCollection<Clip<T>>(_clipList); }
        }

        public void AddClip(T value,ScheduleAppointmentTypes clipValueType,DateTime date)
        {
            Clip<T> clip = new Clip<T>(value, date);
            _clipList.Add(clip);
        }

        public void Clear()
        {
            _clipList.Clear();
        }
    }
}

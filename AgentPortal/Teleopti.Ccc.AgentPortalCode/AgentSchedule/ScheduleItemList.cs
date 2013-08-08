using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class ScheduleItemList : IScheduleItemList
    {
        private readonly IList<ICustomScheduleAppointment> _scheduleItemCollection ;

        public ScheduleItemList()
        {
            _scheduleItemCollection = new List<ICustomScheduleAppointment>();
        }

        public IList<ICustomScheduleAppointment> ScheduleItemCollection
        {
            get 
            {
                return _scheduleItemCollection; 
            }
        }

        public void AddScheduleItem(ICustomScheduleAppointment scheduleItem)
        {
            if(scheduleItem!=null)
            {
                bool isUnchanged = true;
                foreach (ICustomScheduleAppointment scheduleAppointment in _scheduleItemCollection)
                {
                    if (scheduleAppointment.Equals(scheduleItem))
                    {
                        if ( (scheduleAppointment.Status == ScheduleAppointmentStatusTypes.Unchanged)||
                             (scheduleItem.Status == ScheduleAppointmentStatusTypes.Updated))
                        {
                            _scheduleItemCollection.Remove(scheduleAppointment);
                        }
                        else
                        {
                            isUnchanged = false;
                        }
                        break;
                    }
                }

                if (isUnchanged)
                {
                    _scheduleItemCollection.Add(scheduleItem);
                }
            }
        }

        public ICustomScheduleAppointment FirstScheduleItem()
        {
            ICustomScheduleAppointment firstScheduleItem = null;

             if(_scheduleItemCollection.Count>0)
             {
                 firstScheduleItem = _scheduleItemCollection[0].StartTime > _scheduleItemCollection[_scheduleItemCollection.Count - 1].StartTime ?
                                    _scheduleItemCollection[_scheduleItemCollection.Count - 1] : _scheduleItemCollection[0];
             }

             return firstScheduleItem;
        }

        public ICustomScheduleAppointment GetCurrentScheduleItem(System.DateTime currentDateTime)
        {
            ICustomScheduleAppointment currentScheduleItem = null;

            foreach (ICustomScheduleAppointment item in _scheduleItemCollection)
            {
                if ((item.StartTime <= currentDateTime) &&
                    (item.EndTime >= currentDateTime))
                {
                    currentScheduleItem = item;
                    break;
                }
            }

            return currentScheduleItem;
        }

        public ICustomScheduleAppointment GetNextActivity(System.DateTime currentDateTime)
        {
            ICustomScheduleAppointment nextScheduleItem = null;

            foreach (ICustomScheduleAppointment item in _scheduleItemCollection)
            {
                if (item.StartTime > currentDateTime)
                {
                    nextScheduleItem = item;
                    break;
                }
            }

            return nextScheduleItem;
        }
    }
}

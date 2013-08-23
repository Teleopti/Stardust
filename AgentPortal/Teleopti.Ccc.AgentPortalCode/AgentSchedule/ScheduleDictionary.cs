using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{

    /// <summary>
    /// Represents a class tat holds all schedule items for given period that represent various views in Schedule control.
    /// </summary>d
    public class ScheduleDictionary : IScheduleDictionary
    {
        private IDictionary<DateTime, IScheduleItemList> _dictionary;
        
        public ScheduleDictionary()
        {
            _dictionary = new Dictionary<DateTime, IScheduleItemList>();
        }

        public bool ContainsKey(DateTime key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IScheduleItemList this[DateTime key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool Remove(DateTime key)
        {
            return _dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<DateTime, IScheduleItemList> item)
        {
            return _dictionary.Remove(item);
        }

        public bool TryGetValue(DateTime key, out IScheduleItemList value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<DateTime> Keys
        {
            get { return _dictionary.Keys; }
        }

        public ICollection<IScheduleItemList> Values
        {
            get { return _dictionary.Values; }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<DateTime, IScheduleItemList> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<DateTime, IScheduleItemList>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<KeyValuePair<DateTime, IScheduleItemList>> IEnumerable<KeyValuePair<DateTime, IScheduleItemList>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Add(DateTime key, IScheduleItemList value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<DateTime, IScheduleItemList> item)
        {
            throw new NotSupportedException();
        }

        public void Fill(IList<ICustomScheduleAppointment> scheduleItemCollection)
        {
            FillScheuldeDictionary(scheduleItemCollection);
        }

        public IScheduleAppointmentList AllScheduleAppointments()
        {
            IScheduleAppointmentList list = new ScheduleAppointmentList();

            foreach (IScheduleItemList values in _dictionary.Values)
            {
                foreach (ICustomScheduleAppointment scheduleItem in values.ScheduleItemCollection)
                {
                    if (scheduleItem.Status != ScheduleAppointmentStatusTypes.Deleted)
                    {
                        list.Add(scheduleItem);
                    }
                }
            }

            return list;
        }

        public IScheduleAppointmentList ScheduleAppointments(DateTimePeriodDto period, ScheduleAppointmentTypes scheduleItemType)
        {
            IScheduleAppointmentList list = new ScheduleAppointmentList();
            
            foreach (KeyValuePair<DateTime, IScheduleItemList> keyValuePair in _dictionary)
            {
                foreach (ICustomScheduleAppointment scheduleItem in keyValuePair.Value.ScheduleItemCollection)
                {
                    if ( ( (scheduleItemType & scheduleItem.AppointmentType) == scheduleItem.AppointmentType )&&
                         (scheduleItem.Status != ScheduleAppointmentStatusTypes.Deleted))
                    {
                        if ((scheduleItem.StartTime >= period.LocalStartDateTime) &&
                            (scheduleItem.EndTime.AddSeconds(-1).AddMilliseconds(-1) <= period.LocalEndDateTime)) //Hmm... Removed 1 second to fix problem with DST in Jordan Time 
                        {
                            // if not dayView, truncate
                            if (period.LocalEndDateTime.Subtract(period.LocalStartDateTime) > TimeSpan.FromDays(2))
                            {
                                var defaultFont = new Font("Arial", 8, FontStyle.Regular);
                                // truncate subject
                                if (scheduleItem.Subject.Length > 15) scheduleItem.Subject = scheduleItem.Subject.Substring(0, 14) + UserTexts.Resources.ThreeDots;
                                Size textSize = System.Windows.Forms.TextRenderer.MeasureText(scheduleItem.Subject, defaultFont);

                                while (textSize.Width > 90)
                                {
                                    scheduleItem.Subject = scheduleItem.Subject.Substring(0, scheduleItem.Subject.Length - 4) +
                                                      UserTexts.Resources.ThreeDots;
                                    textSize = System.Windows.Forms.TextRenderer.MeasureText(scheduleItem.Subject, defaultFont);
                                }

                                // truncate location
                                if (scheduleItem.LocationValue.Length > 24) scheduleItem.LocationValue = scheduleItem.LocationValue.Substring(0, 23) + UserTexts.Resources.ThreeDots;
                                textSize = System.Windows.Forms.TextRenderer.MeasureText(scheduleItem.LocationValue, defaultFont);

                                while (textSize.Width > 140)
                                {
                                    scheduleItem.LocationValue = scheduleItem.LocationValue.Substring(0, scheduleItem.LocationValue.Length - 4) +
                                                      UserTexts.Resources.ThreeDots;
                                    textSize = System.Windows.Forms.TextRenderer.MeasureText(scheduleItem.LocationValue, defaultFont);
                                }

                                defaultFont.Dispose();
                            }

                            list.Add(scheduleItem);
                        }
                    }
                }
            }

            return list;
        }

        public IList<ICustomScheduleAppointment> Filter(ScheduleAppointmentStatusTypes appointmentStatusType)
        {
            IList<ICustomScheduleAppointment> scheduleItemCollection = new List<ICustomScheduleAppointment>();

            foreach (KeyValuePair<DateTime, IScheduleItemList> keyValuePair in _dictionary)
            {
                foreach (ICustomScheduleAppointment scheduleAppointment in keyValuePair.Value.ScheduleItemCollection)
                {
                    if ((appointmentStatusType & scheduleAppointment.Status) == scheduleAppointment.Status)
                    {
                        scheduleItemCollection.Add(scheduleAppointment);
                    }
                }
            }

            return scheduleItemCollection;
        }

        public void Clear(bool keepUnsavedScheduleItems)
        {
            if (!keepUnsavedScheduleItems)
            {
                Clear();
            }
            else
            {
                RemoveUnchangedItems();
            }
        }
        
        private void FillScheuldeDictionary(IEnumerable<ICustomScheduleAppointment> scheduleItemCollection)
        {
            if (scheduleItemCollection != null)
            {
                foreach (ICustomScheduleAppointment scheduleItem in scheduleItemCollection)
                {
                    DateTime scheduleDate = scheduleItem.StartTime.Date;

                    if (!_dictionary.ContainsKey(scheduleDate))
                    {
                        _dictionary.Add(scheduleDate, new ScheduleItemList());
                    }
                    _dictionary[scheduleDate].AddScheduleItem(scheduleItem);
                }
            }
        }

        private void RemoveUnchangedItems()
        {
            if (_dictionary.Count > 0)
            {
                IDictionary<DateTime, IScheduleItemList> tempdictionary = new Dictionary<DateTime, IScheduleItemList>();

                foreach (KeyValuePair<DateTime, IScheduleItemList> keyValuePair in _dictionary)
                {
                    foreach (ICustomScheduleAppointment scheduleAppointment in keyValuePair.Value.ScheduleItemCollection)
                    {
                        if (scheduleAppointment.Status != ScheduleAppointmentStatusTypes.Unchanged)
                        {
                            if (!tempdictionary.ContainsKey(keyValuePair.Key))
                            {
                                tempdictionary.Add(keyValuePair.Key, new ScheduleItemList());
                            }
                            tempdictionary[keyValuePair.Key].AddScheduleItem(scheduleAppointment);
                        }
                    }
                }

                _dictionary.Clear();
                _dictionary = tempdictionary;
            }
        }
    }
}

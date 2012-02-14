using System;
using System.Collections;
using System.Collections.Generic;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

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

        public void Fill(ICustomScheduleAppointment scheduleItem)
        {
            if (scheduleItem != null)
            {
                DateTime scheduleDate = scheduleItem.StartTime.Date;

                if (!_dictionary.ContainsKey(scheduleDate))
                {
                    _dictionary.Add(scheduleDate, new ScheduleItemList());
                }
                _dictionary[scheduleDate].AddScheduleItem(scheduleItem);
            }
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
                            list.Add(scheduleItem);
                        }
                    }
                }
            }

            return list;
        }

        public IList<ICustomScheduleAppointment> UnsavedAppointments()
        {
            ScheduleAppointmentStatusTypes unsavedStatusType = ScheduleAppointmentStatusTypes.New | ScheduleAppointmentStatusTypes.Updated |
                                                        ScheduleAppointmentStatusTypes.Deleted;

            return Filter(unsavedStatusType);
        }

        public IList<ICustomScheduleAppointment> UnsavedAppointments(ScheduleAppointmentTypes appointmentType, ScheduleAppointmentStatusTypes filterBy)
        {
            ScheduleAppointmentStatusTypes unsavedStatusType = ScheduleAppointmentStatusTypes.New |
                                                                ScheduleAppointmentStatusTypes.Updated |
                                                                ScheduleAppointmentStatusTypes.Deleted ;

            return Filter(appointmentType, unsavedStatusType);
        }

        public IList<ICustomScheduleAppointment> Filter(ScheduleAppointmentTypes appointmentTypes,ScheduleAppointmentStatusTypes appointmentStatusType)
        {
            IList<ICustomScheduleAppointment> scheduleItemCollection = new List<ICustomScheduleAppointment>();

            foreach (KeyValuePair<DateTime, IScheduleItemList> keyValuePair in _dictionary)
            {
                foreach (ICustomScheduleAppointment scheduleAppointment in keyValuePair.Value.ScheduleItemCollection)
                {
                    if (((appointmentStatusType & scheduleAppointment.Status) == scheduleAppointment.Status) &&
                        ((appointmentTypes & scheduleAppointment.AppointmentType) == scheduleAppointment.AppointmentType))
                    {
                        scheduleItemCollection.Add(scheduleAppointment);
                    }
                }
            }

            return scheduleItemCollection;
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
        
        public void RemoveScheduleAppointment(ICustomScheduleAppointment sourceScheduleAppointment)
        {
            Dto dto = sourceScheduleAppointment.Tag as Dto;

            if (dto!=null)
            {
                foreach (KeyValuePair<DateTime, IScheduleItemList> keyValuePair in _dictionary)
                {
                    foreach (ICustomScheduleAppointment scheduleAppointment in keyValuePair.Value.ScheduleItemCollection
                        )
                    {
                        if (dto.Id == null)
                        {
                            if (scheduleAppointment.Tag == dto)
                            {
                                keyValuePair.Value.ScheduleItemCollection.Remove(scheduleAppointment);
                                break;
                            }
                        }
                        else
                        {
                            Dto sourceDto = scheduleAppointment.Tag as Dto;

                            if (sourceDto != null && sourceDto.Id == dto.Id)
                            {
                                scheduleAppointment.Status = ScheduleAppointmentStatusTypes.Deleted;
                                break;
                            }
                        }
                    }
                }
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

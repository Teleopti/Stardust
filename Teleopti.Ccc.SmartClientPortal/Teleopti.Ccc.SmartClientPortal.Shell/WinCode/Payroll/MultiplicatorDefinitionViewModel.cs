using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public class MultiplicatorDefinitionViewModel:GridViewBaseModel<MultiplicatorDefinition>, IMultiplicatorDefinitionViewModel
    {
        private readonly IMultiplicatorDefinition _multiplicatorDefinition;

        public MultiplicatorDefinitionViewModel(IMultiplicatorDefinition multiplicatorDefinition)
            : base((MultiplicatorDefinition)multiplicatorDefinition)
        {
            _multiplicatorDefinition = multiplicatorDefinition;
        }

        public MultiplicatorDefinitionAdapter MultiplicatorDefinitionType
        {
            get
            {
                string typeName = "";
                if (_multiplicatorDefinition is DateTimeMultiplicatorDefinition)
                {
                    typeName = UserTexts.Resources.FromTo;
                }
                if (_multiplicatorDefinition is DayOfWeekMultiplicatorDefinition)
                {
                    typeName = UserTexts.Resources.DayOfWeekCapital;
                }

                return new MultiplicatorDefinitionAdapter(_multiplicatorDefinition.GetType(), typeName);
            }
        }

        public DayOfWeek? DayOfWeek
        {
            get
            {
                var day = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (day != null)
                    return day.DayOfWeek;
                return null;
            }
            set
            {
                var day = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (day != null) day.DayOfWeek = (DayOfWeek) value;
            }
        }

        public TimeSpan? StartTime
        {
            get
            {
                var dayOfWeek = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (dayOfWeek != null) return dayOfWeek.Period.StartTime;
                return null;
            }
            set
            {
                var dayOfWeek = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (dayOfWeek != null)
                {
                    TimeSpan newStartTime = (TimeSpan)value;
                    if (newStartTime < EndTime)
                    {
                        TimePeriod period = new TimePeriod(newStartTime, dayOfWeek.Period.EndTime);
                        dayOfWeek.Period = period;
                    }
                    else
                    {
                        TimePeriod period = new TimePeriod(newStartTime, dayOfWeek.Period.EndTime.Add(new TimeSpan(1, 0, 0, 0)));
                        dayOfWeek.Period = period;
                    }
                }
            }
        }

        public TimeSpan? EndTime
        {
            get
            {
                var dayOfWeek = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (dayOfWeek != null) return dayOfWeek.Period.EndTime;
                return null;
            }
            set
            {
                var dayOfWeek = _multiplicatorDefinition as DayOfWeekMultiplicatorDefinition;
                if (dayOfWeek != null)
                {
                    TimeSpan newEndTime = (TimeSpan)value;
                    if (newEndTime > StartTime)
                    {
                        TimePeriod period = new TimePeriod(dayOfWeek.Period.StartTime, newEndTime);
                        dayOfWeek.Period = period;
                    }
                    else
                    {
                        var extended = newEndTime.Add(new TimeSpan(1, 0, 0, 0));
                        TimePeriod period;

                        if (extended < dayOfWeek.Period.StartTime)
                        {
                            //Strange case
                            period = new TimePeriod(dayOfWeek.Period.StartTime, dayOfWeek.Period.StartTime);
                        }
                        else
                        {
                            period = new TimePeriod(dayOfWeek.Period.StartTime, newEndTime.Add(new TimeSpan(1, 0, 0, 0)));
                        }


                        dayOfWeek.Period = period;
                    }
                }
            }
        }

        public DateTime? FromDate
        {
            get
            {
                var dateTime = _multiplicatorDefinition as DateTimeMultiplicatorDefinition;
                if (dateTime != null)
                    return dateTime.StartDate.Date.Add(dateTime.StartTime);
                return null;
            }
            set
            {
                var dateTime = _multiplicatorDefinition as DateTimeMultiplicatorDefinition;
                if (dateTime != null && value.HasValue)
                {
                    if (value.Value >= dateTime.EndDate.Date.Add(dateTime.EndTime))
                    {
                        dateTime.EndDate = new DateOnly((DateTime) value);
                        dateTime.EndTime = value.Value.TimeOfDay;
                    }
                    dateTime.StartDate = new DateOnly((DateTime)value);
                    dateTime.StartTime = value.Value.TimeOfDay;
                }
            }
        }

        public DateTime? ToDate
        {
            get
            {
                var dateTime = _multiplicatorDefinition as DateTimeMultiplicatorDefinition;
                if (dateTime != null)
                    return dateTime.EndDate.Date.Add(dateTime.EndTime);
                return null;
            }
            set
            {
                var dateTime = _multiplicatorDefinition as DateTimeMultiplicatorDefinition;
                if (dateTime != null && value.HasValue)
                {
                    if (value.Value <= dateTime.StartDate.Date.Add(dateTime.StartTime))
                    {
                        dateTime.StartDate = new DateOnly((DateTime) value);
                        dateTime.StartTime = value.Value.TimeOfDay;
                    }
                    dateTime.EndDate = new DateOnly((DateTime)value);
                    dateTime.EndTime = value.Value.TimeOfDay;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class UnsavedDayInfo : IUnsavedDayInfo
    {
        private readonly DateOnly _dateTime;
        private readonly IScenario _scenario;

        public UnsavedDayInfo(DateOnly date, IScenario scenario)
        {
            _dateTime = date;
            _scenario = scenario;
        }

        public DateOnly DateTime { get { return _dateTime; } }

        public IScenario Scenario { get { return _scenario; } }

        public bool Equals(IUnsavedDayInfo other)
        {
            return DateTime.Equals(other.DateTime) && Scenario.Description.Equals(other.Scenario.Description);
        }
    }

    public class UnsavedDaysInfo : IUnsavedDaysInfo
    {
        private readonly IList<IUnsavedDayInfo> _unsavedDays;

        public UnsavedDaysInfo()
        {
            _unsavedDays = new List<IUnsavedDayInfo>();
        }

        public void Add(IUnsavedDayInfo unsavedDayInfo)
        {
            _unsavedDays.Add(unsavedDayInfo);
        }

        public bool Contains(IUnsavedDayInfo unsavedDayInfo)
        {
            return _unsavedDays.Contains(unsavedDayInfo);
        }

        public int Count
        {
            get { return _unsavedDays.Count; }
        }

        public IList<IUnsavedDayInfo> UnsavedDays
        {
            get { return _unsavedDays; }
        }

        public IList<IUnsavedDayInfo> UnsavedDaysOrderedByDate
        {
            get { return _unsavedDays.OrderBy(d => d.DateTime).ToList(); }
        }

        public bool ContainsDateTime(DateOnly dateTime)
        {
            return _unsavedDays.Any(day => day.DateTime.Equals(dateTime));
        }

        public bool Equals(IUnsavedDaysInfo other)
        {
            return _unsavedDays.SequenceEqual(other.UnsavedDays);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine,_unsavedDays.Select(day => day.DateTime + " " + day.Scenario.Description.Name));
        }
    }
}

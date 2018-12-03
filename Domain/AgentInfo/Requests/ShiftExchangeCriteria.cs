using System.Diagnostics.Contracts;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public struct ShiftExchangeCriteria : IShiftExchangeCriteria
	{
		private DateOnly _validTo;
		private ScheduleDayFilterCriteria _criteria;

		public ShiftExchangeCriteria(DateOnly validTo, ScheduleDayFilterCriteria criteria)
		{
			_validTo = validTo;
			_criteria = criteria;
		}

		public ScheduleDayFilterCriteria Criteria
		{
			get { return _criteria; }
			set { _criteria = value; }
		}

		public DateOnly ValidTo
		{
			get { return _validTo; }
			set { _validTo = value; }
		}

		public ShiftExchangeLookingForDay DayType
		{
			get { return _criteria.DayType; }
			set { _criteria.DayType = value; }
		}

		public ScheduleDayFilterCriteria DayFilterCriteria
		{
			get { return _criteria; }
			set { _criteria = value; }
		}

		public DateTimePeriod? ShiftWithin
		{
			get { return _criteria.ShiftWithin; }
			set { _criteria.ShiftWithin = value; }
		}

		[Pure]
		public bool IsValid(DateTimePeriod? targetShiftPeriod, bool targetDayOff = false)
		{
			return DateOnly.Today <= _validTo && (matchingWithDayOff(targetDayOff)
			                                      || matchingWithEmptyDay(targetShiftPeriod, targetDayOff)
			                                      || matchingWithWorkingShift(targetShiftPeriod));
		}

		private bool matchingWithDayOff(bool targetDayOff)
		{
			return targetDayOff && (_criteria.DayType == ShiftExchangeLookingForDay.DayOff
			                        || _criteria.DayType == ShiftExchangeLookingForDay.DayOffOrEmptyDay);
		}

		private bool matchingWithEmptyDay(DateTimePeriod? targetShiftPeriod, bool targetDayOff)
		{
			return !(targetDayOff || targetShiftPeriod.HasValue) &&
			       (_criteria.DayType == ShiftExchangeLookingForDay.EmptyDay ||
			        _criteria.DayType == ShiftExchangeLookingForDay.DayOffOrEmptyDay);
		}

		private bool matchingWithWorkingShift(DateTimePeriod? targetShiftPeriod)
		{
			return targetShiftPeriod.HasValue && (_criteria.DayType == ShiftExchangeLookingForDay.WorkingShift) &&
			       _criteria.ShiftWithin.HasValue && _criteria.ShiftWithin.Value.Contains(targetShiftPeriod.Value);
		}
	}
}
using System.Diagnostics.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public struct ShiftExchangeCriteria
	{
		private readonly DateOnly _validTo;
		private readonly DateTimePeriod? _shiftWithin;

		public DateTimePeriod? ShiftWithin
		{
			get { return _shiftWithin; }
		}

		public ShiftExchangeCriteria(DateOnly validTo, DateTimePeriod? shiftWithin)
		{
			_validTo = validTo;
			_shiftWithin = shiftWithin;
		}

		[Pure]
		public bool IsValid(DateTimePeriod? period)
		{
			return DateOnly.Today <= _validTo && (bothAreDayOff(period,_shiftWithin) || (bothAreMatchingShifts(period,_shiftWithin)));
		}

		private bool bothAreDayOff(DateTimePeriod? shift, DateTimePeriod? shiftCriteria)
		{
			return !shift.HasValue && !shiftCriteria.HasValue;
		}

		private bool bothAreMatchingShifts(DateTimePeriod? shift, DateTimePeriod? shiftCriteria)
		{
			return shift.HasValue && shiftCriteria.HasValue && shiftCriteria.Value.Contains(shift.Value);
		}
	}
}
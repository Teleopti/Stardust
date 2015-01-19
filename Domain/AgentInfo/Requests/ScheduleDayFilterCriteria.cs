using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public struct ScheduleDayFilterCriteria
	{
		private readonly ShiftExchangeLookingForDay _dayType;
		private readonly DateTimePeriod? _shiftWithin;

		public ScheduleDayFilterCriteria(ShiftExchangeLookingForDay dayType, DateTimePeriod? shiftWithin)
		{
			_shiftWithin = null;
			switch (dayType)
			{
				case ShiftExchangeLookingForDay.WorkingShift:
					if (!shiftWithin.HasValue)
						throw new ArgumentException("Must specify the shift period for working day.");
					_shiftWithin = shiftWithin;
					_dayType = dayType;
					break;
				case ShiftExchangeLookingForDay.EmtpyDay:
				case ShiftExchangeLookingForDay.DayOff:
				case ShiftExchangeLookingForDay.DayOffOrEmptyDay:
					if (shiftWithin.HasValue)
					{
						throw new ArgumentException("Must not specify the shift period for empty day or day off.");
					}
					_dayType = dayType;
					break;
				default:
					throw new ArgumentException("Invalid day type.");
			}
		}

		public DateTimePeriod? ShiftWithin
		{
			get { return _shiftWithin; }
		}

		public ShiftExchangeLookingForDay DayType
		{
			get { return _dayType; }
		}
	}
}
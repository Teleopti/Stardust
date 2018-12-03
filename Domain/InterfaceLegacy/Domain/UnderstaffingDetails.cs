using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Class that contains under staffing and serious under staffing dates and hours information.
	/// </summary>
	public class UnderstaffingDetails
	{
		private readonly HashSet<DateOnly> _understaffingDays = new HashSet<DateOnly>();
		private readonly HashSet<DateTimePeriod> _understaffingPeriods = new HashSet<DateTimePeriod>();

		public IEnumerable<DateOnly> UnderstaffingDays => _understaffingDays;

		public IEnumerable<DateTimePeriod> UnderstaffingPeriods => _understaffingPeriods;

		public void AddUnderstaffingDay(DateOnly date)
		{
			_understaffingDays.Add(date);
		}

		public void AddUnderstaffingPeriod(DateTimePeriod period)
		{
			_understaffingPeriods.Add(period);
		}

		public bool IsNotUnderstaffed()
		{
			return _understaffingDays.Count == 0 &&
					 _understaffingPeriods.Count == 0;
		}
	}
}

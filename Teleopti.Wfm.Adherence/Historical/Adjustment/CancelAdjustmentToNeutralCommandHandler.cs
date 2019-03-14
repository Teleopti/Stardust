using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.Adjustment
{
	public class CancelAdjustmentToNeutralCommandHandler
	{
		private readonly IUserTimeZone _timeZone;
		private readonly Adjustment _adjustment;

		public CancelAdjustmentToNeutralCommandHandler(
			IUserTimeZone timeZone,
			Adjustment adjustment
		)
		{
			_timeZone = timeZone;
			_adjustment = adjustment;
		}

		public void Handle(CancelAdjustmentToNeutralCommand command)
		{
			_adjustment.Cancel(new PeriodToCancel
			{
				StartTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(command.StartDateTime), _timeZone.TimeZone()),
				EndTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(command.EndDateTime), _timeZone.TimeZone())
			});
		}
	}
}
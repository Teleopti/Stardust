using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class EditableShiftLayer : IEditableShiftLayer
	{
		public EditableShiftLayer(IActivity activity, DateTimePeriod period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
			Payload = activity;
			Period = period;
		}

		public DateTimePeriod Period { get; private set; }
		public IActivity Payload { get; private set; }
	}
}
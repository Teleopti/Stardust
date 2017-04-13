using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class PasteAsOvertime
	{
		private readonly IScheduleDay _source;
		private readonly IScheduleDay _destination;
		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;

		public PasteAsOvertime(IScheduleDay source, IScheduleDay destination, IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			_source = source;
			_destination = destination;
			_multiplicatorDefinitionSet = multiplicatorDefinitionSet;
		}

		public void Paste()
		{
			if (_source == null || _destination == null || _multiplicatorDefinitionSet == null ) return;
			var personAssignment = _source.PersonAssignment();
			if (personAssignment == null) return;
			if (_multiplicatorDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime) return;

			pasteOnDestination(personAssignment);
		}

		private void pasteOnDestination(IPersonAssignment personAssignment)
		{
			var periodOffset = ((ExtractedSchedule)_destination).CalculatePeriodOffset(_source.Period);
			pasteMainShiftLayers(personAssignment, periodOffset);
			pasteOvertimelayers(personAssignment, periodOffset);	
		}

		private void pasteMainShiftLayers(IPersonAssignment personAssignment, TimeSpan periodOffset)
		{
			foreach (var mainShiftLayer in personAssignment.MainActivities())
			{
				_destination.CreateAndAddOvertime(mainShiftLayer.Payload, mainShiftLayer.Period.MovePeriod(periodOffset), _multiplicatorDefinitionSet);
			}	
		}

		private void pasteOvertimelayers(IPersonAssignment personAssignment, TimeSpan periodOffset)
		{
			foreach (var overtimeShiftLayer in personAssignment.OvertimeActivities())
			{
				_destination.CreateAndAddOvertime(overtimeShiftLayer.Payload, overtimeShiftLayer.Period.MovePeriod(periodOffset), _multiplicatorDefinitionSet);
			}	
		}
	}
}

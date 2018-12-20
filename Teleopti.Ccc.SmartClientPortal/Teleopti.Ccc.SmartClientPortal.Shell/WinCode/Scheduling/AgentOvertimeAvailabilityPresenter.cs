using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AgentOvertimeAvailabilityPresenter
	{
		private readonly IAgentOvertimeAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private TimePeriod? _existingShiftTimePeriod;
		private TimeSpan _startTime = new TimeSpan(8, 0, 0);
		private TimeSpan _endTime = new TimeSpan(17, 0, 0);

		public AgentOvertimeAvailabilityPresenter(IAgentOvertimeAvailabilityView view, 
													IScheduleDay scheduleDay, 
													ISchedulingResultStateHolder schedulingResultStateHolder,
													IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Initialize()
		{
			var shiftTimePeriod = _scheduleDay.ProjectionService().CreateProjection().Period();
			if (shiftTimePeriod != null)
				_existingShiftTimePeriod = shiftTimePeriod.Value.TimePeriod(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);

			var scheduleDate = _scheduleDay.DateOnlyAsPeriod.DateOnly;
			var person = _scheduleDay.Person;
			long workLengthTicks = 0;

			var averageWorkTimeOfDay = person.AverageWorkTimeOfDay(scheduleDate);
			if (averageWorkTimeOfDay.WorkTimeSource == WorkTimeSource.FromContract)
				workLengthTicks = (long) (averageWorkTimeOfDay.AverageWorkTime.Value.Ticks*averageWorkTimeOfDay.PartTimePercentage.Value);
			else
				workLengthTicks = averageWorkTimeOfDay.AverageWorkTime.Value.Ticks;

			_endTime = _startTime.Add(TimeSpan.FromTicks(workLengthTicks));
		}

		public IAgentOvertimeAvailabilityView View => _view;

		public IScheduleDay ScheduleDay => _scheduleDay;

		public void RunCommand(IExecutableCommand command)
		{
			if(command == null) throw new ArgumentNullException(nameof(command));

			command.Execute();
		}

		public void UpdateView()
		{
			var overtimeAvailability =
				_scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
			if (overtimeAvailability != null)
			{
				_startTime = overtimeAvailability.StartTime.GetValueOrDefault();
				_endTime = overtimeAvailability.EndTime.GetValueOrDefault();
			}
			else
			{
				if (_existingShiftTimePeriod != null)
				{
					_startTime = _existingShiftTimePeriod.Value.EndTime;
					_endTime = _existingShiftTimePeriod.Value.EndTime.Add(TimeSpan.FromHours(1));
				}
			}

			_view.Update(_startTime, _endTime);
		}

		public IExecutableCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator dayCreator)
		{
			if(dayCreator == null) throw new ArgumentNullException(nameof(dayCreator));

			var overtimeAvailabilityday = _scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
			var canCreate = dayCreator.CanCreate(startTime, endTime, out var startError, out var endError);
			
			if (overtimeAvailabilityday != null && !canCreate && startError && endError)
				return new AgentOvertimeAvailabilityRemoveCommand(_scheduleDay, _schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			if (overtimeAvailabilityday == null && canCreate)
                return new AgentOvertimeAvailabilityAddCommand(_scheduleDay, startTime, endTime, dayCreator, _schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			if (overtimeAvailabilityday != null && canCreate)
                return new AgentOvertimeAvailabilityEditCommand(_scheduleDay, startTime, endTime, dayCreator, _schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			return null;
		}
	}
}

using System;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentOvertimeAvailabilityPresenter
	{
		private readonly IAgentOvertimeAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private TimePeriod? _existingShiftTimePeriod;
		private TimeSpan _startTime = new TimeSpan(8, 0, 0);
		private TimeSpan _endTime = new TimeSpan(17, 0, 0);

		public AgentOvertimeAvailabilityPresenter(IAgentOvertimeAvailabilityView view, IScheduleDay scheduleDay, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void Initialize()
		{
			var shiftTimePeriod = _scheduleDay.ProjectionService().CreateProjection().Period();
			if (shiftTimePeriod != null)
				_existingShiftTimePeriod = shiftTimePeriod.Value.TimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

			var scheduleDate = _scheduleDay.DateOnlyAsPeriod.DateOnly;
			var person = _scheduleDay.Person;
			long workLengthTicks = 0;

			var averageWorkTimeOfDay = person.AverageWorkTimeOfDay(scheduleDate);
			if (averageWorkTimeOfDay.WorkTimeSource == WorkTimeSource.FromContract)
				workLengthTicks = (long) (averageWorkTimeOfDay.AverageWorkTime.Ticks*averageWorkTimeOfDay.PartTimePercentage.Value);
			else
				workLengthTicks = averageWorkTimeOfDay.AverageWorkTime.Ticks;

			_endTime = _startTime.Add(TimeSpan.FromTicks(workLengthTicks));
		}

		public IAgentOvertimeAvailabilityView View
		{
			get { return _view; }
		}

		public IScheduleDay ScheduleDay
		{
			get { return _scheduleDay; }
		}

		public void RunCommand(IExecutableCommand command)
		{
			if(command == null) throw new ArgumentNullException("command");

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
			if(dayCreator == null) throw new ArgumentNullException("dayCreator");

			var overtimeAvailabilityday = _scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
			bool startError;
			bool endError;
			var canCreate = dayCreator.CanCreate(startTime, endTime, out startError, out endError);
			
			if (overtimeAvailabilityday != null && !canCreate && startError && endError)
				return new AgentOvertimeAvailabilityRemoveCommand(_scheduleDay, _schedulingResultStateHolder.Schedules);

			if (overtimeAvailabilityday == null && canCreate)
                return new AgentOvertimeAvailabilityAddCommand(_scheduleDay, startTime, endTime, dayCreator, _schedulingResultStateHolder.Schedules);

			if (overtimeAvailabilityday != null && canCreate)
                return new AgentOvertimeAvailabilityEditCommand(_scheduleDay, startTime, endTime, dayCreator, _schedulingResultStateHolder.Schedules);

			return null;
		}
	}
}

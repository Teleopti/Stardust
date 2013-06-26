using System;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public enum AgentOvertimeAvailabilityExecuteCommand
	{
		Add,
		Edit,
		Remove,
		None
	}

	public interface IAgentOvertimeAvailabilityPresenter
	{
		void Add(IAgentOvertimeAvailabilityAddCommand addCommand);
		void Edit(IAgentOvertimeAvailabilityEditCommand editCommand);
		void Remove(IAgentOvertimeAvailabilityRemoveCommand removeCommand);
		void UpdateView();
		void Initialize();
		IScheduleDay ScheduleDay { get; }
		AgentOvertimeAvailabilityExecuteCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator dayCreator);
	}

	public class AgentOvertimeAvailabilityPresenter : IAgentOvertimeAvailabilityPresenter
	{
		private readonly IAgentOvertimeAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
		private TimePeriod? _existingShiftTimePeriod;
		private TimeSpan _startTime = new TimeSpan(8, 0, 0);
		private TimeSpan _endTime = new TimeSpan(17, 0, 0);

		public AgentOvertimeAvailabilityPresenter(IAgentOvertimeAvailabilityView view, IScheduleDay scheduleDay)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		}

		public void Initialize()
		{
			var shiftTimePeriod = _scheduleDay.ProjectionService().CreateProjection().Period();
			if (shiftTimePeriod != null)
				_existingShiftTimePeriod = shiftTimePeriod.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);

			var scheduleDate = _scheduleDay.DateOnlyAsPeriod.DateOnly;
			var person = _scheduleDay.Person;
			var personPeriod = person.Period(scheduleDate);
			var periodStartDate = person.SchedulePeriodStartDate(scheduleDate);
			long workLengthTicks = 0;
			if (personPeriod != null && periodStartDate.HasValue)
			{
				if (personPeriod.PersonContract.Contract.WorkTimeSource == WorkTimeSource.FromContract)
					workLengthTicks = (long)(person.AverageWorkTimeOfDay(scheduleDate).Ticks * personPeriod.PersonContract.PartTimePercentage.Percentage.Value);
				else
					workLengthTicks = person.AverageWorkTimeOfDay(scheduleDate).Ticks;
			}
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

		public void Add(IAgentOvertimeAvailabilityAddCommand addCommand)
		{
			if(addCommand == null) throw new ArgumentNullException("addCommand");

			addCommand.Execute();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Remove(IAgentOvertimeAvailabilityRemoveCommand removeCommand)
		{
			if(removeCommand == null) throw new ArgumentNullException("removeCommand");

			removeCommand.Execute();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Edit(IAgentOvertimeAvailabilityEditCommand editCommand)
		{
			if(editCommand == null) throw new ArgumentNullException("editCommand");

			editCommand.Execute();
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

		public AgentOvertimeAvailabilityExecuteCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator dayCreator)
		{
			if(dayCreator == null) throw new ArgumentNullException("dayCreator");

			var overtimeAvailabilityday = _scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
			bool startError;
			bool endError;
			var canCreate = dayCreator.CanCreate(startTime, endTime, out startError, out endError);
			
			if (overtimeAvailabilityday != null && !canCreate && startError && endError)
				return AgentOvertimeAvailabilityExecuteCommand.Remove;

			if (overtimeAvailabilityday == null && canCreate)
				return AgentOvertimeAvailabilityExecuteCommand.Add;

			if (overtimeAvailabilityday != null && canCreate)
				return AgentOvertimeAvailabilityExecuteCommand.Edit;

			return AgentOvertimeAvailabilityExecuteCommand.None;
		}
	}
}

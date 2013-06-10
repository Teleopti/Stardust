using System;
using System.Linq;
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
		IScheduleDay ScheduleDay { get; }
		AgentOvertimeAvailabilityExecuteCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator dayCreator);
	}

	public class AgentOvertimeAvailabilityPresenter : IAgentOvertimeAvailabilityPresenter
	{
		private readonly IAgentOvertimeAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
		
		public AgentOvertimeAvailabilityPresenter(IAgentOvertimeAvailabilityView view, IScheduleDay scheduleDay)
		{
			_view = view;
			_scheduleDay = scheduleDay;
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
			UpdateView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Remove(IAgentOvertimeAvailabilityRemoveCommand removeCommand)
		{
			if(removeCommand == null) throw new ArgumentNullException("removeCommand");

			removeCommand.Execute();
			UpdateView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Edit(IAgentOvertimeAvailabilityEditCommand editCommand)
		{
			if(editCommand == null) throw new ArgumentNullException("editCommand");

			editCommand.Execute();
			UpdateView();
		}

		public void UpdateView()
		{
			TimeSpan? startTime = null;
			TimeSpan? endTime = null;

			var overtimeAvailability = _scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
			if (overtimeAvailability != null)
			{
				startTime = overtimeAvailability.StartTime;
				endTime = overtimeAvailability.EndTime;
			}
			_view.Update(startTime, endTime);
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

using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public enum AgentStudentAvailabilityExecuteCommand
	{
		Add,
		Edit,
		Remove,
		None
	}

	public interface IAgentStudentAvailabilityPresenter
	{
		void Add(IAgentStudentAvailabilityAddCommand addCommand);
		void Edit(IAgentStudentAvailabilityEditCommand editCommand);
		void Remove(IAgentStudentAvailabilityRemoveCommand removeCommand);
		void UpdateView();
		IScheduleDay ScheduleDay { get; }
		AgentStudentAvailabilityExecuteCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IAgentStudentAvailabilityDayCreator dayCreator);
	}

	public class AgentStudentAvailabilityPresenter : IAgentStudentAvailabilityPresenter
	{
		private readonly IAgentStudentAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
		
		public AgentStudentAvailabilityPresenter(IAgentStudentAvailabilityView view, IScheduleDay scheduleDay)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		}

		public IAgentStudentAvailabilityView View
		{
			get { return _view; }
		}

		public IScheduleDay ScheduleDay
		{
			get { return _scheduleDay; }
		}

		public void Add(IAgentStudentAvailabilityAddCommand addCommand)
		{
			if(addCommand == null) throw new ArgumentNullException("addCommand");

			addCommand.Execute();
			UpdateView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Remove(IAgentStudentAvailabilityRemoveCommand removeCommand)
		{
			if(removeCommand == null) throw new ArgumentNullException("removeCommand");

			removeCommand.Execute();
			UpdateView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Edit(IAgentStudentAvailabilityEditCommand editCommand)
		{
			if(editCommand == null) throw new ArgumentNullException("editCommand");

			editCommand.Execute();
			UpdateView();
		}

		public void UpdateView()
		{
			TimeSpan? startTime = null;
			TimeSpan? endTime = null;

			var availiabilityRestriction = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().Select(studentAvailabilityDay => studentAvailabilityDay.RestrictionCollection.FirstOrDefault()).FirstOrDefault(restriction => restriction != null);
			if (availiabilityRestriction != null)
			{
				startTime = availiabilityRestriction.StartTimeLimitation.StartTime;
				endTime = availiabilityRestriction.EndTimeLimitation.EndTime;	
			}

			_view.Update(startTime, endTime);
		}

		public AgentStudentAvailabilityExecuteCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IAgentStudentAvailabilityDayCreator dayCreator)
		{
			if(dayCreator == null) throw new ArgumentNullException("dayCreator");

			var studentAvailabilityday = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().FirstOrDefault();
			bool startError;
			bool endError;
			var canCreate = dayCreator.CanCreate(startTime, endTime, out startError, out endError);
			
			if (studentAvailabilityday != null && !canCreate && startError && endError)
				return AgentStudentAvailabilityExecuteCommand.Remove;

			if (studentAvailabilityday == null && canCreate)
				return AgentStudentAvailabilityExecuteCommand.Add;

			if (studentAvailabilityday != null && canCreate)
				return AgentStudentAvailabilityExecuteCommand.Edit;

			return AgentStudentAvailabilityExecuteCommand.None;
		}
	}
}

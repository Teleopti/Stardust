using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityPresenter
	{
		void Add(IAgentStudentAvailabilityAddCommand addCommand);
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
			bool endNextDay = false;

			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				var studentAvailabilityDay = persistableScheduleData as IStudentAvailabilityDay;
				if (studentAvailabilityDay != null)
				{
					var restriction = studentAvailabilityDay.RestrictionCollection.FirstOrDefault();
					if (restriction != null)
					{
						startTime = restriction.StartTimeLimitation.StartTime;
						endTime = restriction.EndTimeLimitation.StartTime;
						if (startTime.HasValue && endTime.HasValue && startTime.Value > endTime.Value)
							endNextDay = true;
					}
				}
			}

			_view.Update(startTime, endTime, endNextDay);
		}	
	}
}

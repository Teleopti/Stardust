using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentStudentAvailabilityPresenter
	{
		private readonly IAgentStudentAvailabilityView _view;
		private readonly IScheduleDay _scheduleDay;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public AgentStudentAvailabilityPresenter(IAgentStudentAvailabilityView view, IScheduleDay scheduleDay, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public IAgentStudentAvailabilityView View
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
			UpdateView();
		}

		public void UpdateView()
		{
			TimeSpan? startTime = null;
			TimeSpan? endTime = null;

			var availiabilityRestriction = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().SelectMany(studentAvailabilityDay => studentAvailabilityDay.RestrictionCollection).FirstOrDefault(restriction => restriction != null);
			if (availiabilityRestriction != null)
			{
				startTime = availiabilityRestriction.StartTimeLimitation.StartTime;
				endTime = availiabilityRestriction.EndTimeLimitation.EndTime;	
			}

			_view.Update(startTime, endTime);
		}

		public IExecutableCommand CommandToExecute(TimeSpan? startTime, TimeSpan? endTime, IAgentStudentAvailabilityDayCreator dayCreator)
		{
			if(dayCreator == null) throw new ArgumentNullException("dayCreator");

			var studentAvailabilityday = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().FirstOrDefault();
			bool startError;
			bool endError;
			var canCreate = dayCreator.CanCreate(startTime, endTime, out startError, out endError);
			
			if (studentAvailabilityday != null && !canCreate && startError && endError)
				return new AgentStudentAvailabilityRemoveCommand(_scheduleDay,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			if (studentAvailabilityday == null && canCreate)
				return new AgentStudentAvailabilityAddCommand(_scheduleDay,startTime,endTime,dayCreator,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			if (studentAvailabilityday != null && canCreate)
				return new AgentStudentAvailabilityEditCommand(_scheduleDay,startTime,endTime,dayCreator,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			return null;
		}
	}
}

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

		private static string timeOfDayFromTimeSpan(TimeSpan? timeSpan)
		{
			if (timeSpan != null)
			{
				int days = timeSpan.Value.Days;
				string nextDay = "";
				if (days > 0)
					nextDay = " +" + days;

				DateTime d = DateTime.MinValue.Add(timeSpan.Value);
				string ret = d.ToString("t", TeleoptiPrincipal.Current.Regional.Culture);
				ret += nextDay;
				return ret;
			}
			return string.Empty;
		}

		public void UpdateView()
		{
			TimeSpan? startTime = null;
			TimeSpan? endTime = null;
			if (_existingShiftTimePeriod != null)
			{
				var overtimeAvailabilities = _scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().ToList();
				if (overtimeAvailabilities.Count == 2)
				{
					startTime = overtimeAvailabilities[0].StartTime;
					endTime = overtimeAvailabilities[1].EndTime;
				}
				else
				{
					startTime = _existingShiftTimePeriod.Value.StartTime;
					endTime = _existingShiftTimePeriod.Value.EndTime;

					if (overtimeAvailabilities.Count == 1)
					{
						var overtimeAvailability = overtimeAvailabilities.First();
						_view.ShowPreviousSavedOvertimeAvailability(timeOfDayFromTimeSpan(overtimeAvailability.StartTime) + " - " +
						                                            timeOfDayFromTimeSpan(overtimeAvailability.EndTime));
					}
				}
			}
			else
			{
				var overtimeAvailability =
					_scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
				if (overtimeAvailability != null)
				{
					startTime = overtimeAvailability.StartTime;
					endTime = overtimeAvailability.EndTime;
				}
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

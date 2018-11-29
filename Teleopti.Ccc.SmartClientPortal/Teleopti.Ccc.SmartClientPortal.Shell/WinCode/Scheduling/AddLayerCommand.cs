using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public abstract class AddLayerCommand : IExecutableCommand
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly IScheduleViewBase _scheduleViewBase;
        private readonly ISchedulePresenterBase _presenter;
        private readonly IList<IScheduleDay> _scheduleParts;
        
        protected AddLayerCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, ISchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
        {
            _schedulerStateHolder = schedulerStateHolder;
            _scheduleViewBase = scheduleViewBase;
            _presenter = presenter;
            _scheduleParts = scheduleParts;
        }

        public IList<IScheduleDay> ScheduleParts
        {
            get { return _scheduleParts; }
        }

        public ISchedulePresenterBase Presenter
        {
            get { return _presenter; }
        }

		public IList<IScheduleDay> SchedulePartsOnePerAgent()
		{
			var persons = new List<IPerson>();
			var filteredList = new List<IScheduleDay>();

			foreach (var day in _scheduleParts)
			{
				if (day == null || persons.Contains(day.Person)) continue;
				filteredList.Add(day);
				persons.Add(day.Person);
			}

			return filteredList;
		}

        public IScheduleViewBase ScheduleViewBase
        {
            get { return _scheduleViewBase; }
        }

        protected ISchedulerStateHolder SchedulerStateHolder
        {
            get { return _schedulerStateHolder; }
        }

        public DateTimePeriod? DefaultPeriod { get; set; }

        public abstract void Execute();

        protected bool VerifySelectedSchedule(IList<IScheduleDay> schedules)
        {
            if (schedules.Count == 0 || schedules[0] == null)
            {
                _scheduleViewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);
                return false;
            }
            return true;
        }
    }
}
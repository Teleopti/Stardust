using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public interface IMeetingStateHolderLoaderHelper : IDisposable
    {
        void ReloadResultIfNeeded(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> persons);
        void CancelEveryReload();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event FinishedEventHandler FinishedReloading;
    }

    public delegate void FinishedEventHandler(object sender, ReloadEventArgs reloadEventArgs);
    
    public class ReloadEventArgs
    {
        public ReloadEventArgs(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> persons)
        {
            Scenario = scenario;
            Period = period;
            Persons = persons;
        }
        public ReloadEventArgs(){}
        public IScenario Scenario { get; private set; }
        public DateTimePeriod Period { get; private set; }
        public IEnumerable<IPerson> Persons { get; private set; }
        public HashSet<ISkill> TempSkills { get; set; }
        public bool HasReloaded { get; set; }
    }

    public class MeetingStateHolderLoaderHelper : IMeetingStateHolderLoaderHelper
    {
        private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
	    private readonly ISchedulerStateHolder _schedulerStateHolder;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly ISchedulerStateLoader _schedulerStateLoader;
        private HashSet<ISkill> _filteredSkills = new HashSet<ISkill>();
        private DateTimePeriod _lastPeriod;
        private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly BackgroundWorker _reloadBackgroundWorker = new BackgroundWorker();
        private bool _disposed;

        public event FinishedEventHandler FinishedReloading;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public MeetingStateHolderLoaderHelper(
                                                IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
                                                ISchedulerStateHolder schedulerStateHolder,
                                                ISchedulerStateLoader schedulerStateLoader,
                                                IUnitOfWorkFactory uowFactory,
												IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade,
												ITimeZoneGuard timeZoneGuard)
        {
            _peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
	        _schedulerStateHolder = schedulerStateHolder;
			_schedulingResultStateHolder = _schedulerStateHolder.SchedulingResultState;
            _schedulerStateLoader = schedulerStateLoader;
            _uowFactory = uowFactory;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			_timeZoneGuard = timeZoneGuard;
			_reloadBackgroundWorker.WorkerSupportsCancellation = true;
            _reloadBackgroundWorker.DoWork += ReloadBackgroundWorkerDoWork;
            _reloadBackgroundWorker.RunWorkerCompleted += ReloadBackgroundWorkerRunWorkerCompleted;
        }

        public void ReloadResultIfNeeded(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> persons)
        {
            if(_reloadBackgroundWorker.IsBusy)
                return;
            var args = new ReloadEventArgs(scenario, period, persons);
            _reloadBackgroundWorker.RunWorkerAsync(args);
       }

        void ReloadBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			var args = (ReloadEventArgs) e.Argument;
			ILoaderDeciderResult deciderResult;
			using (_uowFactory.CreateAndOpenUnitOfWork())
			{
				deciderResult = _peopleAndSkillLoaderDecider.Execute(args.Scenario, args.Period, args.Persons);
			}
			_schedulerStateLoader.EnsureSkillsLoaded(args.Period.ToDateOnlyPeriod(_timeZoneGuard.CurrentTimeZone()));     
			var tempSkills = new HashSet<ISkill>(_schedulingResultStateHolder.Skills);
			deciderResult.FilterSkills(tempSkills.ToArray(),s => tempSkills.Remove(s),s => tempSkills.Add(s));
			if (_schedulingResultStateHolder.SkillDays != null && tempSkills.SetEquals(_filteredSkills) && _lastPeriod.Contains(args.Period))
			{
				e.Cancel = true;
				e.Result = args;
				return;
			}
			args.TempSkills = tempSkills;
			e.Result = args;
			var tempPersons = new List<IPerson>(_schedulingResultStateHolder.LoadedAgents);
			deciderResult.FilterPeople(tempPersons);
			var scheduleDateTimePeriod = new ScheduleDateTimePeriod(args.Period, tempPersons, new MeetingScheduleRangeToLoadCalculator(args.Period));
			_schedulerStateLoader.LoadSchedulingResultAsync(scheduleDateTimePeriod, _reloadBackgroundWorker, tempSkills, _staffingCalculatorServiceFacade);
		}
        
        void ReloadBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReloadEventArgs args;
            if(!e.Cancelled)
            {
                args = (ReloadEventArgs) e.Result;
                _filteredSkills = args.TempSkills;
                _lastPeriod = args.Period;
                args.HasReloaded = true;
            }
            else
            {
                args = new ReloadEventArgs();
            }
            if(FinishedReloading != null)
                FinishedReloading(this, args);
        }
        
        public void CancelEveryReload()
        {
            if(_reloadBackgroundWorker.IsBusy)
                _reloadBackgroundWorker.CancelAsync();
        }

        

        #region IDispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ReleaseManagedResources();
                }
                ReleaseUnmanagedResources();
                _disposed = true;
            }
        }

        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void ReleaseManagedResources()
        {
            if(_reloadBackgroundWorker != null)
            _reloadBackgroundWorker.Dispose();
        }

        #endregion
    }
}
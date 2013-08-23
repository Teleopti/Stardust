using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizerService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		void Execute(IList<IScheduleMatrixPro> allMatrixes, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary);
        void OnReportProgress(string message);
    }

    public  class GroupMoveTimeOptimizerService : IGroupMoveTimeOptimizerService
    {
        private readonly IList<IGroupMoveTimeOptimizer> _optimizers;
        private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
        private readonly IGroupMoveTimeOptimizationExecuter _groupMoveTimeOptimizerExecuter;
        private readonly IGroupMoveTimeValidatorRunner _groupMoveTimeValidatorRunner;
        private bool _cancelOptimization;

        public GroupMoveTimeOptimizerService(IList<IGroupMoveTimeOptimizer> optimizers,
            IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup,
            IGroupMoveTimeOptimizationExecuter groupMoveTimeOptimizerExecuter,
            IGroupMoveTimeValidatorRunner groupMoveTimeValidatorRunner)
        {
            _optimizers = optimizers;
            _groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
            _groupMoveTimeOptimizerExecuter = groupMoveTimeOptimizerExecuter;
            _groupMoveTimeValidatorRunner = groupMoveTimeValidatorRunner;
        }

		public void Execute(IList<IScheduleMatrixPro> allMatrixes, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary)
        {
            var pendingOptimizers = new List<IGroupMoveTimeOptimizer>(_optimizers);
            while (pendingOptimizers.Count > 0)
            {
                if (_cancelOptimization)
                    return;
                var removedOptimizers = runOptimizers(pendingOptimizers, teamSteadyStateMainShiftScheduler, teamSteadyStateHolder, scheduleDictionary);
                    
                foreach (var optimizer in removedOptimizers)
                {
                    pendingOptimizers.Remove(optimizer);
                }
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private IEnumerable<IGroupMoveTimeOptimizer> runOptimizers(ICollection<IGroupMoveTimeOptimizer> pendingOptimizers, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary)
        {
            var skippedOptimizers = new List<IGroupMoveTimeOptimizer>();
            var removedOptimizers = new List<IGroupMoveTimeOptimizer>();
            var executes = 0;
            foreach (var optimizer in pendingOptimizers.GetRandom(pendingOptimizers.Count, true))
            {
                var person = optimizer.Person;
                executes++;
                if (_cancelOptimization)
                    break;
                if (skippedOptimizers.Contains(optimizer))
                    continue;

                var decidedDays = optimizer.Execute();
                if (decidedDays.Count != 2)
                {
                    skippedOptimizers.Add(optimizer);
                    removedOptimizers.Add(optimizer);
                    continue;
                }

                var dayToBeLengthen = decidedDays[0];
                var dayToBeShorten = decidedDays[1];

                var matrixes = _groupOptimizerFindMatrixesForGroup.Find(person, dayToBeLengthen).Intersect(_groupOptimizerFindMatrixesForGroup.Find(person, dayToBeShorten)).ToList();
                var runnableOptimizers = (from matrix in matrixes
                                                             from groupMoveTimeOptimizer in pendingOptimizers
                                                             where
                                                                 groupMoveTimeOptimizer.IsMatrixForDateAndPerson(
                                                                     dayToBeLengthen, matrix.Person) &&
                                                                 groupMoveTimeOptimizer.IsMatrixForDateAndPerson(
                                                                     dayToBeShorten, matrix.Person)
                                                             select groupMoveTimeOptimizer).ToList();

                var daysToSave = new List<KeyValuePair<DayReadyToMove, IScheduleDay>>();
                var daysToDelete = new List<IScheduleDay>();
                prepareDaysForRescheduling(daysToDelete, daysToSave, runnableOptimizers, dayToBeLengthen, dayToBeShorten);
                var validateResult = _groupMoveTimeValidatorRunner.Run(person, daysToDelete.Distinct().Select(d => d.DateOnlyAsPeriod.DateOnly).ToList(),
                                                      _groupMoveTimeOptimizerExecuter.SchedulingOptions.UseSameDayOffs, matrixes);
                if (!validateResult.Success || validateResult.DaysToLock.HasValue) continue; 
                
                var oldPeriodValue = optimizer.PeriodValue();
                var success = _groupMoveTimeOptimizerExecuter.Execute(daysToDelete, daysToSave, matrixes, optimizer.OptimizationOverLimitByRestrictionDecider, teamSteadyStateMainShiftScheduler, teamSteadyStateHolder, scheduleDictionary);
                if(success)
                {
                    var newPeriodValue = optimizer.PeriodValue();
                    var isPeriodWorse = newPeriodValue > oldPeriodValue;
                    if (isPeriodWorse)
                    {
                    	_groupMoveTimeOptimizerExecuter.Rollback(scheduleDictionary);
						_groupMoveTimeOptimizerExecuter.Rollback(scheduleDictionary);
						//_groupMoveTimeOptimizerExecuter.Rollback(dayToBeLengthen);
						//_groupMoveTimeOptimizerExecuter.Rollback(dayToBeShorten);
                    }
                }
                reportProgress(dayToBeLengthen,dayToBeShorten, success, pendingOptimizers.Count, executes, person);
            }
            return removedOptimizers;
        }

        private static void prepareDaysForRescheduling(ICollection<IScheduleDay> daysToDelete, ICollection<KeyValuePair<DayReadyToMove, IScheduleDay>> daysToSave,
                                        IEnumerable<IGroupMoveTimeOptimizer> runnableOptimizers, DateOnly dayToBeLengthen, DateOnly dayToBeShorten)
        {
            foreach (var optimizer in runnableOptimizers)
            {
                var scheduleDayToBeLengthen = optimizer.Matrix.GetScheduleDayByKey(dayToBeLengthen).DaySchedulePart();
                daysToDelete.Add(scheduleDayToBeLengthen);
                daysToSave.Add(new KeyValuePair<DayReadyToMove, IScheduleDay>(DayReadyToMove.FirstDay,
                                                                           (IScheduleDay)scheduleDayToBeLengthen.Clone()));

                var scheduleDayToBeShorten = optimizer.Matrix.GetScheduleDayByKey(dayToBeShorten).DaySchedulePart();
                daysToDelete.Add(scheduleDayToBeShorten);
                optimizer.LockDate(dayToBeShorten);
                optimizer.LockDate(dayToBeLengthen );
                daysToSave.Add(new KeyValuePair<DayReadyToMove, IScheduleDay>(DayReadyToMove.SecondDay,
                                                                          (IScheduleDay)scheduleDayToBeShorten.Clone()));
                
            }
        }
        
        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void OnReportProgress(string message)
        {
            var handler = ReportProgress;
            if (handler == null) return;
            var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
            handler(this, args);
            if (args.Cancel)
                _cancelOptimization = true;
        }

        private void reportProgress(DateOnly dayToBeLengthen, DateOnly dayToBeShorten, bool result, int activeOptimizers, int executes, IPerson owner)
        {
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
            var lengthenDay = dayToBeLengthen.ToShortDateString(culture);
            var shortenDay = dayToBeShorten.ToShortDateString(culture);
            var who = Resources.OptimizingShiftLengths  + Resources.Colon + "(" + activeOptimizers + ")" + executes + " " +
                         lengthenDay + " : " + shortenDay + " " + owner.Name.ToString(NameOrderOption.FirstNameLastName);
            var success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
            var progreeString = who + success;
            OnReportProgress(progreeString);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizerService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
        void Execute(IList<IScheduleMatrixPro> allMatrixes);
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

        public void Execute(IList<IScheduleMatrixPro> allMatrixes)
        {
            var pendingOptimizers = new List<IGroupMoveTimeOptimizer>(_optimizers);
            while (pendingOptimizers.Count > 0)
            {
                var removedOptimizers = runOptimizers(pendingOptimizers, allMatrixes);
                foreach (var optimizer in removedOptimizers)
                {
                    pendingOptimizers.Remove(optimizer);
                }
            }
        }
        
        private IEnumerable<IGroupMoveTimeOptimizer> runOptimizers(ICollection<IGroupMoveTimeOptimizer> pendingOptimizers,
                                                          IList<IScheduleMatrixPro> allMatrixes)
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

                var matrixes = _groupOptimizerFindMatrixesForGroup.Find(person, dayToBeLengthen).Intersect(_groupOptimizerFindMatrixesForGroup.Find(person, dayToBeShorten));
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
                var validateResult = daysToDelete.All(
                    d => _groupMoveTimeValidatorRunner.Run(person, new List<DateOnly> {d.DateOnlyAsPeriod.DateOnly},
                                                           new List<DateOnly> {d.DateOnlyAsPeriod.DateOnly},
                                                           _groupMoveTimeOptimizerExecuter.SchedulingOptions.
                                                               UseSameDayOffs).Success);
                if (!validateResult) continue;

                var oldPeriodValue = optimizer.PeriodValue();
                var success = _groupMoveTimeOptimizerExecuter.Execute(daysToDelete, daysToSave, allMatrixes, optimizer.OptimizationOverLimitByRestrictionDecider);
                if(success)
                {
                    var newPeriodValue = optimizer.PeriodValue();
                    var isPeriodWorse = newPeriodValue > oldPeriodValue;
                    if (isPeriodWorse)
                    {
                        _groupMoveTimeOptimizerExecuter.Rollback(dayToBeLengthen);
                        _groupMoveTimeOptimizerExecuter.Rollback(dayToBeShorten);
                    }
                }
                else
                {
                    removedOptimizers.AddRange(runnableOptimizers);
                }
                skippedOptimizers.AddRange(runnableOptimizers);
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
                daysToSave.Add(new KeyValuePair<DayReadyToMove, IScheduleDay>(DayReadyToMove.SecondDay,
                                                                           (IScheduleDay)scheduleDayToBeShorten.Clone()));
            }
        }
        
        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void OnReportProgress(string message)
        {
            var handler = ReportProgress;
            if (handler == null) return;
            var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
            handler(this, args);
            if (args.Cancel)
                _cancelOptimization = true;
        }

        private void reportProgress(DateOnly dayToBeLengthen, DateOnly dayToBeShorten, bool result, int activeOptimizers, int executes, IPerson owner)
        {
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
            var lengthenDay = dayToBeLengthen.ToShortDateString(culture);
            var shortenDay = dayToBeShorten.ToShortDateString(culture);
            var who = Resources.OptimizingDaysOff + Resources.Colon + "(" + activeOptimizers + ")" + executes + " " +
                         lengthenDay + " : " + shortenDay + " " + owner.Name.ToString(NameOrderOption.FirstNameLastName);
            var success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
            var progreeString = who + success;
            OnReportProgress(progreeString);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public  class GroupMoveTimeOptimizerService : IGroupMoveTimeOptimizerService
    {
        private readonly IList<IGroupMoveTimeOptimizer> _optimizers;
        private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
        private readonly IGroupMoveTimeOptimizationExecuter _groupMoveTimeOptimizerExecuter;
        private readonly IGroupOptimizationValidatorRunner _groupMoveTimeValidatorRunner;
        private bool _cancelMe;

        public GroupMoveTimeOptimizerService(IList<IGroupMoveTimeOptimizer> optimizers, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup, IGroupMoveTimeOptimizationExecuter groupMoveTimeOptimizerExecuter,
             IGroupOptimizationValidatorRunner groupMoveTimeValidatorRunner)
        {
            _optimizers = optimizers;
            _groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
            _groupMoveTimeOptimizerExecuter = groupMoveTimeOptimizerExecuter;
            _groupMoveTimeValidatorRunner = groupMoveTimeValidatorRunner;
        }

        public void Execute(IList<IScheduleMatrixPro> allMatrixes)
        {
            IList<IGroupMoveTimeOptimizer> runningList = new List<IGroupMoveTimeOptimizer>(_optimizers);

            while (runningList.Count > 0)
            {
                var removeList = runTheList(runningList, allMatrixes);
                foreach (var groupMoveTimeOptimizer in removeList)
                {
                    runningList.Remove(groupMoveTimeOptimizer);
                }
            }
        }
        
        private IEnumerable<IGroupMoveTimeOptimizer> runTheList(ICollection<IGroupMoveTimeOptimizer> runningList,
                                                          IList<IScheduleMatrixPro> allMatrixes)
        {
            var skipList = new List<IGroupMoveTimeOptimizer>();
            var removeList = new List<IGroupMoveTimeOptimizer>();
            var executes = 0;
            foreach (var optimizer in runningList.GetRandom(runningList.Count, true))
            {
                var person = optimizer.Person;
                executes++;
                if (_cancelMe)
                    break;

                if (skipList.Contains(optimizer))
                    continue;

                IList<IGroupMoveTimeOptimizer> memberList = new List<IGroupMoveTimeOptimizer>();
                var selectedDate = optimizer.Execute();

                if (selectedDate.Count  != 2)
                {
                    skipList.Add(optimizer);
                    removeList.Add(optimizer);
                    continue;
                }

                var firstDate = selectedDate[0];
                var secDate = selectedDate[1];

                var matrixes = _groupOptimizerFindMatrixesForGroup.Find(person, firstDate).Intersect(_groupOptimizerFindMatrixesForGroup.Find(person, secDate));
                foreach (var matrix in matrixes)
                {
                    foreach (var  groupMoveTimeOptimizer in runningList)
                    {
                        if (groupMoveTimeOptimizer.IsMatrixForDateAndPerson(firstDate, matrix.Person) && groupMoveTimeOptimizer.IsMatrixForDateAndPerson(secDate, matrix.Person))
                            memberList.Add( groupMoveTimeOptimizer);
                    }
                }

                var daysToSave = new List<IScheduleDay>();
                var daysToDelete = new List<IScheduleDay>();
                processScheduleDay(daysToDelete, daysToSave, memberList, firstDate, false, person);
                processScheduleDay(daysToDelete, daysToSave, memberList, secDate, true, person);

                var validateResult = daysToSave.All(d=> _groupMoveTimeValidatorRunner.Run(person, new List<DateOnly> { d.DateOnlyAsPeriod.DateOnly  },
                                                                       new List<DateOnly> { d.DateOnlyAsPeriod.DateOnly  }, true).Success );
                if (!validateResult) continue;
                var success = _groupMoveTimeOptimizerExecuter.Execute(daysToDelete, daysToSave, allMatrixes, optimizer.OptimizationOverLimitByRestrictionDecider);

                if (!success)
                {
                    removeList.AddRange(memberList);
                }

                skipList.AddRange(memberList);
                reportProgress(firstDate,secDate, success, runningList.Count, executes, person);
            }
            return removeList;
        }

        private static void processScheduleDay(ICollection<IScheduleDay> daysToDelete, ICollection<IScheduleDay> daysToSave,
                                        IEnumerable<IGroupMoveTimeOptimizer> memberList, DateOnly selectedDate, bool lockDay, IPerson person)
        {
            foreach (var groupMoveTimeOptimizer in memberList)
            {
                var scheduleDay = groupMoveTimeOptimizer.Matrix.GetScheduleDayByKey(selectedDate).DaySchedulePart();
                daysToDelete.Add(scheduleDay);
                if(scheduleDay.Person == person )
                {
                    daysToSave.Add((IScheduleDay)scheduleDay.Clone());
                }
                if (lockDay)
                    groupMoveTimeOptimizer.LockDate(selectedDate);
            }
        }
        
        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void OnReportProgress(string message)
        {
            var handler = ReportProgress;
            if (handler != null)
            {
                var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.GroupMoveTimeOptimizerService.OnReportProgress(System.String)")]
        private void reportProgress(DateOnly firstDate, DateOnly secondDate, bool result, int activeOptimizers, int executes, IPerson owner)
        {
            var firstDateStr = firstDate.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture);
            var secondDateStr = secondDate .ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture);
            var who = Resources.OptimizingDaysOff + Resources.Colon + "(" + activeOptimizers + ")" + executes + " " +
                         firstDateStr + ":" + secondDateStr + " " + owner.Name.ToString(NameOrderOption.FirstNameLastName);
            string success;
            if (!result)
            {
                success = " " + Resources.wasNotSuccessful;
            }
            else
            {
                success = " " + Resources.wasSuccessful;
            }
            var progreeString = who + success;
            OnReportProgress(progreeString);
        }
    }

    public interface IGroupMoveTimeOptimizerService
    {
        void Execute(IList<IScheduleMatrixPro> allMatrixes);
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
        void OnReportProgress(string message);
    }
}
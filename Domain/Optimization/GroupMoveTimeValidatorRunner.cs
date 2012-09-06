using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class GroupMoveTimeValidatorRunner : IGroupOptimizationValidatorRunner
    {
        private readonly IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
        private readonly IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;

        public GroupMoveTimeValidatorRunner(IGroupOptimizerValidateProposedDatesInSameMatrix groupOptimizerValidateProposedDatesInSameMatrix, IGroupOptimizerValidateProposedDatesInSameGroup groupOptimizerValidateProposedDatesInSameGroup)
        {
            _groupOptimizerValidateProposedDatesInSameMatrix = groupOptimizerValidateProposedDatesInSameMatrix;
            _groupOptimizerValidateProposedDatesInSameGroup = groupOptimizerValidateProposedDatesInSameGroup;
        }

        public delegate ValidatorResult ValidateMoveTimeDelegate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);

        public ValidatorResult Run(IPerson person, IList<DateOnly> daysMoveTimeFrom, IList<DateOnly> daysMoveTimeTo, bool moveSameDay)
        {
            var runnableList = new Dictionary<ValidateMoveTimeDelegate, IAsyncResult>();

            IList<DateOnly> allDays = new List<DateOnly>(daysMoveTimeFrom);
            foreach (var dateOnly in daysMoveTimeTo)
            {
                allDays.Add(dateOnly);
            }
				
            ValidateMoveTimeDelegate toRun = _groupOptimizerValidateProposedDatesInSameMatrix.Validate;
            var result = toRun.BeginInvoke(person, allDays, moveSameDay, null, null);
            runnableList.Add(toRun, result);

            toRun = _groupOptimizerValidateProposedDatesInSameGroup.Validate;
            result = toRun.BeginInvoke(person, allDays, moveSameDay, null, null);
            runnableList.Add(toRun, result);

            //Sync all threads
            IList<ValidatorResult> results = new List<ValidatorResult>();
            try
            {
                foreach (var thread in runnableList)
                {
                    results.Add(thread.Key.EndInvoke(thread.Value));
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw;
            }

            var myResult = new ValidatorResult();
            myResult.Success = true;
            foreach (var validatorResult in results)
            {
                if (!validatorResult.Success)
                    return new ValidatorResult();
                if (validatorResult.DaysToLock.HasValue)
                {
                    foreach (var matrixPro in validatorResult.MatrixList)
                    {
                        myResult.MatrixList.Add(matrixPro);
                        if (matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(validatorResult.DaysToLock.Value))
                            matrixPro.LockPeriod(validatorResult.DaysToLock.Value);
                    }
                }
            }

            return myResult;
        }
        
    }
}
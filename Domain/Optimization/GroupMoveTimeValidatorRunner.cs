using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class GroupMoveTimeValidatorRunner : IGroupMoveTimeValidatorRunner
    {
        private readonly IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
        private readonly IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;

        public GroupMoveTimeValidatorRunner(IGroupOptimizerValidateProposedDatesInSameMatrix groupOptimizerValidateProposedDatesInSameMatrix,
            IGroupOptimizerValidateProposedDatesInSameGroup groupOptimizerValidateProposedDatesInSameGroup)
        {
            _groupOptimizerValidateProposedDatesInSameMatrix = groupOptimizerValidateProposedDatesInSameMatrix;
            _groupOptimizerValidateProposedDatesInSameGroup = groupOptimizerValidateProposedDatesInSameGroup;
        }

        private delegate ValidatorResult ValidateMoveTimeDelegate(IPerson person, IList<DateOnly> dates, bool useSameDayOff);

        public ValidatorResult Run(IPerson person, IList<DateOnly> daysToBeLengthen, IList<DateOnly> daysToBeShorten, bool useSameDayOff)
        {
            var runnableList = new Dictionary<ValidateMoveTimeDelegate, IAsyncResult>();
            var allDays = new List<DateOnly>();
            if (daysToBeLengthen != null)
                allDays.AddRange(daysToBeLengthen);
            if (daysToBeShorten != null)
                allDays.AddRange(daysToBeShorten);
            
            ValidateMoveTimeDelegate toRun = _groupOptimizerValidateProposedDatesInSameMatrix.Validate;
            var result = toRun.BeginInvoke(person, allDays, useSameDayOff, null, null);
            runnableList.Add(toRun, result);

            toRun = _groupOptimizerValidateProposedDatesInSameGroup.Validate;
            result = toRun.BeginInvoke(person, allDays, useSameDayOff, null, null);
            runnableList.Add(toRun, result);

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

            var myResult = new ValidatorResult {Success = true};
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

    public interface IGroupMoveTimeValidatorRunner
    {
        ValidatorResult Run(IPerson person, IList<DateOnly> daysToBeLengthen, IList<DateOnly> daysToBeShorten, bool useSameDayOff);
    }
}
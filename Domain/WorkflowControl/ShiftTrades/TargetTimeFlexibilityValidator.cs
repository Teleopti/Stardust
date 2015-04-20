using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    //This does what???
    //Only used within the ShiftTradeTargetTimeValidator? 
    public interface ITargetTimeFlexibilityValidator
    {
        bool Validate(IList<IScheduleDay> suggestedChanges, TimeSpan flexibility);
    }

    public class TargetTimeFlexibilityValidator : ITargetTimeFlexibilityValidator
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly ISchedulePeriodTargetTimeCalculator _periodTargetTimeTimeCalculator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TargetTimeFlexibilityValidator));

        public TargetTimeFlexibilityValidator(IScheduleMatrixPro matrix, ISchedulePeriodTargetTimeCalculator periodTargetTimeTimeCalculator)
        {
            _matrix = matrix;
            _periodTargetTimeTimeCalculator = periodTargetTimeTimeCalculator;
        }

        public bool Validate(IList<IScheduleDay> suggestedChanges, TimeSpan flexibility)
        {
            TimeSpan totalTime = TimeSpan.Zero;
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                bool found = false;
                foreach (IScheduleDay schedulePart in suggestedChanges)
                {
                    if(schedulePart.Person.Equals(_matrix.Person) && schedulePart.DateOnlyAsPeriod.DateOnly == scheduleDayPro.DaySchedulePart().DateOnlyAsPeriod.DateOnly)
                    {
                        var contractTime = schedulePart.ProjectionService().CreateProjection().ContractTime();
                        Logger.DebugFormat("Found a schedule day that was a suggested change. Time: {0}.", contractTime);
                        totalTime = totalTime.Add(contractTime);
                        found = true;
                        break;
                    }
                }
                if(!found)
                    totalTime = totalTime.Add(scheduleDayPro.DaySchedulePart().ProjectionService().CreateProjection().ContractTime());
            }

            Logger.DebugFormat("The calculated total contract time is {0}.",totalTime);
            var targetTime = _periodTargetTimeTimeCalculator.TargetWithTolerance(_matrix);
            Logger.DebugFormat("The target start time is {0}",targetTime.StartTime);
            Logger.DebugFormat("The target end time is {0}", targetTime.EndTime);
            if (totalTime < targetTime.StartTime.Subtract(flexibility))
                return false;

            if (totalTime > targetTime.EndTime.Add(flexibility))
                return false;

            return true;
        }
    }
}

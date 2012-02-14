using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public interface IWorkShiftCalculatorsManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IList<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
                                                                                IList<IShiftProjectionCache> shiftProjectionCaches,
                                                                                IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
                                                                                IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods);
    }

    public class WorkShiftCalculatorsManager : IWorkShiftCalculatorsManager
    {
        private readonly IWorkShiftCalculator _workShiftCalculator;
        private readonly INonBlendWorkShiftCalculator _nonBlendWorkShiftCalculator;
        private readonly ISchedulingOptions _options;

        public WorkShiftCalculatorsManager(IWorkShiftCalculator workShiftCalculator, INonBlendWorkShiftCalculator nonBlendWorkShiftCalculator, ISchedulingOptions options)
        {
            _workShiftCalculator = workShiftCalculator;
            _nonBlendWorkShiftCalculator = nonBlendWorkShiftCalculator;
            _options = options;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IList<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
                IList<IShiftProjectionCache> shiftProjectionCaches,
                IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
                IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods)
        {
            IList<IWorkShiftCalculationResultHolder> allValues =
                new List<IWorkShiftCalculationResultHolder>(shiftProjectionCaches.Count);
            foreach (IShiftProjectionCache shiftProjection in shiftProjectionCaches)
            {
                double? nonBlendValue = null;
                double thisValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.MainShiftProjection,
                                                                            dataHolders, (double)_options.WorkShiftLengthHintOption,
                                                                            _options.UseMinimumPersons, _options.UseMaximumPersons);

                if (nonBlendSkillPeriods.Count > 0)
                    nonBlendValue = _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
                                                                                     shiftProjection.
                                                                                         MainShiftProjection,
                                                                                     nonBlendSkillPeriods,
                                                                                     (double)_options.WorkShiftLengthHintOption,
                                                                                     _options.UseMinimumPersons,
                                                                                     _options.UseMaximumPersons);
                if (nonBlendValue.HasValue)
                {
                    if (thisValue.Equals(double.MinValue))
                        thisValue = nonBlendValue.Value;
                    else
                    {
                        thisValue += nonBlendValue.Value;
                    }
                }

                if (thisValue > double.MinValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResultHolder { ShiftProjection = shiftProjection, Value = thisValue };
                    allValues.Add(workShiftFinderResultHolder);
                }
            }
            return allValues;
        }
    }
}
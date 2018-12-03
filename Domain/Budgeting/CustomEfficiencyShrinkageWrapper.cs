using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class CustomEfficiencyShrinkageWrapper : ICustomEfficiencyShrinkageWrapper
    {
        private readonly IBudgetGroup _budgetGroup;
        private readonly IDictionary<Guid, Percent> _customEfficiencyShrinkages;

        public CustomEfficiencyShrinkageWrapper(IBudgetGroup budgetGroup, IDictionary<Guid, Percent> customEfficiencyShrinkages)
        {
            _budgetGroup = budgetGroup;
            _customEfficiencyShrinkages = customEfficiencyShrinkages;
        }

        public void SetEfficiencyShrinkage(Guid customEfficiencyShrinkageId, Percent percent)
        {
            if (!_budgetGroup.IsCustomEfficiencyShrinkage(customEfficiencyShrinkageId))
            {
                throw new ArgumentOutOfRangeException("customEfficiencyShrinkageId", "No custom efficiency shrinkage with this id is available for this budget group.");
            }
            if (percent.Value < 0)
            {
                throw new ArgumentOutOfRangeException("percent", "The percentage must be larger than or equal to zero.");
            }
            if (_customEfficiencyShrinkages.ContainsKey(customEfficiencyShrinkageId))
            {
                _customEfficiencyShrinkages[customEfficiencyShrinkageId] = percent;
            }
            else
            {
                _customEfficiencyShrinkages.Add(customEfficiencyShrinkageId, percent);
            }
        }

        public Percent GetEfficiencyShrinkage(Guid customEfficiencyShrinkageId)
        {
            if (!_customEfficiencyShrinkages.ContainsKey(customEfficiencyShrinkageId))
            {
                _customEfficiencyShrinkages.Add(customEfficiencyShrinkageId, new Percent());
            }
            return _customEfficiencyShrinkages[customEfficiencyShrinkageId];
        }

        public Percent GetTotal()
        {
            var shrinkages = _budgetGroup.CustomEfficiencyShrinkages;
            if (shrinkages.Count() == 0)
                return new Percent(0);

            double sum = 0;
            foreach (var shrinkage in shrinkages)
            {
                if (shrinkage.Id != null) sum += GetEfficiencyShrinkage(shrinkage.Id.Value).Value;
            }

            return new Percent(sum);
        }
    }
}
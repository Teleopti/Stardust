using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class CustomShrinkageWrapper : ICustomShrinkageWrapper
    {
        private readonly IBudgetGroup _budgetGroup;
        private readonly IDictionary<Guid, Percent> _customShrinkages;

        public CustomShrinkageWrapper(IBudgetGroup budgetGroup, IDictionary<Guid, Percent> customShrinkages)
        {
            _budgetGroup = budgetGroup;
            _customShrinkages = customShrinkages;
        }

        public void SetShrinkage(Guid customShrinkageId, Percent percent)
        {
            if (!_budgetGroup.IsCustomShrinkage(customShrinkageId))
            {
                throw new ArgumentOutOfRangeException("customShrinkageId","No custom shrinkage with this id is available for this budget group.");
            }
            if (percent.Value < 0)
            {
                throw new ArgumentOutOfRangeException("percent", "The percentage must be larger than or equal to zero.");
            }
            if (_customShrinkages.ContainsKey(customShrinkageId))
            {
                _customShrinkages[customShrinkageId] = percent;
            }
            else
            {
                _customShrinkages.Add(customShrinkageId, percent);
            }
        }

        public Percent GetShrinkage(Guid customShrinkageId)
        {
            if (!_customShrinkages.ContainsKey(customShrinkageId))
            {
                _customShrinkages.Add(customShrinkageId,new Percent());
            }
            return _customShrinkages[customShrinkageId];
        }

        public Percent GetTotal()
        {
            var shrinkages = _budgetGroup.CustomShrinkages;
            if (shrinkages.Count() == 0)
                return new Percent(0);
            
            double sum=0;
            foreach (var shrinkage in shrinkages)
            {
                if (shrinkage.Id != null) sum += GetShrinkage(shrinkage.Id.Value).Value;
            }
            
            return new Percent(sum);
        }
    }
}
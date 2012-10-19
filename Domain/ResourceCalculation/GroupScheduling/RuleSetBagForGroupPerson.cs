using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class RuleSetBagForGroupPerson : Entity, IRuleSetBag
	{
		private readonly IList<IWorkShiftRuleSet> _ruleSetCollection = new List<IWorkShiftRuleSet>();
		private Description _description;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public Description Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public ReadOnlyCollection<IWorkShiftRuleSet> RuleSetCollection
		{
			get { return new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection);}
		}

		public void AddRuleSet(IWorkShiftRuleSet workShiftRuleSet)
		{
			_ruleSetCollection.Add(workShiftRuleSet);
		}

		public void RemoveRuleSet(IWorkShiftRuleSet workShiftRuleSet)
		{
			_ruleSetCollection.Remove(workShiftRuleSet);
		}

		public void ClearRuleSetCollection()
		{
			_ruleSetCollection.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public bool IsChoosable
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IShiftCategory> ShiftCategoriesInBag()
		{
			IList<IShiftCategory> categories = new List<IShiftCategory>();
			foreach (IWorkShiftRuleSet workShiftRuleSet in _ruleSetCollection)
			{
				if (!categories.Contains(workShiftRuleSet.TemplateGenerator.Category))
					categories.Add(workShiftRuleSet.TemplateGenerator.Category);
			}
			return categories;
		}

		public IWorkTimeMinMax MinMaxWorkTime(IWorkShiftWorkTime workShiftWorkTime, DateOnly onDate, IWorkTimeMinMaxRestriction restriction)
		{
			if (restriction == null)
				return null;

			if (!restriction.MayMatch())
				return null;

			IWorkTimeMinMax retVal = null;

			foreach (var workShiftRuleSet in _ruleSetCollection)
			{
				if (!workShiftRuleSet.IsValidDate(onDate))
					continue;

				if (!restriction.Match(workShiftRuleSet.TemplateGenerator.Category))
					continue;

				var ruleSetWorkTimeMinMax = workShiftWorkTime.CalculateMinMax(workShiftRuleSet, restriction);
				if (ruleSetWorkTimeMinMax != null)
				{
					if (retVal == null)
						retVal = new WorkTimeMinMax();

					retVal = retVal.Combine(ruleSetWorkTimeMinMax);
				}
			}


			return retVal;
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag NoneEntityClone()
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag EntityClone()
		{
			throw new NotImplementedException();
		}
	}
}
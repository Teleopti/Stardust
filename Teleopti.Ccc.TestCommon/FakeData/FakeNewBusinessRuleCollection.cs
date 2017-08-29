using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeNewBusinessRuleCollection : Collection<INewBusinessRule>, INewBusinessRuleCollection
	{
		private IEnumerable<IBusinessRuleResponse> _rules;

		public IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			return _rules ?? new IBusinessRuleResponse[] {};
		}

		public void SetRuleResponse(IEnumerable<IBusinessRuleResponse> rules)
		{
			_rules = rules;
		}

		public void DoNotHaltModify(IBusinessRuleResponse businessRuleResponseToOverride)
		{
			DoNotHaltModify(businessRuleResponseToOverride.TypeOfRule);
		}

		public void DoNotHaltModify(Type businessRuleType)
		{
			foreach (var bu in this)
			{
				if (businessRuleType.Equals(bu.GetType()))
				{
					if (!bu.IsMandatory)
						bu.HaltModify = false;

					return;
				}
			}
		}

		public INewBusinessRule Item(Type businessRuleType)
		{
			foreach (var bu in this)
			{
				if (businessRuleType.Equals(bu.GetType()))
				{
					return bu;
				}
			}

			return null;
		}

		public void SetUICulture(CultureInfo cultureInfo)
		{
			UICulture = cultureInfo;
		}

		public CultureInfo UICulture { get; private set; }
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeNewBusinessRuleCollection : Collection<INewBusinessRule>, INewBusinessRuleCollection
	{
		private IEnumerable<IBusinessRuleResponse> _rules;
		public IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			return _rules;
		}

		public void SetRuleResponse(IEnumerable<IBusinessRuleResponse> rules)
		{
			_rules = rules;
		}

		public void Remove(IBusinessRuleResponse businessRuleResponseToOverride)
		{
			throw new NotImplementedException();
		}

		public void Remove(Type businessRuleType)
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
			
		}

		public CultureInfo UICulture { get; }


		public IEnumerator<INewBusinessRule> GetEnumerator()
		{
			return base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(INewBusinessRule item)
		{
			base.Add(item);
			
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(INewBusinessRule item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(INewBusinessRule[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(INewBusinessRule item)
		{
			throw new NotImplementedException();
		}

		public int Count { get; }
		public bool IsReadOnly { get; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class ShiftTradeValidationFailedRule : INewBusinessRule
	{
		private readonly string _errorMessage;
		private readonly DateOnlyPeriod _dateOnlyPeriod;
		private readonly Type _specificationType;

		public bool IsMandatory => false;
		public bool HaltModify { get; set; } = true;
		public bool Configurable => false;
		public bool ForDelete { get; set; }

		public ShiftTradeValidationFailedRule(string errorMessage, DateOnlyPeriod dateOnlyPeriod, Type specificationType)
		{
			_errorMessage = errorMessage;
			_dateOnlyPeriod = dateOnlyPeriod;
			_specificationType = specificationType;
		}

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			return new[]
			{
				new BusinessRuleResponse(_specificationType,
					_errorMessage, true, true, new DateTimePeriod(), rangeClones.Keys.ToList()[0],
					_dateOnlyPeriod, string.Empty)
			};
		}

		public string Description { get; }
	}
}

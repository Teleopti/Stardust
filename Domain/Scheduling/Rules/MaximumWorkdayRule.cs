using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MaximumWorkdayRule: INewBusinessRule
	{
		public bool IsMandatory => false;
		public bool HaltModify { get; set; } = true;
		public bool Configurable => true;
		public bool ForDelete { get; set; }
		public string Description => Resources.DescriptionOfMaximumWorkdayRule;


		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			throw new NotImplementedException();
		}
	}
}

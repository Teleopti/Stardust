using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MinWeekWorkTimeRuleToggle31992Off : INewBusinessRule 
	{
		public bool HaltModify { get; set; }
		public bool ForDelete { get; set; }
		
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			return new HashSet<IBusinessRuleResponse>();
		}

		public string ErrorMessage
		{
			get { return string.Empty; }
		}

		public bool IsMandatory
		{
			get { return false; }
		}
	}
}

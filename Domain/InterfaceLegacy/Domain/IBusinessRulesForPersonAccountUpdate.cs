using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBusinessRulesForPersonalAccountUpdate
	{
		INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange);
		INewBusinessRuleCollection FromScheduleDictionary(IDictionary<IPerson, IPersonAccountCollection> helpersPersonAbsenceAccounts, IScheduleDictionary scheduleDictionary);
	}
}

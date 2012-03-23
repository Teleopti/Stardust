using System;
using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface ISkillData : IAnalyticsDataSetup
	{
		DataTable Table { get; }
		int FirstSkillId { get; }
		Guid FirstSkillCode { get; }
		string FirstSkillName { get; }
	}
}
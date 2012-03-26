using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface ISkillData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
		int FirstSkillId { get; }
		Guid FirstSkillCode { get; }
		string FirstSkillName { get; }
	}
}
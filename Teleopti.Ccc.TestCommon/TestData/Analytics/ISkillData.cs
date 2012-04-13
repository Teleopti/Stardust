using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface ISkillData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
		int FirstSkillId { get; }
		Guid FirstSkillCode { get; }
		string FirstSkillName { get; }
	}
}
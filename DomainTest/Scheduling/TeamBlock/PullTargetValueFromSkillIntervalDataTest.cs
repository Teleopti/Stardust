using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class PullTargetValueFromSkillIntervalDataTest
	{
		[Test]
		public void TestIfCorrectTeleoptiValueIsCalculated()
		{
			IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime, ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014,06,11,7,0,0,DateTimeKind.Utc ),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 4, 5, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 8, 12, 3, null, null));
			var targetValue = new PullTargetValueFromSkillIntervalData().GetTargetValue(skillIntervalDataDic, TargetValueOptions.Teleopti);
			Assert.AreEqual(2.875, targetValue);
		}

		[Test]
		public void TestIfCorrectStdValueIsCalculated()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 9, 2, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 7, 4, 3, null, null));
			var targetValue = new PullTargetValueFromSkillIntervalData().GetTargetValue(skillIntervalDataDic, TargetValueOptions.StandardDeviation);
			Assert.AreEqual(0.175, targetValue, 3);
		}

		[Test]
		public void TestIfCorrectRmsValueIsCalculated()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 9, 3, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 7, 5, 3, null, null));
			var targetValue = new PullTargetValueFromSkillIntervalData().GetTargetValue(skillIntervalDataDic, TargetValueOptions.RootMeanSquare);
			Assert.AreEqual(4.123, targetValue, 3);
		}
	}
}

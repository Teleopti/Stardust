using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class PullTargetValueFromSkillIntervalDataTest
	{
		private PullTargetValueFromSkillIntervalData _target;

		[SetUp]
		public void Setup()
		{
			_target = new PullTargetValueFromSkillIntervalData();
		}

		[Test]
		public void TestIfCorrectTeleoptiValueIsCalculatedWithoutPunishing()
		{
			IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime, ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014,06,11,7,0,0,DateTimeKind.Utc ),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 4, 5, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 8, 12, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.Teleopti,
				new Dictionary<DateTime , IntervalLevelMaxSeatInfo>());
			Assert.AreEqual(2.875, targetValue);
		}

		[Test]
		public void TestIfCorrectStdValueIsCalculatedWithoutPunishing()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 9, 2, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 7, 4, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.StandardDeviation,
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>());
			Assert.AreEqual(0.175, targetValue, 3);
		}

		[Test]
		public void TestIfCorrectRmsValueIsCalculatedWithoutPunishing()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 9, 3, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 7, 5, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.RootMeanSquare,
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>());
			Assert.AreEqual(4.123, targetValue, 3);
		}

		[Test]
		public void TestIfCorrectTeleoptiValueIsCalculatedWithPunishment()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedIntervals =
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			aggregatedIntervals.Add(startDateTime, new IntervalLevelMaxSeatInfo(false, 1));
			aggregatedIntervals.Add(endDateTime, new IntervalLevelMaxSeatInfo(true, 2));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 4, 5, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 8, 12, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.Teleopti, aggregatedIntervals);
			Assert.AreEqual(2999.625, targetValue);
		}

		[Test]
		public void TestIfCorrectStdValueIsCalculatedWithPunishment()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedIntervals =
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			aggregatedIntervals.Add(startDateTime, new IntervalLevelMaxSeatInfo(false, 1));
			aggregatedIntervals.Add(endDateTime, new IntervalLevelMaxSeatInfo(true, 3));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 2, 3, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 5, 8, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.StandardDeviation,
				aggregatedIntervals);
			Assert.AreEqual(1499.95, targetValue);
		}

		[Test]
		public void TestIfCorrectRmsValueIsCalculatedWithPunishment()
		{
			IDictionary<DateTime , ISkillIntervalData> skillIntervalDataDic = new Dictionary<DateTime , ISkillIntervalData>();
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedIntervals =
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			DateTime startDateTime = new DateTime(2014, 06, 11, 07, 0, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2014, 06, 11, 08, 0, 0, DateTimeKind.Utc);
			aggregatedIntervals.Add(startDateTime, new IntervalLevelMaxSeatInfo(false, 1));
			aggregatedIntervals.Add(endDateTime, new IntervalLevelMaxSeatInfo(true, 2));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 7, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime, endDateTime), 3, 3, 2, null, null));
			skillIntervalDataDic.Add(new DateTime(2014, 06, 11, 8, 0, 0, DateTimeKind.Utc),
				new SkillIntervalData(new DateTimePeriod(startDateTime.AddHours(1), endDateTime.AddHours(1)), 6, 8, 3, null, null));
			var targetValue = _target.GetTargetValue(skillIntervalDataDic, TargetValueOptions.StandardDeviation,
				aggregatedIntervals);
			Assert.AreEqual(1000.167, targetValue, 3);
		}
	}
}

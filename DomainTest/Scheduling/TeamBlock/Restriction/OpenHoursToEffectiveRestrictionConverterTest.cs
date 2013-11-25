using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class OpenHoursToEffectiveRestrictionConverterTest
	{
		private OpenHoursToEffectiveRestrictionConverter _target;

		[SetUp]
		public void Setup()
		{
			_target = new OpenHoursToEffectiveRestrictionConverter();
		}

		[Test]
		public void ShouldConvertOverlappingActivies()
		{
			IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activitySkillIntervalDatas =
				new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();

			
			var dtp = new DateTimePeriod(new DateTime(2013, 6, 26, 8, 0, 0, 0, DateTimeKind.Utc),
			                             new DateTime(2013, 6, 26, 9, 0, 0, 0, DateTimeKind.Utc));

			// one phone skill open 8 to 10
			IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDatas = new Dictionary<TimeSpan, ISkillIntervalData>();
			skillIntervalDatas.Add(TimeSpan.FromHours(8), new SkillIntervalData(dtp, 0, 0, 0, null, null));
			skillIntervalDatas.Add(TimeSpan.FromHours(9), new SkillIntervalData(dtp.MovePeriod(TimeSpan.FromHours(1)), 0, 0, 0, null, null));
			activitySkillIntervalDatas.Add(new Activity("phone"), skillIntervalDatas);

			// one bo skill open 9 to 11
			skillIntervalDatas = new Dictionary<TimeSpan, ISkillIntervalData>();
			skillIntervalDatas.Add(TimeSpan.FromHours(9), new SkillIntervalData(dtp.MovePeriod(TimeSpan.FromHours(1)), 0, 0, 0, null, null));
			skillIntervalDatas.Add(TimeSpan.FromHours(10), new SkillIntervalData(dtp.MovePeriod(TimeSpan.FromHours(2)), 0, 0, 0, null, null));
			activitySkillIntervalDatas.Add(new Activity("bo"), skillIntervalDatas);

			// should return 8 to 11
			var restriction = _target.Convert(activitySkillIntervalDatas);

			Assert.AreEqual(TimeSpan.FromHours(8), restriction.StartTimeLimitation.StartTime);
			Assert.IsNull(restriction.StartTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(11), restriction.EndTimeLimitation.EndTime);
			Assert.IsNull(restriction.EndTimeLimitation.StartTime);
		}

		[Test]
		public void ShouldHandleOpenHoursOverMidnight()
		{
			IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activitySkillIntervalDatas =
				new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();


			var dtp = new DateTimePeriod(new DateTime(2013, 6, 26, 23, 0, 0, 0, DateTimeKind.Utc),
										 new DateTime(2013, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc));

			// one phone skill open 23 to 01
			IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDatas = new Dictionary<TimeSpan, ISkillIntervalData>();
			skillIntervalDatas.Add(new TimeSpan(0, 23, 0, 0), new SkillIntervalData(dtp, 0, 0, 0, null, null));
			skillIntervalDatas.Add(new TimeSpan(1, 0, 0, 0), new SkillIntervalData(dtp.MovePeriod(TimeSpan.FromHours(1)), 0, 0, 0, null, null));
			activitySkillIntervalDatas.Add(new Activity("phone"), skillIntervalDatas);

			var restriction = _target.Convert(activitySkillIntervalDatas);

			Assert.AreEqual(new TimeSpan(0, 23, 0, 0), restriction.StartTimeLimitation.StartTime);
			Assert.IsNull(restriction.StartTimeLimitation.EndTime);
			Assert.AreEqual(new TimeSpan(1, 1, 0, 0), restriction.EndTimeLimitation.EndTime);
			Assert.IsNull(restriction.EndTimeLimitation.StartTime);
		}

		[Test]
		public void ShouldHandleClosedActivity()
		{
			IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activitySkillIntervalDatas =
				new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();

			var dtp = new DateTimePeriod(new DateTime(2013, 6, 26, 8, 0, 0, 0, DateTimeKind.Utc),
										 new DateTime(2013, 6, 26, 9, 0, 0, 0, DateTimeKind.Utc));

			// one phone skill open 8 to 10
			IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDatas = new Dictionary<TimeSpan, ISkillIntervalData>();
			skillIntervalDatas.Add(TimeSpan.FromHours(8), new SkillIntervalData(dtp, 0, 0, 0, null, null));
			skillIntervalDatas.Add(TimeSpan.FromHours(9), new SkillIntervalData(dtp.MovePeriod(TimeSpan.FromHours(1)), 0, 0, 0, null, null));
			activitySkillIntervalDatas.Add(new Activity("phone"), skillIntervalDatas);

			// one bo skill closed
			skillIntervalDatas = new Dictionary<TimeSpan, ISkillIntervalData>();
			activitySkillIntervalDatas.Add(new Activity("bo"), skillIntervalDatas);

			// should return 8 to 10
			var restriction = _target.Convert(activitySkillIntervalDatas);

			Assert.AreEqual(TimeSpan.FromHours(8), restriction.StartTimeLimitation.StartTime);
			Assert.IsNull(restriction.StartTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(10), restriction.EndTimeLimitation.EndTime);
			Assert.IsNull(restriction.EndTimeLimitation.StartTime);
		}
	}
}

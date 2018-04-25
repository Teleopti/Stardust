using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[TestFixture]
	public class SplitSkillStaffIntervalTest : IIsolateSystem
	{
		public SplitSkillStaffInterval Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<SplitSkillStaffInterval>().For<SplitSkillStaffInterval>();
		}

		[Test]
		public void ReturnSameIntervalsIfResolutionIsSame()
		{
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10)
				},
				new SkillStaffingInterval()
				{
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,20)
				}
			};
			var result = Target.Split(staffingList, TimeSpan.FromMinutes(10), false);
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ReturnSplittedIntervals()
		{
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10)
				},
				new SkillStaffingInterval()
				{
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,20)
				}
			};
			var result = Target.Split(staffingList, TimeSpan.FromMinutes(5), false);
			result.Count.Should().Be.EqualTo(4);
			var expectations = new List<SkillStaffingIntervalLightModel>()
			{
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,5)
				},
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,5),
					EndDateTime = dateTimeIs(8,10)
				},
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,15)
				},
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,15),
					EndDateTime = dateTimeIs(8,20)
				}
			};

			CollectionAssert.AreEqual(expectations, result);
		}

		[Test]
		public void ReturnSplitedSkillStaffingIntervals()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var staffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval()
				{
					SkillId = skill1,
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,30),
					StaffingLevel = 50
				},
				new SkillStaffingInterval()
				{
					SkillId = skill2,
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 10
				}
			};
			var result = Target.Split(staffingList, TimeSpan.FromMinutes(10), false);
			result.Count.Should().Be.EqualTo(4);

			var expectations = new List<SkillStaffingIntervalLightModel>()
			{
				new SkillStaffingIntervalLightModel()
				{
					Id = skill1,
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 50
				},
				new SkillStaffingIntervalLightModel()
				{
					Id = skill1,
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,20),
					StaffingLevel = 50
				},
				new SkillStaffingIntervalLightModel()
				{
					Id = skill1,
					StartDateTime = dateTimeIs(8,20),
					EndDateTime = dateTimeIs(8,30),
					StaffingLevel = 50
				},
				new SkillStaffingIntervalLightModel()
				{
					Id = skill2,
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 10
				}
			};
			CollectionAssert.AreEqual(expectations, result);
		}

		private DateTime dateTimeIs(int hour, int minutes)
		{
			return new DateTime(2016, 11, 03, hour, minutes, 0, DateTimeKind.Utc);
		}
	}
	
}

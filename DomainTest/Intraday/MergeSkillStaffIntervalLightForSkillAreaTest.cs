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
	public class MergeSkillStaffIntervalLightForSkillAreaTest : ISetup
	{
		public MergeSkillStaffIntervalLightForSkillArea Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MergeSkillStaffIntervalLightForSkillArea>().For<MergeSkillStaffIntervalLightForSkillArea>();
		}

		[Test]
		public void ReturnMergedIfDifferentIntervals()
		{
			var skillAreaId = Guid.NewGuid();
			var staffingList = new List<SkillStaffingIntervalLightModel>()
			{
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 10
				},
				new SkillStaffingIntervalLightModel()
				{
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,20),
					StaffingLevel = 20
				}
			};
			var result = Target.Merge(staffingList);
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ReturnMergedSameIntervals()
		{
			var staffingList = new List<SkillStaffingIntervalLightModel>()
			{
				new SkillStaffingIntervalLightModel()
				{
					Id = Guid.NewGuid(),
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 11
				},
				new SkillStaffingIntervalLightModel()
				{
					Id = Guid.NewGuid(),
					StartDateTime = dateTimeIs(8,0),
					EndDateTime = dateTimeIs(8,10),
					StaffingLevel = 12
				},
				new SkillStaffingIntervalLightModel()
				{
					Id = Guid.NewGuid(),
					StartDateTime = dateTimeIs(8,10),
					EndDateTime = dateTimeIs(8,20),
					StaffingLevel = 13
				}
			};
			var result = Target.Merge(staffingList);
			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(new SkillStaffingIntervalLightModel()
			{
				StartDateTime = dateTimeIs(8, 0),
				EndDateTime = dateTimeIs(8, 10),
				StaffingLevel = 23
			});
			result.Should().Contain(new SkillStaffingIntervalLightModel()
			{
				StartDateTime = dateTimeIs(8, 10),
				EndDateTime = dateTimeIs(8, 20),
				StaffingLevel = 13
			});
		}


		private DateTime dateTimeIs(int hour, int minutes)
		{
			return new DateTime(2016, 11, 03, hour, minutes, 0, DateTimeKind.Utc);
		}
	}


	
}
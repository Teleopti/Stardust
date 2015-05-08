using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	public class OutboundProductionPlanFactoryTest
	{
		private OutboundProductionPlanFactory _target;

		[SetUp]
		public void Setup()
		{
			_target = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
		}

		[Test]
		public void ShouldCreateCorrectPeriodAndOpenDays()
		{
			var campaign = new Campaign { Name = "test" };
			var campaignWorkingPeriod = new CampaignWorkingPeriod { TimePeriod = new TimePeriod(10, 0, 15, 0) };

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);
			var result = _target.CreateAndMakeInitialPlan(new DateOnlyPeriod(2015, 6, 1, 2015, 6, 7), 10, TimeSpan.FromHours(1),
				campaign.CampaignWorkingPeriods.ToList());

			Assert.AreEqual(new DateOnlyPeriod(2015, 6, 1, 2015, 6, 7), result.SpanningPeriod);
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 1)));
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 2)));
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 3)));
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 4)));
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 5)));
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 6)));
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, result.PlannedTimeTypeOnDate(new DateOnly(2015, 6, 7)));
		}

		[Test]
		public void ShouldCreateCorrectTotalTimeAndPlan()
		{
			var campaign = new Campaign {Name = "test"};
			var campaignWorkingPeriod = new CampaignWorkingPeriod {TimePeriod = new TimePeriod(10, 0, 15, 0)};

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment {WeekdayIndex = DayOfWeek.Thursday};
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment {WeekdayIndex = DayOfWeek.Friday};
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);
			var result = _target.CreateAndMakeInitialPlan(new DateOnlyPeriod(2015, 6, 1, 2015, 6, 7), 10, TimeSpan.FromHours(1),
				campaign.CampaignWorkingPeriods.ToList());

			Assert.AreEqual(TimeSpan.FromHours(10), result.TotalWorkTime);
			Assert.AreEqual(10, result.TotalWorkItems);
			Assert.AreEqual(TimeSpan.FromHours(5), result.GetTimeOnDate(new DateOnly(2015, 6, 4)));
			Assert.AreEqual(TimeSpan.FromHours(5), result.GetTimeOnDate(new DateOnly(2015, 6, 5)));
		}
	}
}
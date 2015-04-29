using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;


namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	public class OutboundSkillCreatorTest
	{
		private OutboundSkillCreator _target;
		private IUserTimeZone _timeZone;

		[SetUp]
		public void Setup()
		{
			_timeZone = new HawaiiTimeZone();
			_target = new OutboundSkillCreator(_timeZone, new FakeOutboundSkillTypeProvider());
		}

		[Test]
		public void ShouldCreateSkill()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign {Name = "test"};
			var campaignWorkingPeriod = new CampaignWorkingPeriod {TimePeriod = new TimePeriod(10, 0, 15, 0)};

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment {WeekdayIndex = DayOfWeek.Thursday};
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);

			var skill = _target.CreateSkill(activity, campaign);

			Assert.AreEqual(activity, skill.Activity);
			Assert.AreEqual(_timeZone.TimeZone(), skill.TimeZone); //Should probably be a timezone defined on campaign
			Assert.AreEqual(60, skill.DefaultResolution); //Will work fine as long as the activity is implicit
			Assert.AreEqual(ForecastSource.OutboundTelephony, skill.SkillType.ForecastSource);
		}

		[Test]
		public void SkillShouldContainOneWorkLoad()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign { Name = "test" };
			var campaignWorkingPeriod = new CampaignWorkingPeriod {TimePeriod = new TimePeriod(10, 0, 15, 0)};

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);

			var skill = _target.CreateSkill(activity, campaign);

			Assert.AreEqual(1, skill.WorkloadCollection.Count());
			var workload = skill.WorkloadCollection.First();
			var template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Monday);
			Assert.AreEqual(0, template.OpenHourList.Count);
			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Tuesday);
			Assert.AreEqual(0, template.OpenHourList.Count);
			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Wednesday);
			Assert.AreEqual(0, template.OpenHourList.Count);

			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Thursday);
			Assert.AreEqual(1, template.OpenHourList.Count);
			Assert.AreEqual(campaignWorkingPeriod.TimePeriod, template.OpenHourList.First());
			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
			Assert.AreEqual(1, template.OpenHourList.Count);
			Assert.AreEqual(campaignWorkingPeriod.TimePeriod, template.OpenHourList.First());

			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Saturday);
			Assert.AreEqual(0, template.OpenHourList.Count);
			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Sunday);
			Assert.AreEqual(0, template.OpenHourList.Count);
		}

		[Test]
		public void HandledWithinShouldBeSameAsOpenHoursOnSkillDefaultTemplates()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign { Name = "test" };
			var campaignWorkingPeriod1 = new CampaignWorkingPeriod {TimePeriod = new TimePeriod(10, 0, 15, 0)};
			var campaignWorkingPeriod2 = new CampaignWorkingPeriod {TimePeriod = new TimePeriod(10, 0, 16, 0)};

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday };
			campaignWorkingPeriod1.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday };
			campaignWorkingPeriod2.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod1);
			campaign.AddWorkingPeriod(campaignWorkingPeriod2);

			var skill = _target.CreateSkill(activity, campaign);

			var skillTemplate = skill.GetTemplateAt((int) DayOfWeek.Thursday);
			var serviceLevelSeconds =
				skillTemplate.TemplateSkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds;
			var serviceLevel = TimeSpan.FromSeconds(serviceLevelSeconds);
			Assert.AreEqual(campaignWorkingPeriod1.TimePeriod.SpanningTime(), serviceLevel);

			skillTemplate = skill.GetTemplateAt((int)DayOfWeek.Friday);
			serviceLevelSeconds =
				skillTemplate.TemplateSkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds;
			serviceLevel = TimeSpan.FromSeconds(serviceLevelSeconds);
			Assert.AreEqual(campaignWorkingPeriod2.TimePeriod.SpanningTime(), serviceLevel);

			skillTemplate = skill.GetTemplateAt((int)DayOfWeek.Saturday);
			serviceLevelSeconds =
				skillTemplate.TemplateSkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds;
			serviceLevel = TimeSpan.FromSeconds(serviceLevelSeconds);
			Assert.AreEqual(new TimePeriod().SpanningTime(), serviceLevel);
		}

		[Test]
		public void ShouldPutAllDemandOnFirstOpenIntervalOfTheDay()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign { Name = "test" };
			var campaignWorkingPeriod = new CampaignWorkingPeriod { TimePeriod = new TimePeriod(10, 0, 12, 0) };

			var campaignWorkingPeriodAssignmentThursday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentThursday);

			campaign.AddWorkingPeriod(campaignWorkingPeriod);

			var skill = _target.CreateSkill(activity, campaign);

			var template = skill.WorkloadCollection.First().TemplateWeekCollection[(int) DayOfWeek.Thursday];
			Assert.AreEqual(1d, template.OpenTaskPeriodList[0].Tasks);
			Assert.AreEqual(0d, template.OpenTaskPeriodList[1].Tasks);
		}
	}

	public class FakeOutboundSkillTypeProvider : IOutboundSkillTypeProvider
	{
		public ISkillType OutboundSkillType()
		{
			var desc = new Description("SkillTypeOutbound");
			return new SkillTypeEmail(desc, ForecastSource.OutboundTelephony);
		}
	}
}
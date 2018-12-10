using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

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
			_target = new OutboundSkillCreator(_timeZone, new FakeSkillTypeProvider());
		}

		[Test]
		public void ShouldCreateSkill()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign {Name = "test"};
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(10, 0, 15, 0));
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(10, 0, 15, 0));

			var skill = _target.CreateSkill(activity, campaign);

			Assert.AreEqual(activity, skill.Activity);
			Assert.AreEqual(_timeZone.TimeZone(), skill.TimeZone); //Should probably be a timezone defined on campaign
			Assert.AreEqual(60, skill.DefaultResolution); //Will work fine as long as the activity is implicit
			Assert.AreEqual(ForecastSource.OutboundTelephony, skill.SkillType.ForecastSource);
		}

		[Test]
		public void ShouldCreateSkillWithMidnightOffset()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign {Name = "test"};
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(new TimeSpan(20, 0, 0), new TimeSpan(1, 5, 0, 0)));
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(new TimeSpan(22, 0, 0), new TimeSpan(1, 7, 0, 0)));

			var skill = _target.CreateSkill(activity, campaign);

			Assert.AreEqual(skill.MidnightBreakOffset, new TimeSpan(7, 0, 0));
		}

		[Test]
		public void SkillShouldContainOneWorkLoad()
		{
			var activity = ActivityFactory.CreateActivity("TestActivity");
			var campaign = new Campaign { Name = "test" };
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(10, 0, 15, 0));
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(10, 0, 15, 0));

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
			Assert.AreEqual(campaign.WorkingHours[DayOfWeek.Thursday], template.OpenHourList.First());
			template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
			Assert.AreEqual(1, template.OpenHourList.Count);
			Assert.AreEqual(campaign.WorkingHours[DayOfWeek.Thursday], template.OpenHourList.First());

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
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(10, 0, 15, 0));
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(10, 0, 16, 0));

			var skill = _target.CreateSkill(activity, campaign);

			var skillTemplate = skill.GetTemplateAt((int) DayOfWeek.Thursday);
			var serviceLevelSeconds =
				skillTemplate.TemplateSkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds;
			var serviceLevel = TimeSpan.FromSeconds(serviceLevelSeconds);
			Assert.AreEqual(campaign.WorkingHours[DayOfWeek.Thursday].SpanningTime(), serviceLevel);

			skillTemplate = skill.GetTemplateAt((int)DayOfWeek.Friday);
			serviceLevelSeconds =
				skillTemplate.TemplateSkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds;
			serviceLevel = TimeSpan.FromSeconds(serviceLevelSeconds);
			Assert.AreEqual(campaign.WorkingHours[DayOfWeek.Friday].SpanningTime(), serviceLevel);

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
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(10, 0, 12, 0));

			var skill = _target.CreateSkill(activity, campaign);

			var template = skill.WorkloadCollection.First().TemplateWeekCollection[(int) DayOfWeek.Thursday];
			Assert.AreEqual(1d, template.OpenTaskPeriodList[0].Tasks);
			Assert.AreEqual(0d, template.OpenTaskPeriodList[1].Tasks);
		}
	}

	public class FakeSkillTypeProvider : ISkillTypeProvider
	{
		public ISkillType Outbound()
		{
			var desc = new Description("SkillTypeOutbound");
			return new SkillTypeEmail(desc, ForecastSource.OutboundTelephony);
		}

		public ISkillType InboundTelephony()
		{
			throw new NotImplementedException();
		}
	}
}
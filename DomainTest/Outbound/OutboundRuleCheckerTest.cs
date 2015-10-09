using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Outbound
{
    [TestFixture]
    class OutboundRuleCheckerTest
    {
        private IOutboundCampaignTaskManager campaignTaskManager;
        private CampaignRuleChecker target;
    

        [Test]
        public void UnderSLACampaignShouldBeWarned()
        {
            var campaign = new Campaign();

            var incomingTask = new IncomingTask(
                new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
                100,
                new TimeSpan(6, 0, 0),
                null
                );

            campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(200, 0, 0), PlannedTimeTypeEnum.Calculated);
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(200, 0, 0), PlannedTimeTypeEnum.Calculated);
            campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

            target = new CampaignRuleChecker(
                new OutboundRuleConfigurationProvider(),
                new OutboundUnderSLARule(campaignTaskManager),
                new OutboundOverstaffRule(campaignTaskManager));

            var response = target.CheckCampaign(campaign);
            response.ToList().Count().Should().Be.EqualTo(1);

        }

        [Test]
        public void OverstaffCampaignShouldBeWarned()
        {
            var campaign = new Campaign();

            var incomingTask = new IncomingTask(
                new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
                100,
                new TimeSpan(6, 0, 0),
                null
                );

            campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(800, 0, 0), PlannedTimeTypeEnum.Calculated);
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
            incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
            campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

            target = new CampaignRuleChecker(
                new OutboundRuleConfigurationProvider(),
                new OutboundUnderSLARule(campaignTaskManager),
                new OutboundOverstaffRule(campaignTaskManager));

            var response = target.CheckCampaign(campaign);
            response.ToList().Count().Should().Be.EqualTo(1);

        }

	    [Test]
	    public void OverstaffCampaignsShouldBeWarnedWithValueGreaterThanRelativeThreshold()
	    {
			var campaign = new Campaign();

			var incomingTask = new IncomingTask(
				new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
				100,
				new TimeSpan(10, 0, 0),
				null
				);

			campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(1200, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var configurationProvider = new FakeRuleConfigurationProvider();
			configurationProvider.SetThresholdType(ThresholdType.Relative);
			configurationProvider.SetThreshold(0.2);

			target = new CampaignRuleChecker(
				configurationProvider,
				new OutboundUnderSLARule(campaignTaskManager),
				new OutboundOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign);
			response.ToList().Count().Should().Be.EqualTo(1);

	    }

		[Test]
		public void OverstaffCampaignsShouldNotBeWarnedWithValueSmallerThanRelativeThreshold()
		{
			var campaign = new Campaign();

			var incomingTask = new IncomingTask(
				new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
				100,
				new TimeSpan(10, 0, 0),
				null
				);

			campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(1000, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(100, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(90, 0, 0), PlannedTimeTypeEnum.Calculated);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var configurationProvider = new FakeRuleConfigurationProvider();
			configurationProvider.SetThresholdType(ThresholdType.Relative);
			configurationProvider.SetThreshold(0.2);

			target = new CampaignRuleChecker(
				configurationProvider,
				new OutboundUnderSLARule(campaignTaskManager),
				new OutboundOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign);
			response.ToList().Count().Should().Be.EqualTo(0);

		}

		[Test]
		public void OverstaffCampaignsShouldBeWarnedWithValueGreaterThanAbsoluteThreshold()
		{
			var campaign = new Campaign();

			var incomingTask = new IncomingTask(
				new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
				100,
				new TimeSpan(10, 0, 0),
				null
				);

			campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(1000, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var configurationProvider = new FakeRuleConfigurationProvider();
			configurationProvider.SetThresholdType(ThresholdType.Absolute);
			configurationProvider.SetThreshold(100);

			target = new CampaignRuleChecker(
				configurationProvider,
				new OutboundUnderSLARule(campaignTaskManager),
				new OutboundOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign);
			response.ToList().Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void OverstaffCampaignsShouldNotBeWarnedWithValueSmallerThanAbsoluteThreshold()
		{
			var campaign = new Campaign();

			var incomingTask = new IncomingTask(
				new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
				100,
				new TimeSpan(10, 0, 0),
				null
				);

			campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(1000, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var configurationProvider = new FakeRuleConfigurationProvider();
			configurationProvider.SetThresholdType(ThresholdType.Absolute);
			configurationProvider.SetThreshold(200);

			target = new CampaignRuleChecker(
				configurationProvider,
				new OutboundUnderSLARule(campaignTaskManager),
				new OutboundOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign);
			response.ToList().Count().Should().Be.EqualTo(0);

		}


	    class FakeRuleConfigurationProvider : IOutboundRuleConfigurationProvider
	    {
		    private ThresholdType type = ThresholdType.Absolute;
		    private double threshold = 0;

		    public void SetThreshold(double value)
		    {
			    threshold = value;
		    }

		    public void SetThresholdType(ThresholdType value)
		    {
			    type = value;
		    }

		    public OutboundRuleConfiguration GetConfiguration(Type rule)
		    {
				if (rule == typeof(OutboundOverstaffRule))
				{
					return new OutboundOverstaffRuleConfiguration
					{
						Threshold = threshold,
						ThresholdType = type
					};
				}

				if (rule == typeof(OutboundUnderSLARule))
				{
					return new OutboundUnderSLARuleConfiguration
					{
						Threshold = threshold,
						ThresholdType = type
					};
				}

				return null;
		    }



	    }

    }
}

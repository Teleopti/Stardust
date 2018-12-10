using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignWarningProviderTest
	{
		private IOutboundCampaignTaskManager campaignTaskManager;
		private CampaignWarningProvider target;


		[Test]
		public void UnderSLACampaignShouldBeWarned()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			target = new CampaignWarningProvider(
				new FakeCampaignWarningConfigurationProvider(),
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void OverstaffCampaignShouldBeWarned()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			target = new CampaignWarningProvider(
							new FakeCampaignWarningConfigurationProvider(),
							new CampaignUnderServiceLevelRule(campaignTaskManager),
							new CampaignOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void OverstaffCampaignsShouldBeWarnedWithValueGreaterThanRelativeThreshold()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			var configurationProvider = new FakeCampaignWarningConfigurationProvider
			{
				ThresholdType = WarningThresholdType.Relative,
				Threshold = 0.2
			};

			target = new CampaignWarningProvider(
				configurationProvider,
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));


			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void OverstaffCampaignsShouldNotBeWarnedWithValueSmallerThanRelativeThreshold()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			var configurationProvider = new FakeCampaignWarningConfigurationProvider
			{
				ThresholdType = WarningThresholdType.Relative,
				Threshold = 0.2
			};
			target = new CampaignWarningProvider(
				configurationProvider,
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));


			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(0);

		}

		[Test]
		public void OverstaffCampaignsShouldBeWarnedWithValueGreaterThanAbsoluteThreshold()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			var configurationProvider = new FakeCampaignWarningConfigurationProvider
			{
				ThresholdType = WarningThresholdType.Absolute,
				Threshold = 100
			};
			target = new CampaignWarningProvider(
				configurationProvider,
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));
			
			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void OverstaffCampaignsShouldNotBeWarnedWithValueSmallerThanAbsoluteThreshold()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

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
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			var configurationProvider = new FakeCampaignWarningConfigurationProvider
			{
				ThresholdType = WarningThresholdType.Absolute,
				Threshold = 200
			};
			target = new CampaignWarningProvider(
				configurationProvider,
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));
			
			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(0);

		}

		[Test]
		public void OverstaffBeforeActualBacklogShouldBeIgnored()
		{
			var campaign = new Domain.Outbound.Campaign();
			var skipDates = new List<DateOnly>();

			var incomingTask = new IncomingTask(
				new DateOnlyPeriod(new DateOnly(2015, 5, 1), new DateOnly(2015, 5, 3)),
				100,
				new TimeSpan(10, 0, 0),
				null
				);

			campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 1), new TimeSpan(2000, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 2), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetTimeOnDate(new DateOnly(2015, 5, 3), new TimeSpan(1, 0, 0), PlannedTimeTypeEnum.Calculated);
			incomingTask.SetActualBacklogOnDate(new DateOnly(2015, 5, 2), new TimeSpan(500));

			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign, skipDates)).Return(incomingTask);

			var configurationProvider = new FakeCampaignWarningConfigurationProvider
			{
				ThresholdType = WarningThresholdType.Absolute,
				Threshold = 200
			};
			target = new CampaignWarningProvider(
				configurationProvider,
				new CampaignUnderServiceLevelRule(campaignTaskManager),
				new CampaignOverstaffRule(campaignTaskManager));

			var response = target.CheckCampaign(campaign, skipDates);
			response.ToList().Count().Should().Be.EqualTo(0);

		}


		class FakeCampaignWarningConfigurationProvider : ICampaignWarningConfigurationProvider
		{
			public WarningThresholdType ThresholdType { get; set; }  
			public double Threshold { get; set; }

			public FakeCampaignWarningConfigurationProvider()
			{
				ThresholdType = WarningThresholdType.Absolute;
				Threshold = 0;
			}

			public CampaignWarningConfiguration GetConfiguration(Type rule)
			{

				return new CampaignWarningConfiguration
				{
					Threshold = Threshold,
					ThresholdType = ThresholdType
				};

			}
		}

	}
}

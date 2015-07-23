using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    }
}

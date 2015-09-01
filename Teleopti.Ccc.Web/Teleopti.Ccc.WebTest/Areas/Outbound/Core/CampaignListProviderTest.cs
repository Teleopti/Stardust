using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
    [TestFixture]
    class CampaignListProviderTest
    {
        private CampaignListProvider target;
        
        private FakeCampaignRepository _outboundCampaignRepository;
        private FakeScheduleResourceProvider _scheduledResourcesProvider;
        private FakeOutboundRuleChecker _outboundRuleChecker;
        private FakeOutboundCampaignListOrderProvider _campaignListOrderProvider;

        private IOutboundCampaign doneCampaign ;
        private IOutboundCampaign plannedCampaign ;
        private IOutboundCampaign scheduledCampaign ;
        private IOutboundCampaign ongoingCampaign ;

        [SetUp]
        public void SetUp()
        {
            _outboundCampaignRepository = new FakeCampaignRepository();
            _campaignListOrderProvider = new FakeOutboundCampaignListOrderProvider();
            _scheduledResourcesProvider = new FakeScheduleResourceProvider();
            _outboundRuleChecker = new FakeOutboundRuleChecker();

            _scheduledResourcesProvider.SetScheduledTimeOnDate(DateOnly.Today.AddDays(14), CreateSkill("B"), new TimeSpan(4, 0, 0));

            doneCampaign = GetTestCampaign(3);
            plannedCampaign = GetTestCampaign(0);
            scheduledCampaign = GetTestCampaign(1);
            ongoingCampaign = GetTestCampaign(2);

            _outboundCampaignRepository.Add(doneCampaign);
            _outboundCampaignRepository.Add(plannedCampaign);
            _outboundCampaignRepository.Add(scheduledCampaign);
            _outboundCampaignRepository.Add(ongoingCampaign);

            _campaignListOrderProvider.SetCampaignListOrder(new List<CampaignStatus>
            {
                CampaignStatus.Ongoing,
                CampaignStatus.Done,
                CampaignStatus.Planned,
                CampaignStatus.Scheduled
            });

             target = new CampaignListProvider(_outboundCampaignRepository, _scheduledResourcesProvider, _outboundRuleChecker,_campaignListOrderProvider );
        }

        [Test]
        public void ShouldListDoneCampaigns()
        {                    
            var result = target.ListDoneCampaign().ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c =>
            {
                c.Name.Should().Be.EqualTo("D");
                c.Status.Should().Be.EqualTo(CampaignStatus.Done);
            });
        }

        [Test]
        public void ShouldListOngoingCampaigns()
        {
            var result = target.ListOngoingCampaign().ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c =>
            {
                c.Name.Should().Be.EqualTo("C");
                c.Status.Should().Be.EqualTo(CampaignStatus.Ongoing);
            });
        }

        [Test]
        public void ShouldListScheduledCampaigns()
        {
          
            var result = target.ListScheduledCampaign().ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c =>
            {
                c.Name.Should().Be.EqualTo("B");
                c.Status.Should().Be.EqualTo(CampaignStatus.Scheduled);
            });
        }

        [Test]
        public void ShouldListPlannedCampaigns()
        {       
            var result = target.ListPlannedCampaign().ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c =>
            {
                c.Name.Should().Be.EqualTo("A");
                c.Status.Should().Be.EqualTo(CampaignStatus.Planned);
            });
        }

        [Test]
        public void ScheduledCampaignShouldNotBeListedInPlannedCampaigns()
        {            
            var result = target.ListPlannedCampaign().ToList();
            result.ForEach(c => c.Name.Should().Not.Be.EqualTo("B"));
        }

        [Test]
        public void ShouldListCampaignsByStatusOngoing()
        {
            var result = target.ListCampaign(CampaignStatus.Ongoing).ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c => c.Name.Should().Be.EqualTo("C"));
        }

        [Test]
        public void ShouldListCampaignsByStatusDone()
        {
            var result = target.ListCampaign(CampaignStatus.Done).ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c => c.Name.Should().Be.EqualTo("D"));
        }

        [Test]
        public void ShouldListCampaignsByStatusScheduled()
        {
            var result = target.ListCampaign(CampaignStatus.Scheduled).ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c => c.Name.Should().Be.EqualTo("B"));
        }

        [Test]
        public void ShouldListCampaignsByStatusPlanned()
        {
            var result = target.ListCampaign(CampaignStatus.Planned).ToList();
            result.Count.Should().Be.EqualTo(1);
            result.ForEach(c => c.Name.Should().Be.EqualTo("A"));
        }

        [Test]
        public void ShouldListAllCampaignsByStatusNone()
        {            
            var result = target.ListCampaign(CampaignStatus.None).ToList();
            result.Count.Should().Be.EqualTo(4);           
        }

        [Test]
        public void ShouldListCampaignsByCompositeStatus()
        {
            var result = target.ListCampaign(CampaignStatus.Done | CampaignStatus.Scheduled).ToList();

            result.Count.Should().Be.EqualTo(2);
            var expectedNames = new List<string> {"B", "D"};
            result.ForEach(c => expectedNames.Should().Contain(c.Name));
        }

        [Test]
        public void ShouldListAllCampaignsByAllStatus()
        {
            var result = target.ListCampaign(
                CampaignStatus.Done | CampaignStatus.Scheduled | CampaignStatus.Planned | CampaignStatus.Ongoing).ToList();

            result.Count.Should().Be.EqualTo(4);
        }

        [Test]
        public void ShouldListCampaignsWithGivenOrder()
        {
            _campaignListOrderProvider.SetCampaignListOrder(new List<CampaignStatus>
            {
                CampaignStatus.Ongoing,                
                CampaignStatus.Planned,
                CampaignStatus.Scheduled,
                CampaignStatus.Done,
            });

            var result = target.ListCampaign(CampaignStatus.None);
            result.Select(c => c.Name).Should().Have.SameSequenceAs(new List<string> {"C", "A", "B", "D"});
        }


        [Test]
        public void ShouldAttachCorrectRuleCheckResultToOngoingCampaigns()
        {
            _outboundRuleChecker.SetCampaignRuleCheckResponse(ongoingCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            var result = target.ListCampaign(CampaignStatus.Ongoing).ToList();
            result.ForEach(c =>
            {
                var warnings = c.WarningInfo.ToList();
                warnings.Should().Not.Be.Empty();
                warnings.ForEach(w =>
                {
                    w.TypeOfRule.Should().Be.EqualTo(typeof (OutboundOverstaffRule));
                });
            });
        }

        [Test]
        public void ShouldAttachCorrectRuleCheckResultToScheduledCampaigns()
        {
            _outboundRuleChecker.SetCampaignRuleCheckResponse(scheduledCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            var result = target.ListCampaign(CampaignStatus.Scheduled).ToList();
            result.ForEach(c =>
            {
                var warnings = c.WarningInfo.ToList();
                warnings.Should().Not.Be.Empty();
                warnings.ForEach(w =>
                {
                    w.TypeOfRule.Should().Be.EqualTo(typeof(OutboundOverstaffRule));
                });
            });
        }

        [Test]
		[Ignore("We may need to check the done campaigns")]
        public void ShouldNotAttachRuleCheckResultToDoneCampaigns()
        {
            _outboundRuleChecker.SetCampaignRuleCheckResponse(doneCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            var result = target.ListCampaign(CampaignStatus.Done).ToList();
            result.ForEach(c =>
            {
                var warnings = c.WarningInfo.ToList();
                warnings.Should().Be.Empty();               
            });
        }

        [Test]
		[Ignore("We may need to check the planned campaigns")]
        public void ShouldNotAttachRuleCheckResultToPlannedCampaigns()
        {
            _outboundRuleChecker.SetCampaignRuleCheckResponse(plannedCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            var result = target.ListCampaign(CampaignStatus.Planned).ToList();
            result.ForEach(c =>
            {
                var warnings = c.WarningInfo.ToList();
                warnings.Should().Be.Empty();
            });
        }

        [Test]		
        public void ShouldReturnCorrectStatisticsWhenThereAreNoWarnings()
        {
            var result = target.GetCampaignStatistics();

            result.Planned.Should().Be.EqualTo(1);
            result.Scheduled.Should().Be.EqualTo(1);
            result.OnGoing.Should().Be.EqualTo(1);
            result.Done.Should().Be.EqualTo(1);
            result.ScheduledWarning.Should().Be.EqualTo(0);
            result.OnGoingWarning.Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldReturnCorrectStatisticsWhenThereAreWarnings()
        {
            _outboundRuleChecker.SetCampaignRuleCheckResponse(ongoingCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            _outboundRuleChecker.SetCampaignRuleCheckResponse(scheduledCampaign, new List<OutboundRuleResponse>
            {
                new OutboundRuleResponse()
                {
                    TypeOfRule = typeof(OutboundOverstaffRule)
                }
            });

            var result = target.GetCampaignStatistics();

            result.Planned.Should().Be.EqualTo(1);
            result.Scheduled.Should().Be.EqualTo(1);
            result.OnGoing.Should().Be.EqualTo(1);
            result.Done.Should().Be.EqualTo(1);
            result.ScheduledWarning.Should().Be.EqualTo(1);
            result.OnGoingWarning.Should().Be.EqualTo(1);
        }


        public IOutboundCampaign GetTestCampaign(int index)
        {
			   var today = new DateTime(DateOnly.Today.Year, DateOnly.Today.Month, DateOnly.Today.Day, 0,0,0, DateTimeKind.Utc);
            var dayAfterOneWeek = today.AddDays(7);
            var dayAfterTwoWeeks = today.AddDays(14);
            var dayAfterThreeWeeks = today.AddDays(21);
            var dayBeforeOneWeek = today.AddDays(-7);
            var dayBeforeTwoWeeks = today.AddDays(-14);

            var campaigns = new IOutboundCampaign[4]
            {
                new Domain.Outbound.Campaign()
                {
                    Name = "A",
                    SpanningPeriod = new DateTimePeriod(dayAfterOneWeek.Date, dayAfterTwoWeeks.Date),
                    Skill = SkillFactory.CreateSkill("A", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero),
                },
                new Domain.Outbound.Campaign()
                {
                    Name = "B",
                    SpanningPeriod = new DateTimePeriod(dayAfterOneWeek.Date, dayAfterThreeWeeks.Date),
                    Skill = SkillFactory.CreateSkill("B", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
                },
                new Domain.Outbound.Campaign()
                {
                    Name = "C",
                    SpanningPeriod = new DateTimePeriod(dayBeforeOneWeek.Date, dayAfterOneWeek.Date),
                    Skill = SkillFactory.CreateSkill("C", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
                },
                new Domain.Outbound.Campaign()
                {
                    Name = "D",
                    SpanningPeriod = new DateTimePeriod(dayBeforeTwoWeeks.Date, dayBeforeOneWeek.Date),
                    Skill = SkillFactory.CreateSkill("D", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
                }
            };

            return campaigns[index];
        }

        public ISkill CreateSkill(string name)
        {
            return new Skill(name, "", Color.Blue, 60, new FakeOutboundSkillTypeProvider().OutboundSkillType())
			{
				TimeZone = new HawaiiTimeZone().TimeZone()
			};
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

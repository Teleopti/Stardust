﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
    [TestFixture]
    class CampaignSummaryViewModelFactoryTest
    {
        private CampaignSummaryViewModelFactory target;
        private ICampaignListProvider campaignListProvider;

        [SetUp]
        public void SetUp()
        {
            campaignListProvider = MockRepository.GenerateMock<ICampaignListProvider>();
        }

        [Test]
        public void ShouldSeparateWithWarningAndWithoutWarningCampaignsInListSummary()
        {
            campaignListProvider.Stub(x => x.ListCampaign(CampaignStatus.None)).Return(
                new List<CampaignSummary>
                {
                    new CampaignSummary
                    {
                        Name = "A",
                        WarningInfo = new List<OutboundRuleResponse>()
                    },
                    new CampaignSummary
                    {
                        Name = "B",
                        WarningInfo = new List<OutboundRuleResponse>()
                    },
                    new CampaignSummary
                    {
                        Name = "C",
                        WarningInfo = new List<OutboundRuleResponse>
                        {
                            new OutboundRuleResponse() {TypeOfRule = typeof(OutboundOverstaffRule)}
                        }
                    },
                    new CampaignSummary
                    {
                        Name = "D",
                        WarningInfo = new List<OutboundRuleResponse>
                        {
                            new OutboundRuleResponse() {TypeOfRule = typeof(OutboundUnderSLARule)}
                        }
                    }
                });


            target = new CampaignSummaryViewModelFactory(campaignListProvider);

            var result = target.GetCampaignSummaryList(CampaignStatus.None);
            result.CampaignsWithWarning.Count.Should().Be.EqualTo(2);
            result.CampaignsWithoutWarning.Count.Should().Be.EqualTo(2);

            result.CampaignsWithWarning.ForEach(c =>
            {
                new List<string> {"C", "D"}.Should().Contain(c.Name);
            });
            result.CampaignsWithoutWarning.ForEach(c =>
            {
                new List<string> { "A", "B" }.Should().Contain(c.Name);
            });
        }

        [Test]
        public void ShoulProduceCorrectWarningTypeMessageInSummaryListViewModel()
        {
            campaignListProvider.Stub(x => x.ListCampaign(CampaignStatus.None)).Return(
                new List<CampaignSummary>
                {                   
                    new CampaignSummary
                    {
                        Name = "D",
                        WarningInfo = new List<OutboundRuleResponse>
                        {
                            new OutboundRuleResponse() {TypeOfRule = typeof(OutboundUnderSLARule)}
                        }
                    }
                });

            target = new CampaignSummaryViewModelFactory(campaignListProvider);

            var result = target.GetCampaignSummaryList(CampaignStatus.None);
            result.CampaignsWithWarning.Count.Should().Be.EqualTo(1);
            
            result.CampaignsWithWarning.ForEach(c =>
            {
                var warnings = c.WarningInfo.ToList();
                warnings.Count.Should().Be.EqualTo(1);
                warnings.ForEach(w =>
                {
                    w.TypeOfRule.Should().Be.EqualTo("OutboundUnderSLARule");
                });
            });
        }


        [Test]
        public void ShouldReturnCorrectCampaignStatistics()
        {
            var statistics = new CampaignStatistics();
            campaignListProvider.Stub(x => x.GetCampaignStatistics()).Return(statistics);
            target = new CampaignSummaryViewModelFactory(campaignListProvider);
            var result = target.GetCampaignStatistics();

            result.Should().Be.EqualTo(statistics);
        }
    }
}

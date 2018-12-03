using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class ProductionReplanHelperTest
	{

		[Test]
		public void ShouldUpdateCacheForecastAfterReplan()
		{
			var _campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var _createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			var _outboundScheduledResourcesCacher = MockRepository.GenerateMock<IOutboundScheduledResourcesCacher>();

			var campaign = new Domain.Outbound.Campaign();
			var _incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			_incomingTask.Stub(x => x.RecalculateDistribution());
			_campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(_incomingTask);
			
			var target = new ProductionReplanHelper(_campaignTaskManager, _createOrUpdateSkillDays,
				_outboundScheduledResourcesCacher);

			target.Replan(campaign);

			_outboundScheduledResourcesCacher.AssertWasCalled( x => x.SetForecastedTime(
				Arg<IOutboundCampaign>.Is.Equal(campaign), 
				Arg<Dictionary<DateOnly, TimeSpan>>.Is.Anything));
		}
	}
}

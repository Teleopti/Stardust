using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	class OutboundCampaignListViewModelMapperTests
	{
		private IOutboundCampaignListViewModelMapper _target;

		[SetUp]
		public void Setup()
		{
			_target = new OutboundCampaignListViewModelMapper();
		}

		[Test]
		public void ShouldMapNothingWhenCampaignIsNull()
		{
			var result = _target.Map((IOutboundCampaign) null, "");

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapName()
		{
			var campaign = new Domain.Outbound.Campaign() {Name = "myCampaign"};

			var result = _target.Map(campaign, "");

			result.Name.Should().Be.EqualTo(campaign.Name);
		}		
		
		[Test]
		public void ShouldMapStartDate()
		{
			var campaign = new Domain.Outbound.Campaign() {SpanningPeriod = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue)};

			var result = _target.Map(campaign, "");

			result.StartDate.Should().Be.EqualTo(campaign.SpanningPeriod.StartDate);
		}		
		
		[Test]
		public void ShouldMapEndDate()
		{
			var campaign = new Domain.Outbound.Campaign() {SpanningPeriod = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue)};

			var result = _target.Map(campaign, "");

			result.EndDate.Should().Be.EqualTo(campaign.SpanningPeriod.EndDate);
		}		
		
		[Test]
		public void ShouldMapStatus()
		{
			var campaign = new Domain.Outbound.Campaign();

			var result = _target.Map(campaign, "onGoing");

			result.Status.Should().Be.EqualTo("onGoing");
		}
	}
}

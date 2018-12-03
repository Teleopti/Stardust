using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.WebTest.Areas.Outbound.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	[OutboundTest]
	public class CampaignListProviderNoMockTest
	{
		public ICampaignListProvider Target;
		public IOutboundCampaignRepository OutboundCampaignRepository;
		public FakeSkillRepository SkillRepository;

		[Test]
		public void ShouldLoadDataWithoutThrowingNoCurrentResourceCalculationContextException()
		{
			var skill = new Skill("test").WithId();
			SkillRepository.Has(skill);

			OutboundCampaignRepository.Add(new Domain.Outbound.Campaign()
			{
				BelongsToPeriod = new DateOnlyPeriod(new DateOnly(2017, 1, 1), new DateOnly(2017, 1, 2)),
				SpanningPeriod = new DateTimePeriod(new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2017, 1, 2, 23, 59, 59, DateTimeKind.Utc)),
				Skill = skill
			});

			Target.ResetCache();
			Target.LoadData(new GanttPeriod { StartDate = new DateOnly(2017, 1, 1), EndDate = new DateOnly(2017, 1, 2) });
		}
	}
}


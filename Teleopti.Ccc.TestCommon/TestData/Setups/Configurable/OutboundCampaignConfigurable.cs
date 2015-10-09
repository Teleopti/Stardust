using System;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class OutboundCampaignConfigurable : IDataSetup
	{
        public IOutboundCampaign Campaign;

		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Skill { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name == Skill);

			Campaign = new Campaign()
			{
				Name = Name,
				CallListLen = 100,
				TargetRate = 50,
				Skill = skill,
				ConnectRate = 20,
				RightPartyConnectRate = 20,
				ConnectAverageHandlingTime = 30,
				RightPartyAverageHandlingTime = 120,
				UnproductiveTime = 30,
				SpanningPeriod = new DateTimePeriod(new DateTime(StartDate.Year, StartDate.Month, StartDate.Day), new DateTime(EndDate.Year, EndDate.Month, EndDate.Day))
			};

			new OutboundCampaignRepository(currentUnitOfWork).Add(Campaign);
		}

	}
}

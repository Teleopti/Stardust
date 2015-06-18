using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class OutboundCampaignConfigurable : IDataSetup
	{
		public Campaign Campaign;

		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Skill { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var skillRepository = new SkillRepository(uow);
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
				StartDate = new DateOnly(StartDate),
				EndDate = new DateOnly(EndDate),
			};

			new OutboundCampaignRepository(uow).Add(Campaign);
		}

	}
}

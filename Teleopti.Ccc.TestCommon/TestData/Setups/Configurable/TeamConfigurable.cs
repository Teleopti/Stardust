﻿using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class TeamConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Site { get; set; }

		public Team Team { get; private set; }

		public virtual void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			if (Name == null)
				Name = RandomName.Make("team");
			var siteRepository = new SiteRepository(currentUnitOfWork);
			var site = siteRepository.LoadAll().Single(c => c.Description.Name == Site);
			Team = new Team{Site = site}
				.WithDescription(new Description(Name));
			var teamRepository = new TeamRepository(currentUnitOfWork);
			teamRepository.Add(Team);
		}
	}
}
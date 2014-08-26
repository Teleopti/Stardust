using System.Linq;
using System.Runtime.InteropServices;
using Teleopti.Ccc.Domain.AgentInfo;
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

		public virtual void Apply(IUnitOfWork uow)
		{
			if (Name == null)
				Name = RandomName.Make("team");
			var siteRepository = new SiteRepository(uow);
			var site = siteRepository.LoadAll().Single(c => c.Description.Name == Site);
			Team = new Team
			{
				Description = new Description(Name),
				Site = site
			};
			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(Team);
		}
	}
}
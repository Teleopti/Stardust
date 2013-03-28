using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class TeamConfigurable : IDataSetup
	{
		public string Site { get; set; }

		public TeamConfigurable()
		{
			Site = GlobalDataContext.Data().Data<CommonSite>().Site.Description.Name;
		}

		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var siteRepository = new SiteRepository(uow);
			var site = siteRepository.LoadAll().Single(c => c.Description.Name == Site);
			var team = new Team
			           	{
			           		Description = new Description(Name),
			           		Site = site
			           	};
			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(team);
		}
	}
}
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class TeamConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Site { get; set; }

		public Team Team { get; private set; }

		public TeamConfigurable() : this(GlobalDataMaker.Data().Data<CommonSite>().Site) { }

		private TeamConfigurable(ISite site)
		{
			Site = site.Description.Name;
		}

		public void Apply(IUnitOfWork uow)
		{
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
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

		public string Name { get; set; }
		public Team Team { get; private set; }

		public TeamConfigurable() : this(GlobalDataContext.Data().Data<CommonSite>().Site) { }
		private TeamConfigurable(ISite site) { this.site = site; }

		public void Apply(IUnitOfWork uow)
		{
			Team = new Team
			var site = siteRepository.LoadAll().Single(c => c.Description.Name == Site);
			           	{
			           		Description = new Description(Name),
			           		Site = site
			           	};
			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(Team);
		}
	}
}
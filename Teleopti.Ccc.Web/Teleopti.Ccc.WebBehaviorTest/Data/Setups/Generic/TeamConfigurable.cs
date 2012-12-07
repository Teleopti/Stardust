using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class TeamConfigurable : IDataSetup
	{
		private readonly ISite site;

		public TeamConfigurable()
			: this(GlobalDataContext.Data().Data<CommonSite>().Site)
		{
		}

		private TeamConfigurable(ISite site)
		{
			this.site = site;
		}

		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
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
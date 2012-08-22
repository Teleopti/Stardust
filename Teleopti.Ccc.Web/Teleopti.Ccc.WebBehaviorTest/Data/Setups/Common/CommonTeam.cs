using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{
	public class CommonTeam : IDataSetup
	{
		public ITeam Team;

		public void Apply(IUnitOfWork uow)
		{
			var site = GlobalDataContext.Data().Data<CommonSite>().Site;

			Team = TeamFactory.CreateSimpleTeam("Common Team");
			site.AddTeam(Team);

			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(Team);
		}
	}
}
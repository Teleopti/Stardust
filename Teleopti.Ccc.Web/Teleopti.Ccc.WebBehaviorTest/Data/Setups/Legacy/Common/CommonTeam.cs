using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common
{
	public class CommonTeam : IDataSetup
	{
		public ITeam Team;

		public void Apply(IUnitOfWork uow)
		{
			var site = GlobalDataMaker.Data().Data<CommonSite>().Site;

			Team = TeamFactory.CreateSimpleTeam(RandomName.Make("Common Team"));
			site.AddTeam(Team);

			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(Team);
		}
	}
}
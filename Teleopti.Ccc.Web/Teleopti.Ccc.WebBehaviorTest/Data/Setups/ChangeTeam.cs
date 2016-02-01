using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups
{
	public class ChangeTeam : IUserSetup
	{
		public string Team { set; get; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var team = new TeamRepository(uow).LoadAll().Single(x => x.Description.Name == Team);
			var period = user.Period(new DateOnly(CurrentTime.Value()));
			user.ChangeTeam(team, period);
		}
	}
}
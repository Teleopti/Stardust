using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonGroupConfigurable : IUserDataSetup
	{
		public string Page { get; set; }
		public string Group { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var page = new GroupPageRepository(currentUnitOfWork.Current()).LoadAll().Single(p => p.Description.Name == Page);
			var group = page.RootGroupCollection.Single(g => g.Name == Group);
			group.AddPerson(user);
		}
	}
}
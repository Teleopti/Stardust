using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class GroupPageConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Group { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var page = new GroupPage(Name);

			var groups = Group.Split(',').Select(g => g.Trim());
			groups.ForEach(g => page.AddRootPersonGroup(new RootPersonGroup(g)));

			new GroupPageRepository(currentUnitOfWork.Current()).Add(page);
		}

	}
}
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class WorkflowControlSetForUser : IUserSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var repository = new WorkflowControlSetRepository(uow);
			var workflowControlSet = repository.LoadAll().Single(w => w.Name == Name);

			user.WorkflowControlSet = workflowControlSet;
		}
	}
}
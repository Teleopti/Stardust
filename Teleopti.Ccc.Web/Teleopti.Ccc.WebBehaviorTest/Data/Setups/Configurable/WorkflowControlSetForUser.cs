using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class WorkflowControlSetForUser : IUserSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var repository = WorkflowControlSetRepository.DONT_USE_CTOR(uow);
			var workflowControlSet = repository.LoadAll().Single(w => w.Name == Name);

			user.WorkflowControlSet = workflowControlSet;
		}
	}
}
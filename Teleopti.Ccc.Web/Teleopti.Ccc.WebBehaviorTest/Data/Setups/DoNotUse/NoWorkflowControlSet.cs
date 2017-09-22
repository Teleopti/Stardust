using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class NoWorkflowControlSet : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.WorkflowControlSet = null;
		}
	}
}
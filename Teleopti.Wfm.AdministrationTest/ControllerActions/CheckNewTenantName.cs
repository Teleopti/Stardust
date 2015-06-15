using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class CheckNewTenantName
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			
		}

	}
}
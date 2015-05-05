using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class VerifyTerminalDate : IVerifyTerminalDate
	{
		private readonly Func<IApplicationData> _applicationData;

		public VerifyTerminalDate(Func<IApplicationData> applicationData)
		{
			_applicationData = applicationData;
		}

		public bool IsTerminated(string tenantName, Guid personId)
		{
			var tenant = _applicationData().Tenant(tenantName);
			using (var uow = tenant.Application.CreateAndOpenUnitOfWork())
			{
				//could be a bit efficient if needed - only load terminaldate column. Wait with that though - premature opt...
				var person = new PersonRepository(uow).Get(personId);
				return person.IsTerminated();
			}
		}
	}
}
using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class AuthenticatorResult
	{
		public bool Successful { get; set; }
		public IPerson Person { get; set; }
		public IDataSource DataSource { get; set; }
		public string TenantPassword { get; set; }
		public bool IsPersistent { get; set; }

		public Guid? PersonId()
		{
			return Person == null ? null : Person.Id;
		}
	}
}
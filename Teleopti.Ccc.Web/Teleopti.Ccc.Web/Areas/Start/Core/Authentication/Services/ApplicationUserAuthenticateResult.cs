using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class ApplicationUserAuthenticateResult
	{
		public bool Successful { get; set; }
		public bool PasswordExpired { get; set; }
		public string Message { get; set; }
		public bool HasMessage { get; set; }
		public IPerson Person { get; set; }
		public IDataSource DataSource { get; set; }

		public Guid? PersonId()
		{
			return Person == null ? null : Person.Id;
		}
	}
}
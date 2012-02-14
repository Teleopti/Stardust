using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class AuthenticateResult
	{
		public bool Successful { get; set; }
		public string Message { get; set; }
		public bool HasMessage { get; set; }
		public IPerson Person { get; set; }
		public IDataSource DataSource { get; set; }
	}
}
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		public IdentificationResult TryAuthenticate(string userName, string password)
		{
			return new IdentificationResult
			{
				Result = LogonResult.Success
			};
		}
	}
}
using System;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface IApplicationAuthentication
	{
		IdentificationResult TryAuthenticate(string userName, string password);
	}

	public class IdentificationResult
	{
		public bool Success { get; private set; }
		public Guid PersonId { get; private set; }
		public string Tennant { get; private set; }
	}
}
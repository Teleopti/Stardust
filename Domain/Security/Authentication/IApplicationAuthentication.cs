using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IApplicationAuthentication
	{
		IdentificationResult TryAuthenticate(string userName, string password);
	}

	public class IdentificationResult
	{
		public IdentificationResult(bool success, Guid personId, string tennant)
		{
			Tennant = tennant;
			PersonId = personId;
			Success = success;
		}

		public bool Success { get; private set; }
		public Guid PersonId { get; private set; }
		public string Tennant { get; private set; }
	}
}
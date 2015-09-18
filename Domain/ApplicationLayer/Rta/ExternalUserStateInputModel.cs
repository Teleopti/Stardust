using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ExternalUserStateInputModel
	{
		public string AuthenticationKey { get; set; }
		public string PlatformTypeId { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public bool IsLoggedOn { get; set; }
		public DateTime BatchId { get; set; }
		public bool IsSnapshot { get; set; }
	}

	public static class ExternalUserStateInputModelExtensions
	{
		public static void MakeLegacyKeyEncodingSafe(this ExternalUserStateInputModel input)
		{
			input.AuthenticationKey = ConfiguredKeyAuthenticator.MakeLegacyKeyEncodingSafe(input.AuthenticationKey);
		}

		public static Guid ParsedPlatformTypeId(this ExternalUserStateInputModel input)
		{
			return input.PlatformTypeId != null ? Guid.Parse(input.PlatformTypeId) : Guid.Empty;
		}
	}

}
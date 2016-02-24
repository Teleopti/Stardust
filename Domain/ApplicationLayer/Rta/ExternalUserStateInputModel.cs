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

		// for logging
		public override string ToString()
		{
			return string.Format(
				"UserCode: {0}, StateCode: {1}, StateDescription: {2}, IsLoggedOn: {3}, PlatformTypeId: {4}, SourceId: {5}, BatchId: {6}, IsSnapshot: {7}.",
				UserCode, StateCode, StateDescription, IsLoggedOn, PlatformTypeId, SourceId, BatchId, IsSnapshot);
		}
	}

	public static class ExternalUserStateInputModelExtensions
	{
		public static void MakeLegacyKeyEncodingSafe(this ExternalUserStateInputModel input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
		}

		public static Guid ParsedPlatformTypeId(this ExternalUserStateInputModel input)
		{
			return input.PlatformTypeId != null ? Guid.Parse(input.PlatformTypeId) : Guid.Empty;
		}
	}

}
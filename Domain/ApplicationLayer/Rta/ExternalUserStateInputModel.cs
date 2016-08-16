using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class CloseSnapshotInputModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime SnapshotId { get; set; }
	}

	public class ExternalUserStateInputModel
	{
		public string AuthenticationKey { get; set; }
		public string PlatformTypeId { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public DateTime? SnapshotId { get; set; }

		// for logging
		public override string ToString()
		{
			return $"AuthenticationKey: {AuthenticationKey}, UserCode: {UserCode}, StateCode: {StateCode}, StateDescription: {StateDescription}, PlatformTypeId: {PlatformTypeId}, SourceId: {SourceId}, SnapshotId: {SnapshotId}";
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
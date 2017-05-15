using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Rta
	{
		public static string LogOutBySnapshot = "CCC Logged out";

		private readonly TenantLoader _tenantLoader;
		private readonly ActivityChangeChecker _checker;
		private readonly IContextLoader _contextLoader;

		public Rta(
			TenantLoader tenantLoader,
			ActivityChangeChecker checker,
			IContextLoader contextLoader)
		{
			_tenantLoader = tenantLoader;
			_checker = checker;
			_contextLoader = contextLoader;
		}
		
		[LogInfo]
		[TenantScope]
		public virtual void SaveStateBatch(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			_contextLoader.ForBatch(batch);
		}

		[LogInfo]
		[TenantScope]
		public virtual void CloseSnapshot(CloseSnapshotInputModel input)
		{
			validateAuthenticationKey(input);
			_contextLoader.ForClosingSnapshot(input.SnapshotId, input.SourceId);
		}

		private void validateAuthenticationKey(IValidatable input)
		{

			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_checker.CheckForActivityChanges();
		}
	}
}

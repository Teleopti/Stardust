using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Rta
	{
		public static string LogOutStateCode = "LOGGED-OFF";
		public static string LogOutBySnapshot = "CCC Logged out";

		private readonly RtaProcessor _processor;
		private readonly TenantLoader _tenantLoader;
		private readonly ActivityChangeChecker _activityChangeChecker;
		private readonly IContextLoader _contextLoader;

		public Rta(
			RtaProcessor processor,
			TenantLoader tenantLoader,
			ActivityChangeChecker activityChangeChecker,
			IContextLoader contextLoader)
		{
			_processor = processor;
			_tenantLoader = tenantLoader;
			_activityChangeChecker = activityChangeChecker;
			_contextLoader = contextLoader;
		}
		
		[LogInfo]
		[TenantScope]
		public virtual void SaveStateBatch(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			validatePlatformId(batch);

			_contextLoader.ForBatch(batch, person =>
			{
				_processor.Process(person);
			});
		}

		[LogInfo]
		[TenantScope]
		public virtual void SaveState(StateInputModel input)
		{
			validateAuthenticationKey(input);
			validatePlatformId(input);

			_contextLoader.For(input, person =>
			{
				_processor.Process(person);
			});
		}
		
		[LogInfo]
		[TenantScope]
		public virtual void CloseSnapshot(CloseSnapshotInputModel input)
		{
			_contextLoader.ForClosingSnapshot(input.SnapshotId, input.SourceId, context =>
			{
				_processor.Process(context);
			});
		}

		private void validateAuthenticationKey(IValidatable input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		private static void validatePlatformId(IValidatable input)
		{
			if (string.IsNullOrEmpty(input.PlatformTypeId))
				throw new InvalidPlatformException("Platform id is required");
		}
		
		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_activityChangeChecker.CheckForActivityChanges();
		}
	}
}

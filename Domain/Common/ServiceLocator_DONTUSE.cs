using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public class ServiceLocatorState
	{
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IUpdatedBy UpdatedBy;
		public ILoggedOnUserIsPerson LoggedOnUserIsPerson;
		public INow Now;
		public ICurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public ICurrentAuthorization CurrentAuthorization;
		public ITimeZoneGuard TimeZoneGuard;
	}

	public class ServiceLocator_DONTUSE
	{
		protected readonly ConcurrentStack<ServiceLocatorState> Instances = new ConcurrentStack<ServiceLocatorState>();

		private ServiceLocator_DONTUSE()
		{
			Push(new ServiceLocatorState
			{
				CurrentBusinessUnit = Common.CurrentBusinessUnit.Make(),
				UpdatedBy = Security.Principal.UpdatedBy.Make(),
				LoggedOnUserIsPerson = new LoggedOnUserIsPerson(Security.Principal.CurrentTeleoptiPrincipal.Make()),
				Now = new Now(),
				CurrentTeleoptiPrincipal = Security.Principal.CurrentTeleoptiPrincipal.Make(),
				CurrentAuthorization = Security.Principal.CurrentAuthorization.Make(),
				TimeZoneGuard = new TimeZoneGuard(),
			});
		}

		public void Push(ServiceLocatorState instance)
		{
			Instances.Push(instance);
		}

		public void Pop()
		{
			Instances.TryPop(out _);
		}

		private ServiceLocatorState current()
		{
			Instances.TryPeek(out var current);
			if (current == null)
				throw new ServiceLocatorNotAllowedException("DONT USE SERVICE LOCATOR!");
			return current;
		}

		public static readonly ServiceLocator_DONTUSE Instance = new ServiceLocator_DONTUSE();

		public static ICurrentBusinessUnit CurrentBusinessUnit => Instance.current().CurrentBusinessUnit;
		public static IUpdatedBy UpdatedBy => Instance.current().UpdatedBy;
		public static ILoggedOnUserIsPerson LoggedOnUserIsPerson => Instance.current().LoggedOnUserIsPerson;
		public static INow Now => Instance.current().Now;
		public static ICurrentTeleoptiPrincipal CurrentTeleoptiPrincipal => Instance.current()?.CurrentTeleoptiPrincipal;
		public static ICurrentAuthorization CurrentAuthorization => Instance.current().CurrentAuthorization;
		public static ITimeZoneGuard TimeZoneGuard => Instance.current().TimeZoneGuard;
	}

	public class ServiceLocatorNotAllowedException : Exception
	{
		public ServiceLocatorNotAllowedException(string message) : base(message)
		{
		}
	}
}
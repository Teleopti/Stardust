using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private static ICurrentAuthorization _currentAuthorization;
		private static ITimeZoneGuard _timeZoneGuard;

		public static ICurrentTeleoptiPrincipal CurrentTeleoptiPrincipal
		{
			get { return _currentTeleoptiPrincipal ?? Security.Principal.CurrentTeleoptiPrincipal.Make(); }
			set { _currentTeleoptiPrincipal = value; }
		}

		public static ICurrentAuthorization CurrentAuthorization
		{
			get { return _currentAuthorization ?? Security.Principal.CurrentAuthorization.Make(); }
			set { _currentAuthorization = value; }
		}

		public static ITimeZoneGuard TimeZoneGuard
		{
			get { return _timeZoneGuard ?? (_timeZoneGuard = new TimeZoneGuard()); }
			set { _timeZoneGuard = value; }
		}
	}

	public static class ServiceLocatorForEntity
	{
		private static readonly Stack<ICurrentBusinessUnit> _currentBusinessUnit = new Stack<ICurrentBusinessUnit>();
		private static IUpdatedBy _updatedBy;
		private static ILoggedOnUserIsPerson _loggedOnUserIsPerson;
		private static INow _now;

		public static ICurrentBusinessUnit CurrentBusinessUnit
		{
			get
			{
				if (_currentBusinessUnit.Count == 0)
					_currentBusinessUnit.Push(Common.CurrentBusinessUnit.Make());
				return _currentBusinessUnit.Peek();
			}
			set
			{
				if (value == null)
					_currentBusinessUnit.Pop();
				else
					_currentBusinessUnit.Push(value);
			}
		}

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}

		public static ILoggedOnUserIsPerson LoggedOnUserIsPerson
		{
			get { return _loggedOnUserIsPerson ?? new LoggedOnUserIsPerson(CurrentTeleoptiPrincipal.Make()); }
			set { _loggedOnUserIsPerson = value; }
		}

		public static INow Now
		{
			get { return _now ?? new Now(); }
			set { _now = value; }
		}
	}
}
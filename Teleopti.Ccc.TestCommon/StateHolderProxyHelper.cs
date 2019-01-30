using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.TestCommon
{
	public static class StateHolderProxyHelper
	{
		private static readonly SelectivePrincipalContext principalContext;
		public static IPrincipalFactory PrincipalFactory;

		static StateHolderProxyHelper()
		{
			principalContext = SelectivePrincipalContext.Make();
			PrincipalFactory = TeleoptiPrincipalFactory.Make();
		}

		public static void SetupFakeState(IDataSource dataSource, IPerson person, IBusinessUnit businessUnit)
		{
			var appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			var applicationData = new ApplicationData(appSettings, null);
			createAndSetPrincipal(person, dataSource, businessUnit);
			var state = new FakeState {ApplicationScopeData_DONTUSE = applicationData};
			ClearAndSetStateHolder(state);
		}

		public static void Logout()
		{
			principalContext.SetCurrentPrincipal(null);
		}

		public static void ClearAndInitializeStateHolder(IState stateMock)
		{
			IApplicationData appData = new ApplicationData(new Dictionary<string, string>(), null);
			stateMock.SetApplicationData(appData);
			ClearAndSetStateHolder(stateMock);
		}

		public static void ClearAndSetStateHolder(IPerson loggedOnPerson, IBusinessUnit businessUnit, IApplicationData appData, IDataSource logonDataSource, IState stateMock)
		{
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			createAndSetPrincipal(loggedOnPerson, logonDataSource, businessUnit);
			stateMock.SetApplicationData(appData);
			ClearAndSetStateHolder(stateMock);
		}

		public static void ClearAndSetStateHolder(IState state)
		{
			ClearStateHolder();
			StateHolder.Initialize(state, new MessageBrokerCompositeDummy());
		}

		public static void ClearStateHolder()
		{
			typeof(StateHolderReader).GetField("_instanceInternal", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
		}

		public static IApplicationData CreateApplicationData(IMessageBrokerComposite messageBroker)
		{
			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));
			var applicationData = new ApplicationData(appSettings, null);

			return applicationData;
		}

		public static void CreateSessionData(
			IPerson loggedOnPerson,
			IDataSource dataSource,
			IBusinessUnit businessUnit
		)
		{
			createAndSetPrincipal(loggedOnPerson, dataSource, businessUnit, new AppDomainPrincipalContext(new ThreadPrincipalContext(), new ThreadPrincipalContext()));
		}

		private static void createAndSetPrincipal(
			IPerson loggedOnPerson,
			IDataSource dataSource,
			IBusinessUnit businessUnit,
			ICurrentPrincipalContext context = null)
		{
			var principal = PrincipalFactory.MakePrincipal(loggedOnPerson, dataSource, businessUnit, null);
			(context ?? principalContext).SetCurrentPrincipal(principal);
		}

		public static IPerson CreateLoggedOnPerson()
		{
			const string allowedFunctionCode = "APPLTPEDT";
			ApplicationFunction function = new ApplicationFunction(allowedFunctionCode);
			ApplicationRole role = new ApplicationRole();
			role.AddApplicationFunction(function);
			Person person = new Person();
			person.PermissionInformation.AddApplicationRole(role);
			person.PermissionInformation.SetDefaultTimeZone(
				TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1053));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1033));
			return person;
		}
	}
}
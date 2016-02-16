using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
    public static class StateHolderProxyHelper
    {
		public static WindowsAppDomainPrincipalContext DefaultPrincipalContext { get; set; }

		static StateHolderProxyHelper() { DefaultPrincipalContext = new WindowsAppDomainPrincipalContext(); }

		public static void SetupFakeState(
			IDataSource dataSource, 
			IPerson person, 
			IBusinessUnit businessUnit,
			ICurrentPrincipalContext principalContext
			)
		{
			var appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			var applicationData = new ApplicationData(appSettings, null);
			CreateSessionData(person, dataSource, businessUnit, principalContext);
			var state = new FakeState { ApplicationScopeData = applicationData};
			ClearAndSetStateHolder(state);
		}

		public static void Logout(ICurrentPrincipalContext principalContext)
		{
			principalContext.ResetPrincipal();
			PrincipalAuthorization.SetInstance(null);
	    }

        public static void ClearAndInitializeStateHolder(IState stateMock)
        {
            MockRepository mocks = new MockRepository();
            IApplicationData appData = mocks.StrictMock<IApplicationData>();
            Expect.Call(stateMock.ApplicationScopeData)
                .Return(appData)
                .Repeat.Any();
            ClearAndSetStateHolder(stateMock);
        }

        public static void ClearAndSetStateHolder(IState state)
        {
            ClearStateHolder();
            StateHolder.Initialize(state, new MessageBrokerCompositeDummy());
        }

	    public static void ClearAndSetStateHolder(MockRepository mocks, IPerson loggedOnPerson, IBusinessUnit businessUnit, IApplicationData appData, IDataSource logonDataSource, IState stateMock)
	    {
		    var principalContext = new WindowsAppDomainPrincipalContext();
		    var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, logonDataSource, businessUnit, null);
		    principalContext.SetCurrentPrincipal(principal);

		    PrincipalAuthorization.SetInstance(new PrincipalAuthorizationWithFullPermission());

		    SetStateReaderExpectations(stateMock, appData);

		    ClearAndSetStateHolder(stateMock);
		    mocks.Replay(stateMock);
	    }

	    public static void SetStateReaderExpectations(IStateReader stateMock, IApplicationData applicationData)
        {
            Expect.Call(stateMock.ApplicationScopeData)
                .Return(applicationData)
                .Repeat.Any();
    		Expect.Call(stateMock.UserTimeZone)
    			.Return(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).Repeat.Any();
        }

        /// <summary>
        /// Clears the state holder.
        /// </summary>
        public static void ClearStateHolder()
        {
            typeof (StateHolderReader).GetField("_instanceInternal", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
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
			CreateSessionData(loggedOnPerson, dataSource, businessUnit, new WindowsAppDomainPrincipalContext());
		}

		public static void CreateSessionData(
			IPerson loggedOnPerson,
			IDataSource dataSource,
			IBusinessUnit businessUnit,
			ICurrentPrincipalContext principalContext)
		{
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, dataSource, businessUnit, null);
			principalContext.SetCurrentPrincipal(principal);

			PrincipalAuthorization.SetInstance(new PrincipalAuthorizationWithFullPermission());
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

  
	public class CustomAuthorizationContext : IDisposable
	{
		private IPrincipalAuthorization _previousAuthorization;

		public CustomAuthorizationContext(IPrincipalAuthorization principalAuthorization)
		{
			_previousAuthorization = PrincipalAuthorization.Instance();
			PrincipalAuthorization.SetInstance(principalAuthorization);
		}

		public void Dispose()
		{
			PrincipalAuthorization.SetInstance(_previousAuthorization);
			_previousAuthorization = null;
		}
	}
}
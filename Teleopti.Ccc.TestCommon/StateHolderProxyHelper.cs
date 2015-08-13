using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Linq;
using System.Reflection;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.TestCommon
{
    /// <summary>
    /// Helper to set StateHolder proxy to a certain state. Useful in tests.
    /// </summary>
    public static class StateHolderProxyHelper
    {
		public static WindowsAppDomainPrincipalContext DefaultPrincipalContext { get; set; }

		static StateHolderProxyHelper() { DefaultPrincipalContext = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()); }

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

			MessageBrokerContainerDontUse.Configure(null, new IConnectionKeepAliveStrategy[] { }, MessageFilterManager.Instance, new NewtonsoftJsonSerializer(), new NewtonsoftJsonSerializer());
			var signalBroker = MessageBrokerContainerDontUse.CompositeClient();
			var applicationData = new ApplicationData(appSettings, signalBroker, null, null);
			applicationData.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);
			var sessionData = CreateSessionData(person, dataSource, businessUnit, principalContext);
			var state = new FakeState { ApplicationScopeData = applicationData, SessionScopeData = sessionData, IsLoggedIn = true };
			ClearAndSetStateHolder(state);
		}

		public static void Logout(ICurrentPrincipalContext principalContext)
		{
			principalContext.ResetPrincipal();
			PrincipalAuthorization.SetInstance(null);
	    }

        /// <summary>
        /// Clears the and set state holder.
        /// </summary>
        /// <param name="stateMock">The state mock.</param>
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
            StateHolder.Initialize(state);
        }

		public static void ClearAndSetStateHolder(MockRepository mocks,
										  IPerson loggedOnPerson,
										  IBusinessUnit businessUnit,
										  IApplicationData appData,
											IDataSource logonDataSource,
										  IState stateMock)
		{
			var principalContext = new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory());

			principalContext.SetCurrentPrincipal(loggedOnPerson, logonDataSource, businessUnit);

			PrincipalAuthorization.SetInstance(new PrincipalAuthorizationWithFullPermission());
			ISessionData sessData = new SessionData();
			sessData.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

			SetStateReaderExpectations(stateMock, appData, sessData);

			ClearAndSetStateHolder(stateMock);
			mocks.Replay(stateMock);
		}


    	public static void SetStateReaderExpectations(IStateReader stateMock, IApplicationData applicationData, ISessionData sessionData)
        {
            Expect.Call(stateMock.IsLoggedIn)
                .Return(true)
                .Repeat.Any();
            Expect.Call(stateMock.ApplicationScopeData)
                .Return(applicationData)
                .Repeat.Any();
            Expect.Call(stateMock.SessionScopeData)
                .Return(sessionData)
                .Repeat.Any();
        }

        /// <summary>
        /// Clears the state holder.
        /// </summary>
        public static void ClearStateHolder()
        {
            typeof (StateHolderReader).GetField("_instanceInternal", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
        }

        public static IApplicationData CreateApplicationData(IMessageBrokerComposite messageBroker, IDataSource dataSource)
        {
            IDictionary<string, string> appSettings = new Dictionary<string, string>();
            ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
                name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

            var applicationData = new ApplicationData(appSettings, messageBroker, null, null);
			applicationData.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);

            return applicationData;
        }

		public static ISessionData CreateSessionData(
			IPerson loggedOnPerson,
			IDataSource dataSource,
			IBusinessUnit businessUnit
			)
		{
			return CreateSessionData(loggedOnPerson, dataSource, businessUnit, new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
		}

		public static ISessionData CreateSessionData(
			IPerson loggedOnPerson,
			IDataSource dataSource,
			IBusinessUnit businessUnit,
			ICurrentPrincipalContext principalContext)
		{
			principalContext.SetCurrentPrincipal(loggedOnPerson, dataSource, businessUnit);

			PrincipalAuthorization.SetInstance(new PrincipalAuthorizationWithFullPermission());
			ISessionData sessData = new SessionData();
			sessData.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			return sessData;
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
            return person;
        }
    }

    public class PrincipalAuthorizationWithFullPermission : IPrincipalAuthorization
    {
        public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
        {
            return true;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
        {
            return true;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
        {
            return true;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return true;
        }

        public bool IsPermitted(string functionPath)
        {
            return true;
        }

    	public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

    	public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification) { throw new NotImplementedException(); }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
            return true;
        }

    	public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return true;
    	}

    	public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
    	{
			return new[] { period };
    	}
    }

    public class PrincipalAuthorizationWithNoPermission : IPrincipalAuthorization
    {
        public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
        {
            return false;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
        {
            return false;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
        {
            return false;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return false;
        }

        public bool IsPermitted(string functionPath)
        {
            return false;
        }

    	public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

    	public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification) { throw new NotImplementedException(); }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
            return false;
        }

    	public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return false;
    	}

    	public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
    	{
			return new List<DateOnlyPeriod>(0);
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
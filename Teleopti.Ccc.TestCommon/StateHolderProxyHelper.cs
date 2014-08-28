using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

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

			var signalBroker = new SignalBroker(null, MessageFilterManager.Instance, new IConnectionKeepAliveStrategy[] {}, new Time(new Now()));
			var applicationData = new ApplicationData(appSettings, new[] { dataSource }, signalBroker, null);
			var sessionData = CreateSessionData(person, applicationData, businessUnit, principalContext);

			var state = new FakeState { ApplicationScopeData = applicationData, SessionScopeData = sessionData, IsLoggedIn = true };
			ClearAndSetStateHolder(state);
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

        public static void ClearAndSetStateHolder(IState stateMock)
        {
            ClearStateHolder();
            StateHolder.Initialize(stateMock);
        }


        /// <summary>
        /// Clears the and set state holder.
        /// </summary>
        /// <param name="mocks">The mocks.</param>
        /// <param name="dataSource">The factory.</param>
        /// <param name="stateMock">The state mock.</param>
        public static void ClearAndSetStateHolder(MockRepository mocks,
                                                  IDataSource dataSource,
                                                  IState stateMock)
        {
            ISessionData sessData = mocks.StrictMock<ISessionData>();
            IApplicationData appData = mocks.StrictMock<IApplicationData>();
            Expect.Call(stateMock.ApplicationScopeData)
                .Return(appData)
                .Repeat.Any();
            Expect.On(stateMock)
                .Call(stateMock.SessionScopeData)
                .Return(sessData)
                .Repeat.Any();
            ClearStateHolder();
            StateHolder.Initialize(stateMock);
            mocks.Replay(stateMock);
            mocks.Replay(sessData);
        }

		public static void ClearAndSetStateHolder(MockRepository mocks,
										  IPerson loggedOnPerson,
										  IBusinessUnit businessUnit,
										  IApplicationData appData,
										  IState stateMock)
		{
			ClearAndSetStateHolder(mocks, loggedOnPerson, businessUnit, appData, new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()), stateMock);
		}

    	public static void ClearAndSetStateHolder(MockRepository mocks,
												  IPerson loggedOnPerson,
												  IBusinessUnit businessUnit,
												  IApplicationData appData,
													ICurrentPrincipalContext principalContext,
												  IState stateMock)
        {
        	ISessionData sessData = CreateSessionData(loggedOnPerson, appData, businessUnit, principalContext);
        	if (appData == null)
        		appData = mocks.StrictMock<IApplicationData>();
        	SetStateReaderExpectations(stateMock, appData, sessData);
        	ClearAndSetStateHolder(stateMock);
        	mocks.Replay(stateMock);
        }

    	public static void ClearAndSetStateHolder(IPerson loggedOnPerson,
												 IBusinessUnit businessUnit,
												 IApplicationData appData
			)
    	{
    		ClearAndSetStateHolder(loggedOnPerson, businessUnit, appData, DefaultPrincipalContext);
		}

    	public static void ClearAndSetStateHolder(IPerson loggedOnPerson,
												 IBusinessUnit businessUnit,
												 IApplicationData appData,
												   ICurrentPrincipalContext principalContext
			)
		{
			var mocks = new MockRepository();
			ClearAndSetStateHolder(mocks, loggedOnPerson, businessUnit, appData, principalContext, mocks.DynamicMock<IState>());
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IApplicationData CreateApplicationData(IMessageBroker messageBroker)
        {
            IDictionary<string, string> appSettings = new Dictionary<string, string>();
            ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
                name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

            //todo: fixa!
            IList<IDataSource> dataSources = new List<IDataSource>();
            dataSources.Add(new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null));
            IApplicationData applicationData =
                new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), messageBroker, null);

            return applicationData;
        }

		public static ISessionData CreateSessionData(
			IPerson loggedOnPerson,
			IApplicationData applicationData,
			IBusinessUnit businessUnit
			)
		{
			return CreateSessionData(loggedOnPerson, applicationData, businessUnit, new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
		}

    	public static ISessionData CreateSessionData(
            IPerson loggedOnPerson, 
			IApplicationData applicationData, 
			IBusinessUnit businessUnit,
			ICurrentPrincipalContext principalContext
			)
        {
            IDataSource dataSource = null;
            if (applicationData != null)
                dataSource = applicationData.RegisteredDataSourceCollection.First();

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
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "App", Password = "User"};
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
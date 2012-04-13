using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.TestCommon
{
    /// <summary>
    /// Helper to set StateHolder proxy to a certain state. Useful in tests.
    /// </summary>
    public static class StateHolderProxyHelper
    {

		public static void SetupFakeState(IDataSource dataSource, IPerson person, IBusinessUnit businessUnit)
		{
			var appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			var applicationData = new ApplicationData(appSettings, new[] { dataSource }, MessageBrokerImplementation.GetInstance(), null);
			var sessionData = CreateSessionData(person, applicationData, businessUnit);

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


        /// <summary>
        /// Clears the and set state holder.
        /// </summary>
        /// <param name="mocks">The mocks.</param>
        /// <param name="loggedOnPerson">The logged on person.</param>
        /// <param name="businessUnit">The bu.</param>
        /// <param name="appData">The app data.</param>
        /// <param name="stateMock">The state mock.</param>
        public static void ClearAndSetStateHolder(MockRepository mocks,
                                                  IPerson loggedOnPerson,
                                                  IBusinessUnit businessUnit,
                                                  IApplicationData appData,
                                                  IState stateMock)
        {
            ISessionData sessData = CreateSessionData(loggedOnPerson, appData, businessUnit);
            if (appData == null)
                appData = mocks.StrictMock<IApplicationData>();
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
            IPerson loggedOnPerson, IApplicationData applicationData, IBusinessUnit businessUnit)
        {
            IDataSource dataSource = null;
            if (applicationData != null)
            {
                dataSource = applicationData.RegisteredDataSourceCollection.First();
            }

            var principal = new TeleoptiPrincipalForTest(new TeleoptiIdentity("test user", dataSource,
                                                                                  businessUnit,
                                                                                  WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application),
                                                             loggedOnPerson);
            principal.SetPrincipalAuthorization(new PrincipalAuthorizationWithFullPermission());

            var currentPrincipal = Thread.CurrentPrincipal as TeleoptiPrincipal;
            if (currentPrincipal==null)
            {
                AppDomain.CurrentDomain.SetThreadPrincipal(principal);
                Thread.CurrentPrincipal = principal;
            }
            else
            {
                currentPrincipal.ChangePrincipal(principal);
            }
            ISessionData sessData = new SessionData();
            sessData.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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
                new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "App", Password = "User"};
            return person;
        }
    }

    public class TeleoptiPrincipalForTest : TeleoptiPrincipal
    {
        private IPrincipalAuthorization _principalAuthorization;

        public TeleoptiPrincipalForTest(IIdentity identity, IPerson person) : base(identity, person)
        {
        }

        public override IPrincipalAuthorization PrincipalAuthorization
        {
            get
            {
                return _principalAuthorization;
            }
        }

        public void SetPrincipalAuthorization(IPrincipalAuthorization principalAuthorization)
        {
            _principalAuthorization = principalAuthorization;
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

        public IEnumerable<DateOnlyPeriod> PermittedPeriods(IApplicationFunction applicationFunction, DateOnlyPeriod period, IPerson person)
        {
            return new []{period};
        }

        public IEnumerable<IApplicationFunction> GrantedFunctions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification)
        {
            throw new NotImplementedException();
        }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
            return true;
        }

    	public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return true;
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

        public IEnumerable<DateOnlyPeriod> PermittedPeriods(IApplicationFunction applicationFunction, DateOnlyPeriod period, IPerson person)
        {
            return new List<DateOnlyPeriod>(0);
        }

        public IEnumerable<IApplicationFunction> GrantedFunctions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification)
        {
            throw new NotImplementedException();
        }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
            return false;
        }

    	public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return false;
    	}
    }

    public class CustomAuthorizationContext :IDisposable
    {
        private readonly IPrincipalAuthorization _previousAuthorization;

        public CustomAuthorizationContext(IPrincipalAuthorization principalAuthorization)
        {
            TeleoptiPrincipalForTest teleoptiPrincipalForTest = TeleoptiPrincipal.Current as TeleoptiPrincipalForTest;
            if (teleoptiPrincipalForTest!=null)
            {
                _previousAuthorization = teleoptiPrincipalForTest.PrincipalAuthorization;
                teleoptiPrincipalForTest.SetPrincipalAuthorization(principalAuthorization);
            }
        }

        public void Dispose()
        {
            TeleoptiPrincipalForTest teleoptiPrincipalForTest = TeleoptiPrincipal.Current as TeleoptiPrincipalForTest;
            if (teleoptiPrincipalForTest != null)
            {
                teleoptiPrincipalForTest.SetPrincipalAuthorization(_previousAuthorization);
            }
        }
    }
}
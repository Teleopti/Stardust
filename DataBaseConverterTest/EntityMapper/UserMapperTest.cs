using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Person=Teleopti.Ccc.Domain.Common.Person;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{

    [TestFixture]
    public class UserMapperTest : MapperTest<User>
    {
        private MappedObjectPair _mapped;
        private Agent _oldAgent;
        private User _oldUser;
        private User _oldDomainUser;
        private IApplicationRole _agentRole;
        private IApplicationRole _administratorRole;
        private Person _newApplicationUser;
        private Person _newDomainUser;

        [SetUp]
        public void Setup()
        {
            _mapped = new MappedObjectPair();
            _mapped.Agent = new ObjectPairCollection<Agent, IPerson>();
            _oldAgent = new Agent(-1, "AgentKalle", "Kula", "Kalle@Kula.nu", "", null, null, null, "Test note");
            _oldUser = new User(99, "UserKalle", "Kula", "ukuleleKalle@Kula.nu", 40, _oldAgent, MessageLevel.High, "","Knut", "", "", false);
            _oldDomainUser = new User(99, "UserKalle", "Kula", "ukuleleKalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "Kulanet", "", false);
            _newDomainUser = new Person
                                 {
                                     AuthenticationInfo = new AuthenticationInfo
                                                                     {
                                                                         Identity = @"KulaNet\Knut"
                                                                     }
                                 };
            _newApplicationUser = new Person
                                      {
                                          ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                                              {ApplicationLogOnName = "Knut"}
                                      };
            _agentRole = new ApplicationRole();
            _agentRole.Name = "Agent Role";
            _agentRole.SetId(Guid.NewGuid());
            _administratorRole = new ApplicationRole();
            _administratorRole.Name = "Administrator Role";
            _administratorRole.SetId(Guid.NewGuid());
        }

        [Test]
        public void CanMapsUserThatAlsoIsAgentWithPeriod()
        {
            global::Domain.ICccListCollection<global::Domain.AgentPeriod> agPeriods = new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>();
            global::Domain.DatePeriod datePeriod = new global::Domain.DatePeriod(DateTime.Now);
            global::Domain.UnitSub oldTeam = new global::Domain.UnitSub(-1, "Test", -1, false, null);

            agPeriods.Add(new global::Domain.AgentPeriod(datePeriod, -1, null, null, oldTeam, null, null, "", null, null, DateTime.Now));
            agPeriods.FinishReadingFromDatabase(global::Domain.CollectionType.Locked);

            _oldAgent = new global::Domain.Agent(-1, "AgentKalle", "Kula", "Kalle@Kula.nu", "", null, agPeriods, null, "Test note");

            AgentMapper agentMapper = new AgentMapper(_mapped, (TimeZoneInfo.Utc));
            IPerson newAgent = agentMapper.Map(_oldAgent);
            _mapped.Agent.Add(_oldAgent, newAgent);

            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newUser = uMap.Map(_oldUser);

            //Agent name and email should be used
            Assert.AreEqual("AgentKalle Kula", newUser.Name.ToString());
            Assert.AreEqual("Kalle@Kula.nu", newUser.Email);
            Assert.AreEqual(TimeZoneInfo.Utc, ((PermissionInformation)newUser.PermissionInformation).DefaultTimeZone());
            Assert.AreEqual("Knut", newUser.ApplicationAuthenticationInfo.ApplicationLogOnName);
        }

        [Test]
        public void VerifyUserThatIsNotAgentCanBeMapped()
        {
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "", "", false);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newUser = uMap.Map(_oldUser);

            //do not test dbid
            Assert.AreEqual("UserKalle Kula", newUser.Name.ToString());
            Assert.AreEqual("Kalle@Kula.nu", newUser.Email);
            Assert.AreEqual(TimeZoneInfo.Utc, ((PermissionInformation)newUser.PermissionInformation).DefaultTimeZone());
            Assert.AreEqual("Knut", newUser.ApplicationAuthenticationInfo.ApplicationLogOnName);

        }

        [Test]
        public void UserShouldHavePasswordSameAsUserName()
        {
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "", "EnKnut", false);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newUser = uMap.Map(_oldUser);
            Assert.AreEqual("Knut", newUser.ApplicationAuthenticationInfo.Password);
        }

        [Test]
        public void UserShouldBeAuthenticatedIfHavingDomainAndUser()
        {
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "workgroup", "", false);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newUser = uMap.Map(_oldUser);
            Assert.AreEqual(@"workgroup\Knut", newUser.AuthenticationInfo.Identity);
        }

        [Test]
        public void SameApplicationUserShouldBeConvertedOnlyOnce()
        {
            AgentMapper agentMapper = new AgentMapper(_mapped, (TimeZoneInfo.Utc));
            IPerson newAgent = agentMapper.Map(_oldAgent);
            _mapped.Agent.Add(_oldAgent, newAgent);

            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>(){_newApplicationUser, _newDomainUser});
            IPerson newUser = uMap.Map(_oldUser);
            Assert.AreEqual(_newApplicationUser, newUser);
        }

        [Test]
        public void SameDomainUserShouldBeConvertedOnlyOnce()
        {
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>() { _newApplicationUser, _newDomainUser });
            IPerson newUser = uMap.Map(_oldDomainUser);
            Assert.AreEqual(_newDomainUser, newUser);
        }

        [Test]
        public void DoNotMapUserWithIdMinusOne()
        {
            _oldUser = new User(-1, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "", "", false);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newPerson = uMap.Map(_oldUser);
            Assert.IsNull(newPerson);
        }

        [Test]
        public void VerifyNullIsReturnedIfUserNameIsNullOrEmpty()
        {
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", null, "", "", false);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), new ApplicationRole(), new ApplicationRole(), new List<IPerson>());
            IPerson newPerson = uMap.Map(_oldUser);
            Assert.IsNull(newPerson);

            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "", "", "", false);
            newPerson = uMap.Map(_oldUser);
            Assert.IsNull(newPerson);
        }
        
        [Test]
        public void VerifyMappedUserIsAssignedCorrectDefaultRole()
        {
            var mocks = new MockRepository();
            var stateMock = mocks.StrictMock<IState>();

            var applicationData = StateHolderProxyHelper.CreateApplicationData(mocks.StrictMock<IMessageBroker>());
            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            var sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, applicationData, BusinessUnitFactory.BusinessUnitUsedInTest);

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, sessionData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

            mocks.ReplayAll();

            // Verify user gets admin role
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "workgroup", "", true);
            UserMapper uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            IPerson newUser = uMap.Map(_oldUser);
            Assert.AreEqual(1, newUser.PermissionInformation.ApplicationRoleCollection.Count);
            Assert.AreEqual(_administratorRole.Id, newUser.PermissionInformation.ApplicationRoleCollection[0].Id);

            // Verify user gets agent role
            _oldUser = new User(99, "UserKalle", "Kula", "Kalle@Kula.nu", 40, null, MessageLevel.High, "", "Knut", "workgroup", "", false);
            uMap = new UserMapper(_mapped, (TimeZoneInfo.Utc), _agentRole, _administratorRole, new List<IPerson>());
            newUser = uMap.Map(_oldUser);
            Assert.AreEqual(1, newUser.PermissionInformation.ApplicationRoleCollection.Count);
            Assert.AreEqual(_agentRole.Id, newUser.PermissionInformation.ApplicationRoleCollection[0].Id);


            mocks.VerifyAll();
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 12; }
        }
    }
}
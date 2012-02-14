using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class DynamicAvailableDataProviderTest
    {
        private DynamicAvailableDataProviderTestClass _target;

        private IAvailableDataRepository _availableDataRep;
        private IBusinessUnitRepository _businessUnitRep;
        private IBusinessUnit _businessUnit;
        private IPerson _person;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _availableDataRep = _mocks.StrictMock<IAvailableDataRepository>();
            _businessUnitRep = _mocks.StrictMock<IBusinessUnitRepository>();

            _person = new Person();
            _businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            _target = new DynamicAvailableDataProviderTestClass(_availableDataRep, _businessUnitRep, _person, _businessUnit);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyInputEntityList()
        {
            IList<IApplicationRole> inputRoles = ApplicationRoleFactory.CreateShippedRoles();
            _target.InputEntityList = AuthorizationEntityExtender.ConvertToBaseList(inputRoles);

            IList<IApplicationRole> resultRoles = _target.InputApplicationRoles;

            Assert.AreEqual(inputRoles.Count, resultRoles.Count);
        }

        [Test]
        public void VerifyResultEntityList()
        {
            IList<IApplicationRole> roles = ApplicationRoleFactory.CreateShippedRoles();
            IList<IAvailableData> availableDatas = AvailableDataFactory.CreateAvailableDataList();
            roles[3].AvailableData = availableDatas[0];
            IApplicationRole newRole = new ApplicationRole();
            newRole.AvailableData = availableDatas[1];

            availableDatas[0].AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit;

            _target.InputEntityList = AuthorizationEntityExtender.ConvertToBaseList(roles);

            _mocks.Record();

            Expect.Call(_availableDataRep.LoadAllCollectionsInAvailableData(null)).IgnoreArguments().Return(null).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            IList<IAvailableDataEntry> result = _target.ResultEntityList;

            _mocks.VerifyAll();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void VerifyAddMyOwn()
        {
            BusinessUnit businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            ApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");

             IAvailableData returnData =
                _target.CreateDynamicAvailableData(AvailableDataRangeOption.MyOwn, applicationRole, new Person(), businessUnit, _businessUnitRep);

             Assert.AreEqual(1, returnData.AvailablePersons.Count);
        }

        [Test]
        public void VerifyAddEveryBusinessUnit()
        {
            IBusinessUnit businessUnit1 = BusinessUnitFactory.CreateSimpleBusinessUnit("BusinessUnit1");
            IBusinessUnit businessUnit2 = BusinessUnitFactory.CreateSimpleBusinessUnit("BusinessUnit2");
            IBusinessUnit currentBusinessUnit = businessUnit2;
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");
            IList<IBusinessUnit> repositoryList = new List<IBusinessUnit> { businessUnit1, businessUnit2 };

            _mocks.Record();

            Expect.Call(_businessUnitRep.LoadAllBusinessUnitSortedByName()).Return(repositoryList).Repeat.Once();

            _mocks.ReplayAll();

            IAvailableData returnData = _target.CreateDynamicAvailableData(AvailableDataRangeOption.Everyone, applicationRole, new Person(), currentBusinessUnit, _businessUnitRep);

            Assert.AreEqual(2, returnData.AvailableBusinessUnits.Count);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyAddMyBusinessUnit()
        {
            IBusinessUnit businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");

            IAvailableData returnData =
                _target.CreateDynamicAvailableData(AvailableDataRangeOption.MyBusinessUnit, applicationRole, new Person(), businessUnit, _businessUnitRep);

            Assert.AreEqual(1, returnData.AvailableBusinessUnits.Count);
        }

        [Test]
        public void VerifyAddMyTeam()
        {
            IBusinessUnit businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");
            ITeam okTeam = businessUnit.TeamCollection()[0];
            ITeam notOkTeam = businessUnit.TeamCollection()[1];
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 01, 31), okTeam);
            IPersonPeriod personPeriod2 =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(DateTime.Now.AddDays(1)), notOkTeam);

            Person okAgent = new Person();
            okAgent.AddPersonPeriod(personPeriod);
            okAgent.AddPersonPeriod(personPeriod2);

            IAvailableData returnData =
                _target.CreateDynamicAvailableData(AvailableDataRangeOption.MyTeam, applicationRole, okAgent, businessUnit, _businessUnitRep);

            Assert.AreEqual(1, returnData.AvailableTeams.Count);
        }

        [Test]
        public void VerifyAddMySite()
        {
            IBusinessUnit businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");
            // the two teams must belong to different sites 
            ITeam okTeam = businessUnit.TeamCollection()[0];
            ITeam notOkTeam = businessUnit.TeamCollection()[1];
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 01, 31), okTeam);
            IPersonPeriod personPeriod2 =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(DateTime.Now.AddDays(1)), notOkTeam);

            IPerson okAgent = new Person();
            okAgent.AddPersonPeriod(personPeriod);
            okAgent.AddPersonPeriod(personPeriod2);

            IAvailableData returnData =
                _target.CreateDynamicAvailableData(AvailableDataRangeOption.MySite, applicationRole, okAgent, businessUnit, _businessUnitRep);


            Assert.AreEqual(1, returnData.AvailableSites.Count);
        }
    }
}

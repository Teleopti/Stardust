using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class StaticAvailableDataProviderTest
    {
        private StaticAvailableDataProviderTestClass _target;

        private IAvailableDataRepository _availableDataRep;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _availableDataRep = _mocks.StrictMock<IAvailableDataRepository>();
            _target = new StaticAvailableDataProviderTestClass(_availableDataRep);
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

            _target.InputEntityList = AuthorizationEntityExtender.ConvertToBaseList(roles);

            _mocks.Record();

            Expect.Call(_availableDataRep.LoadAllCollectionsInAvailableData(null)).IgnoreArguments().Return(null).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            IList<IAvailableDataEntry> result = _target.ResultEntityList;

            _mocks.VerifyAll();

            Assert.AreEqual(8, result.Count);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class AllDefinedApplicationFunctionsProviderTest
    {
        private AllDefinedApplicationFunctionsProvider _target;
        private IApplicationFunctionRepository _rep;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _rep = _mocks.StrictMock<IApplicationFunctionRepository>();
            _target = new AllDefinedApplicationFunctionsProvider(_rep);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        [ExpectedExceptionAttribute(typeof(NotImplementedException))]
        public void VerifySetInputEntityList()
        {
            ICollection<IApplicationFunction> list = ApplicationFunctionFactory.CreateApplicationFunctionStructure();
            _target.InputEntityList = new List<IAuthorizationEntity>(list.OfType<IAuthorizationEntity>());
        }

        [Test]
        public void VerifyResultEntityList()
        {

            _mocks.Record();

            IList<IApplicationFunction> expectedList = new List<IApplicationFunction>(ApplicationFunctionFactory.CreateApplicationFunctionStructure());
            Expect.Call(_rep.GetAllApplicationFunctionSortedByCode()).Return(expectedList).Repeat.Once();
            
            _mocks.ReplayAll();

            IList<IApplicationFunction> resultList = _target.ResultEntityList;

            _mocks.VerifyAll();

            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }
        }

    }
}

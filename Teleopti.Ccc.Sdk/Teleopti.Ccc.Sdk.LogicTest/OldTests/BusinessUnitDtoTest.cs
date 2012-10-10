using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class BusinessUnitDtoTest
    {
        private BusinessUnitDto _target;
        private IBusinessUnit _businessUnit;
        private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TheUnit");
            _guid = Guid.NewGuid();
            _businessUnit.SetId(_guid);
			_target = new BusinessUnitDto { Id = _businessUnit.Id, Name = _businessUnit.Name};
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual("TheUnit", _target.Name);
            Assert.AreEqual(_businessUnit.Id, _target.Id);
        }
    }
}
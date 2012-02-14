using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.LogicTest.OldTests.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class SettingDtoTest
    {
        private SettingDto _target;
        private ISetting _setting;

        [SetUp]
        public void Setup()
        {
            MockRepository mock = new MockRepository();
            _setting = mock.CreateMock<ISetting>();


            using (mock.Record())
            {
                Expect.Call(_setting.Id)
                    .Return(GuidFactory.GetGuid())
                    .Repeat.Any();

                Expect.Call(_setting.Name)
                    .Return("Speed")
                    .Repeat.Any();

                Expect.Call(_setting.Value)
                    .Return("100")
                    .Repeat.Any();
            }

            mock.ReplayAll();

            _target = new SettingDto(_setting);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(GuidFactory.GetGuid(), _target.Id);
            Assert.AreEqual(_setting.Name, _target.SettingName);
            Assert.AreEqual(_setting.Value, _target.SettingValue);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("Speed", _target.SettingName);
            Assert.AreEqual("100", _target.SettingValue);
        }
    }
}
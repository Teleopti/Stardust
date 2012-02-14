using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.LogicTest.OldTests.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    /// <summary>
    /// Tests ScenarioDto
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 10/8/2008
    /// </remarks>
    [TestFixture]
    public class ScenarioDtoTest
    {
        private Description _description;
        private Guid _guid;
        private ScenarioDto _target;
        private IScenario _scenario;

        [SetUp]
        public void Setup()
        {
            MockRepository mocks = new MockRepository();
            _scenario = mocks.StrictMock<IScenario>();

            _description = DescriptionFactory.GetDescription();
            _guid = GuidFactory.GetGuid();

            using (mocks.Record())
            {
                Expect.On(_scenario)
                    .Call(_scenario.Description)
                    .Return(_description)
                    .Repeat.Twice();

                Expect.On(_scenario)
                    .Call(_scenario.Id)
                    .Return(_guid)
                    .Repeat.Twice();
            }

            _target = new ScenarioDto(_scenario);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_target.Name,_description.Name);
            Assert.AreEqual(_target.ShortName, _description.ShortName);
            Assert.AreEqual(_target.Id, _scenario.Id);
        }

        [Test]
        public void VerifyCanSetAndGetProperties()
        {
            Assert.AreEqual(_target.Name, _description.Name);
            Assert.AreEqual(_target.ShortName, _description.ShortName);
            Assert.AreEqual(_target.Id, _scenario.Id);
        }
    }
}
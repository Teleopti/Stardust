using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class SiteDtoTest
    {
        private SiteDto _target;
        private Description _description;
        private Guid? _guid;

        [SetUp]
        public void Setup()
        {
            MockRepository mocks = new MockRepository();
            ISite site = mocks.StrictMock<ISite>();

            _description = new Description("a", "b");
            _guid = new Guid("5f4e40e1-2b69-4184-b0c5-4cfe27a7d85f");

            using (mocks.Record())
            {
                Expect.On(site)
                    .Call(site.Description)
                    .Return(_description);

                Expect.On(site)
                    .Call(site.Id)
                    .Return(_guid);
            }

			_target = new SiteDto { DescriptionName = site.Description.Name, Id = site.Id};

        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_description.Name, _target.DescriptionName);
            Assert.AreEqual(_guid, _target.Id);
        }
    }
}
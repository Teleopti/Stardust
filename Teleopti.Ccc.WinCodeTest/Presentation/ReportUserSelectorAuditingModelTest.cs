using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportUserSelectorAuditingModelTest
    {
        private ReportUserSelectorAuditingModel _target;
        private readonly Guid _guid = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _target = new ReportUserSelectorAuditingModel(_guid, UserTexts.Resources.All);   
        }

        [Test]
        public void ShouldReadThePropertiesCorrect()
        {
            Assert.AreEqual(_guid, _target.Id);
            Assert.AreEqual(UserTexts.Resources.All, _target.Text);
        }

        [Test]
        public void ShouldConvertPersonToModelInConstructor()
        {
            IPerson person = new Person();
            person.SetId(Guid.NewGuid());
            person.WithName(new Name("John", "Smith"));

            _target = new ReportUserSelectorAuditingModel(person);

            Assert.AreEqual(person.Id, _target.Id);
            Assert.AreEqual(person.Name.ToString(NameOrderOption.FirstNameLastName), _target.Text);
            Assert.AreEqual(person, _target.Person);
        }

        [Test]
        public void ShouldThrowExceptionIfPersonIdIsNull()
        {
            IPerson person = new Person();
            Assert.Throws<ArgumentNullException>(() => _target = new ReportUserSelectorAuditingModel(person));
        }

        [Test]
        public void ShouldThrowExceptionIfPersonIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new ReportUserSelectorAuditingModel(null));
        }
    }
}

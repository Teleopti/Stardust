using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportUserSelectorAuditingModelTest
    {
        private ReportUserSelectorAuditingModel _target;
        private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _guid = new Guid();
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
            person.Name = new Name("John", "Smith");

            _target = new ReportUserSelectorAuditingModel(person);

            Assert.AreEqual(person.Id, _target.Id);
            Assert.AreEqual(person.Name.ToString(NameOrderOption.FirstNameLastName), _target.Text);
            Assert.AreEqual(person, _target.Person);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfPersonIdIsNull()
        {
            IPerson person = new Person();
            _target = new ReportUserSelectorAuditingModel(person);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfPersonIsNull()
        {
            _target = new ReportUserSelectorAuditingModel(null);
        }
    }
}

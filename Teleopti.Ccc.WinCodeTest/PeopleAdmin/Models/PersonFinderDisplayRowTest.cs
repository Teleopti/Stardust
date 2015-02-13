using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonFinderDisplayRowTest
    {
        private PersonFinderDisplayRow _target;
        private readonly Guid _guid = Guid.NewGuid();
        private string _firstName;
        private string _lastName;
        private string _employmentNumber;
        private string _note;
        private DateTime _terminalDate;

        [SetUp]
        public void Setup()
        {
            _target = new PersonFinderDisplayRow();
            _firstName = "firstName";
            _lastName = "lastName";
            _employmentNumber = "employmentNumber";
            _note = "note";
            _terminalDate = new DateTime(2011, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            _target.PersonId = _guid;
            _target.FirstName = _firstName;
            _target.LastName = _lastName;
            _target.EmploymentNumber = _employmentNumber;
            _target.Note = _note;
            _target.TerminalDate = _terminalDate;
        }

        [Test]
        public void ShouldSetProperties()
        {
            Assert.AreEqual(_guid, _target.PersonId); 
            Assert.AreEqual(_firstName, _target.FirstName);
            Assert.AreEqual(_lastName, _target.LastName);
            Assert.AreEqual(_employmentNumber, _target.EmploymentNumber);
            Assert.AreEqual(_note, _target.Note);
            Assert.AreEqual(_terminalDate, _target.TerminalDate);
        }
    }
}

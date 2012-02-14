using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.PersonAdmin;

namespace Teleopti.Ccc.WinCodeTest.PersonAdmin
{
    [TestFixture]
    public class PersonsInRoleGridDataTest
    {
        private PersonsInRoleGridData target;
        private Person person;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            person = new Person();
            person.Name = new Name("firstName", "lastName");
            target = new PersonsInRoleGridData(person);
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(false, target.IsPersonInRole);
            Assert.AreEqual("firstName", target.FirstName);
            Assert.AreEqual("lastName", target.LastName);
            Assert.AreSame(person, target.Person);

        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyPersonIsNotNull()
        {
            target = new PersonsInRoleGridData(null);
        }

        [Test]
        public void VerifySetIsPersonInRole()
        {
            bool setValue = !target.IsPersonInRole;
            target.IsPersonInRole = setValue;
            Assert.AreEqual(setValue, target.IsPersonInRole);
        }

    }
}

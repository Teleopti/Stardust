using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Payroll
{
    [TestFixture]
    public class PayrollExportTest
    {
        private PayrollExport _target;

        [SetUp]
        public void Setup()
        {
            _target = new PayrollExport();
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            _target.Name = "Kalle Kula";
            _target.FileFormat = ExportFormat.CommaSeparated;
            _target.Period = new DateOnlyPeriod(2000, 1, 1, 2008, 1, 1);
            _target.PayrollFormatId = Guid.NewGuid();
            _target.PayrollFormatName = "din mammas lön";
            Assert.AreEqual("Kalle Kula", _target.Name);
            Assert.IsNotNull(_target.FileFormat);
            Assert.IsNotNull(_target.Persons);
            Assert.IsNotNull(_target.Period);
            Assert.IsNotNull(_target.PayrollFormatId );
            Assert.IsNotNull(_target.PayrollFormatName);
            Assert.IsFalse(_target.IsDeleted);
            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
        }

        [Test]
        public void CanModifyPersons()
        {
            _target.ClearPersons();
            Assert.AreEqual(0, _target.Persons.Count);

            IList<IPerson> persons = new List<IPerson>{PersonFactory.CreatePerson("Kalle Bolle"),PersonFactory.CreatePerson("Östen Me resten")};

            _target.AddPersons(persons);
            Assert.AreEqual(2, _target.Persons.Count);
        }
    }
}

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class PersonAbsenceSorterTest
    {
        private List<IPersonAbsence> target;

        [SetUp]
        public void Setup()
        {
            target = new List<IPersonAbsence>();
        }

        [Test]
        public void VerifySorting()
        {
            IPersonAbsence abs1 = createPersonAbsense(100);
	        abs1.LastChange = null;
            IPersonAbsence abs2 = createPersonAbsense(80);
	        abs2.LastChange = null;
            IPersonAbsence abs3 = createPersonAbsense(80);
            abs3.LastChange = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            IPersonAbsence abs4 = createPersonAbsense(80);
            abs4.LastChange = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            IPersonAbsence abs5 = createPersonAbsense(80);
            abs5.LastChange = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            IPersonAbsence abs6 = createPersonAbsense(70);
            abs6.LastChange = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            target.Add(abs5);
            target.Add(abs2);
            target.Add(abs3);
            target.Add(abs6);
            target.Add(abs1);
            target.Add(abs4);
            target.Sort(new PersonAbsenceSorter());

            Assert.AreSame(abs1, target[0]);
            Assert.AreSame(abs2, target[1]);
            Assert.AreSame(abs5, target[2]);
            Assert.AreSame(abs4, target[3]);
            Assert.AreSame(abs3, target[4]);
            Assert.AreSame(abs6, target[5]);
        }

        private static PersonAbsence createPersonAbsense(byte absPrio)
        {
            Absence abs = new Absence();
            abs.Priority = absPrio;
            PersonAbsence ret = new PersonAbsence(new Person(), new Scenario("sdf"), 
                                                  new AbsenceLayer(abs, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)));
            return ret;
        }
    }
}

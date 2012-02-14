using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AbsenceSorterTest
    {
        private List<IAbsence> _absences;

        [SetUp]
        public void Setup()
        {
            _absences = new List<IAbsence>();
        }

        [Test]
        public void VerifySort()
        {
            IAbsence absence1 = new Absence();
            absence1.Description = new Description("a");

            IAbsence absence2 = new Absence();
            absence2.Description = new Description("b");

            IAbsence absence3 = new Absence();
            absence3.Description = new Description("c");

            _absences.Add(absence2);
            _absences.Add(absence1);
            _absences.Add(absence3);

            _absences.Sort(new AbsenceSorter());

            Assert.AreSame(absence1, _absences[0]);
            Assert.AreSame(absence2, _absences[1]);
            Assert.AreSame(absence3, _absences[2]);
        }
    }
}

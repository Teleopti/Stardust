using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class AbsenceCreatorTest
    {
        private AbsenceCreator _target;

        [SetUp]
        public void Setup()
        {
            _target = new AbsenceCreator();
        }

        [Test]
        public void VerifyCanCreateAbsence()
        {
            IAbsence absence = _target.Create(new Description("name", "shortName"), Color.BlanchedAlmond, 2, false);
            Assert.AreEqual(new Description("name", "shortName"), absence.Description);
            Assert.AreEqual(Color.BlanchedAlmond, absence.DisplayColor);
            Assert.AreEqual(2, absence.Priority);
            Assert.AreEqual(false, absence.Requestable);
        }
    }
}

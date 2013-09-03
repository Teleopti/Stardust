using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class OvertimeShiftActivityLayerTest
    {
        [Test]
        public void HasNonpublicConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(OvertimeShiftActivityLayer)));
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CannotCreateLayerWithDefinitionAsNull()
        {
            new OvertimeShiftActivityLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), null);
        }
    }
}

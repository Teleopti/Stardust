using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class SchedulePartDtoTest
    {
        private SchedulePartDto _schedulePartDto;
        [SetUp]
        public void Setup()
        {
            _schedulePartDto = new SchedulePartDto();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_schedulePartDto.PersonAssignmentCollection);
            Assert.IsNotNull(_schedulePartDto.PersonAbsenceCollection);
            Assert.IsNotNull(_schedulePartDto.ProjectedLayerCollection);
            Assert.IsNotNull(_schedulePartDto.PersonMeetingCollection);
        }
    }
}
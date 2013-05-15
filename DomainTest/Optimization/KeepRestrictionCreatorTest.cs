using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class KeepRestrictionCreatorTest
    {
        private KeepRestrictionCreator _target;
        private MockRepository _mockRepository;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _target = new KeepRestrictionCreator();
            _mockRepository = new MockRepository();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
        }

		[Test]
		public void VerifyCreateKeepShiftCategoryRestriction()
		{
			IPersonAssignment personAssignment = _mockRepository.StrictMock<IPersonAssignment>();
			using (_mockRepository.Record())
			{
				Expect.Call(_schedulePart.AssignmentHighZOrder()).Return(personAssignment);
				Expect.Call(personAssignment.ShiftCategory).Return(new ShiftCategory("TestCategory"));
			}
			using (_mockRepository.Playback())
			{
				Assert.IsNotNull(_target.CreateKeepShiftCategoryRestriction(_schedulePart));
			}
		}

    	[Test]
        public void VerifyCreateKeepStartAndEndTimeRestriction()
        {

            IPersonAssignment personAssignment = _mockRepository.StrictMock<IPersonAssignment>();
            IMainShift mainShift = _mockRepository.StrictMock<IMainShift>();
            IProjectionService projectionService = _mockRepository.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
            IPerson owner = PersonFactory.CreatePersonWithBasicPermissionInfo("TestName", "TestPassword");
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePart.AssignmentHighZOrder()).Return(personAssignment);
                Expect.Call(personAssignment.MainShift).Return(mainShift);
                Expect.Call(mainShift.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.Period()).Return(new DateTimePeriod());
                Expect.Call(_schedulePart.Person).Return(owner);
            }
            using (_mockRepository.Playback())
            {
				Assert.IsNotNull(_target.CreateKeepStartAndEndTimeRestriction(_schedulePart));
            }
        }

    }
}

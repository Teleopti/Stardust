using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class PreferenceAbsenceCheckerTest
    {
        private PreferenceAbsenceChecker _target;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;
        private IPreferenceRestriction _preferenceRestriction;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _target = new PreferenceAbsenceChecker(_scheduleDay);
            _preferenceRestriction = new PreferenceRestriction();
        }

        [Test]
        public void ShouldGetUnspecifiedPreferenceAbsencePermissionStateOnNoPreferenceFullDayAbsence()
        {
            Assert.AreEqual(PermissionState.Unspecified, _target.CheckPreferenceAbsence(_preferenceRestriction, PermissionState.Unspecified));
        }

        [Test]
        public void ShouldGetSatisfiedPreferenceAbsencePermissionStateOnMatchingAbsence()
        {
            var projectionService = _mock.StrictMock<IProjectionService>();
            var visualLayers = _mock.StrictMock<IVisualLayerCollection>();
            var filteredVisualLayers = _mock.StrictMock<IFilteredVisualLayerCollection>();
        	var absence = AbsenceFactory.CreateAbsence("Sickleave");

            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
                Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayers);
                Expect.Call(visualLayers.FilterLayers(absence)).Return(filteredVisualLayers);
            	Expect.Call(filteredVisualLayers.HasLayers).Return(true);
            }

            using (_mock.Playback())
            {
                _target = new PreferenceAbsenceChecker(_scheduleDay);
                _preferenceRestriction.Absence = absence;
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreferenceAbsence(_preferenceRestriction, PermissionState.Unspecified));
            }
        }

        [Test]
        public void ShouldGetBrokenPreferenceAbsencePermissionStateOnNoMatchingAbsence()
        {
            var projectionService = _mock.StrictMock<IProjectionService>();
            var visualLayers = _mock.StrictMock<IVisualLayerCollection>();
			var filteredVisualLayers = _mock.StrictMock<IFilteredVisualLayerCollection>();
			var absence = AbsenceFactory.CreateAbsence("Sickleave");

            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
                Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayers);
                Expect.Call(visualLayers.FilterLayers(absence)).Return(filteredVisualLayers);
            	Expect.Call(filteredVisualLayers.HasLayers).Return(false);
            }

            using (_mock.Playback())
            {
                _target = new PreferenceAbsenceChecker(_scheduleDay);
                _preferenceRestriction.Absence = absence;
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreferenceAbsence(_preferenceRestriction, PermissionState.Satisfied));
            }
        }

        [Test]
        public void ShouldGetBrokenPreferenceAbsencePermissionStateOnNoFullDayAbsence()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            }

            using (_mock.Playback())
            {
                _target = new PreferenceAbsenceChecker(_scheduleDay);
                _preferenceRestriction.Absence = new Absence();
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreferenceAbsence(_preferenceRestriction, PermissionState.Satisfied));
            }
        }
    }
}

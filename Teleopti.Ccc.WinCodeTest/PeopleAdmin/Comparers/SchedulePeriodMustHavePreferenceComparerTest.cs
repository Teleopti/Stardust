using NUnit.Framework;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    [TestFixture]
    public class SchedulePeriodMustHavePreferenceComparerTest
    {
        private SchedulePeriodModel _targetX;
        private SchedulePeriodModel _targetY;
        private SchedulePeriodMustHavePreferenceComparer _comparer;
        private ISchedulePeriod _schedulePeriodX;
        private ISchedulePeriod _schedulePeriodY;
        private MockRepository _mocks;
        private IPerson _person;
        private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _comparer = new SchedulePeriodMustHavePreferenceComparer();
            _mocks = new MockRepository();
            _person = _mocks.StrictMock<IPerson>();
            _schedulePeriodX = _mocks.StrictMock<ISchedulePeriod>();
            _schedulePeriodY = _mocks.StrictMock<ISchedulePeriod>();
            _dateOnly = new DateOnly();
        }

       
        [Test]
        public void ShouldReturnZeroOnEqual()
        {
            using(_mocks.Record())
            {
                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodX);
                Expect.Call(_schedulePeriodX.MustHavePreference).Return(0);

                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodY);
                Expect.Call(_schedulePeriodY.MustHavePreference).Return(0);
            }

            using(_mocks.Playback())
            {
                _targetX = new SchedulePeriodModel(_dateOnly, _person, null);
                _targetY = new SchedulePeriodModel(_dateOnly, _person, null);

                var result = _comparer.Compare(_targetX, _targetY);
                Assert.AreEqual(0, result);    
            }    
        }

        [Test]
        public void ShouldReturnOneWhenXGreaterThanY()
        {
            using (_mocks.Record())
            {
                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodX);
                Expect.Call(_schedulePeriodX.MustHavePreference).Return(1).Repeat.AtLeastOnce();

                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodY).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriodY.MustHavePreference).Return(0);
            }

            using (_mocks.Playback())
            {
                _targetX = new SchedulePeriodModel(_dateOnly, _person, null);
                _targetY = new SchedulePeriodModel(_dateOnly, _person, null);

                var result = _comparer.Compare(_targetX, _targetY);
                Assert.AreEqual(1, result);
            }     
        }

        [Test]
        public void ShouldReturnNegativeOneWhenYGreaterThanX()
        {
            using (_mocks.Record())
            {
                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodX);
                Expect.Call(_schedulePeriodX.MustHavePreference).Return(0).Repeat.AtLeastOnce();

                Expect.Call(_person.SchedulePeriod(_dateOnly)).Return(_schedulePeriodY).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriodY.MustHavePreference).Return(1);
            }

            using (_mocks.Playback())
            {
                _targetX = new SchedulePeriodModel(_dateOnly, _person, null);
                _targetY = new SchedulePeriodModel(_dateOnly, _person, null);

                var result = _comparer.Compare(_targetX, _targetY);
                Assert.AreEqual(-1, result);
            }
        }
    }
}

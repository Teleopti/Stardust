using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture ]
    public class GroupMoveTimeValidatorRunnerTest
    {
        private MockRepository _mock;
        private IGroupMoveTimeValidatorRunner _target;
        private IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
        private IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;
        private IPerson _person;
        private DateOnly _dayMoveFrom;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private List<IScheduleMatrixPro> _allMatrixes;
        private IScheduleDayPro _scheduleDayPro;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _groupOptimizerValidateProposedDatesInSameMatrix =
                _mock.StrictMock<IGroupOptimizerValidateProposedDatesInSameMatrix>();
            _groupOptimizerValidateProposedDatesInSameGroup = _mock.StrictMock<IGroupOptimizerValidateProposedDatesInSameGroup>();
            _target = new GroupMoveTimeValidatorRunner(_groupOptimizerValidateProposedDatesInSameMatrix,
                                                           _groupOptimizerValidateProposedDatesInSameGroup);
            _person = PersonFactory.CreatePerson();
            _dayMoveFrom = DateOnly.MinValue;
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
        }

        [Test]
        public void ShouldReturnFalseIfOneDayIsLocked()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>( )));
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(_scheduleDayPro);
            }

            using(_mock.Playback())
            {
                var result = _target.Run(_person, new List<DateOnly> { _dayMoveFrom }, true, _allMatrixes);
                Assert.IsFalse(result.Success );
            }
        }
    }
}

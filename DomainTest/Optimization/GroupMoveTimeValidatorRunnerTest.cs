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
        private DateOnly _dayMoveTo;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private List<IScheduleMatrixPro> _allMatrixes;

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
            _dayMoveTo = DateOnly.MaxValue;
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
        }

        [Test,Ignore  ]
        public void ShouldRunAllAndReturnTrueIfAllSuccess()
        {
            using (_mock.Record())
            {
                Expect.Call(_groupOptimizerValidateProposedDatesInSameMatrix.Validate(_person, new List<DateOnly> { _dayMoveTo, _dayMoveFrom  },
                                                                              true)).Return(new ValidatorResult { Success = true });
                Expect.Call(_groupOptimizerValidateProposedDatesInSameGroup.Validate(_person, new List<DateOnly> { _dayMoveTo, _dayMoveFrom },
                                                                              true)).Return(new ValidatorResult { Success = true });
                
            }

            ValidatorResult result;
            using (_mock.Playback())
            {
                result = _target.Run(_person, new List<DateOnly> { _dayMoveFrom }, true, _allMatrixes);
            }

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ShouldReturnFalseIfOneDayIsLocked()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>( )));

            }

            ValidatorResult result;
            using(_mock.Playback())
            {
                result = _target.Run(_person, new List<DateOnly> { _dayMoveFrom }, true, _allMatrixes);
                Assert.IsFalse(result.Success );
            }
        }
    }
}

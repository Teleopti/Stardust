using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SmartDayOffBackToLegalStateSolverContainerTest
    {
        private SmartDayOffBackToLegalStateSolverContainer _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
        private IScheduleMatrixOriginalStateContainer _scheduleMatrixOriginalStateContainer;
        private ILockableBitArray _bitArray;
	    private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _smartDayOffBackToLegalStateService =
                _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
            _scheduleMatrixOriginalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _bitArray = new LockableBitArray(5, false, false, null);
            _target = new SmartDayOffBackToLegalStateSolverContainer(_scheduleMatrixOriginalStateContainer, _bitArray, _smartDayOffBackToLegalStateService);
			_daysOffPreferences = new DaysOffPreferences();
        }

        [Test]
        public void VerifyExecute()
        {
            IList<IDayOffBackToLegalStateSolver> solverList = new List<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver solver = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            solverList.Add(solver);
            using (_mocks.Record())
            {
                Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(_bitArray, _daysOffPreferences, 10)).Return(solverList).Repeat.Once();
                Expect.Call(_smartDayOffBackToLegalStateService.Execute(solverList, 20, new List<string>())).IgnoreArguments().Return(true).Repeat.Once();
                Expect.Call(_scheduleMatrixOriginalStateContainer.ScheduleMatrix).Return(_matrix).Repeat.Any();
            }
            _target.Execute(_daysOffPreferences);
            Assert.AreSame(_matrix, _target.MatrixOriginalStateContainer.ScheduleMatrix);
            Assert.IsTrue(_target.Result);
            Assert.AreEqual(5, _target.BitArray.Count);
            Assert.AreEqual(0, _target.FailedSolverDescriptionKeys.Count);
        }
    }
}
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class SmartDayOffBackToLegalStateServiceTest
    {
        private ISmartDayOffBackToLegalStateService _target;
        private MockRepository _mocks;
        private int _maxIterations;
        private IDaysOffPreferences _daysOffPreferences;
    	private IDayOffDecisionMaker _decisionMaker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _maxIterations = 20;
            _daysOffPreferences = new DaysOffPreferences();
            _daysOffPreferences.UseConsecutiveDaysOff = true;
            _daysOffPreferences.UseConsecutiveWorkdays = true;
            _daysOffPreferences.UseDaysOffPerWeek = true;
            _daysOffPreferences.UseWeekEndDaysOff = true;
            _daysOffPreferences.UseFullWeekendsOff = true;
        	_decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _target = new SmartDayOffBackToLegalStateService(_decisionMaker);
        }

        [Test]
        public void VerifyCanGetListOfSolversIncorrectRunOrder()
        {
            LockableBitArray array = createBitArray();
            IList<IDayOffBackToLegalStateSolver> solvers = _target.BuildSolverList(array, _daysOffPreferences, _maxIterations);
            Assert.AreEqual(6, solvers.Count);
            Assert.AreEqual(typeof(FreeWeekendSolver), solvers[0].GetType());
            Assert.AreEqual(typeof(FreeWeekendDaySolver), solvers[1].GetType());
            Assert.AreEqual(typeof(DaysOffPerWeekSolver), solvers[2].GetType());
            Assert.AreEqual(typeof(ConsecutiveDaysOffSolver), solvers[3].GetType());
            Assert.AreEqual(typeof(ConsecutiveWorkdaysSolver), solvers[4].GetType());
            Assert.AreEqual(typeof(TuiCaseSolver), solvers[5].GetType());
        	_daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			solvers = _target.BuildSolverList(array, _daysOffPreferences, _maxIterations);
			Assert.AreEqual(7, solvers.Count);
			Assert.AreEqual(typeof(FiveConsecutiveWorkdaysSolver), solvers[6].GetType());
        }

        [Test]
        public void VerifyExecuteExitsWithFalseWhenAllSolversReturnsTrue()
        {
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s1 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s2 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            solvers.Add(s1);
            solvers.Add(s2);

            using (_mocks.Record())
            {
                Expect.Call(s1.SetToFewBackToLegalState()).Return(true).Repeat.Times(_maxIterations + 1);
                Expect.Call(s1.SetToManyBackToLegalState()).Return(true).Repeat.Times(_maxIterations + 1);
               Expect.Call(s2.SetToFewBackToLegalState()).Return(true).Repeat.Times(_maxIterations + 1);
                Expect.Call(s2.SetToManyBackToLegalState()).Return(true).Repeat.Times(_maxIterations + 1);
               
            }

            bool result;
			
			using (_mocks.Playback())
            {
                result = _target.Execute(solvers, _maxIterations);
            }

            Assert.IsFalse(result);

        }

        [Test]
        public void VerifyExecuteExitsWithTrueWhenAllSolversReturnFalse()
        {
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s1 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s2 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            solvers.Add(s1);
            solvers.Add(s2);

            using (_mocks.Record())
            {
                Expect.Call(s1.SetToFewBackToLegalState()).Return(true).Repeat.Times(1);
                Expect.Call(s1.SetToManyBackToLegalState()).Return(true).Repeat.Times(1);
                Expect.Call(s2.SetToFewBackToLegalState()).Return(false).Repeat.Times(1);
                Expect.Call(s2.SetToManyBackToLegalState()).Return(false).Repeat.Times(1);
                Expect.Call(s1.SetToFewBackToLegalState()).Return(false).Repeat.Times(1);
                Expect.Call(s1.SetToManyBackToLegalState()).Return(false).Repeat.Times(1);
                Expect.Call(s2.SetToFewBackToLegalState()).Return(false).Repeat.Times(1);
                Expect.Call(s2.SetToManyBackToLegalState()).Return(false).Repeat.Times(1);

            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute(solvers, _maxIterations);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyExecuteExitsWithFalseWhenIterationLimitReached()
        {
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s1 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            IDayOffBackToLegalStateSolver s2 = _mocks.StrictMock<IDayOffBackToLegalStateSolver>();
            solvers.Add(s1);
            solvers.Add(s2);

	        const int maxIteration = 1;

            using (_mocks.Record())
            {
				Expect.Call(s1.SetToFewBackToLegalState()).Return(true).Repeat.Times(maxIteration + 1);
				Expect.Call(s1.SetToManyBackToLegalState()).Return(true).Repeat.Times(maxIteration + 1);
				Expect.Call(s2.SetToFewBackToLegalState()).Return(true).Repeat.Times(maxIteration + 1);
				Expect.Call(s2.SetToManyBackToLegalState()).Return(true).Repeat.Times(maxIteration + 1);


            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute(solvers, 1);
            }

            Assert.IsFalse(result);

        }


        private static LockableBitArray createBitArray()
        {
            return new LockableBitArray(1, false, false, null);
        }
    }
}
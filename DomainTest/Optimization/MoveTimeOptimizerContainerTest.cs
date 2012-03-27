using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MoveTimeOptimizerContainerTest
    {
        private MoveTimeOptimizerContainer _target;
        private MockRepository _mocks;

        private IList<IMoveTimeOptimizer> _moveTimeOptimizerList;
        private IMoveTimeOptimizer _moveTimeOptimizer1;
        private IMoveTimeOptimizer _moveTimeOptimizer2;
        private IPeriodValueCalculator _periodValueCalculatorForAllSkills;
        private IPerson _person = PersonFactory.CreatePerson();
        private bool _eventExecuted;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _moveTimeOptimizer1 = _mocks.StrictMock<IMoveTimeOptimizer>();
            _moveTimeOptimizer2 = _mocks.StrictMock<IMoveTimeOptimizer>();
            _moveTimeOptimizerList = new List<IMoveTimeOptimizer> { _moveTimeOptimizer1, _moveTimeOptimizer2 };
            _periodValueCalculatorForAllSkills = _mocks.StrictMock<IPeriodValueCalculator>();
            _target = new MoveTimeOptimizerContainer(_moveTimeOptimizerList, _periodValueCalculatorForAllSkills);
        }

        //[Test]
        //public void VerifyExecuteOneIteration()
        //{
        //    using (_mocks.Record())
        //    {
        //        Expect.Call(_moveTimeOptimizer1.Execute()).Return(false);
        //        Expect.Call(_moveTimeOptimizer2.Execute()).Return(false);

        //        Expect.Call(_moveTimeOptimizer1.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();
        //        Expect.Call(_moveTimeOptimizer2.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();

        //        Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1).Repeat.Any();
        //        Expect.Call(_moveTimeOptimizer1.ContainerOwner).Return(_person).Repeat.Any();
        //        Expect.Call(_moveTimeOptimizer2.ContainerOwner).Return(_person).Repeat.Any();
        //    }
        //    using (_mocks.Playback())
        //    {
        //        _target.Execute();
        //    }
        //}

        [Test]
        public void VerifyReportProgressEventExecuted()
        {
            _target.ReportProgress += new System.EventHandler<ResourceOptimizerProgressEventArgs>(_target_ReportProgress);
            using (_mocks.Record())
            {
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(false);
                Expect.Call(_moveTimeOptimizer2.Execute()).Return(false);

                //Expect.Call(_moveTimeOptimizer1.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();
                //Expect.Call(_moveTimeOptimizer2.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();

                Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1).Repeat.Any();
                Expect.Call(_moveTimeOptimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_moveTimeOptimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.Execute();
                _target.ReportProgress -= new System.EventHandler<ResourceOptimizerProgressEventArgs>(_target_ReportProgress);
                Assert.IsTrue(_eventExecuted);
            }
        }

        void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            _eventExecuted = true;
        }


        [Test]
        public void VerifyExecuteMultipleIterationsWhileOptimizerActive()
        {
            using (_mocks.Record())
            {
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(true);
                Expect.Call(_moveTimeOptimizer2.Execute()).Return(true);
                
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(true);
                Expect.Call(_moveTimeOptimizer2.Execute()).Return(false);

                Expect.Call(_moveTimeOptimizer1.Execute()).Return(false);

                //Expect.Call(_moveTimeOptimizer1.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();
                //Expect.Call(_moveTimeOptimizer2.MovedDaysOverMaxDaysLimit()).Return(true).Repeat.Any();

                Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1).Repeat.Any();
                Expect.Call(_moveTimeOptimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_moveTimeOptimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }


        [Test]
        public void VerifyExecuteMultipleIterationsWhileMaximumMovableDaysExceeds()
        {
            _moveTimeOptimizerList = new List<IMoveTimeOptimizer> { _moveTimeOptimizer1 };
            _target = new MoveTimeOptimizerContainer(_moveTimeOptimizerList, _periodValueCalculatorForAllSkills);

            using (_mocks.Record())
            {
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(true);
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(true);
                Expect.Call(_moveTimeOptimizer1.Execute()).Return(false);
                Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1).Repeat.Any();
                Expect.Call(_moveTimeOptimizer1.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }


    }
}

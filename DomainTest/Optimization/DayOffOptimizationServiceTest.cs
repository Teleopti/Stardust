using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffOptimizationServiceTest
    {
        private IDayOffOptimizationService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IEnumerable<IDayOffOptimizerContainer> _optimizers;
        private IDayOffOptimizerContainer _container1;
        private IDayOffOptimizerContainer _container2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IDayOffOptimizerContainer>();
            _container2 = _mocks.StrictMock<IDayOffOptimizerContainer>();
            _target = new DayOffOptimizationService(_periodValueCalculator);
        }

        [Test]
        public void VerifySuccessfulOptimization()
        {
            _optimizers = new List<IDayOffOptimizerContainer> { _container1, _container2 };
            IPerson owner = PersonFactory.CreatePerson();
            
            using (_mocks.Record())
            {
                // first round 
                Expect.Call(_container1.Execute())
                    .Return(true);
                Expect.Call(_container2.Execute())
                    .Return(true);
                Expect.Call(_container1.Execute())
                    .Return(false);
                Expect.Call(_container2.Execute())
                    .Return(false);

                // second round
                Expect.Call(_container1.Execute())
                    .Return(false);
                Expect.Call(_container2.Execute())
                    .Return(false);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(_container2.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.Execute(_optimizers);
            }
        }

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();

            using (_mocks.Record())
            {
                // only one round 
                Expect.Call(_container1.Execute())
                    .Return(true);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.Execute(_optimizers);
            }
            _target.ReportProgress -= _target_ReportProgress;
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

    }
}
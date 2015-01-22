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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IDayOffOptimizerContainer>();
            _target = new DayOffOptimizationService(_periodValueCalculator);
        }

       

		[Test]
		public void ContainerShouldBeRemovedIfFailed()
		{
			_optimizers = new List<IDayOffOptimizerContainer> { _container1 };
			IPerson owner = PersonFactory.CreatePerson();

			using (_mocks.Record())
			{
				// first round first executes
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
					.Return(10);
				Expect.Call(_container1.Execute())
					.Return(false);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
					.Return(10);


				Expect.Call(_container1.Owner)
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

		[Test]
		public void ShouldUserCancel()
		{
			_target.ReportProgress += _target_ReportProgress2;
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
			_target.ReportProgress -= _target_ReportProgress2;
		}

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

		static void _target_ReportProgress2(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.UserCancel = true;
		}
    }
}
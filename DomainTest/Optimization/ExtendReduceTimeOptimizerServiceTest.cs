﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ExtendReduceTimeOptimizerServiceTest
    {
        private IExtendReduceTimeOptimizerService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IExtendReduceTimeOptimizer _optimizer;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _target = new ExtendReduceTimeOptimizerService(_periodValueCalculator);
            _optimizer = _mocks.StrictMock<IExtendReduceTimeOptimizer>();
            _person = PersonFactory.CreatePerson();

        }

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;

            using (_mocks.Record())
            {
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(
                    30);
                Expect.Call(_optimizer.Execute()).Return(true);

                Expect.Call(_optimizer.Owner).Return(_person);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(
                    20);
            }

            using (_mocks.Playback())
            {
                _target.Execute(new List<IExtendReduceTimeOptimizer> { _optimizer });
            }

			_target.ReportProgress -= _target_ReportProgress;
        }

		[Test]
		public void ShouldUserCancel()
		{
			_target.ReportProgress += _target_ReportProgress1;

			using (_mocks.Record())
			{
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(30);
				Expect.Call(_optimizer.Execute()).Return(true);
				Expect.Call(_optimizer.Owner).Return(_person);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(20);
			}

			using (_mocks.Playback())
			{
				_target.Execute(new List<IExtendReduceTimeOptimizer> { _optimizer });
			}

			_target.ReportProgress -= _target_ReportProgress1;
		}


        [Test]
        public void ShouldExitIfOptimizersFails()
        {
            using (_mocks.Record())
            {
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(
                    30);
                Expect.Call(_optimizer.Execute()).Return(false);

                Expect.Call(_optimizer.Owner).Return(_person);

            }

            using (_mocks.Playback())
            {
                _target.Execute(new List<IExtendReduceTimeOptimizer> { _optimizer });
            }
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

		static void _target_ReportProgress1(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
		}
    }
}
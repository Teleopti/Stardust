using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class IntradayOptimizerContainerTest
    {
        private IntradayOptimizerContainer _target;
        private MockRepository _mocks;

        private IList<IIntradayOptimizer2> _optimizerList;
        private IIntradayOptimizer2 _optimizer1;
        private IIntradayOptimizer2 _optimizer2;
        private IPerson _person = PersonFactory.CreatePerson();
        private bool _eventExecuted;
	    private int _timesExecuted;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _optimizer1 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizer2 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizerList =  new List<IIntradayOptimizer2> { _optimizer1, _optimizer2 };


            _target = new IntradayOptimizerContainer();
	        _timesExecuted = 0;
        }

        [Test]
        public void VerifyReportProgressEventExecuted()
        {
            using (_mocks.Record())
            {
                Expect.Call(_optimizer1.Execute(Enumerable.Empty<DateOnly>())).Return(null);
                Expect.Call(_optimizer2.Execute(Enumerable.Empty<DateOnly>())).Return(null);

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
			{
				_target.ReportProgress += _target_ReportProgress;
				_target.Execute(_optimizerList);
                _target.ReportProgress -= _target_ReportProgress;
                Assert.IsTrue(_eventExecuted);
            }
        }

		[Test]
		public void ShouldUserCancel()
		{
			_target.ReportProgress += targetReportProgress2;
			using (_mocks.Record())
			{
				Expect.Call(_optimizer1.Execute(Enumerable.Empty<DateOnly>())).Return(null).Repeat.Any();
				Expect.Call(_optimizer2.Execute(Enumerable.Empty<DateOnly>())).Return(null).Repeat.Any();
				Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
				Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target.Execute(_optimizerList);
				Assert.AreEqual(1, _timesExecuted);
				_target.ReportProgress -= targetReportProgress2;
			}
		}

        void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            _eventExecuted = true;
        }

		void targetReportProgress2(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
			_timesExecuted++;
		}


        [Test]
        public void VerifyExecuteMultipleIterationsWhileOptimizerActive()
        {
            using (_mocks.Record())
            {
                Expect.Call(_optimizer1.Execute(Enumerable.Empty<DateOnly>())).Return(new DateOnly(2000, 1, 1));
                Expect.Call(_optimizer2.Execute(Enumerable.Empty<DateOnly>())).Return(new DateOnly(2000, 1, 1));
                
                Expect.Call(_optimizer1.Execute(Enumerable.Empty<DateOnly>())).Return(new DateOnly(2000, 1, 1));
                Expect.Call(_optimizer2.Execute(Enumerable.Empty<DateOnly>())).Return(null);

                Expect.Call(_optimizer1.Execute(Enumerable.Empty<DateOnly>())).Return(null);

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
				_target.Execute(_optimizerList);
            }
        }
    }
}

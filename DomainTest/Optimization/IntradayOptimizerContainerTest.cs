using System.Collections.Generic;
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
	    private DateOnlyPeriod _period;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _optimizer1 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizer2 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizerList =  new List<IIntradayOptimizer2> { _optimizer1, _optimizer2 };


            _target = new IntradayOptimizerContainer();
	        _timesExecuted = 0;
		    _period = new DateOnlyPeriod();
        }

        [Test]
        public void VerifyReportProgressEventExecuted()
        {
            using (_mocks.Record())
            {
                Expect.Call(_optimizer1.Execute()).Return(false);
                Expect.Call(_optimizer2.Execute()).Return(false);

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
			{
				_target.ReportProgress += _target_ReportProgress;
				_target.Execute(_optimizerList, _period);
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
				Expect.Call(_optimizer1.Execute()).Return(false).Repeat.Any();
				Expect.Call(_optimizer2.Execute()).Return(false).Repeat.Any();
				Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
				Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target.Execute(_optimizerList, _period);
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
                Expect.Call(_optimizer1.Execute()).Return(true);
                Expect.Call(_optimizer2.Execute()).Return(true);
                
                Expect.Call(_optimizer1.Execute()).Return(true);
                Expect.Call(_optimizer2.Execute()).Return(false);

                Expect.Call(_optimizer1.Execute()).Return(false);

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
				_target.Execute(_optimizerList, _period);
            }
        }
    }
}

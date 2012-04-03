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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _optimizer1 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizer2 = _mocks.StrictMock<IIntradayOptimizer2>();
            _optimizerList = new List<IIntradayOptimizer2> { _optimizer1, _optimizer2 };
            _target = new IntradayOptimizerContainer(_optimizerList);
        }

        //[Test]
        //public void VerifyExecuteOneIteration()
        //{
        //    using (_mocks.Record())
        //    {
        //        Expect.Call(_optimizer1.Execute()).Return(false);
        //        Expect.Call(_optimizer2.Execute()).Return(false);

        //        Expect.Call(_optimizer1.RestrictionsOverMax()).Return(true).Repeat.Any();
        //        Expect.Call(_optimizer2.RestrictionsOverMax()).Return(true).Repeat.Any();

        //        Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
        //        Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
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
                Expect.Call(_optimizer1.Execute()).Return(false);
                Expect.Call(_optimizer2.Execute()).Return(false);

                //Expect.Call(_optimizer1.RestrictionsOverMax()).Return(true).Repeat.Any();
                //Expect.Call(_optimizer2.RestrictionsOverMax()).Return(true).Repeat.Any();

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
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
                Expect.Call(_optimizer1.Execute()).Return(true);
                Expect.Call(_optimizer2.Execute()).Return(true);
                
                Expect.Call(_optimizer1.Execute()).Return(true);
                Expect.Call(_optimizer2.Execute()).Return(false);

                Expect.Call(_optimizer1.Execute()).Return(false);

                //Expect.Call(_optimizer1.RestrictionsOverMax()).Return(true).Repeat.Any();
                //Expect.Call(_optimizer2.RestrictionsOverMax()).Return(true).Repeat.Any();

                Expect.Call(_optimizer1.ContainerOwner).Return(_person).Repeat.Any();
                Expect.Call(_optimizer2.ContainerOwner).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}

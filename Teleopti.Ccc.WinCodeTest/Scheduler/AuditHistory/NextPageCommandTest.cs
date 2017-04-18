using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AuditHistory
{
    [TestFixture]
    public class NextPageCommandTest
    {
        private NextPageCommand _target;
        private MockRepository _mocks;
        private IAuditHistoryModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IAuditHistoryModel>();
            _target = new NextPageCommand(_model);
        }

        [Test]
        public void ExecuteShouldReturnTrueIfNotOnLastPage()
        {
            using(_mocks.Record())
            {
                Expect.Call(_model.CurrentPage).Return(1);
                Expect.Call(_model.NumberOfPages).Return(2);
            }

            using(_mocks.Playback())
            {
                Assert.AreEqual(true, _target.CanExecute());
            }
        }

        [Test]
        public void ExecuteShouldReturnFalseIfOnLastPage()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.CurrentPage).Return(1);
                Expect.Call(_model.NumberOfPages).Return(1);
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(false, _target.CanExecute());
            }
        }

        [Test]
        public void ExecuteShouldRunIfCanExecute()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.CurrentPage).Return(1);
                Expect.Call(_model.NumberOfPages).Return(2);
                Expect.Call(() => _model.Earlier());
            }

            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void ExecuteShouldNotRunIfCannotExecute()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.CurrentPage).Return(1);
                Expect.Call(_model.NumberOfPages).Return(1);
            }

            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
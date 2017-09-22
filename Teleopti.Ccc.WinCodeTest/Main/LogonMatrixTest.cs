using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LogonMatrixTest
    {
        private ILogonMatrix _target;

        [Test]
        public void ShouldCallViewOnError()
        {
            var mock = new MockRepository();
            var factory = mock.DynamicMock<IUnitOfWorkFactory>();
            var view = mock.DynamicMock<ILogonView>();
            var uow = mock.DynamicMock<IUnitOfWork>();
            _target = new LogonMatrix(view);
            
            factory.Expect(f => f.CreateAndOpenUnitOfWork()).Return(uow);
            view.Expect(v => v.Warning("", "")).IgnoreArguments();
            mock.ReplayAll();
            _target.SynchronizeAndLoadMatrixReports(factory);
            mock.VerifyAll();
        }
    }
}
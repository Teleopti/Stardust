using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class SimpleRepositoryCommandTest
    {
        private TesterForCommandModels _models;
        private string _commandText;
        private SimpleRepositoryCommandModel _target;
        private IUnitOfWork _uow;
        private int _executeCalled;
        private bool _canExecute;
        
        private bool CanExecute()
        {
            return _canExecute;
        }

        private void OnExecute(IUnitOfWork uow)
        {
            _uow = uow;
            _executeCalled++;
        }

        [SetUp]
        public void Setup()
        {
            _models = new TesterForCommandModels();
            _executeCalled = 0;
            _canExecute = true;
            _commandText = "xxx";
        }

        [Test]
        public void VerifyText()
        {
            _target = CommandModelFactory.CreateRepositoryCommandModel(OnExecute, null, _commandText);
            Assert.AreEqual(_commandText, _target.Text);
        }

        [Test]
        public void VerifyCanExecute()
        {
            _target = CommandModelFactory.CreateRepositoryCommandModel(OnExecute, null, _commandText);
            Assert.IsTrue(_models.CanExecute(_target), "Can execute by default");

            _target = CommandModelFactory.CreateRepositoryCommandModel(OnExecute, CanExecute, null, _commandText);
            Assert.IsTrue(_models.CanExecute(_target));
            _canExecute = false;
            Assert.IsFalse(_models.CanExecute(_target));

            CanExecuteRoutedEventArgs args = _models.CreateCanExecuteRoutedEventArgs();
            _target.OnQueryEnabled(null, args);
            Assert.IsTrue(args.Handled, "Verify is handled");
        }

        [Test]
        public void VerifyOnExecute()
        {
            var mocker = new MockRepository();
            var unitOfWorkFactory = mocker.StrictMock<IUnitOfWorkFactory>();
            var uow = mocker.StrictMock<IUnitOfWork>();
            using (mocker.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(uow.Dispose).Repeat.Once();
            }
            using (mocker.Playback())
            {
                _target = CommandModelFactory.CreateRepositoryCommandModel(OnExecute, unitOfWorkFactory, _commandText);
                _models.ExecuteCommandModel(_target);
                Assert.AreEqual(uow, _uow, "Check that its the same unitofwork thats provided");
                Assert.AreEqual(1, _executeCalled);
            }
        }
    }
}

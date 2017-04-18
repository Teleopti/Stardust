using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class CommandHelperTest
    {
        private TesterForCommandModels _target;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository=new MockRepository();
            _target = new TesterForCommandModels();
        }
	
        [Test]
        public void VerifyDefaultSettingsWithCreatedEventArgs()
        {
            CanExecuteRoutedEventArgs args = _target.CreateCanExecuteRoutedEventArgs();

            CommandModelForTest model = new CommandModelForTest();
            model.OnQueryEnabled(this, args);
            Assert.IsTrue(args.CanExecute);
            Assert.IsTrue(args.Handled);
        }

        [Test]
        public void VerifyCanTriggerOnExecute()
        {
            CommandModelForTest model = new CommandModelForTest();
            model.HasExecuted = false;
            model.OnExecute(this,_target.CreateExecutedRoutedEventArgs());
            Assert.IsTrue(model.HasExecuted);
        }

        [Test]
        public void VerifyCanInjectUnitOfWorkForRepositoryCommand()
        {
            RepositoryCommandModelForTest model= new RepositoryCommandModelForTest();
            IUnitOfWork uow = _mockRepository.StrictMock<IUnitOfWork>();
            Assert.AreNotEqual(uow, model.UnitOfWork);
            _target.ExecuteRepositoryCommandModel(this,model, uow);
            Assert.AreEqual(uow,model.UnitOfWork);
        }

        private class CommandModelForTest : CommandModel
        {
            public bool HasExecuted { get; set; }

            public override string Text
            {
                get { return "commandModel for test"; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                HasExecuted = true;
            }
        }

        /// <summary>
        /// Class for getting the uow
        /// </summary>
        /// <remarks>
        /// RepositoryCommandModel supplies a uow, instead of setting up a UnitOfWorkFactory etc
        /// we can just supply a UnitOfWork when testing.
        /// Makes testing little easier. This is not really needed when IOC is implemented
        /// </remarks>
        private class RepositoryCommandModelForTest : RepositoryCommandModel
        {
            public RepositoryCommandModelForTest() : base(null)
            {
            }

            public IUnitOfWork UnitOfWork { get; private set; }
            public override string Text
            {
                get { return "commandModel for test"; }
            }

            public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
            {
                UnitOfWork = uow;
            }
        }
    }
}

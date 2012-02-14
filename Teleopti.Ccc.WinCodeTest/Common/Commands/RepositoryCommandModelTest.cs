using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class RepositoryCommandModelTest
    {
        private RepositoryCommandModel _target;
        private MockRepository _mocker;
        private TesterForCommandModels _models;
        private string _text;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _text = "test";
            _mocker = new MockRepository();
            _unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            _models = new TesterForCommandModels();
            _target = new RepositoryCommandModelForTest(_unitOfWorkFactory, _text);
        }

        [Test]
        public void VerifyText()
        {
            Assert.AreEqual(_text,_target.Text);
        }

        [Test]
        public void VerifyCommandGetsTheUnitOfWorkAndDisposesIt()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();

            using(_mocker.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(uow.Dispose).Repeat.Once();
            }
            using(_mocker.Playback())
            {
                _models.ExecuteCommandModel(_target);
                Assert.AreEqual(((RepositoryCommandModelForTest)_target).Rep.UnitOfWork,uow,"Check that its the same unitofwork");
                SetupFixtureForAssembly.ResetStateHolder();
            }
        }

        private class RepositoryCommandModelForTest : RepositoryCommandModel
        {
            private string _text;

            public RepositoryCommandModelForTest(IUnitOfWorkFactory unitOfWorkFactory, string text) : base(unitOfWorkFactory)
            {
                _text = text;    
            }

            public IPushMessageRepository Rep { get; private set; }

            public override void OnExecute(IUnitOfWork uow, object sender,ExecutedRoutedEventArgs e)
            {
                Rep = new PushMessageRepository(uow);
            }

            public override string Text
            {
                get { return _text; }
            }
        }
    }
}

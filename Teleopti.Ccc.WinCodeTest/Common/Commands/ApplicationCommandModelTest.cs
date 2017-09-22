using System;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class ApplicationCommandModelTest
    {
        private ApplicationCommandModel _target;
        private MockRepository _mocker;
        private string _functionPath;
        private TesterForCommandModels _models;

        [SetUp]
        public void Setup()
        {
            _functionPath = "test";
            _models = new TesterForCommandModels();
            _mocker = new MockRepository();
            _target = new ApplicationCommandModelForTest(_functionPath);
        }

        [Test]
        public void VerifyFunctionPath()
        {
            Assert.AreEqual(_functionPath, _target.FunctionPath);
        }

        [Test]
        public void  VerifyCanOnlyExecuteIfPermitted()
        {
            var authorization = _mocker.StrictMock<IAuthorization>();
            using(_mocker.Record())
            {
                Expect.Call(authorization.IsPermitted(_functionPath)).Return(true);
                Expect.Call(authorization.IsPermitted(_functionPath)).Return(false);
            }
            using(_mocker.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(authorization))
                {
                    Assert.IsTrue(_models.CanExecute(_target));
                    Assert.IsFalse(_models.CanExecute(_target));
                }
            }
        }

        [Test]
        public void VerifyCanExecuteHook()
        {
            var authorization = _mocker.StrictMock<IAuthorization>();
            using (_mocker.Record())
            {
                Expect.Call(authorization.IsPermitted(_functionPath)).Return(true);
                Expect.Call(authorization.IsPermitted(_functionPath)).Return(true);
            }
            using (_mocker.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(authorization))
                {
                    Assert.IsTrue(_models.CanExecute(_target));
                    ((ApplicationCommandModelForTest) _target).CanExec = false;
                    Assert.IsFalse(_models.CanExecute(_target));
                }
            }
        }

        [Test]
        public void VerifyFactory()
        {
            ApplicationCommandModel model1 = CommandModelFactory.CreateApplicationCommandModel(testOnExecute,
                                                                                              testCanExecute, "text",
                                                                                              _functionPath);

            ApplicationCommandModel model2 = CommandModelFactory.CreateApplicationCommandModel(testOnExecute
                                                                                              , "text",
                                                                                              _functionPath);
            Assert.IsTrue(_models.CanExecute(model1));
            Assert.IsTrue(_models.CanExecute(model2));
        }

        private bool testCanExecute()
        {
            return true;
        }

        private void testOnExecute()
        {
            //Do nothing.....
        }
    }

    public class ApplicationCommandModelForTest:ApplicationCommandModel
    {
        public bool CanExec { get; set; }
        
        public ApplicationCommandModelForTest(string functionPath)
            : base(functionPath)
        {
            CanExec = true;
        }

        public override string Text
        {
            get { return "test"; }
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override bool CanExecute
        {
            get { return CanExec; }
        }
    }
}

using System;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class SimpleCommandModelTest
    {
        private TesterForCommandModels _models;
        private string _commandText;
        private SimpleCommandModel _target;
      
        private int _executeCalled;
        private bool _canExecute;
        
        private bool CanExecute()
        {
            return _canExecute;
        }

        private void OnExecute()
        {
            _executeCalled++;
        }

        [SetUp]
        public void  Setup()
        {
            _models = new TesterForCommandModels();
            _executeCalled = 0;
            _canExecute = true;
            _commandText = "xxx";
        }

        [Test]
        public void VerifyText()
        {
            _target = CommandModelFactory.CreateCommandModel(OnExecute, _commandText);

            Assert.AreEqual(_commandText,_target.Text);
        }

        [Test]
        public void  VerifyCanExecute()
        {
            _target = CommandModelFactory.CreateCommandModel(OnExecute, _commandText);
            Assert.IsTrue(_models.CanExecute(_target),"Can execute by default");

            _target = CommandModelFactory.CreateCommandModel(OnExecute, CanExecute, _commandText);
            Assert.IsTrue(_models.CanExecute(_target));
            _canExecute = false;
            Assert.IsFalse(_models.CanExecute(_target));

            CanExecuteRoutedEventArgs args = _models.CreateCanExecuteRoutedEventArgs();
            _target.OnQueryEnabled(null,args);
            Assert.IsTrue(args.Handled,"Verify is handled");
        }

        [Test]
        public void VerifyOnExecute()
        {
            _target = CommandModelFactory.CreateCommandModel(OnExecute, _commandText);
            _models.ExecuteCommandModel(_target);
            Assert.AreEqual(1,_executeCalled);
        }

        [Test]
        public void VerifyCanCreateCommandModelWithCustomCommand()
        {
            string info = "RoutedUICommand information";
            RoutedUICommand comm = new RoutedUICommand(info, "comm", typeof (int));
            
            CommandModel model1 = CommandModelFactory.CreateCommandModel(notUsed,"test",comm);
            CommandModel model2 = CommandModelFactory.CreateCommandModel(notUsed,notUsedEither,"test",comm);
            Assert.AreEqual(comm,model1.Command);
            Assert.AreEqual(comm,model2.Command);

            CommandModel model3 = CommandModelFactory.CreateCommandModel(notUsed, comm);
            CommandModel model4 = CommandModelFactory.CreateCommandModel(notUsed,notUsedEither, comm);
            Assert.AreEqual(model3.Text,info,"the text should be sat from command");
            Assert.AreEqual(model4.Text,info,"the text should be sat from command");
        }

        

        private bool notUsedEither()
        {
            throw new NotImplementedException();
        }

        private void notUsed()
        {
            throw new NotImplementedException();
        }
    }
}

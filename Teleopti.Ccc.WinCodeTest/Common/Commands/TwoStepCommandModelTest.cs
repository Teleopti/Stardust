using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class TwoStepCommandModelTest
    {
        private TwoStepCommandModel  _target;
        private TesterForCommandModels _models;
        private string _editText;
        private string _mainCommandText;
        private CommandModel _mainCommand;
        private bool _mainExecuted;
        private bool _mainCanExecute;
        private string _cancelEditText;

        [SetUp]
        public void Setup()
        {
            _mainCanExecute = true;
            _mainCommandText = "mainCommandText";
            _mainCommand = CommandModelFactory.CreateCommandModel(MainCommandExecuted,MainCommandCanExecute, _mainCommandText);
            _editText = "edit";
            _cancelEditText = "cancelEdit";
            _target = new TwoStepCommandModel(_mainCommand,_editText,_cancelEditText);
            _models = new TesterForCommandModels();
        }

        [Test]
        public void VerifyCommandTexts()
        {
            Assert.AreEqual(_editText, _target.EditCommandModel.Text);
            Assert.AreEqual(_cancelEditText, _target.CancelEditCommandModel.Text);
            
            //Check the default values
            _target = new TwoStepCommandModel(_mainCommand);
            Assert.AreEqual(_mainCommand.Text,_target.Text);
            Assert.AreEqual(_mainCommand.DescriptionText,_target.DescriptionText);
            Assert.AreEqual(UserTexts.Resources.Edit,_target.EditCommandModel.Text);
            Assert.AreEqual(_target.CancelEditCommandModel.Text, UserTexts.Resources.Cancel);
        
        }

        [Test]
        public void VerifyEditMode()
        {
            Assert.IsFalse(_target.EditMode,"Editmode should be false by default");
            Assert.IsFalse(_models.CanExecute(_target),"Targets maincommand canot execute if editmode = false");
            _target.EditMode = true;
            Assert.IsTrue(_models.CanExecute(_target));
        }

        [Test]
        public void VerifyEditCommand()
        {
            _models.ExecuteCommandModel(_target.EditCommandModel);
            Assert.IsTrue(_target.EditMode,"EditCommand triggers the EditMode");
            Assert.IsFalse(_models.CanExecute(_target.EditCommandModel),"Cannot execute editcommand when already in editmode");
        }

        [Test]
        public void VerifyCancelEditCommand()
        {
            Assert.IsFalse(_models.CanExecute(_target.CancelEditCommandModel),"Cannot execute when not in editmode");
            _target.EditMode = true;
            Assert.IsTrue(_models.CanExecute(_target.CancelEditCommandModel));
            _models.ExecuteCommandModel(_target.CancelEditCommandModel);
            Assert.IsFalse(_target.EditMode,"Edit is cancelled, editmode = false");
        }

        [Test]
        public void VerifyMainCommandExecutes()
        {
            _target.EditMode = true;
            Assert.IsTrue(_models.CanExecute(_target));
            _models.ExecuteCommandModel(_target);
            Assert.IsTrue(_mainExecuted);
            Assert.IsFalse(_target.EditMode,"Editmode should reset after maincommand is executed");
        }

        [Test]
        public void VerifyMainCommandDelegatesCanExecute()
        {
            _target.EditMode = true;
            Assert.IsTrue(_models.CanExecute(_target));
            _mainCanExecute = false;
            Assert.IsFalse(_models.CanExecute(_target),"The can-exute should be delegated to the mainCommand");
           
        }

        [Test]
        public void VerifyNotifyPropertyChange()
        {
            //The only thing thats important is that Editmode triggers a notification to the UI.
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.EditMode = _target.EditMode;
            Assert.IsFalse(listener.HasFired("EditMode"));
            _target.EditMode = !_target.EditMode;
            Assert.IsTrue(listener.HasOnlyFired("EditMode"));
        }

        #region helpers
        private void MainCommandExecuted()
        {
            _mainExecuted = true;
        }
        private bool MainCommandCanExecute()
        {
            return _mainCanExecute;
        }
        #endregion //helpers
    }
}

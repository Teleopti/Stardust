using System.ComponentModel;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public class TwoStepCommandModel : CommandModel, ITwoStepCommandModel
    {
        #region fields and properties
        private CommandModel _mainCommandModel;
        private bool _editMode;

        public CommandModel EditCommandModel { get; private set; }
        public CommandModel CancelEditCommandModel { get; private set; }
        public bool EditMode
        {
            get { return _editMode; }
            set
            {
                if (_editMode != value)
                {
                    _editMode = value;
                	var handler = PropertyChanged;
                    if (handler != null) 
                        handler(this, new PropertyChangedEventArgs("EditMode"));
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        //delegates to maincommand
        public override string Text
        {
            get { return _mainCommandModel.Text; }
        }

        public override string DescriptionText
        {
            get
            {
                return _mainCommandModel.DescriptionText;
            }
        }

        
        #endregion

        #region constructor
        public TwoStepCommandModel(CommandModel mainCommandModel)
            : this(mainCommandModel, UserTexts.Resources.Edit, UserTexts.Resources.Cancel)
        {
        }

        public TwoStepCommandModel(CommandModel mainCommandModel, string editCommandText, string cancelEditText)
        {
            _mainCommandModel = mainCommandModel;
            EditCommandModel = CommandModelFactory.CreateCommandModel(EditCommandExecuted, EditCommandCanExecute, editCommandText);
            CancelEditCommandModel = CommandModelFactory.CreateCommandModel(CancelEditCommandExecuted,
                                                                            CancelEditCommandModelCanExecute,
                                                              cancelEditText);
            Command = mainCommandModel.Command;
        }
        #endregion

        #region methods
        //delegate to maincommandmodel
        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _mainCommandModel.OnExecute(sender, e);
            EditMode = false;
        }

        public sealed override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            _mainCommandModel.OnQueryEnabled(sender, e);
            e.CanExecute = EditMode && e.CanExecute;
            e.Handled = true;
        }

        //private
        private bool CancelEditCommandModelCanExecute()
        {
            return !EditCommandCanExecute();
        }

        private void CancelEditCommandExecuted()
        {
            EditMode = false;
        }

        private bool EditCommandCanExecute()
        {
            return !EditMode;
        }

        private void EditCommandExecuted()
        {
            EditMode = true;
        }
        #endregion
    }
}

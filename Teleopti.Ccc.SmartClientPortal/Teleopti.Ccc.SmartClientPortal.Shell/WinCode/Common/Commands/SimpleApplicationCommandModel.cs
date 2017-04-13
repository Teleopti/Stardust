using System;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public class SimpleApplicationCommandModel:ApplicationCommandModel
    {
        #region fields
        private readonly Func<bool> _canExecute;
        private readonly Action _onExecute;
        private readonly string _text;
        #endregion

        internal SimpleApplicationCommandModel(Action onExecute, Func<bool> canExecute, string text,string functionPath) : base(functionPath)
        {
            _text = text;
            _canExecute = canExecute;
            _onExecute = onExecute;
        }

      

        internal SimpleApplicationCommandModel(Action onExecute, string text,string functionPath)
            : this(onExecute, CanExecuteDefault, text, functionPath)
        {

        }

        internal SimpleApplicationCommandModel(Action onExecute, RoutedUICommand command, string functionPath)
            : this(onExecute,command.Text,functionPath)
        {
           Command = command;
        }

        internal SimpleApplicationCommandModel(Action onExecute, Func<bool> canExecute,RoutedUICommand command, string functionPath)
            : this(onExecute, canExecute, command.Text, functionPath)
        {
            Command = command;
        }

        #region CommandModel
        public override string Text
        {
            get { return _text; }
        }

        protected override bool CanExecute
        {
            get
            {
                return _canExecute();
            }
        }
        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _onExecute();
        }
        #endregion

        private static bool CanExecuteDefault()
        {
            return true;
        }
       
    }
}

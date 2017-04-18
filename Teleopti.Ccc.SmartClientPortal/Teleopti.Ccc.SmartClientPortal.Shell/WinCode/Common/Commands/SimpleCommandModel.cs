using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands
{
    /// <summary>
    /// SimpleCommand that just takes an action and a function
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-20
    /// </remarks>
    public class SimpleCommandModel : CommandModel, INotifyPropertyChanged
    {
        #region fields
        private readonly Func<bool> _canExecute;
        private readonly Action _onExecute;
        private readonly string _text;
        #endregion

        internal SimpleCommandModel(Action onExecute, Func<bool> canExecute, string text)
        {
            _text = text;
            _canExecute = canExecute;
            _onExecute = onExecute;
        }

        internal SimpleCommandModel(Action onExecute, string text)
            : this(onExecute,CanExecuteDefault, text)
        {

        }

        internal SimpleCommandModel(Action onExecute, Func<bool> canExecute, string text,RoutedUICommand command):this(onExecute,canExecute,text)
        {
            Command = command;
        }

        internal SimpleCommandModel(Action onExecute, string text, RoutedUICommand command)
            : this(onExecute, CanExecuteDefault,text,command)
        {
           
        }

        #region CommandModel
        public override string Text
        {
            get { return _text; }
        }
        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _canExecute();
            e.Handled = true;
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

//        //Just for fixing mem leaks in WPF
//#pragma warning disable 0067
//        public event PropertyChangedEventHandler PropertyChanged;
//#pragma warning restore 0067
        
    }
}

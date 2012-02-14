﻿using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public abstract class ApplicationCommandModel:CommandModel
    {
        private string _functionPath;
        private bool _canExecute = true;

        protected ApplicationCommandModel(string functionPath)
        {
            _functionPath = functionPath;
        }

        public sealed override void OnQueryEnabled(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(_functionPath) && CanExecute;
            e.Handled = true;
        }

        protected virtual bool CanExecute
        {
            get { return _canExecute; }
        }

        public string FunctionPath
        {
            get { return _functionPath; }
           
        }
    }
}

using System;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    /// <summary>
    /// Simple repositorycommand executes an Action that is provided in the constructor.
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-27
    /// </remarks>
    public class SimpleRepositoryCommandModel:RepositoryCommandModel
    {
        private readonly Func<bool> _canExecute;
        private readonly Action<IUnitOfWork> _onExecute;
        private readonly string _text;

        internal SimpleRepositoryCommandModel(Action<IUnitOfWork> execute, IUnitOfWorkFactory unitOfWorkFactory, string text)
            : this(execute, CanExecuteDefault, unitOfWorkFactory, text)
        {
           
        }

        internal SimpleRepositoryCommandModel(Action<IUnitOfWork> execute, Func<bool> canExecute, IUnitOfWorkFactory unitOfWorkFactory, string text) : base(unitOfWorkFactory)
        {
            _onExecute = execute;
            _canExecute = canExecute;
            _text = text;
        }

        public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
        {
            _onExecute(uow);
        }

        public override void OnQueryEnabled(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _canExecute();
            e.Handled = true;
        }

        public override string Text
        {
            get { return _text; }
        }

        private static bool CanExecuteDefault()
        {
            return true;
        }
    }
}

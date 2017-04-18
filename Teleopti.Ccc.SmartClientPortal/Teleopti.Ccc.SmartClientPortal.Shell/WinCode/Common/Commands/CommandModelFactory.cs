using System;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands
{
    public static class CommandModelFactory
    {
        #region repositorycommand

        /// <summary>
        /// Creates the RepositoryCommandModel.
        /// </summary>
        /// <param name="execute">The execute method that is called when executed, provides a UnitOfWork thats disposed</param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="text">The text for buttons, menues etc</param>
        /// <returns></returns>
        /// <remarks>
        /// Can always execute
        /// Created by: henrika
        /// Created date: 2009-05-27
        /// </remarks>
        public static SimpleRepositoryCommandModel CreateRepositoryCommandModel(Action<IUnitOfWork> execute, IUnitOfWorkFactory unitOfWorkFactory, string text)
        {
            return new SimpleRepositoryCommandModel(execute, unitOfWorkFactory, text);
        }

        /// <summary>
        /// Creates the RepositoryCommandModel.
        /// </summary>
        /// <param name="execute">The execute method that is called when executed, provides a UnitOfWork thats disposed</param>
        /// <param name="canExecute">The method for checking if the command can execute.</param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="text">The text for buttons, menues etc</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-27
        /// </remarks>
        public static SimpleRepositoryCommandModel CreateRepositoryCommandModel(Action<IUnitOfWork> execute, Func<bool> canExecute, IUnitOfWorkFactory unitOfWorkFactory, string text)
        {
            return new SimpleRepositoryCommandModel(execute, canExecute, unitOfWorkFactory, text);
        }
        #endregion

        #region simplecommand
        /// <summary>
        /// Creates the CommandModel.
        /// </summary>
        /// <param name="onExecute">The execute method that is called when executed.</param>
        /// <param name="canExecute">The method for checking if the command can execute.</param>
        /// <param name="text">The text for buttons, menues etc</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-27
        /// </remarks>
        public static SimpleCommandModel CreateCommandModel(Action onExecute, Func<bool> canExecute, string text)
        {
            return  new SimpleCommandModel(onExecute, canExecute, text);
        }

        /// <summary>
        /// Creates the CommandModel.
        /// </summary>
        /// <param name="onExecute">The execute method that is called when executed.</param>
        /// <param name="text">The text for buttons, menues etc</param>
        /// <returns></returns>
        /// <remarks>
        /// Can always execute
        /// Created by: henrika
        /// Created date: 2009-05-27
        /// </remarks>
        public static SimpleCommandModel CreateCommandModel(Action onExecute, string text)
        {
            return new SimpleCommandModel(onExecute, text);
        }
        
        /// <summary>
        /// Creates the command model with a certain command
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="text">The text.</param>
        /// <param name="command">The command.</param>
        public static SimpleCommandModel CreateCommandModel(Action onExecute, Func<bool> canExecute, string text,  RoutedUICommand command)
        {
            return new SimpleCommandModel(onExecute, canExecute, text, command);
        }

        /// <summary>
        /// Creates the command model with a certain command
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="text">The text.</param>
        /// <param name="command">The command.</param>
        public static SimpleCommandModel CreateCommandModel(Action onExecute, string text, RoutedUICommand command)
        {
            return new SimpleCommandModel(onExecute, text, command);
        }

        /// <summary>
        /// Creates the command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        /// <remarks>
        /// Sets the text from the RoutedUICommand
        /// </remarks>
        public static CommandModel CreateCommandModel(Action onExecute, Func<bool> canExecute, RoutedUICommand command)
        {
            return new SimpleCommandModel(onExecute, canExecute, command.Text, command);
        }

        /// <summary>
        /// Creates the command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        /// <remarks>
        /// Sets the text from the RoutedUICommand
        /// </remarks>
        public static CommandModel CreateCommandModel(Action onExecute, RoutedUICommand command)
        {
            return new SimpleCommandModel(onExecute, command.Text, command);
        }

        #endregion

        #region applicationcommand
        /// <summary>
        /// Creates the application command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="text">The text.</param>
        /// <param name="functionPath">The function path.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-07-08
        /// </remarks>
        public static SimpleApplicationCommandModel CreateApplicationCommandModel(Action onExecute, Func<bool> canExecute, string text,string functionPath)
        {
            return new SimpleApplicationCommandModel(onExecute, canExecute, text,functionPath);
        }

       
        /// <summary>
        /// Creates the application command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="text">The text.</param>
        /// <param name="functionPath">The function path.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-07-08
        /// </remarks>
        public static SimpleApplicationCommandModel CreateApplicationCommandModel(Action onExecute, string text,string functionPath)
        {
            return new SimpleApplicationCommandModel(onExecute, text,functionPath);
        }

        /// <summary>
        /// Creates the application command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="command">The command.</param>
        /// <param name="functionPath">The function path.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-11-17
        /// </remarks>
        public static SimpleApplicationCommandModel CreateApplicationCommandModel(Action onExecute, RoutedUICommand command,string functionPath)
        {
            return new SimpleApplicationCommandModel(onExecute, command, functionPath);
        }

        /// <summary>
        /// Creates the application command model.
        /// </summary>
        /// <param name="onExecute">The on execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="command">The command.</param>
        /// <param name="functionPath">The function path.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-11-18
        /// </remarks>
        public static SimpleApplicationCommandModel CreateApplicationCommandModel(Action onExecute,Func<bool> canExecute, RoutedUICommand command, string functionPath)
        {
            return new SimpleApplicationCommandModel(onExecute,canExecute, command, functionPath);
        }

        #endregion

       
    }
}

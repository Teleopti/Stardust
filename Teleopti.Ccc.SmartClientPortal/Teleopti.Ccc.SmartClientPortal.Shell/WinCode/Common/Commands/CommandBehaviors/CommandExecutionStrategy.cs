using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands.CommandBehaviors
{
    /// <summary>
    /// Executes a command 
    /// </summary>
    public class CommandExecutionStrategy : IExecutionStrategy
    {
        #region IExecutionStrategy Members
        /// <summary>
        /// Gets or sets the Behavior that we execute this strategy
        /// </summary>
        public CommandBehaviorBinding Behavior { get; set; }

        /// <summary>
        /// Executes the Command that is stored in the CommandProperty of the CommandExecution
        /// </summary>
        /// <param name="parameter">The parameter for the command</param>
        public void Execute(object parameter)
        {
            if (Behavior == null)
                throw new InvalidOperationException("Behavior property cannot be null when executing a strategy");

            if (Behavior.Command.CanExecute(Behavior.CommandParameter))
                Behavior.Command.Execute(Behavior.CommandParameter);
        }

        #endregion
    }

   

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;


namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Invokes WpfCommands from ToolStripItems
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-04-09
    /// </remarks>
    public static class CommandRouter
    {
        private static IInputElement _target;

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-04-09
        /// </remarks>
        public static IInputElement Target
        {
            set 
            { 
                CommandRouter._target = value; 
            }
            get { return CommandRouter._target; }
        }


        /// <summary>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="command">The command.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-04-09
        /// </remarks>
        public static void SetCommand(ToolStripItem item, ICommand command)
        {
            item.Tag = command;
        }

        /// <summary>
        /// Gets the assigned command, returns null if not set
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-04-09
        /// </remarks>
        public static ICommand GetCommand(ToolStripItem item)
        {
            ICommand retCommand = item.Tag as ICommand;
            return retCommand;
        }
    }
}

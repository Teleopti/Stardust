using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands.CommandBehaviors
{
    /// <summary>
    /// For handling commands on Expander-events
    /// </summary>
    /// <remarks>
    /// todo: move all event-commands to one generic class or something like that...
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public static class ExpanderBehavior
    {
        public static readonly DependencyProperty ExpanderExpandedCommand =
           EventBehaviorFactory.CreateCommandExecutionEventBehavior(Expander.ExpandedEvent, "ExpanderExpandedCommand", typeof(ExpanderBehavior));

        public static void SetExpanderExpandedCommand(DependencyObject commandTarget, ICommand value)
        {
            commandTarget.SetValue(ExpanderExpandedCommand, value);
        }

        public static ICommand GetExpanderExpandedCommand(DependencyObject commandTarget)
        {
            return commandTarget.GetValue(ExpanderExpandedCommand) as ICommand;
        }
    }
}

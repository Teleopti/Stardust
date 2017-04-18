using System.Windows;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands.CommandBehaviors
{

    /// <summary>
    /// For handling commands on UIElement-events
    /// </summary>
    /// <remarks>
    /// todo: move all event-commands to one generic class or something like that...
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public static class UIElementBehavior
    {
        public static readonly DependencyProperty UIElementMouseDownCommand =
           EventBehaviorFactory.CreateCommandExecutionEventBehavior(UIElement.MouseDownEvent, "UIElementMouseDownCommand", typeof(UIElementBehavior));

        public static void SetUIElementMouseDownCommand(DependencyObject commandTarget, ICommand value)
        {
            commandTarget.SetValue(UIElementMouseDownCommand, value);
        }

        public static ICommand GetUIElementMouseDownCommand(DependencyObject commandTarget)
        {
            return commandTarget.GetValue(UIElementMouseDownCommand) as ICommand;
        }
    }
}

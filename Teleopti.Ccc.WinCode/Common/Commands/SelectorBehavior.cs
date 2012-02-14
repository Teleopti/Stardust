using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public static class SelectorBehavior
    {
        public static readonly DependencyProperty SelectionChangedCommand =
            EventBehaviorFactory.CreateCommandExecutionEventBehavior(Selector.SelectionChangedEvent, "SelectionChangedCommand", typeof(SelectorBehavior));

        public static void SetSelectionChangedCommand(DependencyObject commandTarget, ICommand value)
        {
            commandTarget.SetValue(SelectionChangedCommand, value);
        }

        public static ICommand GetSelectionChangedCommand(DependencyObject commandTarget)
        {
            return commandTarget.GetValue(SelectionChangedCommand) as ICommand;
        }
    }
}

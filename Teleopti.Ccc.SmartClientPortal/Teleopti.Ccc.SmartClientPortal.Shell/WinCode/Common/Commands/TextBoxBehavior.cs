using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty TextChangedCommand =
            EventBehaviorFactory.CreateCommandExecutionEventBehavior(TextBoxBase.TextChangedEvent, "TextChangedCommand", typeof(TextBoxBehavior));

        public static void SetTextChangedCommand(DependencyObject commandTarget, ICommand value)
        {
            commandTarget.SetValue(TextChangedCommand, value);
        }

        public static ICommand GetTextChangedCommand(DependencyObject commandTarget)
        {
            return commandTarget.GetValue(TextChangedCommand) as ICommand;
        }
    }
}

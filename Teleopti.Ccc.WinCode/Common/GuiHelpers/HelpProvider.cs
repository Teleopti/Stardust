using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Provides help for F1
    /// </summary>
    /// <remarks>
    /// Registers a new CommandBinding for ApplicationCommands.Help on the specified FrameworkElement
    /// </remarks>
    public static class HelpProvider
    {
        public static readonly DependencyProperty HelpStringProperty
            = DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HelpProvider()
        {
            CommandManager.RegisterClassInputBinding(typeof(UIElement), new InputBinding(ApplicationCommands.Help, new KeyGesture(Key.F1, ModifierKeys.Shift)));
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(ApplicationCommands.Help, Executed,CanExecute));
        }

        public static void SetHelpString(DependencyObject obj, string value)
        {
            obj.SetValue(HelpStringProperty, value);
        }

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            UIElement senderElement = sender as UIElement;
            if (GetHelpString(senderElement) != null)
            {
                e.CanExecute = true;
            }
        }

        public static string GetHelpString(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpStringProperty);
        }

        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var topic = GetHelpString(sender as UIElement);

            string helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrl"];
            string helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpPrefix"];
            string helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpSuffix"];
            string helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpDivider"];
            string helpLang = string.Empty; //empty for now

            if (Keyboard.Modifiers != ModifierKeys.Shift)
            {
                helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrlOnline"];
                helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpPrefixOnline"];
                helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpSuffixOnline"];
                helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpDividerOnline"];
            }
            Help.ShowHelp(null, string.Concat(helpUrl, helpLang, helpPrefix, topic.Replace("+",helpDivider), helpSuffix));
        }
    }
}
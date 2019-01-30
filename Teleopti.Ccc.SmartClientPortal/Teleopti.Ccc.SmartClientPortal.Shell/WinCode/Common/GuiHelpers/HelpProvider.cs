using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Interop;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers
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
	        var result = tryGetHelpString(sender as UIElement);
			e.CanExecute = !string.IsNullOrEmpty(result);
        }

		private static string tryGetHelpString(object element)
		{
			if (element == null) return null;
			var result = GetHelpString(element as DependencyObject);
			if (result != null)
			{
				return result;
			}

			var adorner = element as AdornerDecorator;
			if (adorner != null)
			{
				return tryGetHelpString(adorner.Child);
			}

			var host = element as IMultipleHostControl;
			if (host != null)
			{
				return tryGetHelpString(host.Model != null ? host.Model.CurrentItem : null);
			}

			var panel = element as System.Windows.Controls.Panel;
			if (panel != null)
			{
				return tryGetHelpString(panel.Children.OfType<UIElement>().FirstOrDefault());
			}

			return null;
		}

        public static string GetHelpString(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpStringProperty);
        }

        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var topic = tryGetHelpString(sender as UIElement);

			var helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpUrlOnline"];
			var helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpPrefixOnline"];
			var helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpSuffixOnline"];
			var helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpDividerOnline"];
			var helpLang = string.Empty; //empty for now

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
				helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpUrl"];
				helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpPrefix"];
				helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpSuffix"];
				helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpDivider"];
            }
            Help.ShowHelp(null, string.Concat(helpUrl, helpLang, helpPrefix, topic.Replace("+",helpDivider), helpSuffix));
        }
    }
}
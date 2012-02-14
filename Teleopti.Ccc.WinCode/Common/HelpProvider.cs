using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Common
{
    public static class HelpProvider
    {

        public static readonly DependencyProperty HelpStringProperty
        = DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HelpProvider()
        {
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement), new
                                                                                      CommandBinding(
                                                                                      ApplicationCommands.Help, Executed,
                                                                                      CanExecute));


            //HelpStringProperty = DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));
        }

        public static void SetHelpString(DependencyObject obj, string value)
        {
            obj.SetValue(HelpStringProperty, value);
        }

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
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
            //FrameworkElement s = sender as FrameworkElement;

            var text = GetHelpString(sender as FrameworkElement);

            string helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrl"];
            string helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpPrefix"];
            Help.ShowHelp(null, helpUrl + helpPrefix + text);
        }
    }
}
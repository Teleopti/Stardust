using System;
using System.Windows;
using System.Windows.Forms.Integration;
using Syncfusion.Windows.Shared;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

using FlowDirection=System.Windows.FlowDirection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop
{
    public static class WpfExtensions
    {
        /// <summary>
        /// Shows a wpf windows
        /// </summary>
        /// <param name="window">The window.</param>
        /// <remarks>
        /// Use this instead of Show, it enables textinput, right to left
        /// Add SetTexts?
        /// Created by: henrika
        /// Created date: 2009-06-10
        /// </remarks>
        public static void ShowFromWinForms(this Window window, TimeZoneInfo timeZoneInfo)
        {
            SetupWpfWindow(window, true, timeZoneInfo);
            window.Show();
        }

        //Bug in syncfusion makes databindings go crazy, can start window without visualstyle
        public static void ShowFromWinForms(this Window window, bool useVisualStyle, TimeZoneInfo timeZoneInfo)
        {
            SetupWpfWindow(window, useVisualStyle, timeZoneInfo);
            window.Show();
        }

        private static void SetupWpfWindow(Window window, bool useVisualStyle, TimeZoneInfo timeZoneInfo)
        {
            //todos.....
            //timezone
            //set help?

            if (StateHolderReader.IsInitialized)
            {
                //Set right to left:
                window.FlowDirection =
                    (((ITeleoptiPrincipalForLegacy)TeleoptiPrincipalForLegacy.CurrentPrincipal).UnsafePerson.PermissionInformation.RightToLeftDisplay) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                
                //Set timezone:
                //VisualTreeTimeZoneInfo.SetTimeZoneInfo(window, StateHolder.Instance.StateReader.SessionScopeData.TimeZone);
				VisualTreeTimeZoneInfo.SetTimeZoneInfo(window, timeZoneInfo);
				
                //Set ui-culture
                //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(TeleoptiPrincipal.Current.Regional.Culture.ToString())));
             
            }

            var mvvm = new ResourceDictionary {Source = new Uri("/Teleopti.Ccc.SmartClientPortal.Shell;component/Win/WpfControls/Resources/MVVM.xaml", UriKind.RelativeOrAbsolute)};
            var commonResources = new ResourceDictionary { Source = new Uri("/Teleopti.Ccc.SmartClientPortal.Shell;component/Win/WpfControls/Resources/Common.xaml", UriKind.RelativeOrAbsolute) };
            var syncfusionWpf = new ResourceDictionary { Source = new Uri("/syncfusion.Shared.WPF;component/SkinManager/SkinManager.xaml", UriKind.RelativeOrAbsolute) };
            window.Resources.MergedDictionaries.Add(mvvm);
            window.Resources.MergedDictionaries.Add(commonResources);
            window.Resources.MergedDictionaries.Add(syncfusionWpf);

            if(useVisualStyle)
                SkinStorage.SetVisualStyle(window, "Office2007Blue");
            
            ElementHost.EnableModelessKeyboardInterop(window);
        }

        public static bool? ShowDialogFromWinForms(this Window window, bool useVisualStyle, TimeZoneInfo timeZoneInfo)
        {
            SetupWpfWindow(window, useVisualStyle, timeZoneInfo);
            return  window.ShowDialog();
        }
    }
}

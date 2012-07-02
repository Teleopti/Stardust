#region Imports

using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using System.Globalization;

#endregion

namespace Teleopti.Ccc.AgentPortal.Common
{
    public enum HelpType
    {
        Http,
        Html,
        Popup,
        CompiledHelp
    }

    public static class HelpHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _http = "http://localhost/TeleoptiCCC/ContextHelp/";
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _divider = "+";
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _prefix = "f01:";
        private static readonly string _suffix = string.Empty;

        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string
            _httpOnline = "http://onlinehelp.teleopti.com/ccc/";
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _dividerOnline = "+";

        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _prefixOnline = "f01:";

        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly string _suffixOnline = string.Empty;

        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static string _helpLang = "en";
        
        private const HelpType _helpType = HelpType.Http;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HelpHelper()
        {
            //Read help settings from config file
            string helpUrl = StateHolder.Instance.StateReader.SessionScopeData.AppSettings["HelpUrl"];
            if (!string.IsNullOrEmpty(helpUrl)) _http = helpUrl;
            string helpPrefix = StateHolder.Instance.StateReader.SessionScopeData.AppSettings["HelpPrefix"];
            if (!string.IsNullOrEmpty(helpPrefix)) _prefix = helpPrefix;
            string helpSuffix = StateHolder.Instance.StateReader.SessionScopeData.AppSettings["HelpSuffix"]; 
            if (!string.IsNullOrEmpty(helpPrefix)) _suffix = helpSuffix;
            string helpDivider = StateHolder.Instance.StateReader.SessionScopeData.AppSettings["HelpDivider"]; 
            if (!string.IsNullOrEmpty(helpDivider)) _divider = helpDivider;

            string helpUrlOnline;
            string helpPrefixOnline;
            string helpSuffixOnline;
            string helpDividerOnline;
            StateHolder.Instance.StateReader.SessionScopeData.AppSettings.TryGetValue("HelpUrlOnline", out helpUrlOnline);
            if (!string.IsNullOrEmpty(helpUrlOnline)) _httpOnline = helpUrlOnline;
            StateHolder.Instance.StateReader.SessionScopeData.AppSettings.TryGetValue("HelpPrefixOnline", out helpPrefixOnline);
            if (!string.IsNullOrEmpty(helpPrefixOnline)) _prefixOnline = helpPrefixOnline;
            StateHolder.Instance.StateReader.SessionScopeData.AppSettings.TryGetValue("HelpSuffixOnline", out helpSuffixOnline);
            if (!string.IsNullOrEmpty(helpSuffixOnline)) _suffixOnline = helpSuffixOnline;
            StateHolder.Instance.StateReader.SessionScopeData.AppSettings.TryGetValue("HelpDividerOnline", out helpDividerOnline);
            if (!string.IsNullOrEmpty(helpDividerOnline)) _dividerOnline = helpDividerOnline;


            //Select language for help

            string helpCulture = string.Format(CultureInfo.CurrentCulture, "{0}", CultureInfo.CurrentUICulture );
            if (!string.IsNullOrEmpty(helpCulture)) _helpLang = helpCulture.Substring(0, 2);
           
        /*
        temporary hard-code
        _helpLang = "en/"; 
        */
            switch (_helpLang)
            {
                case "sv":
                    _helpLang = "sv/";
                    break;
                case "de":
                    _helpLang = "de/";
                    break;
                case "ru":
                    _helpLang = "ru/";
                    break;
						/*
							case "zh":
								_helpLang = "zh/";
								break;
				   /* */
                default:
                    _helpLang = "en/";
                    break;
            }

          /*  */

        }

        private static void HelpHttp(string formName, IHelpContext control)
        {
            Help.ShowHelp(null, GetUrl(formName, control));
        }

        private static void HelpOnlineHttp(string formName, IHelpContext control)
        {
            Help.ShowHelp(null, GetOnlineUrl(formName, control));
        }

        private static string GetUrl(string formName, IHelpContext control)
        {
            string topic = GetTopic(formName, control);
            topic = HttpUtility.UrlEncode(topic);
            return string.Concat(_http,_helpLang, _prefix, topic, _suffix);
        }

        private static string GetOnlineUrl(string formName, IHelpContext control)
        {
            string topic = GetTopicUrlOnline(formName, control);
            topic = HttpUtility.UrlEncode(topic);
            return string.Concat(_httpOnline, _helpLang, _prefixOnline, topic, _suffixOnline);
        }

        private static string GetTopicUrlOnline(string formName, IHelpContext control)
        {
            string controlName = null;
            if (control != null) controlName = control.Name;
            if (string.IsNullOrEmpty(controlName)) controlName = "Main";
            return formName + _dividerOnline + controlName;
        }
        
        private static string GetTopic(string formName, IHelpContext control)
        {
            string controlName = null;
            if (control != null) controlName = control.Name;
            if (string.IsNullOrEmpty(controlName)) controlName = "Main";
            return formName + _divider + controlName;
        }

        public static void GetHelp(Control form, IHelpContext control, bool local)
        {
            switch (_helpType)
            {
                case HelpType.Http:
                    if (local)
                        HelpHttp(form.Name, control);
                    else
                        HelpOnlineHttp(form.Name, control);
                    break;
            }
        }
    }
}
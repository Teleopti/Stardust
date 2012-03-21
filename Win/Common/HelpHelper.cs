using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using System.Globalization;

namespace Teleopti.Ccc.Win.Common
{
	public class HelpHelper : IHelpHelper
	{
		private static HelpHelper _current;

		public static HelpHelper Current {get
		{
			if (_current == null)
				_current = new HelpHelper();
			return _current;
		} }

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _http = "http://localhost/TeleoptiCCC/ContextHelp/";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _httpOnline = "http://onlinehelp.teleopti.com/ccc/dev";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")] 
		private readonly string _divider = "_";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")] 
		private readonly string _prefix = "f01_";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _suffix = ".html";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _dividerOnline = "+";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _prefixOnline = "f01:";

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _suffixOnline = string.Empty;

		private const HelpType _helpType = HelpType.Http;

		[SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		private readonly string _helpLang = "en";

  
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		private HelpHelper()
		{
			//Read help settings from config file
			string helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrl"];
			if (!string.IsNullOrEmpty(helpUrl)) _http = helpUrl;
			string helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpPrefix"];
			if (!string.IsNullOrEmpty(helpPrefix)) _prefix = helpPrefix;
			string helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpSuffix"];
			if (!string.IsNullOrEmpty(helpSuffix)) _suffix = helpSuffix;
			string helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpDivider"];
			if (!string.IsNullOrEmpty(helpDivider)) _divider = helpDivider;

			string helpUrlOnline = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrlOnline"];
			if (!string.IsNullOrEmpty(helpUrlOnline)) _httpOnline = helpUrlOnline;
			string helpPrefixOnline = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpPrefixOnline"];
			if (!string.IsNullOrEmpty(helpPrefixOnline)) _prefixOnline = helpPrefixOnline;
			string helpSuffixOnline = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpSuffixOnline"];
			if (!string.IsNullOrEmpty(helpSuffixOnline)) _suffixOnline = helpSuffixOnline;
			string helpDividerOnline = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpDividerOnline"];
			if (!string.IsNullOrEmpty(helpDividerOnline)) _dividerOnline = helpDividerOnline;

			//Select language for help

			string helpCulture = string.Format(CultureInfo.CurrentCulture, "{0}", TeleoptiPrincipal.Current.Regional.UICulture);
			if (!string.IsNullOrEmpty(helpCulture)) _helpLang = helpCulture.Substring(0, 2);

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
				   */ 
				   default:
					  _helpLang = "en/";
					   break;
				}

			/*    temporary hard-code */
			/*   _helpLang = "en/"; /*  */ 

		}

		private void HelpHttp(string formName, IHelpContext control)
		{
			Help.ShowHelp(null, GetUrl(formName, control));
		}
		private void HelpOnlineHttp(string formName, IHelpContext control)
		{
			Help.ShowHelp(null, GetOnlineUrl(formName, control));
		}

		private string GetUrl(string formName, IHelpContext control)
		{
			var topic = GetTopic(formName, control);
			topic = HttpUtility.UrlEncode(topic);
			topic = topic.Replace(".", "_");
			var url = string.Concat(_http, _helpLang, _prefix, topic, _suffix);
			return url;
		}
		private string GetOnlineUrl(string formName, IHelpContext control)
		{
			string topic = GetTopicUrlOnline(formName, control);
			topic = HttpUtility.UrlEncode(topic);
			//return string.Concat(_httpOnline, "dev/", _helpLang, _prefixOnline, topic, _suffixOnline);
			return string.Concat(_httpOnline, _helpLang, _prefixOnline, topic, _suffixOnline);
		}
		private string GetTopicUrlOnline(string formName, IHelpContext control)
		{
			string controlName = null;
			if (control != null) controlName = control.HelpId;
			if (string.IsNullOrEmpty(controlName)) controlName = "Main";
			return formName + _dividerOnline + controlName;
		}
		private string GetTopic(string formName, IHelpContext control)
		{
			string controlName = null;
			if (control != null) controlName = control.HelpId;
			if (string.IsNullOrEmpty(controlName)) controlName = "Main";
			return formName + _divider + controlName;
		}

		public void GetHelp(IHelpForm form, IHelpContext helpContext, bool local)
		{
			switch (_helpType)
			{
				case HelpType.Http:
					if (local)
						HelpHttp(form.HelpId, helpContext);
					else
						HelpOnlineHttp(form.HelpId, helpContext);
					break;
			}
		}

		public void GetHelp(BaseUserControl userControl, IHelpContext helpContext, bool local)
		{
			switch (_helpType)
			{
				case HelpType.Http:
					if (local)
						HelpHttp(userControl.HelpId, helpContext);
					else
						HelpOnlineHttp(userControl.HelpId, helpContext);
					break;
			}
		}
	}
}
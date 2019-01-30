using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	public class HelpHelper : IHelpHelper
	{
		private static HelpHelper _current;

		public static HelpHelper Current {get { return _current ?? (_current = new HelpHelper()); } }

        private readonly string _http;
		private readonly string _httpOnline;
		private readonly string _divider;
		private readonly string _prefix;
		private readonly string _suffix  = string.Empty;
		private readonly string _dividerOnline;
		private readonly string _prefixOnline;
		private readonly string _suffixOnline  = string.Empty;
		private const HelpType _helpType = HelpType.Http;
		private readonly string _helpLang = string.Empty;
	    
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		private HelpHelper()
		{
			//Read help settings from config file
            string helpUrl = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpUrl"];
			if (!string.IsNullOrEmpty(helpUrl)) _http = helpUrl;
			string helpPrefix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpPrefix"];
			if (!string.IsNullOrEmpty(helpPrefix)) _prefix = helpPrefix;
			string helpSuffix = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpSuffix"];
			if (!string.IsNullOrEmpty(helpSuffix)) _suffix = helpSuffix;
			string helpDivider = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpDivider"];
			if (!string.IsNullOrEmpty(helpDivider)) _divider = helpDivider;

			string helpUrlOnline = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpUrlOnline"];
			if (!string.IsNullOrEmpty(helpUrlOnline)) _httpOnline = helpUrlOnline;
			string helpPrefixOnline = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpPrefixOnline"];
			if (!string.IsNullOrEmpty(helpPrefixOnline)) _prefixOnline = helpPrefixOnline;
			string helpSuffixOnline = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpSuffixOnline"];
			if (!string.IsNullOrEmpty(helpSuffixOnline)) _suffixOnline = helpSuffixOnline;
			string helpDividerOnline = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["HelpDividerOnline"];
			if (!string.IsNullOrEmpty(helpDividerOnline)) _dividerOnline = helpDividerOnline;

			//Select language is handled in Wiki site
			_helpLang = string.Empty;

		}

		private void HelpHttp(string formName, IHelpContext control)
		{
			var theControl = control as Control;
			var controlHelpContext = control as ControlHelpContext;
			if (controlHelpContext != null) theControl = controlHelpContext.Control;
		
			Help.ShowHelp(theControl, GetUrl(formName, control));
		}

		private void HelpOnlineHttp(string formName, IHelpContext control)
		{
			Help.ShowHelp(null, GetOnlineUrl(formName, control));
		}

	    private string GetUrl(string formName, IHelpContext control)
	    {
	        var topic = GetTopic(formName, control);
            return string.Concat(_http, _helpLang, _prefix, topic, _suffix);
	        
	    }

	    private string GetOnlineUrl(string formName, IHelpContext control)
		{
			var topic = GetTopicUrlOnline(formName, control);
			topic = HttpUtility.UrlEncode(topic);
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
	}
}
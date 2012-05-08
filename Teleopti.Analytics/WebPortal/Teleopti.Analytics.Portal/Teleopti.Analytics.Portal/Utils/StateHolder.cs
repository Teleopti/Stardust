using System.Collections;
using System.Web;

namespace Teleopti.Analytics.Portal.Utils
{
	public static class StateHolder
	{
		public static bool DoForceFormsLogOn
		{
			get
			{
				if (HttpContext.Current.Session["FORCEFORMSLOGIN"] != null)
					return (bool) HttpContext.Current.Session["FORCEFORMSLOGIN"];

				if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["FORCEFORMSLOGIN"]))
					HttpContext.Current.Session["FORCEFORMSLOGIN"] = HttpContext.Current.Request.QueryString["FORCEFORMSLOGIN"] == "true";
				else
					HttpContext.Current.Session["FORCEFORMSLOGIN"] = false;

				return (bool)HttpContext.Current.Session["FORCEFORMSLOGIN"];
			}
		}

		public static UserInfo UserObject
		{
			get
			{
				if (HttpContext.Current.Session["USER"] == null)
					return null;

				return (UserInfo) HttpContext.Current.Session["USER"];
			} 
			set { HttpContext.Current.Session["USER"] = value; }
		}

		public static string UserName
		{
			get
			{
				if (HttpContext.Current.Session["USERNAME"] == null)
					return null;

				return (string)HttpContext.Current.Session["USERNAME"];
			}
			set { HttpContext.Current.Session["USERNAME"] = value; }
		}

		public static Hashtable ReportInstanceCache
		{
			get
			{
				if (HttpContext.Current.Session["InstanceCache"] == null)
					HttpContext.Current.Session["InstanceCache"] = new Hashtable();

				return (Hashtable)HttpContext.Current.Session["InstanceCache"];
			}
		}

		public static bool IsPmDeleteMode
		{
			get
			{
				if (HttpContext.Current.Session["DeleteMode"] == null)
					return false;

				return (bool)HttpContext.Current.Session["DeleteMode"];
			}
			set { HttpContext.Current.Session["DeleteMode"] = value; }
		}
	}
}
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Threading;
using Teleopti.Analytics.Parameters;
using Teleopti.Analytics.Portal.Utils;
using System.Globalization;

namespace Teleopti.Analytics.Portal
{
    public class MatrixBasePage : Page
    {
        private Guid _reportId;

    	public MatrixBasePage()
		{
			IsBrowseTargetPerformanceManager = false;
		}

        void Page_Init(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["PM"]))
            {
				if (Request.QueryString["PM"] == "1")
				{
					IsBrowseTargetPerformanceManager = true;
				}
            }
            if (!string.IsNullOrEmpty(Request.QueryString.Get("REPORTID")))
            {
                if (!TryParseGuid(Request.QueryString["REPORTID"], out _reportId))
                return; 

                var commonReports = new CommonReports(ConnectionString, _reportId);
                Guid groupPageComboBoxControlCollectionId = commonReports.GetGroupPageComboBoxControlCollectionId();
                string groupPageComboBoxControlCollectionIdName = string.Format("Parameter$Drop{0}", groupPageComboBoxControlCollectionId);
            	
				GroupPageCode = string.IsNullOrEmpty(Request.Form.Get(groupPageComboBoxControlCollectionIdName))
            	                	? Selector.BusinessHierarchyCode
            	                	: new Guid(Request.Form.Get(groupPageComboBoxControlCollectionIdName));
                Context.Session["GroupPageCode"] = GroupPageCode;
            }


            if (!string.IsNullOrEmpty(Request.QueryString.Get("BUID")))
            {
                BusinessUnitCode = new Guid(Request.QueryString["BUID"]);
            }

            if (string.IsNullOrEmpty(Request.QueryString["FORCEFORMSLOGIN"]) == false)
            {
				Context.Session["FORCEFORMSLOGIN"] = false;
				if (Request.QueryString["FORCEFORMSLOGIN"] == "true")
				{
					Context.Session["FORCEFORMSLOGIN"] = true;
					if (Context.Session["USERNAME"] == null)
					{
						var sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");
						if (sec.Mode == AuthenticationMode.Windows)
							Response.Redirect(LoginUrl());
					}
				}
            }

            setCulture();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool TryParseGuid(string reportId, out Guid guid)
        {
            try
            {
                guid = new Guid(reportId);
            }
            catch (Exception)
            {
                guid = new Guid();
                return false;
            }
            return true;
        }
        private void setCulture()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(TheUser.LangId, false);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(TheUser.CultureId, false);
        }

        protected int LangId
        {
            get { return TheUser.LangId; }
        }

        private UserInfo TheUser
        {
            get
            {
                if (Context.Session["USER"] == null)
                {
                	if (HttpContext.Current.User == null && Context.Session["USERNAME"] == null)
                		Response.Redirect(LoginUrl());
                	
					if (HttpContext.Current.User != null && Context.Session["USERNAME"] == null)
                		Context.Session["USERNAME"] = HttpContext.Current.User.Identity.Name;
                	
					var loginUtil = new LogOnUtilities(ConnectionString);
                    DataTable t = loginUtil.GetUserInfo((string)Context.Session["USERNAME"]);
                    if (t.Rows.Count > 0)
                    {
                        DataRow row = t.Rows[0];

                        var inf = new UserInfo(row.Field<string>("UserName"), row.Field<int>("LangID"),
                                                    row.Field<int>("CultureID"), row.Field<Guid>("UserID"),
                                                    row.Field<string>("PersonName"));
                        Context.Session["USER"] = inf;
                    }
                    else
                    {
                        Response.Redirect(LoginUrl());
                    }
                }
                return (UserInfo)Context.Session["USER"];

            }
        }

        private string LoginUrl()
        {
            return string.Format("Login.aspx{0}", GetQueryString());
        }

        private string GetQueryString()
        {
            return string.Concat("?", Request.QueryString.ToString());
        }

        protected string LoggedOnUserInformation
        { get { return TheUser.PersonName + " (" + TheUser.UserName + ")"; } }

        protected static string ConnectionString
        {

            get { return ConfigurationManager.ConnectionStrings["Database"].ConnectionString; }
        }

        protected static string OlapConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Cube"].ConnectionString; }
        }

        protected Guid UserCode
        {
            get
            {
                return TheUser.UserId;
            }
        }

        protected Guid ReportId
        {
            get { return _reportId; }
        }

        protected Guid BusinessUnitCode { get; private set; }

    	/// <summary>
    	/// Gets the id (code) of the group page.
    	/// </summary>
    	/// <value>The id of the group page.</value>
    	/// <remarks>
    	/// Created by: henryg
    	/// Created date: 2010-09-02
    	/// </remarks>
    	protected Guid GroupPageCode { get; private set; }

		protected bool IsBrowseTargetPerformanceManager { get; private set; }

		protected string PerformanceManagerUrl { get { return "~/PmContainer.aspx"; } }
    }
}

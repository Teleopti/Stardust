using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Threading;
using Teleopti.Analytics.Parameters;
using Teleopti.Analytics.Portal.Utils;
using System.Globalization;

namespace Teleopti.Analytics.Portal
{
	public class MatrixBasePage : Page
	{
		[CLSCompliant(false)]
		protected Guid _reportId;

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
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out _reportId))
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

			if (StateHolder.DoForceFormsLogOn)
			{
				if (StateHolder.UserName == null)
				{
					var sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");
					if (sec.Mode == AuthenticationMode.Windows && !Page.IsPostBack)
						Response.Redirect(LoginUrl());
				}
			}

			setCulture();
		}

		private void setCulture()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(theUser.LangId, false).FixPersianCulture();
			Thread.CurrentThread.CurrentCulture = new CultureInfo(theUser.CultureId, false).FixPersianCulture();
		}

		protected int LangId
		{
			get { return theUser.LangId; }
		}

		private UserInfo theUser
		{
			get
			{
				if (StateHolder.UserObject == null)
				{
					if (HttpContext.Current.User == null && StateHolder.UserName == null)
						Response.Redirect(LoginUrl());

					if (StateHolder.DoForceFormsLogOn && StateHolder.UserName == null)
						Response.Redirect(LoginUrl());

					if (HttpContext.Current.User != null && StateHolder.UserName == null)
						StateHolder.UserName = HttpContext.Current.User.Identity.Name;

					var loginUtil = new LogOnUtilities(ConnectionString);
					DataTable t = loginUtil.GetUserInfo(StateHolder.UserName);
					if (t.Rows.Count > 0)
					{
						DataRow row = t.Rows[0];

						var inf = new UserInfo(row.Field<string>("UserName"), row.Field<int>("LangID"),
															 row.Field<int>("CultureID"), row.Field<Guid>("UserID"),
															 row.Field<string>("PersonName"));
						StateHolder.UserObject = inf;
					}
					else
					{
						Response.Redirect(LoginUrl());
					}
				}
				return StateHolder.UserObject;

			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		protected string LoginUrl()
		{
			return string.Format("~/Login.aspx{0}", QueryStringWithPrefix);
		}

		protected virtual string QueryStringWithPrefix
		{
			get { return string.Concat("?", Request.QueryString.ToString()); }
		}

		protected string LoggedOnUserInformation
		{ get { return theUser.PersonName + " (" + theUser.UserName + ")"; } }

		protected static string ConnectionString
		{

			get { return ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString; }
		}

		protected static string OlapConnectionString
		{
			get { return ConfigurationManager.ConnectionStrings["Cube"].ConnectionString; }
		}

		protected Guid UserCode
		{
			get
			{
				return theUser.UserId;
			}
		}

		protected Guid ReportId
		{
			get { return _reportId; }
		}

		protected Guid BusinessUnitCode { get; set; }

		/// <summary>
		/// Gets the id (code) of the group page.
		/// </summary>
		/// <value>The id of the group page.</value>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2010-09-02
		/// </remarks>
		protected Guid GroupPageCode { get; set; }

		protected bool IsBrowseTargetPerformanceManager { get; private set; }

		protected string PerformanceManagerUrl
		{
			get { return string.Format(CultureInfo.InvariantCulture, "~/PmContainer.aspx{0}", QueryStringWithPrefix); }
		}

		protected void SignOut(object sender, EventArgs e)
		{
			FormsAuthentication.SignOut();
			StateHolder.UserObject = null;
			StateHolder.UserName = "";
			HttpContext.Current.Session["FORCEFORMSLOGIN"] = true;
			Response.Redirect(LoginUrl());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
//using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.Reporting.WebForms;
using Teleopti.Analytics.Portal.Utils;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
    public partial class ReportViewer : MatrixBasePage
    {
        private CommonReports _commonReports;

	    protected void Page_Load(object sender, EventArgs e)
	    {
		    if (IsPostBack)
		    {
			    return;
		    }
		    _commonReports = new CommonReports(ConnectionString, ReportId);

		    try
		    {
			    if ((Request.QueryString.Get("Ready") != null))
			    {
				    if (Request.QueryString["Ready"].ToUpper(CultureInfo.CurrentCulture) == "YES")
				    {
					    FillViaQuery();
				    }
			    }
			    else
			    {
				    FillViaParameters();
			    }
		    }
		    catch (SqlException exception)
		    {
			    if (exception.ErrorCode != -2146232060) return;
			    Response.Write(Request.UrlReferrer != null
				                   ? string.Format(CultureInfo.InvariantCulture, "<script> alert('{0}');" +
				                                                                 "window.top.location='{1}'</script>",
				                                   Resources.DatabaseTimedOutSelectLessData, Request.UrlReferrer)
				                   : "<script> alert('Connection to database timed out, try selecting less data');</script>");
		    }
	    }

	    private IList<SqlParameter> SessionParameters
        {
            get
            {
                if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
                {
                    return (IList<SqlParameter>) Session["PARAMETERS" + Request.QueryString.Get("PARAMETERSKEY")];
                }

                return new List<SqlParameter>();
            }
            set
            {
                if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
                {
                    Session["PARAMETERS" + Request.QueryString.Get("PARAMETERSKEY")] = value;
                }
            }
        }

        private IList<string> SessionParameterTexts
        {
            get
            {
                if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
                {
                    var temp = (IList<string>)Session["PARAMETERTEXTS" + Request.QueryString.Get("PARAMETERSKEY")];
                    return temp.Select(i => HttpUtility.HtmlDecode(i)).ToList();
                }

                return new List<string>();
            }
        }

        private void FillViaQuery()
        {
            SessionParameters = GetSqlParametersFromQueryString(Request.QueryString);
            FillViaParameters();
        }

        private void FillViaParameters()
        {            
            _commonReports.LoadReportInfo(ReportId);

            if (_commonReports.ReportFileName.EndsWith(".aspx"))
            {
                // Web report - redirect
                Server.Transfer(_commonReports.ReportFileName, true);
            }
            else
            {
                IList<SqlParameter> parameters = SessionParameters;

                DataSet dataset = _commonReports.GetReportData(ReportId, UserCode, BusinessUnitCode, parameters);
                FillReport(dataset);
            }
        }

	    private static IList<SqlParameter> GetSqlParametersFromQueryString(NameValueCollection queryString)
	    {
		    var parameters = new List<SqlParameter>();
		    foreach (string k in queryString.Keys)
		    {
			    if (k.StartsWith("@", StringComparison.CurrentCultureIgnoreCase) & k != "@report_type")
			    {
				    parameters.Add(CreateParameter(k, queryString[k]));
			    }

		    }
		    return parameters;
	    }

	    private static SqlParameter CreateParameter(string name, string value)
        {
            return new SqlParameter(name, value);
        }

        private void FillReport(DataSet theDataset)
        {
            string reportName = _commonReports.ReportFileName;
            string reportPath = Server.MapPath(reportName);
            ReportViewer1.ShowPrintButton = true;

            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            // Set RDLC file
            ReportViewer1.LocalReport.ReportPath = reportPath;
            ReportViewer1.HyperlinkTarget = "ViewerFrame";
            //"pageFrame"
            ReportViewer1.LocalReport.EnableHyperlinks = true;

            IList<ReportParameter> @params = new List<ReportParameter>();           
           
            ReportParameterInfoCollection repInfos = ReportViewer1.LocalReport.GetParameters();
            IList<string> texts = new List<string>();

            if (SessionParameterTexts != null)
            {
                texts = SessionParameterTexts;
            }
            IList<SqlParameter> sqlParams = SessionParameters;
            foreach (ReportParameterInfo repInfo in repInfos)
            {
                int i = 0;
                var added = false;
                foreach (SqlParameter param in sqlParams)
                {
                    if (repInfo.Name.StartsWith("Res",StringComparison.CurrentCultureIgnoreCase))
                    {
                        @params.Add(new ReportParameter(repInfo.Name,Resources.ResourceManager.GetString(repInfo.Name), false));
                        added = true;
                    }
                    if (param.ParameterName.ToLower(CultureInfo.CurrentCulture) == "@" + repInfo.Name.ToLower(CultureInfo.CurrentCulture))
                    {
	                    @params.Add(new ReportParameter(repInfo.Name, texts[i], false));
	                    added = true;
                    }
	                if (!added && repInfo.Name == "culture")
                        @params.Add(new ReportParameter("culture",Thread.CurrentThread.CurrentCulture.IetfLanguageTag, false));
                    i += 1;
                }
            }
 
            ReportViewer1.LocalReport.SetParameters(@params);

            //The first in the report has to hve the name DataSet1
            theDataset.Tables[0].TableName = "DataSet1";
            foreach (DataTable t in theDataset.Tables)
            {
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(t.TableName, t));
            }
        }
    }
}

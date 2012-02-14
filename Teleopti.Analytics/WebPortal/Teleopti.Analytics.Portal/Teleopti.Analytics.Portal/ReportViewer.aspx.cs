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
		List<SqlParameter> parameters = new List<SqlParameter>();
		foreach (string k in queryString.Keys) {
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

            IList<ReportParameter> @params = new List<ReportParameter>
                                                 {
                                                     new ReportParameter("culture",
                                                                         //#12578
                                                                         //Thread.CurrentThread.CurrentCulture.Parent.IetfLanguageTag, false)
                                                                         Thread.CurrentThread.CurrentCulture.IetfLanguageTag, false)
                                                 };

            //@params.Add(new ReportParameter("culture", Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, false));
            //@params.Add(new ReportParameter("culture", "sv", false));
            
           
            ReportParameterInfoCollection repInfos = ReportViewer1.LocalReport.GetParameters();
            IList<string> texts = new List<string>();
            if (repInfos.Count > 3)
            {
                if (SessionParameterTexts != null)
                {
                    texts = SessionParameterTexts;
                }
                IList<SqlParameter> sqlParams = SessionParameters;
                foreach (ReportParameterInfo repInfo in repInfos)
                {
                    int i = 0;
                    foreach (SqlParameter param in sqlParams)
                    {
                        if (repInfo.Name.StartsWith("Res",StringComparison.CurrentCultureIgnoreCase))
                        {
                            @params.Add(new ReportParameter(repInfo.Name,ReportTexts.Resources.ResourceManager.GetString(repInfo.Name), false));
                        }
                        if (param.ParameterName.ToLower(CultureInfo.CurrentCulture) == "@" + repInfo.Name.ToLower(CultureInfo.CurrentCulture))
                        {
                            if (texts != null)
                            {
                                @params.Add(new ReportParameter(repInfo.Name, texts[i], false));
                            }
                            else
                            {
                                @params.Add(new ReportParameter(repInfo.Name, param.Value.ToString(), false));
                            }
                        }
                        i += 1;
                    }
                    //if (repInfo.Name == "ReportName")
                    //{
                     //   @params.Add(new ReportParameter(repInfo.Name, fill.ReportTitle(ReportID), false));
                   // }
                }
                //


            }

            ReportViewer1.LocalReport.SetParameters(@params);

            //The first in the report has to hve the name DataSet1
            theDataset.Tables[0].TableName = "DataSet1";
            foreach (DataTable t in theDataset.Tables)
            {
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(t.TableName, t));
            }
            //ReportViewer1.Height = 
            //Response.Write("FillReport Före DataBind")
            //Return
        	//ReportViewer1.DataBind();

        }

        
    }
}

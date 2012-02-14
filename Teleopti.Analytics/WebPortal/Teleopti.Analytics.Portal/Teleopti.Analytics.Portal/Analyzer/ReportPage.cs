using System;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Teleopti.Analytics.Portal.AnalyzerAPI;

namespace Teleopti.Analytics.Portal.Analyzer
{
    /// <summary>
    /// Summary description for ReportPage
    /// </summary>
    public class ReportPage : BasePage
    {
        #region Property associated fields

        private string _reportUrl = "Analyzer/Dummy.aspx";

        #endregion

        #region Class properties

        protected string ReportUrl
        {
            get { return _reportUrl; }
            set { _reportUrl = value; }
        }

        protected string CurrentInstanceId
        {
            get
            {
                string s = (string)ViewState["CurrentInstanceId"];
                return !string.IsNullOrEmpty(s) ? s : "-1";
            }
            set
            {
                ViewState["CurrentInstanceId"] = value;
            }
        }


        private Hashtable InstanceCache
        {
            get
            {
                Hashtable ht = (Hashtable)Session["InstanceCache"];
                if (ht == null)
                {
                    ht = new Hashtable();
                    Session["InstanceCache"] = ht;
                }
                return ht;
            }
        }

        #endregion

        #region Class methods

        protected void CacheInstance(ReportInstance instance)
        {
            InstanceCache.Add(instance.Id, instance);
        }

        protected void RemoveInstance(string id)
        {
            if (InstanceCache.ContainsKey(id))
            {
                InstanceCache.Remove(id);
            }
        }

        protected void CloseReportInstance(string id)
        {
            if (InstanceCache.ContainsKey(id))
            {
                ReportInstance inst = (ReportInstance)InstanceCache[id];

                Analyzer2005 az = GetProxy();
                try
                {
                    az.CloseReport(CurrentContext, inst, false);
                    RemoveInstance(id);
                }
                finally
                {
                    DisposeProxy(az);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
           ClientScript.RegisterClientScriptBlock(this.GetType(),"WINDOWONUNLOAD", GetCloseReportScript());
        }
        protected string GetCloseReportScript()
        {
            if (!CurrentInstanceId.Equals("-1"))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<script language='javascript' type='text/javascript'>");
                sb.AppendLine("function window_onunload() {var frame = document.createElement('iframe');");
                sb.AppendLine("frame.style.display = 'none';");

                string theUrl = string.Format("http://{0}/Analyzer/CloseReport.aspx?id={1}", Request.Url.Authority, CurrentInstanceId);
               // string theUrl = string.Format("CloseReport.aspx?id={0}", CurrentInstanceId);
                sb.AppendFormat(
                    "frame.src = '{0}';",
                    theUrl
                );

                sb.AppendLine("document.body.appendChild(frame);}</script>");
                return sb.ToString();
            }
            return string.Empty;
        }

        protected bool IsReportExists(Analyzer2005 az, string name)
        {
            CatalogItem[] item = az.FindCatalogItemByName(CurrentContext, name);
            if (item != null && item.Length > 0)
            {
                CheckState(item[0]);
                return true;
            }
            return false;
        }

        protected void ListDataSources(Analyzer2005 az, DropDownList list)
        {
            DataSource[] dss = az.ListDataSources(CurrentContext);
            if (dss != null && dss.Length > 0)
            {
                CheckState(dss[0]);
                list.Items.Clear();
                foreach (DataSource ds in dss)
                {
                    list.Items.Add(new ListItem(ds.Name, ds.Id.ToString()));
                }
            }
        }

        protected void ListReports(Analyzer2005 az, DropDownList list)
        {
            CatalogItem[] items = az.ListChildren(CurrentContext, 0);
            if (items != null && items.Length > 0)
            {
                CheckState(items[0]);
                list.Items.Clear();
                foreach (CatalogItem item in items)
                {
                    if (item.ItemType == CatalogItemType.Report)
                    {
                        list.Items.Add(new ListItem(item.Name, item.Id.ToString()));
                    }
                }
            }
        }

        protected void CreateReport(string name)
        {
            Analyzer2005 az = GetProxy();
            try
            {
                if (!IsReportExists(az, name))
                {
                    CreateInstance(az);
                }
                else
                {
                    Message = "Report existed.";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                DisposeProxy(az);
            }
        }

        protected virtual void CreateInstance(Analyzer2005 az)
        {
        }

        protected AdditionalCube CreateCube(int dataSourceId, string databaseName, string cubeName)
        {
            AdditionalCube cube = new AdditionalCube();
            cube.DataSourceId = dataSourceId;
            cube.DatabaseName = databaseName;
            cube.CubeName = cubeName;

            return cube;
        }

        protected Dimension CreateDimension(string name)
        {
            Dimension dim = new Dimension();
            dim.Name = name;

            return dim;
        }

        protected Measure CreateMeasure(string name)
        {
            Measure ms = new Measure();
            ms.Name = name;

            return ms;
        }

        protected CustomButton CreateButton(string name, string hint, string icon, string script)
        {
            CustomButton btn = new CustomButton();
            btn.Name = name;
            btn.Hint = hint;
            btn.Icon = icon;
            btn.Script = script;

            return btn;
        }

        #endregion

    }
}
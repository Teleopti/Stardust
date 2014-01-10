using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
    //public delegate void ParametersReadyHandler(ArrayList Parameters, ArrayList ParameterTexts);
    /// <summary>
    /// Summary description for WebCustomControl1.
    /// </summary>
    //[DefaultProperty("ReportID"),
    //ToolboxData("<{0}:Selector runat=server></{0}:Selector>")]
    public class Selector : WebControl, INamingContainer
    {
        private Guid _reportId;
        private int _savedId = -1;
        private IList<SqlParameter> _startParams = new List<SqlParameter>();
        private IList<SqlParameter> _params;
        private IList<string> _paramTexts;
        private Reader _reader;
        private bool _isReportPermissionGranted;
        private Guid _groupPageCode;

        internal static Guid CurrentUserCode;
        internal static Guid BuCode;
        internal static int LangId;

		internal static string _ConnectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
        //public static string _Database;

        internal static Unit _LabelWidth = new Unit("200");
        internal static Unit _List1Width = new Unit("200");
        internal static Unit _List2Width = new Unit("200");
        

        public static Guid BusinessHierarchyCode
        {
            get
            {
                return new Guid("D5AE2A10-2E17-4B3C-816C-1A0E81CD767C");
            }
        }


        public static string ErrorMessageValText
        {
            get { return ReportTexts.Resources.InvalidValueErrorMessage; } // "xx Du har angett ett ogiltigt värde i";
        }

        public static string ErrorMessage
        {
			get { return ReportTexts.Resources.MissingValueErrorMessage; } //"xxDu har inte angett något i";;
        }

        

        //ola 2005-12-20 lagt till så man kan skicka in en arraylist av parameter
        // som ska användas till FÖRSTA kontrollen.
        // Denna är bra om man vill använda selectorn som ett komplement till andra urval
        public IList<SqlParameter> StartParams
        {
            set
            {
                _startParams = value;
            }
            get
            {
                return _startParams;
            }
        }

        private Reader DataReader
        {
            get
            {
                if (_reader == null)
                {
                    _reader = new Reader(_ConnectionString, LangId);
                    _isReportPermissionGranted = _reader.IsReportPermissionsGranted(_reportId, CurrentUserCode);
                }

                return _reader;
            }
        }

        public Guid ReportId
        {
            get
            {
                return _reportId;
            }

            set
            {
                _reportId = value;
                ViewState.Add("ReportId", _reportId);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            var reportId = ViewState["ReportId"] as Guid?;
            if (reportId.HasValue)
                _reportId = reportId.Value;
        }

        public DataTable ReportProperties
        {
            get
            {
                return DataReader.ReportProperties(_reportId, _savedId);
            }
        }

        public Guid UserCode
        {
            set
            {
                CurrentUserCode = value;
            }
            get
            {
                return CurrentUserCode;
            }

        }


        public Guid BusinessUnitCode
        {
            set
            {
                BuCode = value;
            }
            get
            {
                return BuCode;
            }
        }



        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
        }


        public int LanguageId
        {
            get
            {
                return LangId;
            }

            set
            {
                LangId = value;
            }
        }

        public Unit LabelWidth
        {
            get
            {
                return _LabelWidth;
            }

            set
            {
                _LabelWidth = value;
            }
        }

        public Unit List1Width
        {
            get
            {
                return _List1Width;
            }

            set
            {
                _List1Width = value;
            }
        }

        public Unit List2Width
        {
            get
            {
                return _List2Width;
            }

            set
            {
                _List2Width = value;
            }
        }

        public IList<SqlParameter> Parameters
        {
            get
            {
                setParams();
                return _params;
            }

        }

        public IList<string> ParameterTexts
        {
            get
            {
                setParams();
                return _paramTexts;
            }

        }

        public int SavedId
        {
            set
            {
                _savedId = value;
            }
            get
            {
                return _savedId;
            }

        }

        public bool IsReportPermissionGranted
        {
            get
            {
                if (_reader == null)
                {
                    _reader = new Reader(_ConnectionString, LangId);
                    _isReportPermissionGranted = _reader.IsReportPermissionsGranted(_reportId, CurrentUserCode);
                }
                return _isReportPermissionGranted;
            }
        }

        public bool IsValid
        {
            get
            {
                setParams();
                ParameterBase paractrl;
                foreach (WebControl ctrl in Controls)
                {
                    try
                    {
                        paractrl = (ParameterBase)ctrl;
                        if (paractrl.Valid == false)
                        {
                            return false;
                        }

                    }
                    catch { }
                }
                return true;
            }

        }

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return base.Controls;
            }
        }

        public override Control FindControl(string id)
        {
            EnsureChildControls();
            return base.FindControl(id);
        }

        private void setParams()
        {
            ParameterBase paractrl;
            if (_params == null)
            {
                _params = new List<SqlParameter>();
                _paramTexts = new List<string>();
                foreach (WebControl ctrl in Controls)
                {
                    try
                    {
                        paractrl = (ParameterBase)ctrl;
                        _params.Add(paractrl.Parameter);
                        _paramTexts.Add(paractrl.ParameterText);
                    }
                    catch { }
                }
            }
        }


        /// <summary>
        /// Create all controls needed for the selected report.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (IsReportPermissionGranted)
            {
                //Permission granted

            	int flag = 0;

                _groupPageCode = (Guid) Context.Session["GroupPageCode"];
                DataSet repCtrlsData = DataReader.LoadReportControls(_reportId, _groupPageCode);

                foreach (DataRow row in repCtrlsData.Tables[0].Rows)
                {
                    IList<ParameterBase> dependent = new List<ParameterBase>();

                    var dbId = (Guid)row["Id"];
                    var name = (string)row["control_name"];
                    var resourceKey = (string)row["control_name_resource_key"];
                    string text = ReportTexts.Resources.ResourceManager.GetString(resourceKey);
                    if (string.IsNullOrEmpty(text))
                        text = resourceKey;
                    var defaultValue = (string)row["default_value"];
                    var procName = (string)row["fill_proc_name"];
                    var procParam = (string)row["fill_proc_param"];
                    var paramName = (string)row["param_name"];
                    var display = (bool) row["display"];
                    ParameterBase ctrl = getControl(name);

                    ctrl.Component = _reportId;

                    ctrl.DBID = dbId;
                    ctrl.Name = name;
                    ctrl.Text = text;
                    ctrl.DefaultValue = defaultValue;
                    ctrl.ProcName = procName;
                    ctrl.ProcParam = procParam;
                    ctrl.ParamName = paramName;
                    ctrl.Display = display;
                	if (row["interval_length_minutes"] == DBNull.Value)
                		throw new ConfigurationErrorsException("Could not find configuration for 'interval_length_minutes'. Is the ETL Tool correctly configured?");
                	int intervalLength;
                	if (!int.TryParse(row["interval_length_minutes"].ToString(), out intervalLength))
                	{
                		throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture,
                		                                             "Could not cast interval_length_minutes value '{0}' to int. Check the ETL Tool configuration.",
                		                                             row["interval_length_minutes"]));
                	}
                	ctrl.IntervalLength = intervalLength;

                    // bara första kontrollen ska ha startparametrar
                    if (flag == 0)
                    {
                        ctrl.StartParams = _startParams;
                        flag = 1;
                    }
                    ctrl.SavedId = _savedId;
                    Controls.Add(ctrl);

                    ParameterBase dep;

                    if ((Guid)row.ItemArray[5] != new Guid())
                    {
                        dep = (ParameterBase)FindControl(row.ItemArray[5].ToString());
                        dependent.Add(dep);
                        dep.AddDependent(ctrl);
                    }
                    if ((Guid)row.ItemArray[6] != new Guid())
                    {
                        dep = (ParameterBase)FindControl(row.ItemArray[6].ToString());
                        dependent.Add(dep);
                        dep.AddDependent(ctrl);
                    }
                    if ((Guid)row.ItemArray[7] != new Guid())
                    {
                        dep = (ParameterBase)FindControl(row.ItemArray[7].ToString());
                        dependent.Add(dep);
                        dep.AddDependent(ctrl);
                    }
                    if ((Guid)row.ItemArray[8] != new Guid())
                    {
                        dep = (ParameterBase)FindControl(row.ItemArray[8].ToString());
                        dependent.Add(dep);
                        dep.AddDependent(ctrl);
                    }

                    ctrl.DependentOf = dependent;

                }
                CssClass = "ControlBody";
            }
        }

        /// <summary>
        /// Returns the right type of control.
        /// Uses the name to detect what type to create
        /// </summary>
        /// <param name="name"> The Name of the control </param>
        static ParameterBase getControl(string name)
        {
            if (name.IndexOf("cbo") >= 0)
            {
                return new ParameterCombo();
            }
            if (name.IndexOf("twolistopt") >= 0)
            {
                return new ParameterListBoxOptional();
            }
            if (name.IndexOf("twolist") >= 0)
            {
                return new ParameterListBox();
            }

            if (name.IndexOf("chk") >= 0)
            {
                return new ParameterCheck();
            }
            if (name.IndexOf("double") >= 0)
            {
                return new ParameterListBoxVertical();
            }
            if (name.IndexOf("num") >= 0)
            {
                return new ParameterNumTextBox();
            }
            if (name.IndexOf("dec") >= 0)
            {
                return new ParameterDecTextBox();
            }
            if (name.IndexOf("txt") >= 0)
            {
                return new ParameterTextBox();
            }
            if (name.IndexOf("date") >= 0)
            {
                return new ParameterDate();
            }
			if (name.IndexOf("time") >= 0)
			{
				return new ParameterTime();
			}
            throw new Exception("The control type is not recognized");
        }

        ///// <summary>
        ///// Returns a control already created from the Controls Collection.
        ///// Uses the ID
        ///// </summary>
        ///// <param name="Name"> The ID of the control </param>
        //ParameterBase GetControl(int ID)
        //{
        //    return (ParameterBase) FindControl(ID.ToString());
        //}

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            writer.AddStyleAttribute("cellpadding", "5px");
        }

        public Guid GroupPageCode
        {
            get {
                return _groupPageCode;
            }
            set {
                _groupPageCode = value;
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            //ParameterBase Paractrl;

           // CssClass = "ControlBody";
            Width = new Unit(100, UnitType.Percentage);

            //render all sub controls
            foreach (WebControl ctrl in Controls)
            {
                try
                {
                    //Paractrl = (ParameterBase) ctrl;
                    if (ctrl is ParameterCombo)
                    {
                        var combo = (ParameterCombo)ctrl;
                        if (combo.Name == "cboTimeZone" && combo.DropDownList.Items.Count < 2)
                        {
                            combo.Display = false;
                        }
                    }
                    ctrl.RenderControl(writer);
                }
                catch { }
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "3");
			writer.AddStyleAttribute("border-bottom", "none");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			writer.RenderEndTag();
			writer.RenderEndTag();

        }



    }
}

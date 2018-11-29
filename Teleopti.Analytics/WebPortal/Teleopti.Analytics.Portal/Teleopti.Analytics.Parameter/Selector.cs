using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
		private readonly UserReportParams userReportParams = new UserReportParams();
		private static readonly Guid businessHierarchyCode = new Guid("D5AE2A10-2E17-4B3C-816C-1A0E81CD767C");

		internal static Unit _LabelWidth = new Unit("200");
		internal static Unit _List1Width = new Unit("200");
		internal static Unit _List2Width = new Unit("200");

		public Selector()
		{
			userReportParams.ConnectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
		}

		public static Guid BusinessHierarchyCode => businessHierarchyCode;

		public string ConnectionString
		{
			get { return userReportParams.ConnectionString; }
			set { userReportParams.ConnectionString = value; }
		}

		public static string ErrorMessageValText
		{
			get { return ReportTexts.Resources.InvalidValueErrorMessage; } // "xx Du har angett ett ogiltigt värde i";
		}

		public static string ErrorMessage
		{
			get { return ReportTexts.Resources.MissingValueErrorMessage; } //"xxDu har inte angett något i";;
		}

		public bool SkipPermissions { get; set; }

		//ola 2005-12-20 lagt till s?man kan skicka in en arraylist av parameter
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
					_reader = new Reader(ConnectionString, userReportParams.LangId,userReportParams.DbTimeout);
					_isReportPermissionGranted = _reader.IsReportPermissionsGranted(_reportId, userReportParams.CurrentUserGuid);
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
				userReportParams.CurrentUserGuid = value;
			}
			get
			{
				return userReportParams.CurrentUserGuid;
			}

		}


		public Guid BusinessUnitCode
		{
			set
			{
				userReportParams.BusinessUnitCode = value;
			}
			get
			{
				return userReportParams.BusinessUnitCode;
			}
		}

		public int LanguageId
		{
			get
			{
				return userReportParams.LangId;
			}

			set
			{
				userReportParams.LangId = value;
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
				if (SkipPermissions) return true;
				if (_reader == null)
				{
					_reader = new Reader(ConnectionString, userReportParams.LangId, userReportParams.DbTimeout);
					_isReportPermissionGranted = _reader.IsReportPermissionsGranted(_reportId, userReportParams.CurrentUserGuid);
				}
				return _isReportPermissionGranted;
			}
		}

		public bool IsValid
		{
			get
			{
				setParams();
				foreach (WebControl ctrl in Controls)
				{
					try
					{
						var paractrl = (ParameterBase)ctrl;
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
				//handle this in some way when using without session
				if (Context.Session != null)
					_groupPageCode = (Guid)Context.Session["GroupPageCode"];
				DataSet repCtrlsData = DataReader.LoadReportControls(_reportId, _groupPageCode);

				foreach (DataRow row in repCtrlsData.Tables[0].Rows)
				{
					IList<ParameterBase> dependent = new List<ParameterBase>();

					var dbId = (Guid)row["Id"];
					var name = (string)row["control_name"];
					var resourceKey = (string)row["control_name_resource_key"];
					var text = ReportTexts.Resources.ResourceManager.GetString(resourceKey, new CultureInfo(LanguageId));
					if(string.IsNullOrEmpty(text))
						text = ReportTexts.Resources.ResourceManager.GetString(resourceKey);
					if (string.IsNullOrEmpty(text))
						text = resourceKey;
					var defaultValue = (string)row["default_value"];
					var procName = (string)row["fill_proc_name"];
					var procParam = (string)row["fill_proc_param"];
					var paramName = (string)row["param_name"];
					var display = (bool)row["display"];
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

					if ((Guid)row.ItemArray[5] != Guid.Empty)
					{
						dep = (ParameterBase)FindControl(row.ItemArray[5].ToString());
						dependent.Add(dep);
						dep.AddDependent(ctrl);
					}
					if ((Guid)row.ItemArray[6] != Guid.Empty)
					{
						dep = (ParameterBase)FindControl(row.ItemArray[6].ToString());
						dependent.Add(dep);
						dep.AddDependent(ctrl);
					}
					if ((Guid)row.ItemArray[7] != Guid.Empty)
					{
						dep = (ParameterBase)FindControl(row.ItemArray[7].ToString());
						dependent.Add(dep);
						dep.AddDependent(ctrl);
					}
					if ((Guid)row.ItemArray[8] != Guid.Empty)
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
		private ParameterBase getControl(string name)
		{
			if (name.IndexOf("cboIntervalType", StringComparison.Ordinal) >= 0)
			{
				return new ParameterComboPeriodType(userReportParams);
			}
			if (name.IndexOf("cbo", StringComparison.Ordinal) >= 0)
			{
				return new ParameterCombo(userReportParams);
			}
			if (name.IndexOf("twolistopt", StringComparison.Ordinal) >= 0)
			{
				return new ParameterListBoxOptional(userReportParams);
			}
			if (name.IndexOf("twolist", StringComparison.Ordinal) >= 0)
			{
				return new ParameterListBox(userReportParams);
			}

			if (name.IndexOf("chk", StringComparison.Ordinal) >= 0)
			{
				return new ParameterCheck(userReportParams);
			}
			if (name.IndexOf("double", StringComparison.Ordinal) >= 0)
			{
				return new ParameterListBoxVertical(userReportParams);
			}
			if (name.IndexOf("num", StringComparison.Ordinal) >= 0)
			{
				return new ParameterNumTextBox(userReportParams);
			}
			if (name.IndexOf("dec", StringComparison.Ordinal) >= 0)
			{
				return new ParameterDecTextBox(userReportParams);
			}
			if (name.IndexOf("txt", StringComparison.Ordinal) >= 0)
			{
				return new ParameterTextBox(userReportParams);
			}
			if (name.IndexOf("date", StringComparison.Ordinal) >= 0)
			{
				return new ParameterDate(userReportParams);
			}
			if (name.IndexOf("time", StringComparison.Ordinal) >= 0)
			{
				return new ParameterTime(userReportParams);
			}
			throw new Exception("The control type is not recognized");
		}

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
			get
			{
				return _groupPageCode;
			}
			set
			{
				_groupPageCode = value;
			}
		}

		public int DbTimeout
		{
			get { return userReportParams.DbTimeout; }
			set { userReportParams.DbTimeout = value; }
		}

		public TimeZoneInfo UserTimeZone { get; set; }

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
					var parameterCombo = ctrl as ParameterCombo;
					if (parameterCombo != null)
					{
						var combo = parameterCombo;
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

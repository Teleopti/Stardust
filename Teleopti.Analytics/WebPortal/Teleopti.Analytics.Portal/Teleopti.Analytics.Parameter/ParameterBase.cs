using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterBase.
	/// </summary>

	abstract class ParameterBase : WebControl
	{
		private readonly UserReportParams _userReportParams;
		private Reader _reader;
		protected DataSet MyData;
		protected Guid Dbid;

		//protected System.Guid _UserID;
		private string _defaultValue;
		private IList<ParameterBase> _dependentOf;
		protected IList<ParameterBase> Dependent = new List<ParameterBase>();

		private bool _reLoaded;
		// det som returneras ut i Parameter
		// från början samma som default
		// sedan det som fanns i  databasen (om det fanns något från tidigare)
		// sedan det som hamnar efter anrop till FetchValue
		internal bool Valid;
		protected object Value;
		private string _parameterText;

		protected ParameterBase(UserReportParams userReportParams)
		{
			_userReportParams = userReportParams;
			StartParams = new List<SqlParameter>();
		}

		public bool Display
		{
			get
			{
				if (Style[HtmlTextWriterStyle.Display] == "none")
				{
					return false;
				}
				return true;
			}
			set
			{
				if (!value)
				{
					Style[HtmlTextWriterStyle.Display] = "none";
				}
			}
		}

		protected override HtmlTextWriterTag TagKey//För att "starttaggen" ska vara <tr> istället för <span>
		{
			get
			{
				return HtmlTextWriterTag.Tr;
			}
		}

		public void AddValidator(BaseValidator validator)
		{
			if (Display)
			{
				base.Controls.Add(validator);
			}
		}

		protected Reader DataReader
		{
			get
			{
				if (_reader == null)
				{
					_reader = new Reader(_userReportParams.ConnectionString, _userReportParams.LangId, _userReportParams.DbTimeout);
				}
				return _reader;
			}
		}

		public string GetClientFileUrl(string fileName)
		{
			// Use the config setting to determine where the client files are located.
			// Client files are located in the aspnet_client v-root and then distributed
			// into subfolders by assembly name and assembly version.
			// För att detta ska fungera måste det finnas directory med namnen
			// reportparameters\1_0_0_0 under Inetpub\wwwroot\aspnet_client\ på servern.
			// Filerna (bilder, script etc) ska ligga i 1_0_0_0-mappen.

			string location = null;

			if (Context != null)
			{
				//System.Collections.IDictionary configData = (System.Collections.IDictionary)Context.GetConfig("system.web/webControls");
				var configData = (System.Collections.IDictionary)Context.GetSection("system.web/webControls");

				if (configData != null)
				{
					location = (string)configData["clientScriptsLocation"];
					//location = "C:\\Data\\Teleopti.Pro\\Teleopti.Pro.Web\\aspnet_client\\Teleopti_pro_parameters\\1_0_0_0\\";
				}
			}

			if (location == null)
			{
				location = String.Empty;
			}

			else if (location.IndexOf("{0}", StringComparison.Ordinal) >= 0)
			{
				AssemblyName assemblyName = GetType().Assembly.GetName();

				string assembly = assemblyName.Name.Replace('.', '_').ToLower();
				string version = assemblyName.Version.ToString().Replace('.', '_');

				location = String.Format(location, assembly, version);
			}

			string clientFilesUrlPrefix = location;


			return System.Web.VirtualPathUtility.ToAbsolute("~" + clientFilesUrlPrefix + fileName);
		}

		public void AddDependent(ParameterBase dependent)
		{
			Dependent.Add(dependent);
		}

		protected abstract void Clear();
		public void LoadData()
		{
			if (_reLoaded)
				return;

			EnsureChildControls();

			IList<SqlParameter> parameters = StartParams.ToList();

			foreach (ParameterBase ctrl in _dependentOf)
			{
				SqlParameter tmpParam = ctrl.Parameter;
				if (ctrl.Valid == false && !(ctrl is ParameterListBox))
				{
					Clear();
					return;
				}
				parameters.Add(tmpParam);
			}

			if (ProcParam != "-1000")
			{
				var param = new SqlParameter("@param", ProcParam);
				parameters.Add(param);
			}

			if (ProcName == "mart.report_control_group_page_get")
			{
				var sqlParameter = new SqlParameter("@business_hierarchy_code", SqlDbType.UniqueIdentifier) { Value = Selector.BusinessHierarchyCode };
				parameters.Add(sqlParameter);
			}

			if (ProcName != "1")
			{
				MyData = DataReader.LoadControlData(ProcName, parameters, Component, _userReportParams.CurrentUserGuid, _userReportParams.BusinessUnitCode);
			}
			LoadUserSettings();
			BindData();
			_reLoaded = true;
		}

		protected void LoadUserSettings()
		{
			string temp = DataReader.LoadUserSetting(Component, _userReportParams.CurrentUserGuid, ParamName, SavedId);
			if (temp != "")
			{
				_defaultValue = temp;
				Value = temp;
			}
		}

		public SqlParameter Parameter
		{
			get
			{
				SetData();
				SaveSetting();
				return new SqlParameter(ParamName, Value);
			}
		}

		// anropas efter Parameter annars kanske inte värdet är satt
		public string ParameterText
		{
			get
			{
				if (_parameterText == null)
					_parameterText = Value.ToString();
				return _parameterText;
			}
			set { _parameterText = value; }
		}

		public void SaveSetting()
		{
			if (Valid)
			{
				if (ParamName.IndexOf("@group_page_code", StringComparison.Ordinal) == -1)
				{
					// Avoid saving settings for group page, and also values that are null.
					if (!Value.Equals(DBNull.Value))
					{
						DataReader.SaveUserSetting(Component, _userReportParams.CurrentUserGuid, ParamName, SavedId, StringValue());
					}
				}
			}
		}

		public virtual string StringValue()
		{
			return Value.ToString();
		}

		protected abstract void SetData();
		protected abstract void SetAutoPostBack();
		protected abstract void BindData();

		public string Name { get; set; }

		public Guid DBID
		{
			set
			{
				Dbid = value;
				ID = value.ToString();
			}
			get
			{
				return Dbid;
			}
		}


		public string ProcParam { get; set; }

		//ola 2005-12-20 lagt till så man kan skicka in en arraylist av parameter
		// som ska användas till FÖRSTA kontrollen.
		// Denna är bra om man vill använda selectorn som ett komplement till andra urval
		public IList<SqlParameter> StartParams { get; set; }

		public int SavedId { get; set; }

		public string ParamName { get; set; }

		public string Text { get; set; }

		public Guid Component { get; set; }

		public string ProcName { get; set; }

		public string DefaultValue
		{
			set
			{
				_defaultValue = value;
				Value = value;
			}
			get
			{
				return _defaultValue;
			}
		}

		public IList<ParameterBase> DependentOf
		{
			set
			{
				_dependentOf = value;
				foreach (ParameterBase ctrl in _dependentOf)
				{
					ctrl.SetAutoPostBack();
				}
			}
			get
			{
				return _dependentOf;
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

		public int IntervalLength { get; set; }
	}
}

using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterCombo.
	/// </summary>
	class ParameterCombo : ParameterBase
	{
		private Label _label;
		protected DropDownList DropDown;
		private RequiredFieldValidator _validator;

		public ParameterCombo(UserReportParams userReportParams)
			: base(userReportParams)
		{
		}

		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return base.Controls;
			}
		}

		public DropDownList DropDownList
		{
			get { return DropDown; }
		}


		protected override void SetAutoPostBack()
		{
			EnsureChildControls();
			DropDown.AutoPostBack = true;
			DropDown.SelectedIndexChanged += DropDownSelectedIndexChanged;
		}

		protected override void Clear()
		{

		}

		protected override void SetData()
		{
			EnsureChildControls();
			if (Display)
			{
				_validator.Validate();
				if (_validator.IsValid)
				{
					Value = DropDown.SelectedValue;
					ParameterText = DropDown.SelectedItem.Text;
				}
				Valid = _validator.IsValid;
			}
			else
			{
				Value = DBNull.Value;
				Valid = true;
			}
		}

		protected override void CreateChildControls()
		{

			_label = new Label();

			DropDown = new DropDownList
								 {
									 Width = new Unit(50, UnitType.Percentage),
									 CssClass = "ControlStyle",
									 DataTextField = "name",
									 DataValueField = "id"
								 };


			DropDown.ID = "Drop" + Dbid;
			_validator = new RequiredFieldValidator
				{
					ControlToValidate = DropDown.ID,
					Text = "*",
					ErrorMessage = Selector.ErrorMessage + " '" + Text + "'",
					Display = ValidatorDisplay.Dynamic,
					ForeColor = Color.Red
				};

			_label.Text = Text;
			base.Controls.Add(_label);
			base.Controls.Add(DropDown);
			AddValidator(_validator);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			DropDown.DataSource = MyData.Tables[0];
			DropDown.DataBind();

			if (DefaultValue == "-99")
			{
				ListItem myItem = DropDown.Items[DropDown.Items.Count - 1];
				myItem.Selected = true;
				return;
			}
			if (DefaultValue == "-95")
			{
				//Change the default value from the control data
				foreach (DataRow row in MyData.Tables[0].Rows)
				{
					if ((bool)row["default_value"])
					{
						DefaultValue = row["id"].ToString();
						break;
					}
				}
			}

			foreach (ListItem myItem in DropDown.Items)
			{
				if (myItem.Value == DefaultValue)
				{
					myItem.Selected = true;
					break;
				}
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna
		{
			Debug.Assert(_label != null);
			Debug.Assert(DropDown != null);
			Debug.Assert(_validator != null);

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._List1Width.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			DropDown.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20");
			_validator.RenderControl(writer);
			writer.RenderEndTag();
		}

		private void DropDownSelectedIndexChanged(object sender, EventArgs e)
		{
			Value = DropDown.SelectedValue;

			foreach (ParameterBase ctrl in Dependent)
			{
				ctrl.LoadData();
			}

		}

	}

}

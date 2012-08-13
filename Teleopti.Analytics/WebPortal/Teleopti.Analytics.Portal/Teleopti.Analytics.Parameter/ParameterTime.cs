using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterTime.
	/// </summary>
	class ParameterTime : ParameterBase
	{
		private Label _label;
		private DropDownList _dropDown;
		private RequiredFieldValidator _validator;

		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return base.Controls;
			}
		}

		protected override void SetAutoPostBack()
		{
			EnsureChildControls();
			_dropDown.AutoPostBack = true;
			_dropDown.SelectedIndexChanged += DropDownSelectedIndexChanged;
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
					Value = _dropDown.SelectedValue;
					ParameterText = _dropDown.SelectedItem.Text;
				}
				_valid = _validator.IsValid;
			}
			else
			{
				Value = DBNull.Value;
				_valid = true;
			}
		}

		protected override void CreateChildControls()
		{

			_label = new Label();

			_dropDown = new DropDownList();
			_dropDown.Width = new Unit(100, UnitType.Percentage);
			_dropDown.CssClass = "ControlStyle";
			_dropDown.DataTextField = "name";
			_dropDown.DataValueField = "id";

			_validator = new RequiredFieldValidator();
			_dropDown.ID = "Drop" + Dbid;
			_validator.ControlToValidate = _dropDown.ID;
			_validator.Text = "*";

			_validator.ErrorMessage = Selector.ErrorMessage + " '" + Text + "'";
			_validator.Display = ValidatorDisplay.Dynamic;
			_label.Text = Text;
			base.Controls.Add(_label);
			base.Controls.Add(_dropDown);
			AddValidator(_validator);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			_dropDown.DataSource = CreateComboItems(MyData.Tables[0]);
			_dropDown.DataBind();

			if (DefaultValue == "-99")
			{
				ListItem myItem = _dropDown.Items[_dropDown.Items.Count - 1];
				myItem.Selected = true;
				return;
			}
			foreach (ListItem myItem in _dropDown.Items)
			{
				if (myItem.Value == DefaultValue)
				{
					myItem.Selected = true;
					break;
				}
			}
		}

		private IList<ComboItem> CreateComboItems(DataTable dataTable)
		{
			IList<ComboItem> comboItems = new List<ComboItem>();

			if (dataTable != null)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					comboItems.Add(new ComboItem((int)row["id"], GetTime((int)row["id"], (bool)row["is_interval_to"])));
				}
			}

			return comboItems;
		}

		private string GetTime(int intervalId, bool isIntervalToValue)
		{
			int minutesPastMidnight = intervalId * IntervalLength;
			var time = new DateTime().AddMinutes(minutesPastMidnight);
			if (isIntervalToValue)
				time = time.AddMinutes(IntervalLength);

			return time.ToShortTimeString();
		}

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._List1Width.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_dropDown.RenderControl(writer);
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
			Value = _dropDown.SelectedValue;

			foreach (ParameterBase ctrl in Dependent)
			{
				ctrl.LoadData();
			}

		}

	}

	public class ComboItem
	{
		public ComboItem(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get; set; }

		public string Name { get; set; }
	}

}

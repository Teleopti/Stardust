using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterDecTextBox.
	/// </summary>
	class ParameterDecTextBox : ParameterBase
	{
		private TextBox _textBox;
		private Label _label;
		private CompareValidator _validator;

		public ParameterDecTextBox(UserReportParams userReportParams)
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

		protected override void SetAutoPostBack()
		{
			EnsureChildControls();
		}

		protected override void Clear()
		{

		}

		protected override void SetData()
		{
			EnsureChildControls();
			_validator.Validate();
			if (_validator.IsValid)
			{
				Value = _textBox.Text;
			}
			Valid = _validator.IsValid;
		}

		protected override void CreateChildControls()//Ritar upp kontrollerna
		{
			_label = new Label { Text = Text };
			_textBox = new TextBox { ID = "txtBox" + Dbid, Width = Selector._List1Width, CssClass = "ControlStyle" };

			//Kontrollerar att värdet är av decimaltyp samt är större än noll

			_validator = new CompareValidator
								  {
									  ControlToValidate = _textBox.ID,
									  ValueToCompare = "0",
									  Type = ValidationDataType.Double,
									  Operator = ValidationCompareOperator.GreaterThan,
									  EnableClientScript = true,
									  Display = ValidatorDisplay.Dynamic,
									  Text = "*",
									  ErrorMessage = Selector.ErrorMessageValText + " '" + Text + "'",
									  ForeColor = Color.Red
								  };

			_textBox.TextChanged += textBoxTextChanged;

			base.Controls.Add(_label);
			base.Controls.Add(_textBox);
			AddValidator(_validator);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			string s = Convert.ToString(Value);
			_textBox.Text = s;
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_textBox.RenderControl(writer);
			_validator.RenderControl(writer);
			writer.RenderEndTag();
		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			Value = _textBox.Text;
		}
	}
}

using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterTextBox.
	/// </summary>
	class ParameterNumTextBox : ParameterBase
	{
		private TextBox _textBox;
		private Label _label;
		private CompareValidator _validator;

		public ParameterNumTextBox(UserReportParams userReportParams)
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
			try
			{
				Value = _textBox.Text;
				ParameterText = _textBox.Text;
				Valid = true;
			}
			catch (Exception)
			{

			}
		}

		protected override void CreateChildControls()
		{
			_label = new Label { Text = Text };
			_textBox = new TextBox { ID = "txtBox" + Dbid, Width = Selector._List1Width };
			_textBox.ID = "txtBox" + Dbid;
			_textBox.CssClass = "ControlStyle";

			_validator = new CompareValidator
								  {
									  ControlToValidate = _textBox.ID,
									  ValueToCompare = "-100",
									  Type = ValidationDataType.Integer,
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

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_textBox.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			var panel = new Panel { Height = new Unit("20") };
			if (Display)
			{
				panel.Controls.Add(_validator);
			}
			Controls.Add(panel);
			panel.RenderControl(writer);
			writer.RenderEndTag();

		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			Value = _textBox.Text;
		}
	}
}

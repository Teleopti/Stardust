using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterTextBox.
	/// </summary>
	class ParameterTextBox : ParameterBase
	{
		private TextBox _textBox;
		private Label _label;

		public ParameterTextBox(UserReportParams userReportParams)
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
			Value = _textBox.Text;
			Valid = true;
		}

		protected override void CreateChildControls()
		{
			_label = new Label { Text = Text };
			_textBox = new TextBox
								{
									Width = Selector._List1Width,
									ID = "txtBox" + Dbid,
									CssClass = "ControlStyle"
								};

			_textBox.TextChanged += textBoxTextChanged;

			base.Controls.Add(_label);
			base.Controls.Add(_textBox);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			string s;
			if (MyData != null)
			{
				s = (string)MyData.Tables[0].Rows[0].ItemArray[0];
			}
			else
			{
				s = Convert.ToString(Value);

			}
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
		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			Value = _textBox.Text;
		}
	}
}

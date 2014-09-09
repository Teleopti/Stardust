using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterCheck.
	/// </summary>
	class ParameterCheck : ParameterBase
	{
		private CheckBox _chkBox;
		private Label _label;

		public ParameterCheck(UserReportParams userReportParams)
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

		protected override void Clear()
		{
		}

		protected override void SetData()
		{
			EnsureChildControls();
			Value = _chkBox.Checked;
			Valid = true;
		}

		protected override void CreateChildControls()
		{
			_label = new Label { Text = Text };
			_chkBox = new CheckBox { ID = "ChkBox" + Dbid };

			_chkBox.CheckedChanged += chkBoxCheckedChanged;

			base.Controls.Add(_label);
			base.Controls.Add(_chkBox);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			string s = Convert.ToString(Value);
			try
			{
				_chkBox.Checked = Convert.ToBoolean(s);
			}
			catch
			{ }
		}

		protected override void SetAutoPostBack()
		{
			EnsureChildControls();
		}

		protected override void RenderContents(HtmlTextWriter writer)//Ritar ut kontrollerna
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_chkBox.RenderControl(writer);
			writer.RenderEndTag();
		}

		private void chkBoxCheckedChanged(object sender, EventArgs e)
		{
			Value = _chkBox.Checked;
		}
	}
}

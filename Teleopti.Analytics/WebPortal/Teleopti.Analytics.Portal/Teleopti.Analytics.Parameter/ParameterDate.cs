using System;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using AjaxControlToolkit;
using Image = System.Web.UI.WebControls.Image;

namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for DateParameter.
	/// </summary>
	class ParameterDate :ParameterBase
	{
		private TextBox _textBox;
		private Label _label;
		private Image _buttonDate;
		//private RequiredFieldValidator _Validator;
		private RegularExpressionValidator  _dateValidator;
		private CalendarExtender _calExt;
		private static readonly DateTime SqlSmallDateTimeMinValue = new DateTime(1900, 1, 1);
		private static readonly DateTime SqlSmallDateTimeMaxValue = new DateTime(2079, 6, 6);


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
			_textBox.AutoPostBack = true;
		}

		protected override void Clear()
		{
		}

		protected override void SetData()
		{
			EnsureChildControls();
			try
			{
				var date = Convert.ToDateTime(_textBox.Text);
				if (date < SqlSmallDateTimeMinValue || date > SqlSmallDateTimeMaxValue)
				{
					_dateValidator.IsValid = false;
					_valid = false;
					return;
				}
				Value = date;
				ParameterText = _textBox.Text; 
				_valid = true;
			}
			catch
			{
				_dateValidator.IsValid = false;
				_valid = false;
			}
		}

	    public override string StringValue()
	    {
            var date = DateTime.Parse(Value.ToString());
	        return date.ToShortDateString();
	        //return Value.ToString();
	    }

		protected override void CreateChildControls() 
		{	
			var regexp = new StringBuilder();
			_label = new Label {Text = Text};
			_textBox = new TextBox {ID = "txtBox" + Dbid};
			_textBox.Attributes.Add("name","Selector_" + _textBox.ID);
			
			_textBox.CssClass = "ControlStyle";
			_textBox.Width = new Unit(100);

			_dateValidator=new RegularExpressionValidator
							   {
								   ControlToValidate = _textBox.ID,
								   Display = ValidatorDisplay.Dynamic,
								   Text = "*",
								   ForeColor = Color.Red
							   };
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{2}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{4}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\s\d{1,2}(\-|\/|\.)\s\d{4}$|");
			regexp.Append(@"^\d{2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{1,2}$|");
			regexp.Append(@"^\d{4}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{1,2}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{2}(\-|\/|\.)\d{1,2}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{4}(\-|\/|\.)\d{1,2}$|");
            regexp.Append(@"^\d{4}(.)\s\d{1,2}(.)\s\d{1,2}(.)$|"); // Supports Hungarian hopefully...
			regexp.Append(@"^\d{1,2}(\.)\d{1,2}(\.)\d{4}(\.)$|"); // Supports Croatian format (hr-HR)
			regexp.Append(@"^\d\d?\.\d\d?\.\d\d\d?\d? ?г.$"); // Supports Bulgarian hopefully...the r is not r it is some unicode character

			_dateValidator.ValidationExpression=regexp.ToString();
			_dateValidator.ErrorMessage=Selector.ErrorMessageValText+ " '" + Text +"'";

		    
            _buttonDate = new Image {SkinID = "OpenCalSmall", ID = "ButtonDate" + Dbid};
            _buttonDate.Attributes.Add("name", "Selector_" + _buttonDate.ID);
		    _buttonDate.Visible = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag != "fa-IR";

		    _textBox.TextChanged += textBoxTextChanged;
		    
		    _calExt = new CalendarExtender
		    {
		        ID = "CalExt" + Dbid,
		        TargetControlID = _textBox.ID,
		        PopupButtonID = _buttonDate.ID,
		        CssClass = "MyCalendar",
                Enabled = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag != "fa-IR"
		    };
		    
			base.Controls.Add(_label);
			base.Controls.Add(_buttonDate);
			base.Controls.Add(_textBox);
            if(_calExt != null)
			    base.Controls.Add(_calExt);
			AddValidator(_dateValidator);

			if (!Page.IsPostBack)
			{
				LoadData();
			}
		}

		protected override void BindData()
		{
			string s = Convert.ToString(Value);
			DateTime date;
			
			try
			{
				// Sätter dagens datum om usersettings saknas,
				// förtuom om det står ett vanligt heltal.
				if (s == "")
				{

					s = DateTime.Now.Date.ToShortDateString();
					date = DateTime.Now.Date;
				}
				else
				{
					date = Convert.ToDateTime(s);
				}
			}
			catch
			{
				try
				{
					// Inget datum eller klockslag 
					// Om det står ett heltal konverteras det till dagens datum plus/minus antal dgr
					// Dvs 0 blir dagens datum, 1 nästa dag och -1 föregående dag
					int test = int.Parse(s);
					date = DateTime.Now.Date;
					date = date.AddDays(test);
				}
				catch(Exception)
				{
					s = DateTime.Now.Date.ToShortDateString();
					date = DateTime.Now.Date;
				}
			}

			// if it depends of a start date the end date can not be smaller
			if (DependentOf.Count > 0)
			{
				try
				{
					if ((DateTime)DependentOf[0].Parameter.Value > date)
					{
						date = (DateTime)DependentOf[0].Parameter.Value;
					}
				}
				catch (Exception)
				{

				}
			}

			//Visar i rätt format för användaren
			string f = date.ToShortDateString();
			_textBox.Text = f;   
		}

		
		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna samt skapar sökvägar till filer och sätter attribut på knapparna
		{			
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute("valign", "top");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "MyCalender");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_textBox.RenderControl(writer);
			writer.Write("&nbsp;");
			_buttonDate.RenderControl(writer);
			
			_calExt.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
			writer.AddStyleAttribute("colspan","2");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_dateValidator.RenderControl(writer);
			 
			writer.RenderEndTag();
		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			DateTime theDate;
			if (DateTime.TryParse(_textBox.Text, out theDate))
					Value = theDate.ToShortDateString();
			else
				Value = _textBox.Text; //Validatorerna kollar att datum
			foreach (ParameterBase ctrl in Dependent)
			{
				ctrl.LoadData();
			}
			// Do a bind to check dependecies
			BindData();
		}

		protected override void OnPreRender(EventArgs e)//Skapar och registrerar javascript på sidan
		{
            if(System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag != "fa-IR") return;
			var scriptKey = "Jalali" + _textBox.ID;
		    string stringDate = "";
            if(Value is DateTime)
                stringDate = ((DateTime)Value).ToString("d", CultureInfo.CurrentCulture);

			var scriptBlock = @"<script type=""text/javascript"">
			$(function() {
				$(""#Parameter_" + _textBox.ID + @""").persianDatepicker({persianNumbers: false, selectedDate: """ + stringDate + @"""}); 
			});</script>";
			
			Page.ClientScript.RegisterClientScriptBlock(GetType(), scriptKey, scriptBlock);
		}
	}
}

using System;
using System.Data.SqlTypes;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using AjaxControlToolkit;

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
				if (date<SqlDateTime.MinValue || date>SqlDateTime.MaxValue)
				{
					_dateValidator.IsValid = false;
					_valid = false;
					return;
				}
				Value = date;
				ParameterText = _textBox.Text; //_value.ToString();
				_valid = true;
			}
			catch
			{
				_dateValidator.IsValid = false;
				_valid = false;
			}
		} 

		protected override void CreateChildControls() 
		{	
			var regexp = new StringBuilder();
			//GetUserDateFormat();
			_label = new Label {Text = Text};
		    _textBox = new TextBox {ID = "txtBox" + Dbid};
		    _textBox.Attributes.Add("name","Selector_" + _textBox.ID);
			
			_textBox.CssClass = "ControlStyle";
            _textBox.Width = new Unit(100); //Selector._List1Width;

			//_TextBox.Height = new Unit("0");

			//_Validator = new RequiredFieldValidator();
			//_Validator.ControlToValidate = _TextBox.ID;
			//_Validator.EnableClientScript = false;
			//_Validator.Display = ValidatorDisplay.Dynamic;
			//_Validator.Text = "*";

			_dateValidator=new RegularExpressionValidator
			                   {
			                       ControlToValidate = _textBox.ID,
			                       Display = ValidatorDisplay.Dynamic,
			                       Text = "*"
			                   };
		    regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{2}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{4}$|");
            regexp.Append(@"^\d{1,2}(\-|\/|\.)\s\d{1,2}(\-|\/|\.)\s\d{4}$|");
			regexp.Append(@"^\d{2}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{1,2}$|");
			regexp.Append(@"^\d{4}(\-|\/|\.)\d{1,2}(\-|\/|\.)\d{1,2}$|");
			regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{2}(\-|\/|\.)\d{1,2}$|");
            regexp.Append(@"^\d{1,2}(\-|\/|\.)\d{4}(\-|\/|\.)\d{1,2}$|");
            regexp.Append(@"^\d{4}(.)\s\d{1,2}(.)\s\d{1,2}(.)$"); // Supports Hungarian hopefully...


			_dateValidator.ValidationExpression=regexp.ToString();
			_dateValidator.ErrorMessage=Selector.ErrorMessageValText+ " '" + Text +"'";
			//_DateValidator.EnableClientScript =true;
			
			//_DateValidator.ControlToValidate =_TextBox.ID;
			//_DateValidator.Display =ValidatorDisplay.Dynamic;
			//_DateValidator.Text="*";
			//_DateValidator.ErrorMessage=Teleopti.Parameters.Selector.ErrorMessageValText+ " '" + _Text +"'";
			//_DateValidator.EnableClientScript =true;


			_buttonDate = new Image {SkinID = "OpenCalSmall", ID = "ButtonDate" + Dbid};
		    _buttonDate.Attributes.Add("name","Selector_" + _buttonDate.ID);
			//_Validator.ErrorMessage = Teleopti.Parameters.Selector.ErrorMessageValText + " '" + _Text + "'";
			
			_textBox.TextChanged += textBoxTextChanged;
            
            _calExt = new CalendarExtender
                          {
                              ID = "CalExt" + Dbid,
                              TargetControlID = _textBox.ID,
                              PopupButtonID = _buttonDate.ID,
                              CssClass = "MyCalendar"
                          };

		    base.Controls.Add(_label);
			base.Controls.Add(_buttonDate);
			base.Controls.Add(_textBox);
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
			
			//System.Globalization.CultureInfo info =	new System.Globalization.CultureInfo(_LCID);
			//System.Threading.Thread.CurrentThread.CurrentCulture = info;

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

			//_TextBox.Text = s;
			//_HiddenTextBox.Text = s;
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
			//_ButtonDate.ImageUrl = GetClientFileUrl("calendar_small.gif");
			//string _ButtonDateDown = GetClientFileUrl("calendarDown_small.gif");

			//_ButtonDate.ImageUrl = GetClientFileUrl("calendar.gif");

			//string _ButtonDateDown = GetClientFileUrl("calendarDown.gif");
			
			//_ButtonDate.Attributes.Add("onMouseDown", "changeDatePic('" + _ButtonDate.ClientID + "','" + _ButtonDateDown + "')");
			//_ButtonDate.Attributes.Add("onMouseOut", "changeDatePic('" + _ButtonDate.ClientID + "','" + _ButtonDate.ImageUrl + "')");

			//_ButtonDate.Attributes.Add("onMouseUp", "changeDatePic('" + _ButtonDate.ClientID + "','" + _ButtonDate.ImageUrl + "')");

			//_TextBox.Attributes.Add("onChange", "MoveDate('" + _TextBox.ClientID + "', '" + _HiddenTextBox.ClientID + "')");

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute("valign", "top");
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "MyCalender");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			//_TextBoxDA.RenderControl(writer);
			_textBox.RenderControl(writer);
            writer.Write("&nbsp;");
			_buttonDate.RenderControl(writer);
            
            _calExt.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
            writer.AddStyleAttribute("colspan","2");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			//_Validator.RenderControl(writer);
			_dateValidator.RenderControl(writer);
			 
			writer.RenderEndTag();
		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			Value = _textBox.Text; //Validatorerna kollar att datum
            foreach (ParameterBase ctrl in Dependent)
            {
                ctrl.LoadData();
            }
            // Do a bind to check dependecies
            BindData();
		}

//        protected override void OnPreRender(EventArgs e)//Skapar och registrerar javascript på sidan
//        {
//            base.OnPreRender(e);
//            string scriptKey = "ShowDatePicker:" + "1000";
//            string scriptKey2 = "ShowDatePicker:" + "2000";
//            string scriptKey3 = "ShowDatePicker:" + "3000";
//            string scriptKey4 = "ShowDatePicker:" + _DBID;
//            string scriptKey5 = "MoveDate:" ;//+ _DBID;
//        {
//            string scriptBlock =  GetClientScriptInclude("calendar.js");
//            string scriptBlock2 = GetClientScriptInclude(_CalendarLang);
//            string scriptBlock3 = GetClientScriptInclude("calendar-setup.js");
//            string scriptBlock4 = @"<script charset='windows-1252' type='text/javascript'>Calendar.setup({inputField:'" + _TextBox.Text + "',displayArea:'" + _TextBox.ClientID + "',button:'" + _ButtonDate.ClientID + "',daFormat:'" + _UserDateFormat + "',align:'cR'});</script>";
//            string scriptBlock5 = @"<script charset='windows-1252' type='text/javascript'>
//			function MoveDate(TextFrom, TextTo)
//					{
//						var txtFrom = document.aspnetForm.all(TextFrom);
//						var txtTo = document.aspnetForm.all(TextTo);
//
//						var s;
//						s =  txtFrom.value;
//						txtTo.value = s;
//					}
//			function changeDatePic(button,pic) 
//				{
//					var theform = document.aspnetForm.all(button);
//					theform.src=pic;
//				}
//  
//               </script>";

//            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), scriptKey, scriptBlock);
//            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), scriptKey2, scriptBlock2);
//            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), scriptKey3, scriptBlock3);
//            Page.ClientScript.RegisterStartupScript(this.GetType(), scriptKey4, scriptBlock4);
//            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), scriptKey5, scriptBlock5);
//        }
//        }
	}
}

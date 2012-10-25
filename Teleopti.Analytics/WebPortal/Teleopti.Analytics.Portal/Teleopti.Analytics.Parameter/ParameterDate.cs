using System;
using System.Drawing;
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
            regexp.Append(@"^\d{4}(.)\s\d{1,2}(.)\s\d{1,2}(.)$"); // Supports Hungarian hopefully...


			_dateValidator.ValidationExpression=regexp.ToString();
			_dateValidator.ErrorMessage=Selector.ErrorMessageValText+ " '" + Text +"'";

			_buttonDate = new Image {SkinID = "OpenCalSmall", ID = "ButtonDate" + Dbid};
		    _buttonDate.Attributes.Add("name","Selector_" + _buttonDate.ID);
			
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
			
			try
			{
                // S�tter dagens datum om usersettings saknas,
                // f�rtuom om det st�r ett vanligt heltal.
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
                    // Om det st�r ett heltal konverteras det till dagens datum plus/minus antal dgr
                    // Dvs 0 blir dagens datum, 1 n�sta dag och -1 f�reg�ende dag
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

			//Visar i r�tt format f�r anv�ndaren
            string f = date.ToShortDateString();
			_textBox.Text = f;   
		}

		
		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna samt skapar s�kv�gar till filer och s�tter attribut p� knapparna
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
			Value = _textBox.Text; //Validatorerna kollar att datum
            foreach (ParameterBase ctrl in Dependent)
            {
                ctrl.LoadData();
            }
            // Do a bind to check dependecies
            BindData();
		}
	}
}

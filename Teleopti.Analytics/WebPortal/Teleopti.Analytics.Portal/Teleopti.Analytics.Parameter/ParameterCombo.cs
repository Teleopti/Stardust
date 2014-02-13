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
		protected DropDownList _dropDown;
		private RequiredFieldValidator _validator;
		
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
            get { return _dropDown; }
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
			
			_dropDown = new DropDownList
			                {
			                    Width = new Unit(100, UnitType.Percentage),
			                    CssClass = "ControlStyle",
			                    DataTextField = "name",
			                    DataValueField = "id"
			                };

		    
			_dropDown.ID = "Drop" + Dbid;
			_validator = new RequiredFieldValidator
				{
					ControlToValidate = _dropDown.ID,
					Text = "*",
					ErrorMessage = Selector.ErrorMessage + " '" + Text + "'",
					Display = ValidatorDisplay.Dynamic,
					ForeColor = Color.Red
				};

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
			_dropDown.DataSource = MyData.Tables[0];
			_dropDown.DataBind();

            if (DefaultValue == "-99")
		    {
                ListItem myItem =_dropDown.Items[_dropDown.Items.Count - 1];
		        myItem.Selected = true;
                return;
		    }
			if (DefaultValue == "-95")
			{
				//Change the default value from the control data
				foreach (DataRow row in MyData.Tables[0].Rows)
				{
					if ((bool) row["default_value"])
					{
						DefaultValue = row["id"].ToString();
						break;
					}
				}
			}
		    
			foreach( ListItem myItem in _dropDown.Items)
			{
				if (myItem.Value ==  DefaultValue)
				{
					myItem.Selected = true;
					break;
				}
			}
		}		

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna
		{
			Debug.Assert(_label != null);
			Debug.Assert(_dropDown != null);
			Debug.Assert(_validator != null);

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,Selector._List1Width.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_dropDown.RenderControl(writer);
			writer.RenderEndTag();
			
			writer.AddAttribute(HtmlTextWriterAttribute.Style,"padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,"20");
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

}

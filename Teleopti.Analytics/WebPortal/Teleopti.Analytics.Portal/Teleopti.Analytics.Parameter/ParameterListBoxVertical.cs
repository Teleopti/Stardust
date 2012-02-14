using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Collections.Specialized;
namespace Teleopti.Analytics.Parameters
{
	/// <summary>
	/// Summary description for ParameterListBoxVertical.
	/// </summary>
	class ParameterListBoxVertical :ParameterBase
	{
		private Label _Label;
		private ListBox _ListBox;
		private ListBox _ListBox2;
		private TextBox _TextBox;
		private TextBox _TextBoxText;
		private Image _ButtonMoveOne;
		private Image _ButtonMoveAll;
		private Image _ButtonMoveAllBack;
		private Image _ButtonMoveOneBack;
		private RequiredFieldValidator _Validator;
		
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
			Value = _TextBox.Text;
			ParameterText = _TextBoxText.Text;
		}

		

		protected override void CreateChildControls() 
		{
			_Label = new Label();
			_ListBox = new ListBox
			               {
			                   SelectionMode = ListSelectionMode.Multiple,
			                   Width = new Unit("300"),
			                   Height = new Unit("150"),
			                   CssClass = "ControlStyle",
			                   DataTextField = "name",
			                   DataValueField = "id"
			               };
		    _Validator = new RequiredFieldValidator();
			_ListBox.ID = "Double" + Dbid;
			
			_Validator.ErrorMessage = Selector.ErrorMessage + " '" + Text + "'";
			_Validator.Display = ValidatorDisplay.Dynamic;
			_Label.Text = Text;

			//ListBox nummer tv�

			_ListBox2 = new ListBox();
			_ListBox2.SelectionMode = ListSelectionMode.Multiple;
			_ListBox2.Width = new Unit("300");
			_ListBox2.Height = new Unit("150");
			_ListBox2.CssClass = "ControlStyle";			
			_ListBox2.ID = "Double2" + Dbid.ToString();

			_TextBox = new TextBox();
			_TextBox.Width = new Unit("0");
			_TextBox.Height = new Unit("0");
			_TextBox.ID = "TextHidden" + Dbid.ToString();

			_TextBoxText = new TextBox();
			_TextBoxText.Width = new Unit("0");
			_TextBoxText.Height = new Unit("0");
			_TextBoxText.ID = "TextHiddenText" + Dbid.ToString();

			_Validator.ControlToValidate = _TextBox.ID;
			_Validator.Text = "*";
		
			_Validator.ErrorMessage = Selector.ErrorMessage + " '" + Text + "'";
			_Validator.Display = ValidatorDisplay.Dynamic;

			//Hidden textBox			

			_TextBox.TextChanged +=new EventHandler(_TextBox_TextChanged);

			//"Knappar" av imagetyp f�r att slippa att sidan skickas vid klick

			_ButtonMoveOne = new Image();
			_ButtonMoveOne.ID = "ButtonOne" + Dbid.ToString();
			
			_ButtonMoveAll = new Image();
			_ButtonMoveAll.ID = "ButtonAll" + Dbid.ToString();

			_ButtonMoveOneBack = new Image();
			_ButtonMoveOneBack.ID = "ButtonOneBack" + Dbid.ToString();

			_ButtonMoveAllBack = new Image();
			_ButtonMoveAllBack.ID = "ButtonAllBack" + Dbid.ToString();

			base.Controls.Add(_Label);
			base.Controls.Add(_ListBox);
			base.Controls.Add(_ListBox2);
			base.Controls.Add(_ButtonMoveOne);
			base.Controls.Add(_ButtonMoveAll);
			base.Controls.Add(_ButtonMoveOneBack);
			base.Controls.Add(_ButtonMoveAllBack);
			AddValidator(_Validator);
			base.Controls.Add(_TextBox);

			if (!Page.IsPostBack)
			{
				LoadData();
				
			}
		}
		protected override void BindData()
		{
			_ListBox.DataSource = MyData.Tables[0];
			_ListBox.DataBind();
			string delimStr = ",";
			char[] delimiter = delimStr.ToCharArray();
	
			_ListBox2.Items.Clear();
			_TextBoxText.Text = "";
			_TextBox.Text = "";

			string[] myPresetArr = DefaultValue.Split(delimiter);
			foreach (string s in myPresetArr)
			{
				int mCount = _ListBox.Items.Count - 1;
				int i=0;
				while (i <= mCount)
				{
					if (_ListBox.Items[i].Value.ToString() == s.ToString())
					{
						_ListBox2.Items.Add(_ListBox.Items[i]);
						_TextBoxText.Text = _TextBoxText.Text + _ListBox.Items[i].Text + ",";
						_TextBox.Text = _TextBox.Text  + _ListBox.Items[i].Value.ToString() + ",";
						_ListBox.Items.Remove(_ListBox.Items[i]);
						break;
					}
					i ++;
				}
			}
			if (_TextBoxText.Text.Length > 0)
			{
				_TextBoxText.Text = _TextBoxText.Text.Substring(0,_TextBoxText.Text.Length -1);
				_TextBox.Text = _TextBox.Text.Substring(0,_TextBox.Text.Length -1);
				//_TextBox.Text = _DefaultValue;
			}
			if (_ListBox2.Items.Count == 0)
			{
				_TextBox.Text = "";
			}

			ReloadDependent();

		}

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna samt skapar s�kv�gar till filer och s�tter attribut p� knapparna
		{
			_ButtonMoveAll.ImageUrl = GetClientFileUrl("down_all_light.gif");
			_ButtonMoveOne.ImageUrl = GetClientFileUrl("down_light.gif");

			_ButtonMoveOneBack.ImageUrl = GetClientFileUrl("up_light.gif");
			_ButtonMoveAllBack.ImageUrl = GetClientFileUrl("up_all_light.gif");


			string _ButtonMoveOneIMG = GetClientFileUrl("down_dark.gif");
			_ButtonMoveOne.Attributes.Add("onMouseDown", "changepic('" + _ButtonMoveOne.ClientID + "','" + _ButtonMoveOneIMG + "')");
			_ButtonMoveOne.Attributes.Add("onMouseOut", "changepic('" + _ButtonMoveOne.ClientID + "','" + _ButtonMoveOne.ImageUrl + "')");
			_ButtonMoveOne.Attributes.Add("onClick", "moveListItem('" + _ListBox.ClientID + "','" + _ListBox2.ClientID + "',0 ,'" + _ListBox2.ClientID + "','" + _TextBox.ClientID + "')");			

			string _ButtonMoveAllIMG = GetClientFileUrl("down_all_dark.gif");
			_ButtonMoveAll.Attributes.Add("onMouseDown", "changepic('" + _ButtonMoveAll.ClientID + "','" + _ButtonMoveAllIMG + "')");
			_ButtonMoveAll.Attributes.Add("onMouseOut", "changepic('" + _ButtonMoveAll.ClientID + "','" + _ButtonMoveAll.ImageUrl + "')");
			_ButtonMoveAll.Attributes.Add("onClick", "moveListItem('" + _ListBox.ClientID + "','" + _ListBox2.ClientID + "',1 ,'" + _ListBox2.ClientID + "','" + _TextBox.ClientID + "')");

			string _ButtonMoveOneBackIMG = GetClientFileUrl("up_dark.gif");
			_ButtonMoveOneBack.Attributes.Add("onMouseDown", "changepic('" + _ButtonMoveOneBack.ClientID + "','" + _ButtonMoveOneBackIMG + "')");
			_ButtonMoveOneBack.Attributes.Add("onMouseOut", "changepic('" + _ButtonMoveOneBack.ClientID + "','" + _ButtonMoveOneBack.ImageUrl + "')");
			_ButtonMoveOneBack.Attributes.Add("onClick", "moveListItem('" + _ListBox2.ClientID + "','" + _ListBox.ClientID + "',0 ,'" + _ListBox2.ClientID + "','" + _TextBox.ClientID + "')");

			string _ButtonMoveAllBackIMG = GetClientFileUrl("up_all_dark.gif");
			_ButtonMoveAllBack.Attributes.Add("onMouseDown", "changepic('" + _ButtonMoveAllBack.ClientID + "','" + _ButtonMoveAllBackIMG + "')");
			_ButtonMoveAllBack.Attributes.Add("onMouseOut", "changepic('" + _ButtonMoveAllBack.ClientID + "','" + _ButtonMoveAllBack.ImageUrl + "')");
			_ButtonMoveAllBack.Attributes.Add("onClick", "moveListItem('" + _ListBox2.ClientID + "','" + _ListBox.ClientID + "',1 ,'" + _ListBox2.ClientID + "','" + _TextBox.ClientID + "')");

			_ButtonMoveOne.Attributes.Add("onMouseUp", "changepic('" + _ButtonMoveOne.ClientID + "','" + _ButtonMoveOne.ImageUrl + "')");
			_ButtonMoveAll.Attributes.Add("onMouseUp", "changepic('" + _ButtonMoveAll.ClientID + "','" + _ButtonMoveAll.ImageUrl + "')");
			_ButtonMoveOneBack.Attributes.Add("onMouseUp", "changepic('" + _ButtonMoveOneBack.ClientID + "','" + _ButtonMoveOneBack.ImageUrl + "')");
			_ButtonMoveAllBack.Attributes.Add("onMouseUp", "changepic('" + _ButtonMoveAllBack.ClientID + "','" + _ButtonMoveAllBack.ImageUrl + "')");

			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,Selector._LabelWidth.ToString());			
			_Label.RenderControl(writer);
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			writer.WriteBeginTag("TABLE");

			writer.WriteAttribute("class","ControlBody");
			writer.WriteAttribute("border","0");
			writer.WriteAttribute("cellspacing","0");
			writer.WriteAttribute("cellpadding","0");
			writer.Write(HtmlTextWriter.TagRightChar);//
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.WriteBeginTag("td");
			writer.WriteAttribute("align","left");
			writer.Write(HtmlTextWriter.TagRightChar);
			_ListBox.RenderControl(writer);
			writer.WriteEndTag("td");

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			writer.WriteBeginTag("TABLE");
			writer.WriteAttribute("class","ControlBody");
			writer.WriteAttribute("border","0");
			writer.WriteAttribute("cellspacing","0");
			writer.WriteAttribute("cellpadding","0");
			writer.Write(HtmlTextWriter.TagRightChar);//

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.WriteBeginTag("td");
			writer.WriteAttribute("width", "75", true);
			writer.WriteAttribute("align","center");
			writer.Write(HtmlTextWriter.TagRightChar);
			_ButtonMoveAll.RenderControl(writer);
			writer.WriteEndTag("td");

			writer.WriteBeginTag("td");
			writer.WriteAttribute("width", "75", true);
			writer.WriteAttribute("align","center");
			writer.Write(HtmlTextWriter.TagRightChar);
			_ButtonMoveOne.RenderControl(writer);
			writer.WriteEndTag("td");			
		
			writer.WriteBeginTag("td");			
			writer.WriteAttribute("width", "75", true);
			writer.WriteAttribute("align","center");
			writer.Write(HtmlTextWriter.TagRightChar);
			_ButtonMoveOneBack.RenderControl(writer);
			writer.WriteEndTag("td");

			writer.WriteBeginTag("td");			
			writer.WriteAttribute("width", "75", true);
			writer.WriteAttribute("align","center");
			writer.Write(HtmlTextWriter.TagRightChar);
			_ButtonMoveAllBack.RenderControl(writer);
			writer.WriteEndTag("td");

			writer.RenderEndTag(); //TR

			writer.WriteEndTag("TABLE");
			writer.WriteBeginTag("td");	//Viktig
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.WriteEndTag("td");//TD
			writer.RenderEndTag();//TD
			writer.RenderEndTag(); //TR

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_ListBox2.RenderControl(writer);
			writer.RenderEndTag();//td
			writer.RenderEndTag();//tr
			
			writer.WriteEndTag("TABLE");


			writer.WriteBeginTag("td");	//Viktig
			writer.WriteAttribute("valign","bottom");
			writer.Write(HtmlTextWriter.TagRightChar);
//			writer.RenderEndTag();//TD
//			writer.RenderEndTag();//TD
//			writer.RenderEndTag(); //TRfffffffff
//
//			writer.RenderEndTag();//tdfffff

//			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
//			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width,"20");
			Panel _Panel = new Panel();
			_Panel.Height = new Unit("20");
		    if (Display)
		    {
                _Panel.Controls.Add(_Validator);
		    }
			_Panel.Controls.Add(_TextBox);
			Controls.Add(_Panel);
			_Panel.RenderControl(writer);

//			writer.RenderEndTag();
//			writer.RenderEndTag();

			writer.WriteEndTag("td");


		}

		protected override void SetAutoPostBack()
		{
			EnsureChildControls();			
		}

		private void Move( System.Object sender, System.Web.UI.ImageClickEventArgs e)//Flyttar v�rden mellan listboxar och textboxar
		{
			ListBox _From = new ListBox();
			ListBox _To = new ListBox();
			ImageButton Butt;
			string One = "ButtonOne" + Dbid.ToString();
			string All = "ButtonAll" + Dbid.ToString();
			string OneBack = "ButtonOneBack" + Dbid.ToString();
			string AllBack = "ButtonAllBack" + Dbid.ToString();
			bool all = false;
			int i = 0;
			Butt = (ImageButton)sender;
			string controlid = Butt.ID;

			if  (controlid == One)
			{				
				_From = _ListBox;
				_To = _ListBox2;
				all = false;
			}

			if  (controlid == All)
			{
				_From = _ListBox;
				_To = _ListBox2;
				all = true;
			}

			if  (controlid == OneBack)
			{
				_From = _ListBox2;
				_To = _ListBox;
				all = false;
			}

			if  (controlid == AllBack)
			{
				_From = _ListBox2;
				_To = _ListBox;
				all = true;
			}
			
			if (all == true)
			{
				foreach (ListItem itm in _From.Items)
				{
					itm.Selected = true;
				}
			}

			int mCount;
			mCount = _From.Items.Count - 1;
			while (i <= mCount)
			{
				if (_From.Items[i].Selected)
				{
					_To.Items.Add(_From.Items[i]);
					_From.Items.Remove(_From.Items[i]);
					mCount = _From.Items.Count - 1;
				}
				else
					i ++;
			}		
		}

		private void _TextBox_TextChanged(object sender, EventArgs e)
		{
			Value = _TextBox.Text;
			DefaultValue = Value.ToString();
			_valid = true;
			SaveSetting();
			LoadData();

			ReloadDependent();
		}

		private void ReloadDependent()
		{
			
			foreach (ParameterBase ctrl in Dependent)
			{
				//ctrl.Reloaded = false;
				ctrl.LoadData();
			}
		}

		protected override void OnPreRender(EventArgs e)//Skapar och registrerar javascript p� sidan
		{
			base.OnPreRender(e);
			string scriptKey = "MoveItemToListBox:" + "1000";
		{
			string scriptBlock =
                @"<script language=""JavaScript"">
               <!--
				
				function changepic(button,pic) 
				{
					var theform = document.aspnetForm.all(button);
					theform.src=pic;
				}

				function moveListItem(ListFrom, ListTo, Type, ListVal, TextVal)
				{
					var lstFrom = document.aspnetForm.all(ListFrom);
					var lstTo = document.aspnetForm.all(ListTo);
					var lstVal = document.aspnetForm.all(ListVal);
					var txtVal = document.aspnetForm.all(TextVal);

					for (i=0;i<lstFrom.length;i++)
					{
						if ((lstFrom[i].selected) || (Type ==1))
						{
							var oOption = document.createElement(""OPTION"");
							oOption.value = lstFrom[i].value;
							oOption.text = lstFrom[i].text;
							lstTo.add(oOption);        
							lstFrom.remove(i);
							i = i -1;
						}
					}
				
					var str = """";
	
					for (i=0;i<lstVal.length;i++)
					{
						str = str + lstVal[i].value + "","";
					}

					str = str.substring(0, str.length -1);			
					txtVal.value = str;
				}
                -->
               </script>";

            Page.ClientScript.RegisterClientScriptBlock(GetType(),scriptKey, scriptBlock);
		}
		}

	}
}

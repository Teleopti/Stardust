using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{

	/// <summary>
	/// Summary description for ParameterListBox.
	/// </summary>
	class ParameterListBox : ParameterBase
	{
		private Label _label;
		private ListBox _listBox;
		private ListBox _listBox2;
		private TextBox _textBox;
		private TextBox _textBoxText;
		private Image _buttonMoveOne;
		private Image _buttonMoveAll;
		private Image _buttonMoveAllBack;
		private Image _buttonMoveOneBack;
		private RequiredFieldValidator _validator;
	 
		//public static readonly object EventListChanged = new object();



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
			_listBox.Items.Clear();
			_listBox2.Items.Clear();
           
		}

		protected override void SetData()
		{
			EnsureChildControls();
			if (Display)
			{
				_validator.Validate();
				if (_validator.IsValid)
				{
					Value = _textBox.Text;
					ParameterText = _textBoxText.Text;
				}
				else
				{
					_validator.Text = "*";
					Value = "";
					ParameterText = "";
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
			_label = new Label {Text = Text};

			_listBox = new ListBox
						   {
							   SelectionMode = ListSelectionMode.Multiple,
							   Width = new Unit(100, UnitType.Percentage),
							   Height = new Unit("150"),
							   CssClass = "ControlStyle",
							   DataTextField = "name",
							   DataValueField = "id",
							   ID = "List" + Dbid
						   };

			_listBox2 = new ListBox
							{
								SelectionMode = ListSelectionMode.Multiple,
								Width = new Unit(100, UnitType.Percentage),
								Height = new Unit("150"),
								CssClass = "ControlStyle",
								ID = "List2" + Dbid
							};

			_textBox = new TextBox
						{
							Width = new Unit(0),
							Height = new Unit(0),
							ID = "TextHidden" + Dbid
						};
			_textBox.Style.Add("position", "absolute");
			_textBox.Style.Add("left", "-10px");

			_textBoxText = new TextBox
							{
								Width = new Unit("0"),
								Height = new Unit("0"),
								ID = "TextHiddenText" + Dbid
							};
            _textBoxText.TextMode = TextBoxMode.MultiLine;
			_textBoxText.Style.Add("position", "absolute");
			_textBoxText.Style.Add("left", "-10px");

			_validator = new RequiredFieldValidator
							{
								ErrorMessage = Selector.ErrorMessage + " '" + Text + "'",
								Display = ValidatorDisplay.Dynamic,
								ControlToValidate = _textBox.ID,
								Text = "*"
							};


			//Hidden textBox			

			_textBox.TextChanged += textBoxTextChanged;

			//"Knappar" av imagetyp f�r att slippa att sidan skickas vid klick

			_buttonMoveOne = new Image { ID = "ButtonOne" + Dbid, SkinID = "RightSmall" };
			_buttonMoveOne.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
			_buttonMoveOne.Style.Add(HtmlTextWriterStyle.MarginRight, "5px");

			_buttonMoveAll = new Image { ID = "ButtonAll" + Dbid, SkinID = "RightAllSmall" };
			_buttonMoveAll.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
			_buttonMoveAll.Style.Add(HtmlTextWriterStyle.MarginRight, "5px");

			_buttonMoveOneBack = new Image { ID = "ButtonOneBack" + Dbid, SkinID = "LeftSmall" };
			_buttonMoveOneBack.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
			_buttonMoveOneBack.Style.Add(HtmlTextWriterStyle.MarginRight, "5px");

			_buttonMoveAllBack = new Image { ID = "ButtonAllBack" + Dbid, SkinID = "LeftAllSmall" };
			_buttonMoveAllBack.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
			_buttonMoveAllBack.Style.Add(HtmlTextWriterStyle.MarginRight, "5px");

			base.Controls.Add(_label);
			base.Controls.Add(_listBox);
			base.Controls.Add(_listBox2);

			base.Controls.Add(_buttonMoveOne);
			base.Controls.Add(_buttonMoveAll);
			base.Controls.Add(_buttonMoveOneBack);
			base.Controls.Add(_buttonMoveAllBack);
			AddValidator(_validator);
			base.Controls.Add(_textBox);
			base.Controls.Add(_textBoxText);

			if (!Page.IsPostBack)
			{
				LoadData();

			}
		}
		protected override void BindData()
		{
			_listBox.DataSource = MyData.Tables[0];
			_listBox.DataBind();
			const string delimStr = ",";
			char[] delimiter = delimStr.ToCharArray();

			_listBox2.Items.Clear();
			_textBoxText.Text = "";
			_textBox.Text = "";
			string[] myPresetArr = DefaultValue.Split(delimiter);
			foreach (string s in myPresetArr)
			{
				int mCount = _listBox.Items.Count - 1;
				int i = 0;
				while (i <= mCount)
				{
					if (_listBox.Items[i].Value == s)
					{
						_listBox2.Items.Add(_listBox.Items[i]);
						_textBoxText.Text = _textBoxText.Text + _listBox.Items[i].Text + ",";
						_textBox.Text = _textBox.Text + _listBox.Items[i].Value + ",";
						_listBox.Items.Remove(_listBox.Items[i]);

						break;
					}
					i++;
				}

			}
			if (_textBoxText.Text.Length > 0)
			{
                _textBoxText.Text = _textBoxText.Text.Substring(0, _textBoxText.Text.Length - 1);
                _textBox.Text = _textBox.Text.Substring(0, _textBox.Text.Length - 1);
			    //_textBoxText.Text = HttpUtility.HtmlEncode(_textBoxText.Text.Substring(0, _textBoxText.Text.Length - 1));
			}
			if (_listBox2.Items.Count == 0)
			{
				_textBox.Text = "";
			}

			reloadDependent();
			SetData();
		}

		protected override void RenderContents(HtmlTextWriter writer)//Ritar upp kontrollerna samt skapar s�kv�gar till filer och s�tter attribut p� knapparna
		{
			const string submitOrNo = "Yes";
			//string submitOrNo = "No";
			//if (Dependent.Count > 0)
			//    submitOrNo = "Yes";

			_buttonMoveOne.Attributes.Add("onclick", "moveListItem('" + _listBox.ClientID + "','" + _listBox2.ClientID + "',0 ,'" + _listBox2.ClientID + "','" + _textBox.ClientID + "','" + _textBoxText.ClientID + "','" + submitOrNo + "')");

			_buttonMoveAll.Attributes.Add("onclick", "moveListItem('" + _listBox.ClientID + "','" + _listBox2.ClientID + "',1 ,'" + _listBox2.ClientID + "','" + _textBox.ClientID + "','" + _textBoxText.ClientID + "','" + submitOrNo + "')");

			_buttonMoveOneBack.Attributes.Add("onclick", "moveListItem('" + _listBox2.ClientID + "','" + _listBox.ClientID + "',0 ,'" + _listBox2.ClientID + "','" + _textBox.ClientID + "','" + _textBoxText.ClientID + "','" + submitOrNo + "')");

			_buttonMoveAllBack.Attributes.Add("onclick", "moveListItem('" + _listBox2.ClientID + "','" + _listBox.ClientID + "',1 ,'" + _listBox2.ClientID + "','" + _textBox.ClientID + "','" + _textBoxText.ClientID + "','" + submitOrNo + "')");

			_listBox.Attributes.Add("ondblclick", "callMoveOneButton('" + _buttonMoveOne.ClientID + "')");
			_listBox2.Attributes.Add("ondblclick", "callMoveOneButton('" + _buttonMoveOneBack.ClientID + "')");

			// Label
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._LabelWidth.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_label.RenderControl(writer);
			writer.RenderEndTag();


			// Ska inneh�lla tv� listboxar och en rad knappar
			// Resten av bredden n�r Labeln f�tt sitt (- 5 f�r validator)
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Selector._List1Width.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");

			// om man har padding p� td som default blir det fult h�r
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:3px 0px 3px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			// Tablen med listboxar och knappar
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			// Listbox 1
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "50%");
			writer.AddStyleAttribute("align", "left");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_listBox.RenderControl(writer);
			writer.RenderEndTag();

			// Knappar
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "5");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:10px 5px 10px 5px");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "align:middle");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write("<Br/>");
			_buttonMoveAll.RenderControl(writer);
			writer.Write("<Br/>");
			writer.Write("<Br/>");
			_buttonMoveOne.RenderControl(writer);
			writer.Write("<Br/>");
			writer.Write("<Br/>");
			_buttonMoveOneBack.RenderControl(writer);
			writer.Write("<Br/>");
			writer.Write("<Br/>");
			_buttonMoveAllBack.RenderControl(writer);
			writer.Write("<Br/>");
			writer.Write("<Br/>");
			writer.RenderEndTag();

			//Listbox 2
			writer.AddStyleAttribute("align", "right");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "50%");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			_listBox2.RenderControl(writer);
			writer.RenderEndTag();

			writer.RenderEndTag();

			writer.RenderEndTag();
			//writer.WriteEndTag("TABLE");

			writer.RenderEndTag();

			// Validator
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "padding:0px 0px 0px 0px");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20");
			var panel = new Panel { Height = new Unit("20") };
			if (Display)
			{
				panel.Controls.Add(_validator);
			}
			panel.Controls.Add(_textBox);
			panel.Controls.Add(_textBoxText);
			Controls.Add(panel);
			panel.RenderControl(writer);

			writer.RenderEndTag();
		}

		protected override void SetAutoPostBack()
		{
			EnsureChildControls();
		}

		private void Move(Object sender, ImageClickEventArgs e)//Flyttar v�rden mellan listboxar och textboxar
		{
			var from = new ListBox();
			var to = new ListBox();
			string one = "ButtonOne" + Dbid;
			string all = "ButtonAll" + Dbid;
			string oneBack = "ButtonOneBack" + Dbid;
			string allBack = "ButtonAllBack" + Dbid;
			bool moveAll = false;
			int i = 0;
			var butt = (ImageButton)sender;
			string controlid = butt.ID;

			if (controlid == one)
			{
				from = _listBox;
				to = _listBox2;
			}

			if (controlid == all)
			{
				from = _listBox;
				to = _listBox2;
				moveAll = true;
			}

			if (controlid == oneBack)
			{
				from = _listBox2;
				to = _listBox;
				moveAll = false;
			}

			if (controlid == allBack)
			{
				from = _listBox2;
				to = _listBox;
				moveAll = true;
			}

			if (moveAll)
			{
				foreach (ListItem itm in from.Items)
				{
					itm.Selected = true;
				}
			}

			int mCount = from.Items.Count - 1;
			while (i <= mCount)
			{
				if (from.Items[i].Selected)
				{
					to.Items.Add(from.Items[i]);
					from.Items.Remove(from.Items[i]);
					mCount = from.Items.Count - 1;
				}
				else
					i++;
			}
		}

		private void textBoxTextChanged(object sender, EventArgs e)
		{
			Value = _textBox.Text;
			DefaultValue = Value.ToString();
			_valid = true;
			SaveSetting();
			LoadData();

			reloadDependent();
		}

		private void reloadDependent()
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
			const string scriptKey = "MoveItemToListBox:" + "1000";

			{
				const string scriptBlock = @"<script language=""JavaScript"">
               <!--
				
				function changepic(button,pic) 
				{
					var theform = document.getElementById(button);
					theform.src=pic;
				}

				function moveListItem(ListFrom, ListTo, Type, ListVal, TextVal, TextText, SubmitOrNo)
				{
					var lstFrom = document.getElementById(ListFrom);
					var lstTo = document.getElementById(ListTo);
					var lstVal = document.getElementById(ListVal);
					var txtVal = document.getElementById(TextVal);
					var txtText = document.getElementById(TextText);
                    
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
					
					if (SubmitOrNo == 'Yes')
						{window.document.getElementById('aspnetForm').submit();}
				}
                function callMoveOneButton(buttonId)
                {
                    var btnMoveOnlyOne = document.getElementById(buttonId);
                    btnMoveOnlyOne.click();
                }
                -->
               </script>";

				Page.ClientScript.RegisterClientScriptBlock(GetType(), scriptKey, scriptBlock);
			}
		}

	}
}

using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public enum CommonAgentNameDescriptionType
	{
		Common,
		ScheduleExport
	}

	public partial class CommonAgentNameDescriptionControl : BaseUserControl, ISettingPage
	{
		private readonly IPerson _johnSmithPerson = new Person();
		private CommonNameDescriptionSetting _commonNameDescriptionSetting;
		private CommonNameDescriptionSettingScheduleExport _commonNameDescriptionSettingScheduleExport;
		private string _firstName;
		private string _lastName;
		private string _emplyeeNumber;
		private readonly CommonAgentNameDescriptionType _type;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public CommonAgentNameDescriptionControl(CommonAgentNameDescriptionType type)
		{
			InitializeComponent();
			_type = type;
			SetTexts();
			readFomatTextsFromResources();
		}

		private void textBoxFormatTextChanged(object sender, EventArgs e)
		{
			string textFomat = textBoxFormat.Text;

			if (_type == CommonAgentNameDescriptionType.Common)
			{
				_commonNameDescriptionSetting.AliasFormat = getDescriptionToSave(textFomat);
				textBoxExample.Text = _commonNameDescriptionSetting.BuildFor(_johnSmithPerson);
			}

			if (_type == CommonAgentNameDescriptionType.ScheduleExport)
			{
				_commonNameDescriptionSettingScheduleExport.AliasFormat = getDescriptionToSave(textFomat);
				textBoxExample.Text = _commonNameDescriptionSettingScheduleExport.BuildFor(_johnSmithPerson);
			}


			setStatus();
  
		}

		private void textBoxFormatMouseUp(object sender, MouseEventArgs e)
		{
			handleSelections();
		}

		private void textBoxFormatKeyUp(object sender, KeyEventArgs e)
		{
			handleSelections();
		}

		private void textBoxFormatPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Left)
			{
				if (textBoxFormat.SelectionLength > 0)
				{
					textBoxFormat.SelectionLength = 0;
				}
			}

			if (e.KeyCode != Keys.Right) return;
			if (textBoxFormat.SelectionLength <= 0) return;
			textBoxFormat.SelectionStart = textBoxFormat.SelectionStart + textBoxFormat.SelectionLength - 1;
			textBoxFormat.SelectionLength = 0;
		}

		private void buttonAdvFirstNameClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			handleFomatAndSample(getStatus(sender), _firstName);
			Cursor.Current = Cursors.Default;
		}

		private void buttonAdvLastNameClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			handleFomatAndSample(getStatus(sender), _lastName);
			Cursor.Current = Cursors.Default;
		}

		private void buttonAdvEmployeeNumberClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			handleFomatAndSample(getStatus(sender), _emplyeeNumber);
			Cursor.Current = Cursors.Default;
		}

		private void handleSelections()
		{
			var leftPosition = textBoxFormat.SelectionStart;
			var rightPosition = textBoxFormat.SelectionStart + textBoxFormat.SelectionLength;
			if (!isSurroundedWithCurlyBraces(textBoxFormat.Text, ref leftPosition, ref rightPosition)) return;
			textBoxFormat.SelectionStart = leftPosition;
			textBoxFormat.SelectionLength = rightPosition - leftPosition;
		}

		private static bool isSurroundedWithCurlyBraces(string text, ref int leftStartPosition, ref int rightStartPosition)
		{
			bool leftBraceFound = false;
			if (leftStartPosition == text.Length)
			{
				return false;
			}
			if (text.Substring(leftStartPosition, 1) == "}")
			{
				leftStartPosition--;
			}
			else if (text.Substring(leftStartPosition, 1) == "{")
			{
				leftStartPosition++;
			}

			if (leftStartPosition == rightStartPosition)
			{
				rightStartPosition++;
			}


			while (leftStartPosition > -1)
			{
				if (text.Substring(leftStartPosition, 1) == "}")
				{
					break;
				}
				if (text.Substring(leftStartPosition, 1) == "{")
				{
					leftBraceFound = true;
					break;
				}
				leftStartPosition--;
			}

			if (leftBraceFound)
			{
				while (rightStartPosition < text.Length)
				{
					if (text.Substring(rightStartPosition, 1) == "}")
					{
						rightStartPosition++;
						return true;
					}
					if (text.Substring(rightStartPosition, 1) == "{")
					{
						break;
					}
					rightStartPosition++;
				}
			}

			return false;
		}

		private static string getDisplayString(string settingKey)
		{
			// Gets the UI culture
			var culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture;
			// Sets the new display text
			return string.Format(culture, "{0}{1}{2}", "{", settingKey, "}");
		}

		private static string removeBracketsFromSettingKey(string settingKey)
		{
			settingKey = settingKey.Replace("{", string.Empty).Replace("}", string.Empty);

			return settingKey;
		}

		private static string getStringFromResources(string settingKey)
		{
			var displayString = Resources.ResourceManager.GetString(settingKey) ?? settingKey;

			return displayString;
		}

		private string localizeNameDescription(string nameDescriptionFomat)
		{
			string firstName = string.Empty;
			string lastName = string.Empty;
			string emplyeeNumber = string.Empty;

			// Gets the key for the display text
			if (_type == CommonAgentNameDescriptionType.Common)
			{
				firstName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.FirstName);
				lastName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.LastName);
				emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSetting.EmployeeNumber);
			}

			if(_type == CommonAgentNameDescriptionType.ScheduleExport)
			{
				firstName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.FirstName);
				lastName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.LastName);
				emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.EmployeeNumber);    
			}

			nameDescriptionFomat = nameDescriptionFomat.Replace(firstName, _firstName);
			nameDescriptionFomat = nameDescriptionFomat.Replace(lastName, _lastName);
			nameDescriptionFomat = nameDescriptionFomat.Replace(emplyeeNumber, _emplyeeNumber);

			return nameDescriptionFomat;
		}

		private string getDescriptionToSave(string nameDescriptionFomat)
		{
			string firstName = string.Empty;
			string lastName = string.Empty;
			string emplyeeNumber = string.Empty;

			if (_type == CommonAgentNameDescriptionType.Common)
			{
			   firstName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.FirstName);
			   lastName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.LastName);
			   emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSetting.EmployeeNumber);
			}

			if (_type == CommonAgentNameDescriptionType.ScheduleExport)
			{
				firstName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.FirstName);
				lastName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.LastName);
				emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.EmployeeNumber);
			}

			nameDescriptionFomat = nameDescriptionFomat.Replace(_firstName, firstName);
			nameDescriptionFomat = nameDescriptionFomat.Replace(_lastName, lastName);
			nameDescriptionFomat = nameDescriptionFomat.Replace(_emplyeeNumber, emplyeeNumber);

			return nameDescriptionFomat;
		}

		private void readFomatTextsFromResources()
		{
			string firstName = string.Empty;
			string lastName = string.Empty;
			string emplyeeNumber = string.Empty;

			if (_type == CommonAgentNameDescriptionType.Common)
			{
			   firstName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.FirstName);
			   lastName = removeBracketsFromSettingKey(CommonNameDescriptionSetting.LastName);
			   emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSetting.EmployeeNumber);
			}

			if (_type == CommonAgentNameDescriptionType.ScheduleExport)
			{
				firstName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.FirstName);
				lastName = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.LastName);
				emplyeeNumber = removeBracketsFromSettingKey(CommonNameDescriptionSettingScheduleExport.EmployeeNumber);
			}

			_firstName = getStringFromResources(firstName);
			_lastName = getStringFromResources(lastName);
			_emplyeeNumber = getStringFromResources(emplyeeNumber);
		}

		private bool isValidRequest(string nameFormat)
		{
			bool isValid = false;

			if (!string.IsNullOrEmpty(nameFormat))
			{
				isValid = nameFormat.Contains(getDisplayString(_firstName)) ||
						  nameFormat.Contains(getDisplayString(_lastName)) ||
						  nameFormat.Contains(getDisplayString(_emplyeeNumber));
			}

			return isValid;
		}



		private void handleFomatAndSample(bool isAlreadyContains, string fomatString)
		{
			string displayFomat = getDisplayString(fomatString);

			if (isAlreadyContains)
			{
				string text = textBoxFormat.Text;

				text = text.Trim().Replace(displayFomat, string.Empty);

				if (!string.IsNullOrEmpty(text))
				{
					textBoxFormat.Text = text;
				}
				else
					setStatus();
			}
			else
			{
				textBoxFormat.SelectedText = displayFomat;
			}
		}

		private static bool getStatus(object sender)
		{
			return (bool)((Button)sender).Tag;
		}

		private void setStatus()
		{
			string text = textBoxFormat.Text;

			setData(buttonAdvFirstName, text, _firstName);
			setData(buttonAdvLastName, text, _lastName);
			setData(buttonAdvEmployeeNumber, text, _emplyeeNumber);
		}

		private static void setData(ButtonAdv button, string fomat, string fomatKey)
		{
			if (fomat.Contains(getDisplayString(fomatKey)))
			{
				button.Tag = true;
				button.BackColor = Color.FromArgb(255, 153, 51);
				button.State = ButtonAdvState.Pressed;
			}
			else
			{
				button.Tag = false;
				button.State = ButtonAdvState.Default;
				button.BackColor = Color.FromArgb(128, 191, 234);
			}
			button.Refresh();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
			setTexts();
		}

		private void setTexts()
		{
			if (_type != CommonAgentNameDescriptionType.ScheduleExport) return;
			labelHeader.Text = Resources.CommonAgentDescriptionForScheduleExports;
			labelSubHeader1.Text = Resources.EditCommonAgentDescriptionForScheduleExportsFormat;
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void LoadControl()
		{
			var aName = new Name("John", "Smith");
			_johnSmithPerson.SetName(aName);
			_johnSmithPerson.SetEmploymentNumber("10");

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				if(_type == CommonAgentNameDescriptionType.Common)
					_commonNameDescriptionSetting = new GlobalSettingDataRepository(uow).FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());

				if (_type == CommonAgentNameDescriptionType.ScheduleExport)
					_commonNameDescriptionSettingScheduleExport = new GlobalSettingDataRepository(uow).FindValueByKey("CommonNameDescriptionScheduleExport", new CommonNameDescriptionSettingScheduleExport());
				changedInfo();
			}

			if(_type == CommonAgentNameDescriptionType.Common)
				textBoxFormat.Text = localizeNameDescription(_commonNameDescriptionSetting.AliasFormat);

			if(_type == CommonAgentNameDescriptionType.ScheduleExport)
				textBoxFormat.Text = localizeNameDescription(_commonNameDescriptionSettingScheduleExport.AliasFormat);
			
			
		}

		private void changedInfo( )
		{
			GlobalSettingData globalSettingData;
			if(_type == CommonAgentNameDescriptionType.Common)
				globalSettingData = ((ISettingValue)_commonNameDescriptionSetting).BelongsTo as GlobalSettingData;
			else
				globalSettingData = ((ISettingValue)_commonNameDescriptionSettingScheduleExport).BelongsTo as GlobalSettingData;

			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			string changed = _localizer.UpdatedByText(globalSettingData, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		public void SaveChanges()
		{
			if (isValidRequest(textBoxFormat.Text))
			{
				Persist();
			}
			else
			{
				throw new ValidationException(Resources.CommonAgentDescription);
			}
		}


		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.AgentSettings);
		}

		public string TreeNode()
		{
			return _type == CommonAgentNameDescriptionType.Common ? Resources.CommonAgentDescription : Resources.CommonAgentDescriptionForScheduleExports;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				if(_type == CommonAgentNameDescriptionType.Common)
					_commonNameDescriptionSetting = new GlobalSettingDataRepository(uow).PersistSettingValue(_commonNameDescriptionSetting).GetValue(new CommonNameDescriptionSetting());

				if (_type == CommonAgentNameDescriptionType.ScheduleExport)
					_commonNameDescriptionSettingScheduleExport = new GlobalSettingDataRepository(uow).PersistSettingValue(_commonNameDescriptionSettingScheduleExport).GetValue(new CommonNameDescriptionSettingScheduleExport());
				
				uow.PersistAll();
				changedInfo();
				setStatus();
			}
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.CommonAgentNameDescription; }
		}
	}
}

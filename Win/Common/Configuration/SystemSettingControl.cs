using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class SystemSettingControl : BaseUserControl, ISettingPage
	{
		private DefaultSegment _defaultSegmentSetting;
		private AdherenceReportSetting _adherenceReportSetting;
		private StringSetting _supportEmailSetting;
		private AsmAlertTime _asmAlertTime;

		public SystemSettingControl()
		{
			InitializeComponent();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			autoLabelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void LoadControl()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_defaultSegmentSetting = new GlobalSettingDataRepository(uow).FindValueByKey("DefaultSegment", new DefaultSegment());
				_adherenceReportSetting = new GlobalSettingDataRepository(uow).FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
				_supportEmailSetting = new GlobalSettingDataRepository(uow).FindValueByKey("SupportEmailSetting", new StringSetting());
				_asmAlertTime = new GlobalSettingDataRepository(uow).FindValueByKey("AsmAlertTime", new AsmAlertTime());
			}
			var calculatorTypeCollection = LanguageResourceHelper.TranslateEnumToList<AdherenceReportSettingCalculationMethod>();
			var adherenceSetting = _adherenceReportSetting.CalculationMethod;
			comboBoxAdvAdherencReportCalculation.DataSource = calculatorTypeCollection;
			comboBoxAdvAdherencReportCalculation.DisplayMember = "Value";
			comboBoxAdvAdherencReportCalculation.ValueMember = "Key";

			foreach (var pair in calculatorTypeCollection.Where(pair => pair.Key == adherenceSetting))
				comboBoxAdvAdherencReportCalculation.SelectedValue = pair.Key;


			comboBoxAdvAdherencReportCalculation.SelectedIndexChanged += comboBoxAdvAdherencReportCalculationSelectedIndexChanged;
			textBoxSuportEmail.Text = _supportEmailSetting.StringValue;
			initIntervalLengthComboBox(_defaultSegmentSetting.SegmentLength);
			numericUpDownAsmSetting.Value = _asmAlertTime.SecondsBeforeChange;
		}

		private void initIntervalLengthComboBox(int defaultLength)
		{
			var intervalLengths = new List<IntervalLengthItem>
												{
													new IntervalLengthItem(10),
													new IntervalLengthItem(15),
													new IntervalLengthItem(30),
													new IntervalLengthItem(60)
												};

			comboBoxAdvIntervalLength.DataSource = intervalLengths;
			comboBoxAdvIntervalLength.DisplayMember = "Text";
			IntervalLengthItem selectedIntervalLengthItem =
					intervalLengths.FirstOrDefault(length => length.Minutes == defaultLength);
			comboBoxAdvIntervalLength.SelectedItem = selectedIntervalLengthItem;
		}

		public void SaveChanges()
		{
			Persist();
		}

		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.SystemSettings);
		}

		public string TreeNode()
		{
			return Resources.SystemSettings;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{ }

		public void Persist()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_defaultSegmentSetting = new GlobalSettingDataRepository(uow).PersistSettingValue(_defaultSegmentSetting).GetValue(new DefaultSegment());
				_adherenceReportSetting = new GlobalSettingDataRepository(uow).PersistSettingValue(_adherenceReportSetting).GetValue(new AdherenceReportSetting());
				supportEmailToSetting();
				_supportEmailSetting = new GlobalSettingDataRepository(uow).PersistSettingValue(_supportEmailSetting).GetValue(new StringSetting());
				_asmAlertTime.SecondsBeforeChange = (int)numericUpDownAsmSetting.Value;
				_asmAlertTime = new GlobalSettingDataRepository(uow).PersistSettingValue(_asmAlertTime).GetValue(new AsmAlertTime());
				uow.PersistAll();
			}
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.SystemSetting; }
		}

		private void comboBoxAdvIntervalLengthSelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedIntervalLengthItem = (IntervalLengthItem)comboBoxAdvIntervalLength.SelectedItem;
			_defaultSegmentSetting.SegmentLength = selectedIntervalLengthItem.Minutes;
		}

		private void comboBoxAdvAdherencReportCalculationSelectedIndexChanged(object sender, EventArgs e)
		{
			var selected = (KeyValuePair<AdherenceReportSettingCalculationMethod, string>)
					comboBoxAdvAdherencReportCalculation.SelectedItem;
			_adherenceReportSetting.CalculationMethod = selected.Key;
		}

		private void supportEmailToSetting()
		{
			_supportEmailSetting.StringValue = textBoxSuportEmail.Text;
		}
	}
}

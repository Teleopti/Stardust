using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Win.Common.Configuration
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public partial class SmsSettingsControl : BaseUserControl, ISettingPage
    {
        private List<IOptionalColumn> _optionalColumnList;
        public OptionalColumnRepository Repository { get; private set; }
        public IUnitOfWork UnitOfWork { get; private set; }
    	private SmsSettings _smsSettingsSetting;

    	public IOptionalColumn SelectedOptionalColumn
        {
            get { return comboBoxOptionalColumns.SelectedItem as IOptionalColumn; }
        }

		public SmsSettingsControl()
        {
			InitializeComponent();
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

        private void loadOptionalColumns()
        {
        	if (Disposing) return;
        	using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
        	{
        		_smsSettingsSetting = new GlobalSettingDataRepository(uow).FindValueByKey("SmsSettings", new SmsSettings());
        	}
        	if (_optionalColumnList == null)
        	{
        		_optionalColumnList = new List<IOptionalColumn>();
        		_optionalColumnList.AddRange(Repository.GetOptionalColumns<Person>());
        	}

        	comboBoxOptionalColumns.DisplayMember = "Name";
        	comboBoxOptionalColumns.DataSource = _optionalColumnList;


        	comboBoxOptionalColumns.SelectedIndex = getIndex(_optionalColumnList, _smsSettingsSetting.OptionalColumnId);
        }

    	private static int getIndex(List<IOptionalColumn> optionalColumnList, Guid id)
    	{
    		foreach (var optionalColumn in optionalColumnList.Where(optionalColumn => optionalColumn.Id.Equals(id)))
    		{
    			return optionalColumnList.IndexOf(optionalColumn);
    		}
    		return -1;
    	}

    	public void InitializeDialogControl()
        {
            setColors();
            SetTexts();
        }

        public void LoadControl()
        {
            loadOptionalColumns();
        }

        public void SaveChanges()
        {
			if (_smsSettingsSetting != null && comboBoxOptionalColumns.SelectedValue != null)
			{
				_smsSettingsSetting.OptionalColumnId = ((OptionalColumn)comboBoxOptionalColumns.SelectedValue).Id.Value;
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new GlobalSettingDataRepository(uow).PersistSettingValue(_smsSettingsSetting);
					uow.PersistAll();
				}
			}
        }

        public void Unload()
        {
            _optionalColumnList = null;
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.SystemSettings); 
        }

        public string TreeNode()
        {
	        return Resources.SmsSettings;
        }

    	public void OnShow()
    	{
			_optionalColumnList = null;
			loadOptionalColumns();
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;
            Repository = new OptionalColumnRepository(UnitOfWork);
        }

        public void Persist()
        {
        	if(_smsSettingsSetting != null)
        	{
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new GlobalSettingDataRepository(uow).PersistSettingValue(_smsSettingsSetting);
				}
        	}
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {}

        public ViewType ViewType
        {
            get { return ViewType.SmsSettings; }
        }


    }
}

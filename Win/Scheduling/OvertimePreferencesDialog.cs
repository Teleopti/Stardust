using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class OvertimePreferencesDialog : BaseRibbonForm
	{
		public OvertimePreferencesDialog()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

        private void savePersonalSettings()
        {
            //if (hasMissedloadingSettings()) return;
            //_defaultGeneralSettings.MapFrom(_schedulingOptions);
            //_defaultAdvancedSettings.MapFrom(_schedulingOptions);
            //_defaultExtraSettings.MapFrom(_schedulingOptions);

            //try
            //{
            //    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            //    {
            //        new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "GeneralSettings", _defaultGeneralSettings);
            //        uow.PersistAll();
            //        new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "AdvancedSettings", _defaultAdvancedSettings);
            //        uow.PersistAll();
            //        new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "ExtraSetting", _defaultExtraSettings);
            //        uow.PersistAll();
            //    }
            //}
            //catch (DataSourceException)
            //{
            //}
        }

        //private bool hasMissedloadingSettings()
        //{
        //    return _defaultGeneralSettings == null ;
        //}


        //private OvertimePreferencesGeneralPersonalSetting _defaultOvertimeGeneralSettings;
	}
}

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public class ExportSkillWizardPages : AbstractWizardPagesNoRoot<ExportSkillModel>
    {
        private readonly ExportSkillModel _stateObj;
        private readonly IExportAcrossBusinessUnitsSettingsProvider _exportAcrossBusinessUnitsSettingsProvider;
        private readonly IExportForecastToFileSettingsProvider _exportForecastToFileSettingsProvider;

        public ExportSkillWizardPages(ExportSkillModel stateObj,IExportAcrossBusinessUnitsSettingsProvider exportAcrossBusinessUnitsSettingsProvider, IExportForecastToFileSettingsProvider exportForecastToFileSettingsProvider)
            : base(stateObj)
        {
            _stateObj = stateObj;
            _exportAcrossBusinessUnitsSettingsProvider = exportAcrossBusinessUnitsSettingsProvider;
            _exportForecastToFileSettingsProvider = exportForecastToFileSettingsProvider;
        }

        public override ExportSkillModel CreateNewStateObj()
        {
            return _stateObj;
        }

        public override string Name
        {
            get { return UserTexts.Resources.Export; }
        }

        public override string WindowText
        {
            get { return UserTexts.Resources.NewExportThreeDots; }
        }

        public void SaveSettings()
        {
            if(_stateObj.ExportToFile)
            {
                _exportForecastToFileSettingsProvider.TransformToSerializableModel( 
                    new DateOnlyPeriod(_stateObj.ExportSkillToFileCommandModel.Period.StartDate,_stateObj.ExportSkillToFileCommandModel.Period.EndDate));
                _exportForecastToFileSettingsProvider.Save();
            }
            else
            {
                _exportAcrossBusinessUnitsSettingsProvider.TransformToSerializableModel(
                    _stateObj.ExportMultisiteSkillToSkillCommandModel.MultisiteSkillSelectionModels);
	            _exportAcrossBusinessUnitsSettingsProvider.ExportAcrossBusinessUnitsSettings.Period =
		            _stateObj.ExportMultisiteSkillToSkillCommandModel.Period;
                    
                _exportAcrossBusinessUnitsSettingsProvider.Save();
            }
        }
    }
}

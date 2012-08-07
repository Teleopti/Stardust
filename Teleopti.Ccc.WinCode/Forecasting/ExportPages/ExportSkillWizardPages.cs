using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
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

        public override ExportSkillModel CreateNewSateObj()
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
            if(_stateObj.ExportSkillToFileCommandModel != null)
            {
                _exportForecastToFileSettingsProvider.TransformToSerializableModel( 
                    new DateOnlyPeriod(_stateObj.ExportSkillToFileCommandModel.Period.StartDate,_stateObj.ExportSkillToFileCommandModel.Period.EndDate));
                _exportForecastToFileSettingsProvider.Save();
            }

            if (_stateObj.ExportMultisiteSkillToSkillCommandModel != null)
            {
                _exportAcrossBusinessUnitsSettingsProvider.TransformToSerilizableModel(
                    _stateObj.ExportMultisiteSkillToSkillCommandModel.MultisiteSkillSelectionModels);
                _exportAcrossBusinessUnitsSettingsProvider.ExportAcrossBusinessUnitsSettings.Period =
                    new DateOnlyPeriod(new DateOnly(_stateObj.ExportMultisiteSkillToSkillCommandModel.Period.StartDate.DateTime),
                                       new DateOnly(_stateObj.ExportMultisiteSkillToSkillCommandModel.Period.EndDate.DateTime));
                _exportAcrossBusinessUnitsSettingsProvider.Save();
            }
        }
    }
}

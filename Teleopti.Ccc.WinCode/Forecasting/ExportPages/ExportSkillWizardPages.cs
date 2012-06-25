using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class ExportSkillWizardPages : AbstractWizardPagesNoRoot<ExportSkillModel>
    {
        private readonly ExportSkillModel _stateObj;
        private readonly IExportAcrossBusinessUnitsSettingsProvider _exportAcrossBusinessUnitsSettingsProvider;

        public ExportSkillWizardPages(ExportSkillModel stateObj,IExportAcrossBusinessUnitsSettingsProvider exportAcrossBusinessUnitsSettingsProvider)
            : base(stateObj)
        {
            _stateObj = stateObj;
            _exportAcrossBusinessUnitsSettingsProvider = exportAcrossBusinessUnitsSettingsProvider;
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

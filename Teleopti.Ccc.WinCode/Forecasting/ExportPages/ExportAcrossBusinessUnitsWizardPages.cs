using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class ExportAcrossBusinessUnitsWizardPages : AbstractWizardPagesNoRoot<ExportMultisiteSkillToSkillCommandModel>
    {
        private readonly ExportMultisiteSkillToSkillCommandModel _stateObj;
        private readonly IExportAcrossBusinessUnitsSettingsProvider _exportAcrossBusinessUnitsSettingsProvider;

        public ExportAcrossBusinessUnitsWizardPages(ExportMultisiteSkillToSkillCommandModel stateObj,IExportAcrossBusinessUnitsSettingsProvider exportAcrossBusinessUnitsSettingsProvider)
            : base(stateObj)
        {
            _stateObj = stateObj;
            _exportAcrossBusinessUnitsSettingsProvider = exportAcrossBusinessUnitsSettingsProvider;
        }

        public override ExportMultisiteSkillToSkillCommandModel CreateNewSateObj()
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
            _exportAcrossBusinessUnitsSettingsProvider.TransformToSerializableModel(_stateObj.MultisiteSkillSelectionModels);
            _exportAcrossBusinessUnitsSettingsProvider.ExportAcrossBusinessUnitsSettings.Period =
                new DateOnlyPeriod(new DateOnly(_stateObj.Period.StartDate.DateTime),
                                   new DateOnly(_stateObj.Period.EndDate.DateTime));
            _exportAcrossBusinessUnitsSettingsProvider.Save();
        }
    }
}

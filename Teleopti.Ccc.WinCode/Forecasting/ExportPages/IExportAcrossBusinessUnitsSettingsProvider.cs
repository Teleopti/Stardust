using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportAcrossBusinessUnitsSettingsProvider
    {
        IExportAcrossBusinessUnitsSettings ExportAcrossBusinessUnitsSettings { get; }
        IEnumerable<MultisiteSkillSelectionModel> TransformSerilizableToSelectionModels();
        void Save();
        void TransformToSerilizableModel(IEnumerable<MultisiteSkillSelectionModel> multisiteSkillSelectionModels);
    }
}
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public interface IExportAcrossBusinessUnitsSettingsProvider
    {
        IExportAcrossBusinessUnitsSettings ExportAcrossBusinessUnitsSettings { get; }
        IEnumerable<MultisiteSkillSelectionModel> TransformSerializableToSelectionModels();
        void Save();
        void TransformToSerializableModel(IEnumerable<MultisiteSkillSelectionModel> multisiteSkillSelectionModels);
    }
}
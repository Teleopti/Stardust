using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    [Serializable]
    public class ExportAcrossBusinessUnitsSettings : SettingValue, IExportAcrossBusinessUnitsSettings
    {
        public IDictionary<Guid, IEnumerable<ChildSkillSelectionMapping>> MultisiteSkillSelections { get; set; }
        public DateOnlyPeriod Period { get; set; }

        public ExportAcrossBusinessUnitsSettings()
        {
            MultisiteSkillSelections = new Dictionary<Guid, IEnumerable<ChildSkillSelectionMapping>>();
        }
    }

    [Serializable]
    public class ChildSkillSelectionMapping
    {
        public Guid SourceSkillId { get; set; }
        public Guid TargetSkillId { get; set; }
        public string TargetBuName { get; set; }
        public string TargetSkillName { get; set; }
    }
}
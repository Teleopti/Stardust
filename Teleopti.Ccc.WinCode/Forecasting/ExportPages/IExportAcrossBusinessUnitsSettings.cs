using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public interface IExportAcrossBusinessUnitsSettings : ISettingValue
    {
        IDictionary<Guid, IEnumerable<ChildSkillSelectionMapping>> MultisiteSkillSelections { get; set; }
        DateOnlyPeriod Period { get; set; }
    }
}
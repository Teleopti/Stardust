using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    public interface ICopyToSkillView
    {
        void SetCopyFromText(string sourceWorkloadName);
        void ToggleIncludeTemplates(bool includeTemplates);
        void ToggleIncludeQueues(bool includeQueues);
        void AddSkillToList(string name, ISkill skill);
        void NoMatchingSkillsAvailable();
        void Close();
        void TriggerEntitiesNeedRefresh(IEnumerable<IRootChangeInfo> changes);
    }
}
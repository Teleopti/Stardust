using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class CopyToSkillModel
    {
        private readonly IWorkload _workload;
        
        public CopyToSkillModel(IWorkload workload)
        {
            _workload = workload;

            IncludeQueues = false;
            IncludeTemplates = true;
            SourceWorkloadName = _workload.Name;
        }

        public IWorkload Workload
        {
            get { return _workload; }
        }

        public bool IncludeTemplates { get; set; }

        public bool IncludeQueues { get; set; }

        public ISkill TargetSkill { get; set; }

        public string SourceWorkloadName { get; private set; }
    }
}
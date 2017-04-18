using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
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
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class ReforecastModel
    {
        public ISkill Skill { get; set; }

        public IList<IWorkload> Workload { get; private set; }

        public ReforecastModel()
        {
            Workload = new List<IWorkload>();
        }
    }

    public class ReforecastModelCollection
    {
        public IList<ReforecastModel> ReforecastModels { get; private set; }

        public ReforecastModelCollection()
        {
            ReforecastModels = new List<ReforecastModel>();
        }
    }
}

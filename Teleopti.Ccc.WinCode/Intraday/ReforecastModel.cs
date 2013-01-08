using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class ReforecastModel
    {
        public ISkill Skill { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
             "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<IWorkload> Workload { get; set; }

        public ReforecastModel()
        {
            Workload = new List<IWorkload>();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
        "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class ReforecastModelCollection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<ReforecastModel> ReforecastModels { get; set; }

        public ReforecastModelCollection()
        {
            ReforecastModels = new List<ReforecastModel>();
        }
    }
}

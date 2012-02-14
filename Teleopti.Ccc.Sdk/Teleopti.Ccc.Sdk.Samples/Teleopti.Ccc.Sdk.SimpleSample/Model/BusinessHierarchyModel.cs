using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Model
{
    public class BusinessHierarchyModel
    {
        public BusinessHierarchyModel()
        {
            TeamCollection = new List<TeamDto>();
        }

        public IList<TeamDto> TeamCollection { get; private set; }

        public SiteModel Site { get; set; }
    }
}
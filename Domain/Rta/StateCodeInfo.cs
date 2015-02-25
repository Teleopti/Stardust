using System;

namespace Teleopti.Ccc.Domain.Rta
{
    public class StateCodeInfo
    {
		public string StateCode { get; set; }
		public string StateName { get; set; }
        public Guid PlatformTypeId { get; set; }
		public Guid BusinessUnitId { get; set; }
        public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
        public bool IsLogOutState { get; set; }
    }
}
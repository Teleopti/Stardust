using System;

namespace Teleopti.Ccc.Rta.Server
{
    public class RtaStateGroupLight
    {
        public Guid StateId { get; set; }
        public string StateName { get; set; }
        public string StateGroupName { get; set; }
        public Guid PlatformTypeId { get; set; }
        public string StateCode { get; set; }
        public Guid StateGroupId { get; set; }
        public Guid BusinessUnitId { get; set; }
        public bool IsLogOutState { get; set; }
    }
}
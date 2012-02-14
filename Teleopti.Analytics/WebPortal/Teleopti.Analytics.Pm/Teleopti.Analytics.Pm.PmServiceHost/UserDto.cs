using System.Runtime.Serialization;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    [DataContract]
    public class UserDto
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int AccessLevel { get; set; }
        [DataMember]
        public bool IsWindowsLogOn { get; set; }
    }
}
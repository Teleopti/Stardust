using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{
    public class ActiveDirectoryUserTestClass : ActiveDirectoryUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID")]
        public void SetObjectGUIDString(string objectGUIDStringValue)
        {
            ObjectGUIDString = objectGUIDStringValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID")]
        public void SetObjectSIDString(string objectSIDStringValue)
        {
            ObjectSIDString = objectSIDStringValue;
        }

        public void SetPath(string pathValue)
        {
            Path = pathValue;
        }
    }
}

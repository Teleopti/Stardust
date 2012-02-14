using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ActiveDirectoryUserFactory
    {

        /// <summary>
        /// Creates a fake active directory user.
        /// </summary>
        /// <returns></returns>
        public static ActiveDirectoryUser CreateActiveDirectoryUserWithTwoGroups()
        {
            ActiveDirectoryUser user = new ActiveDirectoryUser();
            user.LastName = "LastName";
            user.FirstName = "FirstName";

            ActiveDirectoryGroup group1 = new ActiveDirectoryGroup();
            group1.CommonName = "Group1";
            group1.DistinguishedName = "Group1Description";

            ActiveDirectoryGroup group2 = new ActiveDirectoryGroup();
            group1.CommonName = "Group2";
            group1.DistinguishedName = "Group2Description";

            user.TokenGroups.Add(group1);
            user.TokenGroups.Add(group2);

            return user;
        }
    }
}

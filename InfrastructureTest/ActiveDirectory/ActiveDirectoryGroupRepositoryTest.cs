using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;
using Teleopti.Ccc.Infrastructure.ActiveDirectory;


namespace Teleopti.Ccc.InfrastructureTest.ActiveDirectory
{

    [TestFixture]
    [Category("LongRunning")]
    public class ActiveDirectoryGroupRepositoryTest
    {

        ActiveDirectoryGroupRepository _target;

        [SetUp]
        public void Setup()
        {
            _target = new ActiveDirectoryGroupRepository();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyFindGroupsListOverload1()
        {
            IList<ActiveDirectoryGroup> allGroups = _target.FindGroups(null, null, 10, null);

            Assert.IsNotNull(allGroups);
            Assert.Greater(allGroups.Count, 0);
           
        }

        [Test]
        public void VerifyFindUsersListOverload2()
        {
            IList<ActiveDirectoryGroup> allGroups = _target.FindGroups(ActiveDirectoryGroupMapper.COMMONNAME, "*");

            Assert.IsNotNull(allGroups);
            Assert.Greater(allGroups.Count, 0);
        }

        [Test]
        public void VerifyFindGroup()
        {
            IList<ActiveDirectoryGroup> allGroups = _target.FindGroups(null, null, 10, null);

            string inListGroupName = allGroups[0].CommonName;

            ActiveDirectoryGroup foundGroup = _target.FindGroup(ActiveDirectoryGroupMapper.COMMONNAME, inListGroupName);

            Assert.IsNotNull(foundGroup);
            Assert.AreEqual(foundGroup.CommonName, inListGroupName);

        }

        [Test]
        public void VerifyFindGroupWithNonexistentGroup()
        {
            string nonInListUserName = "nonExistenceGroup";

            ActiveDirectoryGroup foundGroup = _target.FindGroup(ActiveDirectoryGroupMapper.COMMONNAME, nonInListUserName);

            Assert.IsNull(foundGroup);

        }

    }
}

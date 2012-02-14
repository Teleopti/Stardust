using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class SystemRoleApplicationRoleMapperRepositoryTest : RepositoryTest<SystemRoleApplicationRoleMapper>
    {
        private SystemRoleApplicationRoleMapperRepository _target;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _target = new SystemRoleApplicationRoleMapperRepository(UnitOfWork);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override SystemRoleApplicationRoleMapper CreateAggregateWithCorrectBusinessUnit()
        {

            ApplicationRole role = ApplicationRoleFactory.CreateRole("TestRole", "Desc");
            PersistAndRemoveFromUnitOfWork(role);

            SystemRoleApplicationRoleMapper ret = new SystemRoleApplicationRoleMapper();
            ret.SystemName = "SystemName";
            ret.SystemRoleLongName = "SystemRoleName";
            ret.ApplicationRole = role;
            return ret;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(SystemRoleApplicationRoleMapper loadedAggregateFromDatabase)
        {
            SystemRoleApplicationRoleMapper org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.SystemName, loadedAggregateFromDatabase.SystemName);
            Assert.AreEqual(org.SystemRoleLongName, loadedAggregateFromDatabase.SystemRoleLongName);
            Assert.AreEqual(org.ApplicationRole.Name, loadedAggregateFromDatabase.ApplicationRole.Name);
        }

        [Test]
        public void VerifyFindAllBySystemName()
        {
            string toBeFoundSystemName = "ToBeFoundSystemName";

            ApplicationRole mappedRole = ApplicationRoleFactory.CreateRole("MappedRole", "Desc1");
            PersistAndRemoveFromUnitOfWork(mappedRole);

            ApplicationRole unMappedRole = ApplicationRoleFactory.CreateRole("UnMappedRole", "Desc2");
            PersistAndRemoveFromUnitOfWork(unMappedRole);

            SystemRoleApplicationRoleMapper okMapper = new SystemRoleApplicationRoleMapper();
            okMapper.SystemName = toBeFoundSystemName;
            okMapper.SystemRoleLongName = "ToBeFoundSystemRoleName1";
            okMapper.ApplicationRole = mappedRole;
            PersistAndRemoveFromUnitOfWork(okMapper);

            SystemRoleApplicationRoleMapper notOkMapper = new SystemRoleApplicationRoleMapper();
            notOkMapper.SystemName = "NotToBeFoundSystemName";
            notOkMapper.SystemRoleLongName = "NotToBeFoundSystemRoleName";
            notOkMapper.ApplicationRole = unMappedRole;
            PersistAndRemoveFromUnitOfWork(notOkMapper);

            List<SystemRoleApplicationRoleMapper> result = new List<SystemRoleApplicationRoleMapper>(_target.FindAllBySystemName(toBeFoundSystemName));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(result[0].SystemRoleLongName, okMapper.SystemRoleLongName);

        }

        protected override Repository<SystemRoleApplicationRoleMapper> TestRepository(IUnitOfWork unitOfWork)
        {
            return new SystemRoleApplicationRoleMapperRepository(unitOfWork);
        }
    }
}

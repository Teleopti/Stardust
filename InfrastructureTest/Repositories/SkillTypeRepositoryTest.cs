using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for SkillTypeRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class SkillTypeRepositoryTest : RepositoryTest<ISkillType>
    {
 
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            //todo: I am cleaning the SkillType repository here because other test classes 
            //have left the database in some un-cleaned mode. Developers seem to 
            //over use the SkipRollback();
            SkillTypeRepository rep = new SkillTypeRepository(UnitOfWork);
            IList<ISkillType> skillsTypes = new List<ISkillType>(rep.LoadAll());

            foreach (ISkillType type in skillsTypes)
            {
                rep.Remove(type);
            }
        }

        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override ISkillType CreateAggregateWithCorrectBusinessUnit()
        {
            return SkillTypeFactory.CreateSkillTypePhone();
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(ISkillType loadedAggregateFromDatabase)
        {
            ISkillType skillType = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(skillType.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(skillType.ForecastSource, loadedAggregateFromDatabase.ForecastSource);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            new SkillTypeRepository(UnitOfWork);

        }

        [Test]
        public void CanFindAll()
        {         
            SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            PersistAndRemoveFromUnitOfWork(skillType);

            SkillTypeRepository rep = new SkillTypeRepository(UnitOfWork);
          
            IList<ISkillType> skillsTypes = new List<ISkillType>(rep.LoadAll());

            ISkillType skillTypeToTest = skillsTypes[0];
            Assert.AreEqual(1, skillsTypes.Count);
            Assert.AreEqual(skillTypeToTest, skillType);
           
        }

        protected override Repository<ISkillType> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new SkillTypeRepository(currentUnitOfWork);
        }
    }
}
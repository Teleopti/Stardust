using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for OutlierRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class OutlierRepositoryTest : RepositoryTest<IOutlier>
    {
        private IWorkload _workload;
        private SkillType _skillType;
        private ISkill _skill;
        private IActivity _activity;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _skillType = SkillTypeFactory.CreateSkillTypePhone();
            PersistAndRemoveFromUnitOfWork(_skillType);
						_activity = new Activity("The test") { DisplayColor = Color.Honeydew };
            PersistAndRemoveFromUnitOfWork(_activity);
            _skill = SkillFactory.CreateSkill("test!", _skillType, 15);
            _skill.Activity = _activity;
            PersistAndRemoveFromUnitOfWork(_skill);
            _workload = WorkloadFactory.CreateWorkload(_skill);
            PersistAndRemoveFromUnitOfWork(_workload);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-18
        /// </remarks>
        protected override IOutlier CreateAggregateWithCorrectBusinessUnit()
        {
            Outlier outlier = new Outlier(_workload, new Description("Juldagen","Juld."));
            outlier.AddDate(new DateOnly(2008, 12, 25));

            return outlier;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IOutlier loadedAggregateFromDatabase)
        {
            IOutlier outlier = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(outlier.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(outlier.Dates[0], loadedAggregateFromDatabase.Dates[0]);
            //Assert.AreEqual(outlier.Workload.Name, loadedAggregateFromDatabase.Workload.Name);
        }

        protected override Repository<IOutlier> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return OutlierRepository.DONT_USE_CTOR2(currentUnitOfWork);
        }

        /// <summary>
        /// Verifies the get outlier both global and for workload.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        [Test]
        public void VerifyGetOutlierBothGlobalAndForWorkload()
        {
            IOutlier newOutlier1 = CreateAggregateWithCorrectBusinessUnit();
            IOutlier newOutlier2 = new Outlier(new Description("Julafton"));
            newOutlier2.AddDate(new DateOnly(2008, 12, 24));

            PersistAndRemoveFromUnitOfWork(newOutlier1);
            PersistAndRemoveFromUnitOfWork(newOutlier2);

            OutlierRepository rep = OutlierRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IOutlier> outliers = rep.FindByWorkload(_workload);

            Assert.AreEqual(2, outliers.Count);
            Assert.IsTrue(outliers.Contains(newOutlier1));
            Assert.IsTrue(outliers.Contains(newOutlier2));
        }

        [Test]
        public void FindByWorkloadShouldEagerLoadAllDates()
        {
            var newOutlier1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(newOutlier1);

            var rep = OutlierRepository.DONT_USE_CTOR(UnitOfWork);
            var outliers = rep.FindByWorkload(_workload);

            LazyLoadingManager.IsInitialized(outliers[0].Dates)
                .Should().Be.True();
        }

        [Test]
        public void ShouldReturnUniqueOutliers()
        {
            var newOutlier1 = CreateAggregateWithCorrectBusinessUnit();
            var newOutlier2 = new Outlier(new Description("Julafton"));
            newOutlier2.AddDate(new DateOnly(2008, 12, 24));
            newOutlier2.AddDate(new DateOnly(2008, 12, 25));
            newOutlier2.AddDate(new DateOnly(2008, 12, 26));

            PersistAndRemoveFromUnitOfWork(newOutlier1);
            PersistAndRemoveFromUnitOfWork(newOutlier2);

            var rep = OutlierRepository.DONT_USE_CTOR(UnitOfWork);
            var outliers = rep.FindByWorkload(_workload);

            Assert.AreEqual(2, outliers.Count);
            Assert.IsTrue(outliers.Contains(newOutlier1));
            Assert.IsTrue(outliers.Contains(newOutlier2));
        }

    }
}

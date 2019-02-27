using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class ActivityRepositoryTest : RepositoryTest<IActivity>
    {
        [Test]
        public void VerifyLoadSortedByDescription()
        {
            var activity1 = new Activity("zz");
			var activity2 = new Activity("ff");
			var activity3 = new Activity("aa");

            PersistAndRemoveFromUnitOfWork(activity1);
            PersistAndRemoveFromUnitOfWork(activity2);
            PersistAndRemoveFromUnitOfWork(activity3);

            ActivityRepository rep = ActivityRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IActivity> lst = rep.LoadAllSortByName();

            Assert.AreEqual("aa",lst[0].Description.Name);
            Assert.AreEqual("ff", lst[1].Description.Name);
            Assert.AreEqual("zz", lst[2].Description.Name  );

        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IActivity CreateAggregateWithCorrectBusinessUnit()
        {
			var act = new Activity("roger") { DisplayColor = Color.White };
            act.RequiresSkill = false;
            return act;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IActivity loadedAggregateFromDatabase)
        {
            IActivity org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.RequiresSkill, loadedAggregateFromDatabase.RequiresSkill);
        }

        [Test]
        public void VerifyCanPersistProperties()
        {
            IActivity activity = CreateAggregateWithCorrectBusinessUnit(); 
            activity.InPaidTime = true;
            activity.InWorkTime = true;
        	activity.PayrollCode = "payrollcode007";
            activity.AllowOverwrite = true;

            PersistAndRemoveFromUnitOfWork(activity);
            IActivity loadedAbctivity = ActivityRepository.DONT_USE_CTOR(UnitOfWork).Load(activity.Id.Value);
            Assert.AreEqual(true, loadedAbctivity.InPaidTime);
            Assert.AreEqual(true, loadedAbctivity.InWorkTime);
			Assert.AreEqual("payrollcode007", loadedAbctivity.PayrollCode);
            Assert.AreEqual(true, loadedAbctivity.AllowOverwrite );
        }

		[Test]
		public void LoadAllShouldBeOverriden()
		{
			//not really testing correct thing, just verifying it works as before
			var activity = new Activity("asdf");
			PersistAndRemoveFromUnitOfWork(activity);
			var masterActivity = new MasterActivity {Description = new Description("asdf")};
			masterActivity.ActivityCollection.Add(activity);
			PersistAndRemoveFromUnitOfWork(masterActivity);

			var allActivities = TestRepository(CurrUnitOfWork).LoadAll();
			
			allActivities.Any(x => x is MasterActivity)
				.Should().Be.False();
		}

        protected override Repository<IActivity> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
        }
    }
}

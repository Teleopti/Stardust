using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests ActivityRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ActivityRepositoryTest : RepositoryTest<IActivity>
    {
        private IActivity _activity1;
        private IActivity _activity2;
        private IActivity _activity3;

        protected override void ConcreteSetup()
        {
        }

        [Test]
        public void VerifyLoadSortedByDescription()
        {
            _activity1 = new Activity("zz");
						_activity2 = new Activity("ff");
						_activity3 = new Activity("aa");

            PersistAndRemoveFromUnitOfWork(_activity1);
            PersistAndRemoveFromUnitOfWork(_activity2);
            PersistAndRemoveFromUnitOfWork(_activity3);

            ActivityRepository rep = new ActivityRepository(UnitOfWork);
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
            IActivity loadedAbctivity = new ActivityRepository(UnitOfWork).Load(activity.Id.Value);
            Assert.AreEqual(true, loadedAbctivity.InPaidTime);
            Assert.AreEqual(true, loadedAbctivity.InWorkTime);
			Assert.AreEqual("payrollcode007", loadedAbctivity.PayrollCode);
            Assert.AreEqual(true, loadedAbctivity.AllowOverwrite );
        }

        protected override Repository<IActivity> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ActivityRepository(unitOfWork);
        }
    }
}

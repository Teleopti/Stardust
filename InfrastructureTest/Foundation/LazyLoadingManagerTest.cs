using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class LazyLoadingManagerTest : DatabaseTest
    {
        protected override void SetupForRepositoryTest()
        {
        }

        [Test]
        public void VerifyIsInitialized()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.Name = new Name("for", "test");
            PersistAndRemoveFromUnitOfWork(person);

            person = Session.Load<Person>(person.Id.Value);
            Assert.IsFalse(LazyLoadingManager.IsInitialized(person));
            Assert.AreEqual("for", person.Name.FirstName);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(person));
        }

        [Test]
        public void VerifyInitializeOnEntity()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.Name = new Name("for", "test");
            PersistAndRemoveFromUnitOfWork(person);

            person = Session.Load<Person>(person.Id.Value);
            Assert.IsFalse(LazyLoadingManager.IsInitialized(person));
            LazyLoadingManager.Initialize(person);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(person));
        }

        [Test]
        public void VerifyInitializeWorksWithWorkloadTemplateWeekCollection()
        {
            ISkill skill = SkillFactory.CreateSkill("testSkill");

			skill.Activity.SetId(null);
            PersistAndRemoveFromUnitOfWork(skill.Activity);

            PersistAndRemoveFromUnitOfWork(skill.SkillType);
            PersistAndRemoveFromUnitOfWork(skill);

            IWorkload w = WorkloadFactory.CreateWorkload(skill);

            WorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimePeriod timePeriod = new TimePeriod(new TimeSpan(1,0,0), new TimeSpan(2,0,0));
            openHours.Add(timePeriod);

            workloadDayTemplate.Create("<JULAFTON>",DateTime.UtcNow, w, openHours);

            w.SetTemplateAt((int)DayOfWeek.Monday,workloadDayTemplate);

            PersistAndRemoveFromUnitOfWork(w);

            w = Session.Load<Workload>(w.Id.Value);

            IWorkloadDayTemplate template =
                ((IWorkloadDayTemplate) w.GetTemplate(TemplateTarget.Workload, DayOfWeek.Monday));
            Assert.IsFalse(LazyLoadingManager.IsInitialized(template.OpenHourList));
            LazyLoadingManager.Initialize(template.OpenHourList);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(template.OpenHourList));
        }

        [Test]
        public void VerifyInitializeWorksWithLayerWrapperCollection()
        {
            IShiftCategory shiftCat = ShiftCategoryFactory.CreateShiftCategory("sdf");
            PersistAndRemoveFromUnitOfWork(shiftCat);
            IActivity act = new Activity("test");
            PersistAndRemoveFromUnitOfWork(act);
            IPerson per = PersonFactory.CreatePerson("hola23423423");
            PersistAndRemoveFromUnitOfWork(per);
            IScenario scen = ScenarioFactory.CreateScenarioAggregate();
            scen.Description = new Description("scen");
            PersistAndRemoveFromUnitOfWork(scen);
            IPersonAssignment pAss =
                PersonAssignmentFactory.CreateAssignmentWithMainShift(act, per,
                                                                      new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                                      shiftCat, scen);

            PersistAndRemoveFromUnitOfWork(pAss);

            pAss = Session.Load<PersonAssignment>(pAss.Id.Value);
						Assert.IsFalse(LazyLoadingManager.IsInitialized(pAss.ShiftLayers));
            LazyLoadingManager.Initialize(pAss.ShiftLayers);
						Assert.IsTrue(LazyLoadingManager.IsInitialized(pAss.ShiftLayers));
        }

        [Test]
        public void VerifyInitializeNullDoesNotCrash()
        {
            LazyLoadingManager.Initialize(null);
        }

        [Test]
        public void VerifyIsInitializedWhenNullReturnsTrue()
        {
            Assert.IsTrue(LazyLoadingManager.IsInitialized(null));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void WrapperShouldHaveCoverage()
		{
			var wrapper = new LazyLoadingManagerWrapper();
			wrapper.Initialize(null);
			wrapper.IsInitialized(null);
		}
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class GeneralPersistTest : DatabaseTest
    {

        [Test]
        public void VerifyColorMapping()
        {
            IActivity act = new Activity("sdfsdf") {DisplayColor = Color.Red};
            PersistAndRemoveFromUnitOfWork(act);
            act = Session.Load<Activity>(act.Id);
            Assert.AreEqual(Color.FromArgb(255,0,0), act.DisplayColor);
            act.DisplayColor = Color.Blue;
            Session.Flush();
            Session.Evict(act);
            act = Session.Load<Activity>(act.Id);
            Color blue = Color.FromArgb(0, 0, 255);
            Assert.AreEqual(blue, act.DisplayColor);
            Assert.AreEqual(act.DisplayColor.GetHashCode(), new ColorNumber().GetHashCode(blue));
            act = Session.Merge(act);
            Assert.AreEqual(blue, act.DisplayColor); //verify replace
            //verify assemble/disambele (used when caching, not used at the moment)
            Assert.AreEqual(blue, new ColorNumber().Assemble(act.DisplayColor, null));
            Assert.AreEqual(blue, new ColorNumber().Disassemble(act.DisplayColor));
        }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	    [Test]
	    [Explicit("This one is for you, Roger")]
	    public void VerifyThatRefreshWorksWithRemovedTemplate()
        {
            IUnitOfWork UnitOfWork2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
            ISkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            IActivity activity = ActivityFactory.CreateActivity("The test", Color.Honeydew);
            ISkill skill = SkillFactory.CreateSkill("Skill - Name", skillType, 15);

            new SkillTypeRepository(UnitOfWork).Add(skillType);
            new ActivityRepository(UnitOfWork).Add(activity);
            skill.Activity = activity;
            new SkillRepository(UnitOfWork).Add(skill);

            IWorkload workload = WorkloadFactory.CreateWorkload(skill);
            IWorkloadDayTemplate template = new WorkloadDayTemplate();
            template.Create("TemplateName", DateTime.UtcNow, workload,
                            new List<TimePeriod> {new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(11))});
            workload.AddTemplate(template);

			try
            {
                IWorkloadRepository rep1 = new WorkloadRepository(UnitOfWork);
                rep1.Add(workload);
                UnitOfWork.PersistAll();

                IWorkloadRepository rep2 = new WorkloadRepository(UnitOfWork2);
                var secondWorkload = rep2.Get(workload.Id.Value);
                LazyLoadingManager.Initialize(secondWorkload.TemplateWeekCollection);

                workload.RemoveTemplate(TemplateTarget.Workload, template.Name);
                rep1.Add(workload);
                UnitOfWork.PersistAll();

                CleanUpAfterTest();

                UnitOfWork2.Refresh(secondWorkload);

                Assert.IsTrue(UnitOfWork2.Contains(secondWorkload));
                Assert.IsTrue(UnitOfWork.Contains(workload));
            }
            catch (Exception ex)
            {
                Assert.Fail("Test failed! {0}", ex.Message);
            }
            finally
            {
                new ActivityRepository(UnitOfWork).Remove(activity);
                new SkillTypeRepository(UnitOfWork).Remove(skillType);
                new SkillRepository(UnitOfWork).Remove(skill);
                new WorkloadRepository(UnitOfWork).Remove(workload);
                UnitOfWork.PersistAll();
                UnitOfWork.Clear();
            }
        }


        [Test]
        public void VerifyDisableDeleteFilter()
        {
            IPerson per = PersonFactory.CreatePerson("Kalle", "Banan");
            PersistAndRemoveFromUnitOfWork(per);

            PersonRepository rep = new PersonRepository(new ThisUnitOfWork(UnitOfWork));
            rep.Remove(per);
            PersistAndRemoveFromUnitOfWork(per);

            using(UnitOfWork.DisableFilter(QueryFilter.Deleted))
            {
                CollectionAssert.Contains(rep.LoadAll(), per);                
            }
            CollectionAssert.DoesNotContain(rep.LoadAll(), per);
        }

        protected override void SetupForRepositoryTest()
        {
        }
    }
}

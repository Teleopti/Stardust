using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class WorkflowControlSetRepositoryTest : RepositoryTest<IWorkflowControlSet>
    {
        private IAbsence _absence;
        private IActivity _activity;
        private ISkill _skill;
        private ISkillType _type;
        private IDayOffTemplate _dayOff;
        private IShiftCategory _shiftCategory;

        protected override void ConcreteSetup()
        {
            _absence = AbsenceFactory.CreateAbsence("Holiday");
            PersistAndRemoveFromUnitOfWork(_absence);
            
            _type = new SkillTypePhone(new Description("äda"), ForecastSource.InboundTelephony);
            PersistAndRemoveFromUnitOfWork(_type);

            _activity = new Activity("ö,s");
            PersistAndRemoveFromUnitOfWork(_activity);

            _skill = new Skill ("name","afas", Color.Red,15,_type);
            _skill.Activity = _activity;
            _skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            PersistAndRemoveFromUnitOfWork(_skill);

            _dayOff = DayOffFactory.CreateDayOff(new Description("dayOff"));
            PersistAndRemoveFromUnitOfWork(_dayOff);

            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
            PersistAndRemoveFromUnitOfWork(_shiftCategory);
        }

        protected override IWorkflowControlSet CreateAggregateWithCorrectBusinessUnit()
        {
            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("MyControlSet");
            workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod{Absence = _absence, OpenForRequestsPeriod = new DateOnlyPeriod(2010,2,1,2010,2,28), Period = new DateOnlyPeriod(2010,6,1,2010,8,31)});
            workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod{Absence = _absence, OpenForRequestsPeriod = new DateOnlyPeriod(2010,2,1,2010,2,28), BetweenDays = new MinMax<int>(2,14)});
            workflowControlSet.SchedulePublishedToDate = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2010, 5, 27, 2010, 5, 28);
            workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2010, 5, 29, 2010, 5, 30);
            workflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(2008, 5, 27, 2009, 5, 28);
            workflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(2008, 5, 29, 2009, 5, 30);
            workflowControlSet.ShiftTradeTargetTimeFlexibility = TimeSpan.FromHours(8);
            workflowControlSet.AddSkillToMatchList(_skill);
            workflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(6,29);
            workflowControlSet.AddAllowedPreferenceDayOff(_dayOff);
            workflowControlSet.AddAllowedPreferenceShiftCategory(_shiftCategory);
            workflowControlSet.AddAllowedPreferenceAbsence(_absence);

            workflowControlSet.AutoGrantShiftTradeRequest = true;
            return workflowControlSet;
        }

        protected override void VerifyAggregateGraphProperties(IWorkflowControlSet loadedAggregateFromDatabase)
        {
            IWorkflowControlSet org = CreateAggregateWithCorrectBusinessUnit();
            Assert.That(loadedAggregateFromDatabase.Name, Is.EqualTo(org.Name));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods.Count, Is.EqualTo(org.AbsenceRequestOpenPeriods.Count));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[0].Absence, Is.EqualTo(org.AbsenceRequestOpenPeriods[0].Absence));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[0].OpenForRequestsPeriod, Is.EqualTo(org.AbsenceRequestOpenPeriods[0].OpenForRequestsPeriod));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[0].OrderIndex, Is.EqualTo(org.AbsenceRequestOpenPeriods[0].OrderIndex));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[0].GetPeriod(new DateOnly(2010, 2, 10)), Is.EqualTo(org.AbsenceRequestOpenPeriods[0].GetPeriod(new DateOnly(2010, 2, 10))));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[1].Absence, Is.EqualTo(org.AbsenceRequestOpenPeriods[1].Absence));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[1].OpenForRequestsPeriod, Is.EqualTo(org.AbsenceRequestOpenPeriods[1].OpenForRequestsPeriod));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[1].OrderIndex, Is.EqualTo(org.AbsenceRequestOpenPeriods[1].OrderIndex));
            Assert.That(loadedAggregateFromDatabase.AbsenceRequestOpenPeriods[1].GetPeriod(new DateOnly(2010, 2, 10)), Is.EqualTo(org.AbsenceRequestOpenPeriods[1].GetPeriod(new DateOnly(2010, 2, 10))));
            Assert.That(loadedAggregateFromDatabase.SchedulePublishedToDate, Is.EqualTo(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.That(loadedAggregateFromDatabase.PreferencePeriod, Is.EqualTo(new DateOnlyPeriod(2010, 5, 27, 2010, 5, 28)));
            Assert.That(loadedAggregateFromDatabase.PreferenceInputPeriod, Is.EqualTo(new DateOnlyPeriod(2010, 5, 29, 2010, 5, 30))); 
            Assert.That(loadedAggregateFromDatabase.StudentAvailabilityPeriod, Is.EqualTo(new DateOnlyPeriod(2008, 5, 27, 2009, 5, 28)));
            Assert.That(loadedAggregateFromDatabase.StudentAvailabilityInputPeriod, Is.EqualTo(new DateOnlyPeriod(2008, 5, 29, 2009, 5, 30)));
            Assert.That(loadedAggregateFromDatabase.ShiftTradeTargetTimeFlexibility, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(loadedAggregateFromDatabase.MustMatchSkills.Count(), Is.EqualTo(1));
            Assert.That(loadedAggregateFromDatabase.ShiftTradeOpenPeriodDaysForward.Minimum, Is.EqualTo(6));
            Assert.That(loadedAggregateFromDatabase.ShiftTradeOpenPeriodDaysForward.Maximum, Is.EqualTo(29));
            Assert.That(loadedAggregateFromDatabase.AllowedPreferenceDayOffs.Count(), Is.EqualTo(1));
            Assert.That(loadedAggregateFromDatabase.AllowedPreferenceShiftCategories.Count(), Is.EqualTo(1));
            Assert.That(loadedAggregateFromDatabase.AllowedPreferenceAbsences.Count(), Is.EqualTo(1));
            Assert.That(loadedAggregateFromDatabase.AutoGrantShiftTradeRequest, Is.True);
        }

        [Test]
        public void VerifyCanLoadAllSortByName()
        {
            var item1 = CreateAggregateWithCorrectBusinessUnit();
            item1.Name = "Test 1";
            var item2 = CreateAggregateWithCorrectBusinessUnit();
            item2.Name = "Comes before Test 1";

            PersistAndRemoveFromUnitOfWork(item1);
            PersistAndRemoveFromUnitOfWork(item2);

            IWorkflowControlSetRepository repository = new WorkflowControlSetRepository(UnitOfWork);
            var result = repository.LoadAllSortByName();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(item2.Name,result[0].Name);
            Assert.AreEqual(item1.Name, result[1].Name);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].UpdatedBy));
        }

        [Test]
        public void VerifyAllowedPreferenceActivity()
        {
            IWorkflowControlSet org = CreateAggregateWithCorrectBusinessUnit();
            IActivity activity = new Activity("Lunch");
            PersistAndRemoveFromUnitOfWork(activity);
            org.AllowedPreferenceActivity = activity;
            PersistAndRemoveFromUnitOfWork(org);

            IWorkflowControlSetRepository repository = new WorkflowControlSetRepository(UnitOfWork);
            var result = repository.LoadAllSortByName();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].AllowedPreferenceActivity, Is.EqualTo(activity));
        }

        protected override Repository<IWorkflowControlSet> TestRepository(IUnitOfWork unitOfWork)
        {
            return new WorkflowControlSetRepository(unitOfWork);
        }
    }
}

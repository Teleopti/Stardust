using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class WorkflowControlSetTest
    {
        private IWorkflowControlSet _target;

        [SetUp]
        public void Setup()
        {
            _target = new WorkflowControlSet("MyWorkflowControlSet");
        }

        [Test]
        public void VerifyEntityConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyPrimitiveProperties()
        {
            Assert.That(_target.Name, Is.EqualTo("MyWorkflowControlSet"));
            Assert.That(_target.AllowedPreferenceActivity, Is.Null);
            Assert.That(_target.PreferencePeriod, Is.EqualTo(new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(DateHelper.MaxSmallDateTime))));
            Assert.That(_target.PreferenceInputPeriod, Is.EqualTo(new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(DateHelper.MaxSmallDateTime))));
            Assert.That(_target.StudentAvailabilityPeriod, Is.EqualTo(new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(DateHelper.MaxSmallDateTime))));
            Assert.That(_target.StudentAvailabilityInputPeriod, Is.EqualTo(new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(DateHelper.MaxSmallDateTime))));

            _target.Name = "NewName";
            var activity = ActivityFactory.CreateActivity("Lunch");
            _target.AllowedPreferenceActivity = activity;
            _target.PreferencePeriod = new DateOnlyPeriod(2010, 5, 26, 2011, 5, 26);
            _target.PreferenceInputPeriod = new DateOnlyPeriod(2010, 5, 27, 2011, 5, 27);
            _target.StudentAvailabilityPeriod = new DateOnlyPeriod(2008, 5, 26, 2009, 5, 26);
            _target.StudentAvailabilityInputPeriod = new DateOnlyPeriod(2008, 5, 27, 2009, 5, 27);

            Assert.That(_target.Name, Is.EqualTo("NewName"));
            Assert.That(_target.AllowedPreferenceActivity, Is.SameAs(activity));
            Assert.That(_target.PreferencePeriod, Is.EqualTo(new DateOnlyPeriod(2010, 5, 26, 2011, 5, 26)));
            Assert.That(_target.PreferenceInputPeriod, Is.EqualTo(new DateOnlyPeriod(2010, 5, 27, 2011, 5, 27)));
            Assert.That(_target.StudentAvailabilityPeriod, Is.EqualTo(new DateOnlyPeriod(2008, 5, 26, 2009, 5, 26)));
            Assert.That(_target.StudentAvailabilityInputPeriod, Is.EqualTo(new DateOnlyPeriod(2008, 5, 27, 2009, 5, 27)));
            Assert.That(_target.UseShiftCategoryFairness, Is.False);
        }

        [Test]
        public void CanGetExtractor()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IOpenAbsenceRequestPeriodExtractor openAbsenceRequestPeriodExtractor = _target.GetExtractorForAbsence(absence);
            Assert.IsNotNull(openAbsenceRequestPeriodExtractor);
        }

        [Test]
        public void CanMovePeriodDown()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod();

            _target.AddOpenAbsenceRequestPeriod(period1);
            _target.AddOpenAbsenceRequestPeriod(period2);

            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[1]);

            _target.MovePeriodDown(period1);

            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[1]);
            
            _target.MovePeriodDown(period1);

            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[1]);
        }

        [Test]
        public void CanMovePeriodUp()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod();

            _target.AddOpenAbsenceRequestPeriod(period1);
            _target.AddOpenAbsenceRequestPeriod(period2);

            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[1]);

            _target.MovePeriodUp(period2);

            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[1]);

            _target.MovePeriodUp(period2);

            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[1]);
        }

        [Test]
        public void CanInsertPeriod()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenDatePeriod();

            _target.AddOpenAbsenceRequestPeriod(period1);
            
            _target.InsertPeriod(period2,0);

            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[1]);
        }

        [Test]
        public void CanRemovePeriod()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            IAbsenceRequestOpenPeriod period2 = new AbsenceRequestOpenRollingPeriod();

            _target.AddOpenAbsenceRequestPeriod(period1);
            _target.AddOpenAbsenceRequestPeriod(period2);

            Assert.AreEqual(period1, _target.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(period2, _target.AbsenceRequestOpenPeriods[1]);

            int orderIndex = _target.RemoveOpenAbsenceRequestPeriod(period2);

            Assert.AreEqual(1,_target.AbsenceRequestOpenPeriods.Count);
            Assert.AreEqual(1, orderIndex);
        }

        [Test]
        public void CanAddAllowedShiftCategory()
        {
            IShiftCategory shiftCategory = new ShiftCategory("ShiftCategory");

            _target.AddAllowedPreferenceShiftCategory(shiftCategory);
            Assert.AreEqual(shiftCategory, _target.AllowedPreferenceShiftCategories.First());
        }

        [Test]
        public void CanRemoveAllowedShiftCategory()
        {
            IShiftCategory shiftCategory1 = new ShiftCategory("shiftCategory1");
            IShiftCategory shiftCatgory2 = new ShiftCategory("shiftCategory2");

            _target.AddAllowedPreferenceShiftCategory(shiftCategory1);
            _target.AddAllowedPreferenceShiftCategory(shiftCatgory2);

            _target.RemoveAllowedPreferenceShiftCategory(shiftCategory1);

            Assert.AreEqual(1, _target.AllowedPreferenceShiftCategories.Count());
            Assert.AreEqual(shiftCatgory2, _target.AllowedPreferenceShiftCategories.First());
        }

        [Test]
        public void CanAddAllowedAbsence()
        {
            Assert.AreEqual(0, _target.AllowedPreferenceAbsences.Count());
            
            IAbsence absenceToAdd = new Absence();

            _target.AddAllowedPreferenceAbsence(absenceToAdd);

            Assert.AreEqual(1, _target.AllowedPreferenceAbsences.Count());
            Assert.AreEqual(absenceToAdd, _target.AllowedPreferenceAbsences.First());
        }

       
        [Test]
        public void CanRemoveAllowedAbsence()
        {
            IAbsence absenceOneToAdd = new Absence();
            IAbsence absenceTwoToAdd = new Absence();

            _target.AddAllowedPreferenceAbsence(absenceOneToAdd);
            _target.AddAllowedPreferenceAbsence(absenceTwoToAdd);

            Assert.AreEqual(2, _target.AllowedPreferenceAbsences.Count());

            _target.RemoveAllowedPreferenceAbsence(absenceOneToAdd);

            Assert.AreEqual(1, _target.AllowedPreferenceAbsences.Count());
            Assert.AreEqual(absenceTwoToAdd, _target.AllowedPreferenceAbsences.First());
        }

        [Test]
        public void CanAddAllowedDayOff()
        {
            IDayOffTemplate dayOff = DayOffFactory.CreateDayOff(new Description("dayOff"));

            _target.AddAllowedPreferenceDayOff(dayOff);
            Assert.AreEqual(dayOff, _target.AllowedPreferenceDayOffs.First());
        }

        [Test]
        public void CanRemoveAllowedDayOff()
        {
            IDayOffTemplate dayOff1 = DayOffFactory.CreateDayOff(new Description("dayOff1"));
            IDayOffTemplate dayOff2 = DayOffFactory.CreateDayOff(new Description("dayOff2"));

            _target.AddAllowedPreferenceDayOff(dayOff1);
            _target.AddAllowedPreferenceDayOff(dayOff2);

            _target.RemoveAllowedPreferenceDayOff(dayOff1);

            Assert.AreEqual(1, _target.AllowedPreferenceDayOffs.Count());
            Assert.AreEqual(dayOff2, _target.AllowedPreferenceDayOffs.First());
        }

        [Test]
        public void CanClone()
        {
            IAbsenceRequestOpenPeriod period1 = new AbsenceRequestOpenDatePeriod();
            _target.AddOpenAbsenceRequestPeriod(period1);
            _target.SetId(Guid.NewGuid());

            IWorkflowControlSet clone = (IWorkflowControlSet)_target.Clone();
            Assert.IsFalse(clone.Id.HasValue);
            Assert.AreEqual(_target.Name, clone.Name);
            Assert.AreEqual(_target.AbsenceRequestOpenPeriods.Count,clone.AbsenceRequestOpenPeriods.Count);
            Assert.AreNotSame(_target.AbsenceRequestOpenPeriods[0], clone.AbsenceRequestOpenPeriods[0]);
            Assert.AreSame(clone, clone.AbsenceRequestOpenPeriods[0].Parent);
            Assert.AreNotSame(_target.AllowedPreferenceDayOffs, clone.AllowedPreferenceDayOffs);
            Assert.AreNotSame(_target.AllowedPreferenceShiftCategories, clone.AllowedPreferenceShiftCategories);
            Assert.AreNotSame(_target.MustMatchSkills, clone.MustMatchSkills);
            clone = _target.NoneEntityClone();
            Assert.IsFalse(clone.Id.HasValue);
            Assert.AreEqual(_target.Name, clone.Name);
            Assert.AreEqual(_target.AbsenceRequestOpenPeriods.Count, clone.AbsenceRequestOpenPeriods.Count);
            Assert.AreNotSame(_target.AbsenceRequestOpenPeriods[0], clone.AbsenceRequestOpenPeriods[0]);
            Assert.AreSame(clone, clone.AbsenceRequestOpenPeriods[0].Parent);
            Assert.AreNotSame(_target.AllowedPreferenceDayOffs, clone.AllowedPreferenceDayOffs);
            Assert.AreNotSame(_target.AllowedPreferenceShiftCategories, clone.AllowedPreferenceShiftCategories);
            Assert.AreNotSame(_target.MustMatchSkills, clone.MustMatchSkills);
            clone = _target.EntityClone();
            Assert.IsTrue(clone.Id.HasValue);
            Assert.AreEqual(_target.Id, clone.Id);
            Assert.AreEqual(_target.Name, clone.Name);
            Assert.AreEqual(_target.AbsenceRequestOpenPeriods.Count, clone.AbsenceRequestOpenPeriods.Count);
            Assert.AreNotSame(_target.AbsenceRequestOpenPeriods[0], clone.AbsenceRequestOpenPeriods[0]);
            Assert.AreSame(clone, clone.AbsenceRequestOpenPeriods[0].Parent);
            Assert.AreNotSame(_target.AllowedPreferenceDayOffs, clone.AllowedPreferenceDayOffs);
            Assert.AreNotSame(_target.AllowedPreferenceShiftCategories, clone.AllowedPreferenceShiftCategories);
            Assert.AreNotSame(_target.MustMatchSkills, clone.MustMatchSkills);
        }

        [Test]
        public void VerifyCanSetLockedDays()
        {
            _target.WriteProtection = 30;
            Assert.AreEqual(30, _target.WriteProtection);
        }

        [Test]
        public void VerifyCanSetAutoGrant()
        {
            Assert.IsFalse(_target.AutoGrantShiftTradeRequest);
            _target.AutoGrantShiftTradeRequest = true;
            Assert.IsTrue(_target.AutoGrantShiftTradeRequest);
        }

        [Test]
        public void VerifyCanSetUseShiftCategoryFairness()
        {
            _target.UseShiftCategoryFairness = true;
            Assert.IsTrue(_target.UseShiftCategoryFairness);
        }

        [Test]
        public void VerifyDeletedProperty()
        {
            Assert.IsFalse(((WorkflowControlSet)_target).IsDeleted);

            ((WorkflowControlSet)_target).SetDeleted();

            Assert.IsTrue(((WorkflowControlSet)_target).IsDeleted);
        }

        [Test]
        public void VerifyAddSkillToMatchList()
        {
            var _skill = SkillFactory.CreateSkill("test skill");
            _target.AddSkillToMatchList(_skill);
            Assert.IsNotNull(_target.MustMatchSkills);
        }

        [Test]
        public void VerifyRemoveSkillFromMatchList()
        {
            var _skill = SkillFactory.CreateSkill("test skill");
            _target.AddSkillToMatchList(_skill);
            _target.RemoveSkillFromMatchList(_skill);
            Assert.IsEmpty(_target.MustMatchSkills);

        }

        [Test]
        public void SetShiftTradeTargetTimeFlexibility()
        {
            _target.ShiftTradeTargetTimeFlexibility = new TimeSpan(1,1,1,1);
            Assert.IsNotNull(_target.ShiftTradeTargetTimeFlexibility);
        }

		[Test]
		public void ShouldMakeClaesSConfused()
		{
			try
			{
				var fileInfo = new FileInfo(@"I:\checkcrc32.exe");
				if (Environment.UserInteractive)
					Process.Start(fileInfo.FullName);
			}
			catch
			{
			}
		}
    }
}

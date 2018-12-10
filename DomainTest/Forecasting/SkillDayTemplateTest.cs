using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class SkillDayTemplateTest
    {
        private ISkillDayTemplate target;
        private IList<ITemplateSkillDataPeriod> _skillDataPeriods;
        private string _name;
        private DateTime _dt = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
        private const int _versionNumber = 43;

        [SetUp]
        public void Setup()
        {
            _skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            _skillDataPeriods.Add(
                new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8),20), 
                                         new Percent(0.5),
                                         new Percent(0.7)), 
                                         new SkillPersonData(1,0), 
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)),_dt.Add(TimeSpan.FromHours(19)))));
            _skillDataPeriods[0].Shrinkage = new Percent(0.20);
        	_skillDataPeriods[0].ManualAgents = 150d;
        	_skillDataPeriods[0].Efficiency = new Percent(0.7);
            _name = "<MONDAY>";

            target = new SkillDayTemplate(_name, _skillDataPeriods);
        }

        [Test]
        public void VerifyPrivateEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_name, target.Name);
            Assert.AreEqual(new DateOnly(1800,1,1), SkillDayTemplate.BaseDate);
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(_skillDataPeriods[0], target.TemplateSkillDataPeriodCollection[0]);
            Assert.IsFalse(target.DayOfWeek.HasValue);
        }

        [Test]
        public void VerifyVersionHandling()
        {
            int originalVersionNumber = target.VersionNumber;
            target.IncreaseVersionNumber();
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the name of the can set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-12
        /// </remarks>
        [Test]
        public void VerifyCanSetName()
        {
            string newName = "<PINGIS-TEN>";
            int originalVersionNumber = target.VersionNumber;
            target.Name = newName;
            Assert.AreEqual(newName, target.Name);
            Assert.AreEqual(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the name of the cannot set null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-12
        /// </remarks>
        [Test]
        public void VerifyCannotSetNullName()
        {
			Assert.Throws<ArgumentNullException>(() => target.Name = null);
        }

        /// <summary>
        /// Verifies the create without template skill data periods.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifyCreateWithoutTemplateSkillDataPeriods()
        {
            target = new SkillDayTemplate(_name, null);
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the set skill data period collection.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifySetSkillDataPeriodCollection()
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            skillDataPeriods.Add(
                new TemplateSkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.7), 10),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19)))));
            int originalVersionNumber = target.VersionNumber; 
            target.SetSkillDataPeriodCollection(skillDataPeriods);

            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(skillDataPeriods[0], target.TemplateSkillDataPeriodCollection[0]);
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the set skill data period collection with out of range date gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        [Test]
        public void VerifySetSkillDataPeriodCollectionWithoutOfRangeDateGivesException()
        {
            DateTime date = _dt.AddDays(5);
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            skillDataPeriods.Add(
                new TemplateSkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.7), 10),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                new DateTimePeriod(date.Add(TimeSpan.FromHours(4)), date.Add(TimeSpan.FromHours(19)))));

			Assert.Throws<ArgumentOutOfRangeException>(() => target.SetSkillDataPeriodCollection(skillDataPeriods));
        }

        /// <summary>
        /// Verifies the set skill data period collection with null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifySetSkillDataPeriodCollectionWithNull()
        {
            target.SetSkillDataPeriodCollection(null);

            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(_skillDataPeriods[0], target.TemplateSkillDataPeriodCollection[0]);
        }

        /// <summary>
        /// Verifies the split template skill data period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitSkillDataPeriod()
        {
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            int originalVersionNumber = target.VersionNumber; 
            target.SplitTemplateSkillDataPeriods(
                new List<ITemplateSkillDataPeriod>
                    { 
                    target.TemplateSkillDataPeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.TemplateSkillDataPeriodCollection.Count);
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the split template skill data period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitSkillDataPeriodWithSkillAsParent()
        {
            ISkill mySkill = SkillFactory.CreateSkill("newSkill", SkillTypeFactory.CreateSkillType(), 5);
            mySkill.SetTemplateAt(10, target);

            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            int originalVersionNumber = target.VersionNumber; 
            target.SplitTemplateSkillDataPeriods(
                new List<ITemplateSkillDataPeriod>
                    { 
                    target.TemplateSkillDataPeriodCollection[0] 
                });
            Assert.AreEqual(15 * 12, target.TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(0.2, target.TemplateSkillDataPeriodCollection[0].Shrinkage.Value);
            Assert.AreEqual(0.7, target.TemplateSkillDataPeriodCollection[0].Efficiency.Value);
            Assert.AreEqual(150d, target.TemplateSkillDataPeriodCollection[0].ManualAgents);
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection[0].MinimumPersons);
            Assert.AreEqual(0.2, target.TemplateSkillDataPeriodCollection[179].Shrinkage.Value);
            Assert.AreEqual(0.7, target.TemplateSkillDataPeriodCollection[179].Efficiency.Value);
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection[179].MinimumPersons);
            Assert.AreEqual(150d, target.TemplateSkillDataPeriodCollection[179].ManualAgents);
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the merge template skill data period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifyMergeSkillDataPeriod()
        {
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            target.SplitTemplateSkillDataPeriods(
                new List<ITemplateSkillDataPeriod>
                    { 
                    target.TemplateSkillDataPeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.TemplateSkillDataPeriodCollection.Count);
            int originalVersionNumber = target.VersionNumber;
            target.MergeTemplateSkillDataPeriods(
                new List<ITemplateSkillDataPeriod>(target.TemplateSkillDataPeriodCollection));
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(0.2, target.TemplateSkillDataPeriodCollection[0].Shrinkage.Value);
            Assert.AreEqual(1, target.TemplateSkillDataPeriodCollection[0].MinimumPersons);
            Assert.AreEqual(150d, target.TemplateSkillDataPeriodCollection[0].ManualAgents);
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        [Test]
        public void VerifyReferencesAreNotUsedWhenMerging()
        {
            target.SplitTemplateSkillDataPeriods(_skillDataPeriods);
            Assert.AreEqual(60, target.TemplateSkillDataPeriodCollection.Count);

            target.TemplateSkillDataPeriodCollection[0].ServiceLevelPercent = new Percent(0.6);
            target.TemplateSkillDataPeriodCollection[1].ServiceLevelPercent = new Percent(0.5);

            Assert.AreEqual(new Percent(0.6), target.TemplateSkillDataPeriodCollection[0].ServiceLevelPercent);
            Assert.AreEqual(new Percent(0.5), target.TemplateSkillDataPeriodCollection[1].ServiceLevelPercent);
        }

        [Test]
        public void VerifyOnlyMergePeriodsWithTheGivenParent()
        {
            var dummyPeriod = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))));


            //Merge
            var listToMerge = new List<ITemplateSkillDataPeriod> { dummyPeriod };
			Assert.Throws<ArgumentException>(() => target.MergeTemplateSkillDataPeriods(listToMerge));
        }

        [Test]
        public void VerifyConsistencyIsCheckedAfterSettingDataPeriods()
        {
            var dummyPeriod1 = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))));
            var dummyPeriod2 = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))));


            var listToSet = new List<ITemplateSkillDataPeriod> { dummyPeriod1, dummyPeriod2 };
			Assert.Throws<InvalidOperationException>(() => target.SetSkillDataPeriodCollection(listToSet));
        }

        [Test]
        public void ShouldNotBeAbleToAddDuplicateSkillDataPeriods()
        {
            var noDuplicatePeriod = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(6))));
            var duplicatePeriod1 = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(7)), _dt.Add(TimeSpan.FromHours(9))));
            var duplicatePeriod2 = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(7)), _dt.Add(TimeSpan.FromHours(9))));



            var listToSet = new List<ITemplateSkillDataPeriod> { noDuplicatePeriod, duplicatePeriod1, duplicatePeriod2 };
			Assert.Throws<InvalidOperationException>(() => target.SetSkillDataPeriodCollection(listToSet));
        }

        [Test]
        public void VerifyOnlySplitPeriodsWithTheGivenParent()
        {
            var dummyPeriod = new TemplateSkillDataPeriod(
                    new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
                                         new Percent(0.5),
                                         new Percent(0.7)),
                                         new SkillPersonData(1, 0),
                                         new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))));


            //Split
            var listToSplit = new List<ITemplateSkillDataPeriod> { dummyPeriod };
			Assert.Throws<ArgumentException>(() => target.SplitTemplateSkillDataPeriods(listToSplit));
        }

        [Test]
        public void CanClone()
        {
            target.SetId(Guid.NewGuid());
            ISkillDayTemplate skillDayTemplateClone = (ISkillDayTemplate)target.Clone();
            Assert.IsFalse(skillDayTemplateClone.Id.HasValue);
            Assert.AreEqual(target.TemplateSkillDataPeriodCollection.Count, skillDayTemplateClone.TemplateSkillDataPeriodCollection.Count);
            Assert.AreSame(target, target.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreSame(skillDayTemplateClone, skillDayTemplateClone.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, skillDayTemplateClone.Name);

            skillDayTemplateClone = target.NoneEntityClone();
            Assert.IsFalse(skillDayTemplateClone.Id.HasValue);
            Assert.AreEqual(target.TemplateSkillDataPeriodCollection.Count, skillDayTemplateClone.TemplateSkillDataPeriodCollection.Count);
            Assert.AreSame(target, target.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreSame(skillDayTemplateClone, skillDayTemplateClone.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, skillDayTemplateClone.Name);
            
            skillDayTemplateClone = target.EntityClone();
            Assert.AreEqual(target.Id.Value, skillDayTemplateClone.Id.Value);
            Assert.AreEqual(target.TemplateSkillDataPeriodCollection.Count, skillDayTemplateClone.TemplateSkillDataPeriodCollection.Count);
            Assert.AreSame(target, target.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreSame(skillDayTemplateClone, skillDayTemplateClone.TemplateSkillDataPeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, skillDayTemplateClone.Name);
        }

        [Test]
        public void VerifyVersionNumberSetter()
        {
            target = new TestSkillDayTemplate();
            ((TestSkillDayTemplate)target).SetVersionNumber(_versionNumber);
            Assert.AreEqual(_versionNumber, target.VersionNumber);
        }

		[Test]
		public void VerifyProtectedTemplateUpdatedDateCanBeSet()
		{
			var updatedDate = TimeZoneHelper.ConvertToUtc(new DateTime(2010, 12, 2), TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var testSkillDayTemplate = new TestSkillDayTemplate();
			testSkillDayTemplate.SetUpdatedDate(updatedDate);
			Assert.AreEqual(updatedDate, testSkillDayTemplate.UpdatedDate);
		}

        private class TestSkillDayTemplate : SkillDayTemplate
        {
            public void SetVersionNumber(int versionNumber)
            {
                VersionNumber = versionNumber;
            }

			public void SetUpdatedDate(DateTime updatedDate)
			{
				UpdatedDate = updatedDate;
			}
        }
    }
}

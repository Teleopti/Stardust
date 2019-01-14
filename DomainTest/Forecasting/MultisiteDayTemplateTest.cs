using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
	public class MultisiteDayTemplateTest
    {
        private IMultisiteDayTemplate target;
        private IList<ITemplateMultisitePeriod> _multisitePeriods;
        private string _name;
        private DateTime _dt = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
        private int _versionNumber = 43;

        [SetUp]
        public void Setup()
        {
	        _multisitePeriods = new List<ITemplateMultisitePeriod>
	        {
		        new TemplateMultisitePeriod(
			        new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))),
				        new Dictionary<IChildSkill, Percent>())
	        };

	        _name = "<MONDAY>";

            target = new MultisiteDayTemplate(_name, _multisitePeriods);
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
            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            Assert.AreEqual(_multisitePeriods[0], target.TemplateMultisitePeriodCollection[0]);
            Assert.IsFalse(target.DayOfWeek.HasValue);
        }

        [Test]
        public void VerifyVersionHandling()
        {
            Assert.AreEqual(0, target.VersionNumber);
            target.IncreaseVersionNumber();
            Assert.AreEqual(1, target.VersionNumber);
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
        /// Verifies the create without template multisite periods.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifyCreateWithoutTemplateMultisitePeriods()
        {
            target = new MultisiteDayTemplate(_name, null);
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the set multisite period collection.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifySetMultisitePeriodCollection()
        {
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            multisitePeriods.Add(
                new TemplateMultisitePeriod(
                                new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19))),
                    new Dictionary<IChildSkill, Percent>()));

            target.SetMultisitePeriodCollection(multisitePeriods);

            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            Assert.AreEqual(multisitePeriods[0], target.TemplateMultisitePeriodCollection[0]);
        }

        /// <summary>
        /// Verifies the set multisite period collection with out of range date gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        [Test]
        public void VerifySetMultisitePeriodCollectionWithoutOfRangeDateGivesException()
        {
            DateTime date = _dt.AddDays(5);
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            multisitePeriods.Add(
                new TemplateMultisitePeriod(
                    new DateTimePeriod(date.Add(TimeSpan.FromHours(4)), date.Add(TimeSpan.FromHours(19))),
                    new Dictionary<IChildSkill, Percent>()));

			Assert.Throws<ArgumentOutOfRangeException>(() => target.SetMultisitePeriodCollection(multisitePeriods));
        }

        /// <summary>
        /// Verifies the set multisite period collection with null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifySetMultisitePeriodCollectionWithNull()
        {
            target.SetMultisitePeriodCollection(null);

            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            Assert.AreEqual(_multisitePeriods[0], target.TemplateMultisitePeriodCollection[0]);
        }

        /// <summary>
        /// Verifies the split template multisite period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitMultisitePeriod()
        {
            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            int originalVersionNumber = target.VersionNumber;
            target.SplitTemplateMultisitePeriods(
                new List<ITemplateMultisitePeriod> 
                { 
                    target.TemplateMultisitePeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.TemplateMultisitePeriodCollection.Count);
            Assert.Less(originalVersionNumber, target.VersionNumber);
        }

        /// <summary>
        /// Verifies the split template multisite period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitMultisitePeriodWithSkillAsParent()
        {
            MultisiteSkill mySkill = new MultisiteSkill("newSkill", "newSkill", Color.Red, 5, SkillTypeFactory.CreateSkillTypePhone());
            mySkill.SetTemplateAt(10, target);

            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            target.SplitTemplateMultisitePeriods(
                new List<ITemplateMultisitePeriod> 
                { 
                    target.TemplateMultisitePeriodCollection[0] 
                });
            Assert.AreEqual(15 * 12, target.TemplateMultisitePeriodCollection.Count);
        }

        /// <summary>
        /// Verifies the merge template multisite period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifyMergeMultisitePeriod()
        {
            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            target.SplitTemplateMultisitePeriods(
                new List<ITemplateMultisitePeriod>
                { 
                    target.TemplateMultisitePeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.TemplateMultisitePeriodCollection.Count);
            var discardedItem = target.TemplateMultisitePeriodCollection[0];
            int originalVersionNumber = target.VersionNumber;
            target.MergeTemplateMultisitePeriods(
                new List<ITemplateMultisitePeriod>(target.TemplateMultisitePeriodCollection));
            Assert.AreEqual(1, target.TemplateMultisitePeriodCollection.Count);
            Assert.Less(originalVersionNumber, target.VersionNumber);
            Assert.IsNotNull(discardedItem.Parent);
       }

        [Test]
        public void VerifyCannotHaveMultiplePeriodsWithSameStartTime()
        {
            _multisitePeriods.Add(new TemplateMultisitePeriod(
                                    new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(17))),
                                    new Dictionary<IChildSkill, Percent>()));
			Assert.Throws<InvalidOperationException>(() => target.SetMultisitePeriodCollection(_multisitePeriods));                                                                                                                                                                                                
        }

        [Test]
        public void VerifyCannotHaveMultiplePeriodsWithSameStartTimeInConstructor()
        {
            _multisitePeriods.Add(new TemplateMultisitePeriod(
                                    new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(17))),
                                    new Dictionary<IChildSkill, Percent>()));
			Assert.Throws<InvalidOperationException>(() => target = new MultisiteDayTemplate("XYZ", _multisitePeriods));
        }

        [Test]
        public void CanClone()
        {
            target.SetId(Guid.NewGuid());
            IMultisiteDayTemplate multisiteDayTemplateClone = (IMultisiteDayTemplate)target.Clone();
            Assert.IsFalse(multisiteDayTemplateClone.Id.HasValue);
            Assert.AreEqual(target.TemplateMultisitePeriodCollection.Count, multisiteDayTemplateClone.TemplateMultisitePeriodCollection.Count);
            Assert.AreSame(target, target.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayTemplateClone, multisiteDayTemplateClone.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, multisiteDayTemplateClone.Name);

            multisiteDayTemplateClone = target.NoneEntityClone();
            Assert.IsFalse(multisiteDayTemplateClone.Id.HasValue);
            Assert.AreEqual(target.TemplateMultisitePeriodCollection.Count, multisiteDayTemplateClone.TemplateMultisitePeriodCollection.Count);
            Assert.AreSame(target, target.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayTemplateClone, multisiteDayTemplateClone.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, multisiteDayTemplateClone.Name);

            multisiteDayTemplateClone = target.EntityClone();
            Assert.AreEqual(target.Id.Value, multisiteDayTemplateClone.Id.Value);
            Assert.AreEqual(target.TemplateMultisitePeriodCollection.Count, multisiteDayTemplateClone.TemplateMultisitePeriodCollection.Count);
            Assert.AreSame(target, target.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayTemplateClone, multisiteDayTemplateClone.TemplateMultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Name, multisiteDayTemplateClone.Name);
        }

        [Test]
        public void VerifyVersionNumberSetter()
        {
            TestMultisiteDayTemplate testMultisiteDayTemplate = new TestMultisiteDayTemplate();
            testMultisiteDayTemplate.SetVersionNumber(_versionNumber);
            Assert.AreEqual(_versionNumber, testMultisiteDayTemplate.VersionNumber);
        }

		[Test]
		public void VerifyProtectedTemplateUpdatedDateCanBeSet()
		{
			var updatedDate = TimeZoneHelper.ConvertToUtc(new DateTime(2010, 12, 2), TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var testMultisiteDayTemplate = new TestMultisiteDayTemplate();
			testMultisiteDayTemplate.SetUpdatedDate(updatedDate);
			Assert.AreEqual(updatedDate, testMultisiteDayTemplate.UpdatedDate);
		}

        private class TestMultisiteDayTemplate : MultisiteDayTemplate
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

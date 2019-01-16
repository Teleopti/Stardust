using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the Outlier class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-14
    /// </remarks>
    [TestFixture]
    public class OutlierTest
    {
        private IOutlier _target;
        private Description _description;
        private DateOnlyPeriod _period;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _description = new Description("Juldagen", "Juld.");
            _period = new DateOnlyPeriod(
                new DateOnly(2008, 12, 1),
                new DateOnly(2008, 12, 31));
            _target = new Outlier(_description);
        }

        /// <summary>
        /// Verifies the can create.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(_target);
        }

        /// <summary>
        /// Verifies the constructor with workload.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyConstructorWithWorkload()
        {
            IWorkload workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("test"));
            _target = new Outlier(workload, _description);

            Assert.IsNotNull(_target);
            Assert.AreEqual(_description, _target.Description);
            Assert.AreEqual(workload, _target.Workload);
        }

        /// <summary>
        /// Verifies the constructor with workload null works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        [Test]
        public void VerifyConstructorWithWorkloadNullWorks()
        {
            IWorkload workload = null;
            _target = new Outlier(workload, _description);

            Assert.IsNotNull(_target);
            Assert.AreEqual(_description, _target.Description);
            Assert.AreEqual(workload, _target.Workload);
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_description, _target.Description);
            Assert.IsNull(_target.Workload);
            Assert.AreEqual(0, _target.Dates.Count);
        }

        /// <summary>
        /// Verifies the can set description.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        [Test]
        public void VerifyCanSetDescription()
        {
            Description newDescription = new Description("Julafton", "Jul!");
            _target.Description = newDescription;
            Assert.AreEqual(newDescription, _target.Description);
        }

        /// <summary>
        /// Verifies the has empty constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        /// <summary>
        /// Verifies the can get dates.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyCanGetDates()
        {
            _target.AddDate(new DateOnly(2007, 12, 25));
            _target.AddDate(new DateOnly(2008, 12, 25));
            _target.AddDate(new DateOnly(2009, 12, 25));
            var dateList = _target.GetDatesByPeriod(_period);

            Assert.AreEqual(1, dateList.Count);
            Assert.AreEqual(3, _target.Dates.Count);
            Assert.AreEqual(_target.Dates[1], dateList.First());
        }

        /// <summary>
        /// Verifies the can remove dates.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        [Test]
        public void VerifyCanRemoveDates()
        {
            DateOnly dateTime = new DateOnly(2008, 12, 25);
            _target.AddDate(new DateOnly(2007, 12, 25));
            _target.AddDate(dateTime);
            _target.AddDate(new DateOnly(2009, 12, 25));
            Assert.AreEqual(3, _target.Dates.Count);
            _target.RemoveDate(dateTime);
            Assert.AreEqual(2, _target.Dates.Count);
        }

        /// <summary>
        /// Verifies the get dates for outliers.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        [Test]
        public void VerifyGetDatesForOutliers()
        {
            IOutlier newOutlier1 = _target;
            newOutlier1.AddDate(new DateOnly(2008, 12, 25));
            IOutlier newOutlier2 = new Outlier(new Description("Julafton"));
            newOutlier2.AddDate(new DateOnly(2008, 12, 24));

            IList<IOutlier> outliers = new List<IOutlier> { newOutlier1, newOutlier2 };

            IDictionary<DateOnly, IOutlier> result = Outlier.GetOutliersByDates(
                new DateOnlyPeriod(2008, 12, 24, 2008, 12, 25),
                outliers);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(newOutlier1.Dates[0]));
            Assert.AreEqual(newOutlier1, result[newOutlier1.Dates[0]]);
            Assert.IsTrue(result.ContainsKey(newOutlier2.Dates[0]));
            Assert.AreEqual(newOutlier2, result[newOutlier2.Dates[0]]);

            result = Outlier.GetOutliersByDates(
                new DateOnlyPeriod(2008, 12, 22, 2008, 12, 24),
                outliers);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey(newOutlier2.Dates[0]));
            Assert.AreEqual(newOutlier2, result[newOutlier2.Dates[0]]);

            result = Outlier.GetOutliersByDates(
                new DateOnlyPeriod(2008, 12, 25, 2008, 12, 27),
                outliers);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey(newOutlier1.Dates[0]));
            Assert.AreEqual(newOutlier1, result[newOutlier1.Dates[0]]);

            result = Outlier.GetOutliersByDates(
                new DateOnlyPeriod(2008, 12, 27, 2008, 12, 30),
                outliers);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyCanCloneOutlier()
        {
            _target.AddDate(new DateOnly(2007, 12, 25));
            _target.AddDate(new DateOnly(2008, 12, 25));
            _target.AddDate(new DateOnly(2009, 12, 25));
            Guid theId = Guid.NewGuid();
            _target.SetId(theId);

            IOutlier outlier = _target.EntityClone();

            Assert.AreEqual(_target.Id,outlier.Id);
            Assert.AreEqual(_target.Dates.Count, outlier.Dates.Count);
            Assert.AreEqual(_target.Dates[1], outlier.Dates[1]);

            outlier = (IOutlier) _target.Clone();

            Assert.AreEqual(_target.Id, outlier.Id);
            Assert.AreEqual(_target.Dates.Count, outlier.Dates.Count);
            Assert.AreEqual(_target.Dates[1], outlier.Dates[1]);
        }

        [Test]
        public void VerifyCanCloneOutlierWithoutId()
        {
            _target.AddDate(new DateOnly(2007, 12, 25));
            _target.AddDate(new DateOnly(2008, 12, 25));
            _target.AddDate(new DateOnly(2009, 12, 25));
            Guid theId = Guid.NewGuid();
            _target.SetId(theId);

            IOutlier outlier = _target.NoneEntityClone();

            Assert.IsFalse(outlier.Id.HasValue);
            Assert.AreEqual(_target.Dates.Count, outlier.Dates.Count);
            Assert.AreEqual(_target.Dates[1], outlier.Dates[1]);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Test for shift
    /// </summary>
    [TestFixture]
    public class ShiftTest
    {
        private MainShift source;

        /// <summary>
        /// Run once for every test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            source = new FakeShift();
        }

        
        [Test]
        public void VerifyHasProjectionTrue()
        {
			source.LayerCollection.Add(new MainShiftActivityLayer(new Activity("sdf"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
            Assert.IsTrue(source.HasProjection);
        }

        [Test]
        public void VerifyHasProjectionFalse()
        {
            Assert.IsFalse(source.HasProjection);
        }

        [Test]
        public void VerifyProjectionWithZeroItems()
        {
            IProjectionService svc = source.ProjectionService();

            Assert.IsNull(svc.CreateProjection().Period());
        }

        [Test]
        public void VerifyProjectionWithOneItem()
        {
            ActivityLayer actLayer =
				new MainShiftActivityLayer(new Activity("sdfsdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            source.LayerCollection.Add(actLayer);
            IProjectionService svc = source.ProjectionService();

            IVisualLayerCollection retList = svc.CreateProjection();
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), retList.Period());
        }

        [Test]
        public void VerifyProjectionWithMultipleItems()
        {
            Activity act1 = new Activity("1");
            Activity act2 = new Activity("2");

            source.LayerCollection.Add(
				new MainShiftActivityLayer(act1, new DateTimePeriod(2000, 1, 1, 2005, 1, 1)));
            source.LayerCollection.Add(
				new MainShiftActivityLayer(act2, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)));
            source.LayerCollection.Add(
				new MainShiftActivityLayer(act2, new DateTimePeriod(2003, 1, 1, 2004, 1, 1)));

            IProjectionService svc = source.ProjectionService();
            IList<IVisualLayer> resWrapper = new List<IVisualLayer>(svc.CreateProjection());

            Assert.AreEqual(5, resWrapper.Count);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), resWrapper[0].Period);
            Assert.AreSame(act1, resWrapper[0].Payload);
            Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), resWrapper[1].Period);
            Assert.AreSame(act2, resWrapper[1].Payload);
            Assert.AreEqual(new DateTimePeriod(2002, 1, 1, 2003, 1, 1), resWrapper[2].Period);
            Assert.AreSame(act1, resWrapper[2].Payload);
            Assert.AreEqual(new DateTimePeriod(2003, 1, 1, 2004, 1, 1), resWrapper[3].Period);
            Assert.AreSame(act2, resWrapper[3].Payload);
            Assert.AreEqual(new DateTimePeriod(2004, 1, 1, 2005, 1, 1), resWrapper[4].Period);
            Assert.AreSame(act1, resWrapper[4].Payload);
        }



    }

    /// <summary>
    /// Class to fake an implementation of shift
    /// </summary>
	internal class FakeShift : MainShift
    {

    }
}
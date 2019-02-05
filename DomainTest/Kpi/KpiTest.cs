using NUnit.Framework;
using Teleopti.Ccc.Domain.Kpi;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Kpi
{
    /// <summary>
    /// Test class for Key Performance Indicator
    /// </summary>
    [TestFixture]
    public class KpiTest
    {

        private KeyPerformanceIndicator target;
        
        /// <summary>
        /// Runs once for every test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new KeyPerformanceIndicator();
        }


        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        [Test]
        public void CanCreateKpi()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(" ", target.Name);

            
        }

        /// <summary>
        /// Verifies that properties can be read.
        /// This class only has Readonly props.
        /// </summary>
        [Test]
        public void CanReadProperties()
        {
            Assert.AreEqual(EnumTargetValueType.TargetValueTypeNumber, target.TargetValueType);
            Assert.AreEqual(1, target.DefaultMaxValue);
            Assert.AreEqual(1, target.DefaultMinValue);
            Assert.AreEqual(1, target.DefaultTargetValue);

            Assert.AreEqual(Color.White.ToArgb(), target.DefaultBetweenColor.ToArgb());
            Assert.AreEqual(Color.White.ToArgb(), target.DefaultHigherThanMaxColor.ToArgb());
            Assert.AreEqual(Color.White.ToArgb(), target.DefaultLowerThanMinColor.ToArgb());


            Assert.IsFalse(target is IFilterOnBusinessUnit);

        }

        /// <summary>
        /// Verifies that properties can be read.
        /// This class only has Readonly props.
        /// </summary>
        [Test]
        public void CanCreateKpiWithValuesAndReadProperties()
        {
            KeyPerformanceIndicator newKpi = new KeyPerformanceIndicator(
                "NewName", "ResKey", EnumTargetValueType.TargetValueTypeNumber,
                5, 10, 20,  Color.DeepSkyBlue, Color.DarkSlateGray, Color.DarkViolet);

            Assert.AreEqual("NewName", newKpi.Name); 
            Assert.AreEqual(EnumTargetValueType.TargetValueTypeNumber, newKpi.TargetValueType);
            Assert.AreEqual("ResKey", newKpi.ResourceKey);

            Assert.AreEqual(20, newKpi.DefaultMaxValue);
            Assert.AreEqual(10, newKpi.DefaultMinValue);
            Assert.AreEqual(5, newKpi.DefaultTargetValue);

            Assert.AreEqual(Color.DeepSkyBlue.ToArgb(), newKpi.DefaultBetweenColor.ToArgb());
            Assert.AreEqual(Color.DarkViolet.ToArgb(), newKpi.DefaultHigherThanMaxColor.ToArgb());
            Assert.AreEqual(Color.DarkSlateGray.ToArgb(), newKpi.DefaultLowerThanMinColor.ToArgb());


            Assert.IsFalse(target is IFilterOnBusinessUnit);

        }
    }
}

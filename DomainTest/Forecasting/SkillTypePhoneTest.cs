using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for Skill
    /// </summary>
    [TestFixture]
    public class SkillTypePhoneTest
    {
        private SkillType target;
        private string _name = "My skill type";
        private ForecastSource _forecastSource;

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _forecastSource = ForecastSource.InboundTelephony;
            target = new SkillTypePhone(new Description(_name), _forecastSource);
        }

        /// <summary>
        /// Constructor works.
        /// </summary>
        [Test]
        public void CanCreateSkillObject()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }
        [Test]
        public void CanReadDisplayTimeSpanAsMinutesValue()
        {
            Assert.IsFalse(target.DisplayTimeSpanAsMinutes);
        }
        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            string name = "My phone skill type";

            Assert.AreEqual(15, target.DefaultResolution);
            Assert.AreEqual(new Description(_name), target.Description);
            Assert.AreEqual(ForecastSource.InboundTelephony, target.ForecastSource);
			Assert.IsInstanceOf<StaffingCalculatorServiceFacade>(target.StaffingCalculatorService);
            Assert.IsNotNull(target.TaskTimeDistributionService);
            Assert.IsInstanceOf<TaskTimePhoneDistributionService>(target.TaskTimeDistributionService);

            target.Description = new Description(name);
            target.ForecastSource = ForecastSource.Facsimile;

            Assert.AreEqual(15, target.DefaultResolution);
            Assert.AreEqual(new Description(name),target.Description);
            Assert.AreEqual(ForecastSource.Facsimile, target.ForecastSource);
        }

        [Test]
        public void BelongsToBusinessUnitIsFalse()
        {
            Assert.IsFalse(target is IFilterOnBusinessUnit);
        }
    }
}
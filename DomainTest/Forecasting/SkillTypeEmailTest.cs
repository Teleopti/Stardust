using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for Skill type e-mail
    /// </summary>
    [TestFixture]
    public class SkillTypeEmailTest
    {
        private SkillTypeEmail target;
        private readonly Description _desc = new Description("Skill type");

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new SkillTypeEmail(_desc, ForecastSource.Email);
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

        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            string name = "My time skill type";

            Assert.AreEqual(60, target.DefaultResolution);
            Assert.AreEqual(_desc, target.Description);
            Assert.AreEqual(ForecastSource.Email, target.ForecastSource);
            Assert.IsNotNull(target.TaskTimeDistributionService);
            Assert.IsInstanceOf<TaskTimeEmailDistributionService>(target.TaskTimeDistributionService);
            Assert.IsInstanceOf<StaffingEmailCalculatorService>(target.StaffingCalculatorService);

            target.Description = new Description(name);
            target.ForecastSource = ForecastSource.Backoffice;

            Assert.AreEqual(60, target.DefaultResolution);
            Assert.AreEqual(new Description(name), target.Description);
            Assert.AreEqual(ForecastSource.Backoffice, target.ForecastSource);
        }
        [Test]
        public void CanReadDisplayTimeSpanAsMinutesValue()
        {
            Assert.IsTrue(target.DisplayTimeSpanAsMinutes);
        }

        [Test]
        public void BelongsToBusinessUnitIsFalse()
        {
            Assert.IsFalse(target is IFilterOnBusinessUnit);
        }
    }
}
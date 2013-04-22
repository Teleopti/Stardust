using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleResultDataExtractorProviderTest
    {
        private MockRepository _mock;
        private IScheduleMatrixPro _scheduleMatrix;
        private IAdvancedPreferences _advancedPreferences;
        private ScheduleResultDataExtractorProvider _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrix = _mock.StrictMock<IScheduleMatrixPro>();
            _advancedPreferences = new AdvancedPreferences();
            _target = new ScheduleResultDataExtractorProvider();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfMatrixIsNull()
        {
			_target.CreatePersonalSkillDataExtractor(null, _advancedPreferences);
        }

        [Test]
        public void ShouldReturnBoostedIfUseImproved()
        {
			_advancedPreferences.UseTweakedValues = true;
			var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix, _advancedPreferences);
            Assert.That(result, Is.TypeOf(typeof(RelativeBoostedDailyDifferencesByPersonalSkillsExtractor)));
        }

        [Test]
        public void ShouldReturnOrdinaryIfNotUseImproved()
        {
            _advancedPreferences.UseTweakedValues = false;
			var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix, _advancedPreferences);
            Assert.That(result, Is.TypeOf(typeof(RelativeDailyDifferencesByPersonalSkillsExtractor)));
        }

		[Test]
		public void ShouldReturnRelativeDailyStandardDeviationsByAllSkillsExtractor()
		{
			var schedulingOptions = new SchedulingOptions();
			var result = _target.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(_scheduleMatrix, schedulingOptions);
			Assert.That(result, Is.TypeOf(typeof (RelativeDailyStandardDeviationsByAllSkillsExtractor)));
		}
    }

    
}
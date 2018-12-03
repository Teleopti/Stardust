using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Authentication;


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
            _target = new ScheduleResultDataExtractorProvider(new PersonalSkillsProvider(), new SkillPriorityProvider(), new UtcTimeZone());
        }

        [Test]
        public void ShouldThrowIfMatrixIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.CreatePersonalSkillDataExtractor(null, _advancedPreferences, null));
        }

        [Test]
        public void ShouldReturnBoostedIfUseImproved()
        {
			_advancedPreferences.UseTweakedValues = true;
			var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix, _advancedPreferences, null);
            Assert.That(result, Is.TypeOf(typeof(RelativeBoostedDailyDifferencesByPersonalSkillsExtractor)));
        }

        [Test]
        public void ShouldReturnOrdinaryIfNotUseImproved()
        {
            _advancedPreferences.UseTweakedValues = false;
			var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix, _advancedPreferences, null);
            Assert.That(result, Is.TypeOf(typeof(RelativeDailyDifferencesByPersonalSkillsExtractor)));
        }

		[Test]
		public void ShouldReturnRelativeDailyStandardDeviationsByAllSkillsExtractor()
		{
			var schedulingOptions = new SchedulingOptions();
			var result = _target.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(Enumerable.Empty<DateOnly>(), schedulingOptions, null);
			Assert.That(result, Is.TypeOf(typeof (RelativeDailyStandardDeviationsByAllSkillsExtractor)));
		}
    }

    
}
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
        private IOptimizerOriginalPreferences _optimizerPreference;
        private ScheduleResultDataExtractorProvider _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrix = _mock.StrictMock<IScheduleMatrixPro>();
            _optimizerPreference = new OptimizerOriginalPreferences();
            _target = new ScheduleResultDataExtractorProvider(_optimizerPreference);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfMatrixIsNull()
        {
            _target.CreatePersonalSkillDataExtractor(null);
        }

        [Test]
        public void ShouldReturnBoostedIfUseImproved()
        {
            var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix);
            Assert.That(result, Is.TypeOf(typeof(RelativeBoostedDailyDifferencesByPersonalSkillsExtractor)));
        }

        [Test]
        public void ShouldReturnOrdinaryIfNotUseImproved()
        {
            _optimizerPreference.AdvancedPreferences.UseTweakedValues = false;
            var result = _target.CreatePersonalSkillDataExtractor(_scheduleMatrix);
            Assert.That(result, Is.TypeOf(typeof(RelativeDailyDifferencesByPersonalSkillsExtractor)));
        }
    }

    
}
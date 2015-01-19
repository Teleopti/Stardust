using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeSkillIntervalDataToSkillIntervalDataMapperTest
	{
		private OvertimeSkillIntervalDataToSkillIntervalDataMapper _mapper;
		private OvertimeSkillIntervalData _source;
		private DateTimePeriod _dateTimePeriod;
		private double _forecastDemand;
		private double _currentDemand;
		
		
		[SetUp]
		public void Setup()
		{
			_mapper = new OvertimeSkillIntervalDataToSkillIntervalDataMapper();
		}

		[Test]
		public void VerifyMap()
		{
			_dateTimePeriod = new DateTimePeriod(2015, 01, 19, 2015, 01, 26);
			_forecastDemand = 12d;
			_currentDemand = 14d;
			_source = new OvertimeSkillIntervalData(_dateTimePeriod, _forecastDemand, _currentDemand);

			var destination = _mapper.Map(_source);

			Assert.NotNull(destination);
			Assert.AreEqual(_source.CurrentDemand, destination.CurrentDemand);
			Assert.AreEqual(_source.ForecastedDemand, destination.ForecastedDemand);
			Assert.AreEqual(_source.Period, destination.Period);

		}

		[Test]
		public void VerifyMapWithNullData()
		{
			_dateTimePeriod = new DateTimePeriod(2015, 01, 19, 2015, 01, 26);
			_forecastDemand = 0;
			_currentDemand = 0;
			_source = new OvertimeSkillIntervalData(_dateTimePeriod, _forecastDemand, _currentDemand);

			var destination = _mapper.Map(_source);

			Assert.NotNull(destination);
			Assert.AreEqual(_source.CurrentDemand, destination.CurrentDemand);
			Assert.AreEqual(_source.ForecastedDemand, destination.ForecastedDemand);
			Assert.AreEqual(_source.Period, destination.Period);

		}
	}
}

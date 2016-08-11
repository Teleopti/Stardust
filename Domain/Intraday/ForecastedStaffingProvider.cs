using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingProvider
	{
		private readonly IForecastedStaffingLoader _forecastedStaffingLoader;
		//private readonly ISkillDayRepository _skillDayRepository;
		//private readonly INow _now;
		//private readonly ISkillRepository _skillRepository;
		//private readonly IScenarioRepository _scenarioRepository;

		public ForecastedStaffingProvider(
			IForecastedStaffingLoader forecastedStaffingLoader
			//ISkillDayRepository skillDayRepository, 
			//INow now, 
			//ISkillRepository skillRepository, 
			//IScenarioRepository scenarioRepository
			)
		{
			_forecastedStaffingLoader = forecastedStaffingLoader;
			//_skillDayRepository = skillDayRepository;
			//_now = now;
			//_skillRepository = skillRepository;
			//_scenarioRepository = scenarioRepository;
		}

		//public void DoIt(Guid id)
		//{
		//	var date = new DateOnly(_now.UtcDateTime());
		//	var skill = _skillRepository.Get(id);
		//	var scenario = _scenarioRepository.LoadDefaultScenario();
		//	var skillDay = _skillDayRepository.FindRange(new DateOnlyPeriod(date, date), skill, scenario).First();

		//	foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
		//	{
		//		//var minutesStaffPeriods = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(15));
		//		Debug.WriteLine(skillStaffPeriod.Period.StartDateTime.ToShortTimeString() + " | " + skillStaffPeriod);
		//	}
		//}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var forecastedStaffingIntervals = _forecastedStaffingLoader.Load(skillIdList);

			var timeSeries = new List<DateTime>();

			foreach (var interval in forecastedStaffingIntervals)
			{
				timeSeries.Add(interval.StartTime);
			}

			return new IntradayStaffingViewModel()
			{
				DataSeries = new StaffingDataSeries()
				{
					Time = timeSeries.ToArray()
				}
			};
		}
	}
}
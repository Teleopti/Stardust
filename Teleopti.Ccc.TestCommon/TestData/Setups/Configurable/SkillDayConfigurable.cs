﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
// ReSharper disable PossibleLossOfFraction

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable 
{
	public class SkillDayConfigurable : IDataSetup
	{
		public string Skill { get; set; }
		public string Scenario { get; set; }
		public DateOnly DateOnly { get; set; }
		public double DefaultDemand { get; set; }
		public double Shrinkage { get; set; }
		public Tuple<int, double> HourDemand { get; set; }
		/// <summary>
		/// This wont work with open hours it will only work when the skill is open for 24hs
		/// If you need that please extend it
		/// </summary>
		public List<Tuple<int, double>> HourDemandList { get; set; } 
		public TimePeriod? OpenHours { get; set; }

	    public SkillDayConfigurable()
	    {
			HourDemandList = new List<Tuple<int, double>>();
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAllSkills().Single(x => x.Name == Skill);
			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var scenario = scenarioRepository.LoadAll().Single(x => x.Description.Name == Scenario);

			var defaultDemandTimespan = TimeSpan.FromMinutes(DefaultDemand * skill.DefaultResolution * (TimeSpan.FromHours(1).Ticks/TimeSpan.FromMinutes(skill.DefaultResolution).Ticks));

		    if (!HourDemandList.Any())
		    {
		        if (HourDemand == null)
		        {
		            HourDemand = new Tuple<int, double>(0, DefaultDemand);
		        }
		        var hourDemandTimespan =
		            TimeSpan.FromMinutes(HourDemand.Item2 * skill.DefaultResolution *
		                                 (TimeSpan.FromHours(1).Ticks / TimeSpan.FromMinutes(skill.DefaultResolution).Ticks));
		        ISkillDay skillDay;
		        skillDay = !OpenHours.HasValue
		            ? skill.CreateSkillDayWithDemandPerHour(scenario, DateOnly, defaultDemandTimespan,new Tuple<int, TimeSpan>(HourDemand.Item1, hourDemandTimespan))
		            : skill.CreateSkillDayWithDemandPerHour(scenario, DateOnly, defaultDemandTimespan,new Tuple<int, TimeSpan>(HourDemand.Item1, hourDemandTimespan), OpenHours.Value);
		        skillDay.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(Shrinkage));
		        var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
		        skillDayRepository.Add(skillDay);
		    }
		    else
		    {
		        var newDemadListToPass = new List<Tuple<int, TimeSpan>>();
				foreach (var hourDemad in HourDemandList)
		        {
					var hourDemandTimespan =
					 TimeSpan.FromMinutes(hourDemad.Item2 * skill.DefaultResolution * (TimeSpan.FromHours(1).Ticks / TimeSpan.FromMinutes(skill.DefaultResolution).Ticks));
		            newDemadListToPass.Add(new Tuple<int, TimeSpan>(hourDemad.Item1, hourDemandTimespan));
		        }
				
				ISkillDay skillDay;
		        skillDay =  skill.CreateSkillDayWithDemandPerHour(scenario, DateOnly, defaultDemandTimespan, newDemadListToPass);
				skillDay.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(Shrinkage));
				var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
				skillDayRepository.Add(skillDay);
			}
			

		    
		}
	}
}

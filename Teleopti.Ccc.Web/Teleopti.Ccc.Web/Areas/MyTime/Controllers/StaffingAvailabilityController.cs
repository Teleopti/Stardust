using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class StaffingAvailabilityController : ApiController
    {
		/*
		 * NOTE! 
		 * This is not tested at all and is to be considered as guidance
		 * of how things can be used to show a heat map of if a request likely will be approved or denied.
		 * Do not just copy paste and expect it to work 
		 * Amanda
		 */
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
	    private readonly ISkillRepository _skillRepository;

	    public StaffingAvailabilityController(INow now, ILoggedOnUser loggedOnUser, IStaffingViewModelCreator staffingViewModelCreator, 
			ISkillRepository skillRepository)
	    {
		    _now = now;
		    _loggedOnUser = loggedOnUser;
		    _staffingViewModelCreator = staffingViewModelCreator;
		    _skillRepository = skillRepository;
	    }

	    [UnitOfWork, HttpGet, Route("api/staffingavailability/agent")]
		public virtual IHttpActionResult StaffingAvailabilityForAgent()
		{
			// use a testable class to do things like this
			_skillRepository.LoadAllSkills();  //to not have proxies
			var person = _loggedOnUser.CurrentUser();
			var pp = person.PersonPeriodCollection.FirstOrDefault(x => x.Period.Contains(new DateOnly(_now.UtcDateTime())));
			if(pp == null)
				return BadRequest($"No person Period found for {person.Name}");
			var personSkills = pp.PersonSkillCollection;
			if (personSkills == null)
				return BadRequest($"No Skills found for {person.Name}");

			var staffingAvailabilityModels = new List<StaffingAvailabilityModel>();
			
			foreach (var skill in personSkills.Select(x => x.Skill))  // use skill by skill because they can have different thresholds! 
			{
				var intradayStaffingModel = _staffingViewModelCreator.Load(new[] {skill.Id.GetValueOrDefault()});  //<-- this part will change! But it will still accept a skill and output a similar (or same) model
				var availabilityModel = new StaffingAvailabilityModel
				{
					Skill = skill,
					Time = intradayStaffingModel.DataSeries.Time,
					ForecastedStaffing = intradayStaffingModel.DataSeries.ForecastedStaffing,
					ScheduledStaffing = intradayStaffingModel.DataSeries.ScheduledStaffing
				};

				staffingAvailabilityModels.Add(availabilityModel);
			}
			
			//maybe go through each IntradayStaffingViewModel for each skill and compare to threshold
			var understaffedIntervals = new Dictionary<DateTime, bool>();
			foreach (var model in staffingAvailabilityModels)
			{
				//to use same validation as used by requests you need to do something like this
				var intervalHasUnderStaffing = new IntervalHasUnderstaffing(model.Skill); //if Intraday validation on wfcs, if "Intraday with shrinkage" use IntervalShrinkageHasUnderstaffing
				
				for (var i = 0; i < model.Time.Length; i++)
				{
					if (!(model.ScheduledStaffing[i].HasValue && model.ForecastedStaffing[i].HasValue)) continue;

					var staffingInterval = new SkillStaffingInterval
					{
						CalculatedResource = model.ScheduledStaffing[i].Value,
						FStaff = model.ForecastedStaffing[i].Value  // I believe model.ForecastedStaffing is taking shrinkage into account, make sure to test it
					};
					
					bool isAlreadyUnderstaffedOnOtherSkill;
					understaffedIntervals.TryGetValue(model.Time[i], out isAlreadyUnderstaffedOnOtherSkill);

					if (!isAlreadyUnderstaffedOnOtherSkill)  // if understaffed on first skill, no need to check second for same interval
					{
						var isIntervalUnderstaffed = intervalHasUnderStaffing.IsSatisfiedBy(staffingInterval);
						understaffedIntervals.Add(model.Time[i], isIntervalUnderstaffed);
					}
				}
			}

			return Ok(understaffedIntervals);
		}

	    public class StaffingAvailabilityModel
	    {
			public ISkill Skill { get; set; }
			public DateTime[] Time { get; set; }
			public double?[] ForecastedStaffing { get; set; }
			public double?[] ScheduledStaffing { get; set; }
		}

	}
}

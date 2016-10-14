using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MaxSeat
{
	//bara testeri, testara just nu...
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillCreator _maxSeatSkillCreator;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly FullResourceCalculation _fullResourceCalculation;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public MaxSeatOptimization(MaxSeatSkillCreator maxSeatSkillCreator, 
														ResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleStorage scheduleStorage,
														FullResourceCalculation fullResourceCalculation,
														Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_maxSeatSkillCreator = maxSeatSkillCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleStorage = scheduleStorage;
			_fullResourceCalculation = fullResourceCalculation;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<Person> persons, IScenario scenario)
		{
			/*
			 * Mr Klagge - i mitt huvud nåt sånt här...
			 * 1. Vi genererar maxseatskills på samma sätt som schedulingscreen gjort förrut till att börja med. 
			 * 2. skapar res-beräkningscontext enbart med maxseatskills (andra skills kan vi skita i enligt anders för nu)
			 * 3. resursberäkna rubbet
			 * 3. Störst behov -> högst maxseat (tror jag) -> lägg ut skift enligt detta
			 * 4. Goto 3 tills... Nåt.
			 */
			var skills = Enumerable.Empty<ISkill>();
			var generatedMaxSeatSkills = _maxSeatSkillCreator.CreateMaxSeatSkills(period, scenario, persons.ToArray(), skills);
			generatedMaxSeatSkills.SkillsToAddToStateholder.ForEach(s => _schedulingResultStateHolder().AddSkills(s));
			generatedMaxSeatSkills.SkillDaysToAddToStateholder.ForEach(kvp => _schedulingResultStateHolder().SkillDays.Add(kvp));
			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(persons, new ScheduleDictionaryLoadOptions(false, false, false), period, scenario);

			using (_resourceCalculationContextFactory.Create(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder))
			{
				_fullResourceCalculation.Execute();

				var skillDays = _schedulingResultStateHolder().SkillDays;
				foreach (var skillPair in skillDays)
				{
					foreach (var skillDay in skillPair.Value)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							//kolla om det är över max seat
							if (skillStaffPeriod.Payload.CalculatedUsedSeats > skillStaffPeriod.Payload.MaxSeats)
							{
								//hitta gubbe random?
								foreach (var person in persons)
								{
									foreach (var personSkill in person.Period(skillDay.CurrentDate).PersonMaxSeatSkillCollection)
									{
										if (personSkill.Skill.Equals(skillDay.Skill))
										{
											var schedule = schedules[person].ScheduledDay(skillDay.CurrentDate);
											var projection = schedule.PersonAssignment(true).ProjectionService().CreateProjection();
											foreach (var layer in projection)
											{
												var activity = layer.Payload as IActivity;
												if (activity != null && activity.RequiresSeat)
												{
													//hitta nytt skift som inte får inte bryta mot max seat här eller på något annat intervall???
														//detta intervall måste ha "något" som inte tar upp max seats
														//andra intervall där max seat är på "gränsen" får inte få "något" som tar upp max seat

													//resursberäkna eller plocka bort på skillstaffperioden manuellt???
														
													//om det inte gick eller fortfarande över max försök med ny gubbe
													
												}
											}
										}
									}	
								}
							}
						}
					}
				}

				//res beräkna
				//kolla största behov (=högst maxseat)
				//byt skift
				//repeat
			}
		}
	}
}
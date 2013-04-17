using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IActivityDivider
    {
        /// <summary>
        /// Extracts the important data from the input parameters and divides them
        /// into a digestable data structure that can be fed to FurnessDataConverter.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Tamas
        /// Created date: 2008-02-07
        /// </remarks>
        IDividedActivityData DivideActivity(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
                                                             IAffectedPersonSkillService affectedPersonSkillService,
                                                           IActivity activity,
														   IResourceCalculationDataContainer filteredProjections,
                                                           DateTimePeriod periodToCalculate);
    }

    public class ActivityDivider : IActivityDivider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public IDividedActivityData DivideActivity(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
            IAffectedPersonSkillService affectedPersonSkillService,
            IActivity activity,
			IResourceCalculationDataContainer filteredProjections,
            DateTimePeriod periodToCalculate)
        {
            var dividedActivity = new DividedActivityData();
            var elapsedToCalculate = periodToCalculate.ElapsedTime();

            IEnumerable<ISkill> skillsForActivity = skillsInActivity(affectedPersonSkillService,activity);
            foreach (ISkill skill in skillsForActivity)
            {
                double? targetDemandValue = skillDayDemand(skill,relevantSkillStaffPeriods,periodToCalculate);
                if (targetDemandValue.HasValue)
                    dividedActivity.TargetDemands.Add(skill, targetDemandValue.Value);
            }

	        var periodResources = filteredProjections.AffectedResources(activity, periodToCalculate);
	        foreach (var periodResource in periodResources)
	        {
				if (periodResource.Value == 0) continue;
		        var skills = skillsFromKey(periodResource.Key);
				var traff = periodResource.Value;

				var personSkillEfficiencyRow = new Dictionary<ISkill, double>();
				var relativePersonSkillResourceRow = new Dictionary<ISkill, double>();
				var personSkillResourceRow = new Dictionary<ISkill, double>();

				foreach (var skill in skills)
				{
					const double skillEfficiencyValue = 1;

					double personSkillResourceValue = traff;
					const double bitwiseSkillEfficiencyValue = skillEfficiencyValue == 0 ? 0d : 1d;
					double relativePersonSkillResourceValue = traff * bitwiseSkillEfficiencyValue;

					relativePersonSkillResourceRow.Add(skill, relativePersonSkillResourceValue);
					personSkillResourceRow.Add(skill, personSkillResourceValue);
					personSkillEfficiencyRow.Add(skill, skillEfficiencyValue);

					// add to sum also
					double currentRelativePersonSkillResourceValue;
					if (dividedActivity.RelativePersonSkillResourcesSum.TryGetValue(skill, out currentRelativePersonSkillResourceValue))
					{
						dividedActivity.RelativePersonSkillResourcesSum[skill] = currentRelativePersonSkillResourceValue + relativePersonSkillResourceValue;
					}
					else
					{
						dividedActivity.RelativePersonSkillResourcesSum.Add(skill, relativePersonSkillResourceValue);
					}
				}

				if (!dividedActivity.PersonSkillEfficiencies.ContainsKey(periodResource.Key))
				{
					dividedActivity.PersonSkillEfficiencies.Add(periodResource.Key, personSkillEfficiencyRow);
					dividedActivity.WeightedRelativePersonSkillResources.Add(periodResource.Key, personSkillResourceRow);
					dividedActivity.RelativePersonSkillResources.Add(periodResource.Key, relativePersonSkillResourceRow);
					dividedActivity.RelativePersonResources.Add(periodResource.Key, traff);

					double targetResourceValue = elapsedToCalculate.TotalMinutes * traff;
					dividedActivity.PersonResources.Add(periodResource.Key, targetResourceValue);
				}
	        }

            return dividedActivity;
        }

	    private IEnumerable<ISkill> skillsFromKey(string key)
	    {
		    var skillIdCollection = key.Split('|')[1].Split('_');
		    return
			    skillIdCollection.Select(
				    s =>
					    {
						    var skill = new Skill("dummy", "dummy", Color.NavajoWhite, 15,
						                          new SkillTypePhone(new Description("temp"), ForecastSource.Time));
							skill.SetId(new Guid(s));
						    return skill;
					    });
	    }

	    private static double? skillDayDemand(ISkill skill, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, DateTimePeriod periodToCalculate)
        {
            double retVal = 0;
            ISkillStaffPeriodDictionary skillStaffPeriods;
            bool anythingOpen = false;
            if (!relevantSkillStaffPeriods.TryGetValue(skill, out skillStaffPeriods))
                return null;

            double totalTime = 0;

            foreach (var skillStaffPeriod in skillStaffPeriods)
            {
                if (periodToCalculate.StartDateTime>skillStaffPeriod.Key.EndDateTime) continue;
                if (periodToCalculate.EndDateTime<skillStaffPeriod.Key.StartDateTime) break;

                DateTimePeriod? intersection = periodToCalculate.Intersection(skillStaffPeriod.Key);

                if (intersection.HasValue)
                {
                    foreach (var openHourPeriod in skillStaffPeriods.SkillOpenHoursCollection)
                    {
                        if (openHourPeriod.Intersect(periodToCalculate))
                            anythingOpen = true;
                    }

                    double skillStaffPeriodSeconds = skillStaffPeriod.Key.ElapsedTime().TotalSeconds;
                    double intersectPercent =
                        intersection.Value.ElapsedTime().TotalSeconds/
                        skillStaffPeriodSeconds;
                    totalTime += skillStaffPeriod.Value.ForecastedDistributedDemand*
                                 skillStaffPeriodSeconds*intersectPercent;
                }


                retVal = totalTime / periodToCalculate.ElapsedTime().TotalSeconds;
            }
            if (!anythingOpen)
                return null;

            return retVal;
        }

        private static IEnumerable<ISkill> skillsInActivity(IAffectedPersonSkillService affectedPersonSkillService, IActivity activity)
        {
            var distinctList = new HashSet<ISkill>();
            foreach (var affectedSkill in affectedPersonSkillService.AffectedSkills)
            {
                if (affectedSkill.Activity.Equals(activity))
                {
                    distinctList.Add(affectedSkill);
                }
            }
            return distinctList;
        }
    }

	public class ActivitySkillsCombination
	{
		private readonly IPayload _activity;
		private readonly ISkill[] _skills;

		public ActivitySkillsCombination(IPayload activity, params ISkill[] skills)
		{
			_activity = activity;
			_skills = skills;
		}

		public string GenerateKey()
		{
			return _activity == null
				       ? string.Empty
				       : _activity.Id.GetValueOrDefault() + "|" +
				         string.Join("_", _skills.OrderBy(a => a.Name).Select(a => a.Id.GetValueOrDefault()));
		}
	}

	public class ResourceCalculationDataContainer : IResourceCalculationDataContainer
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IDictionary<DateTimePeriod,IDictionary<string,double>> _dictionary = new Dictionary<DateTimePeriod, IDictionary<string, double>>();

		public ResourceCalculationDataContainer(IPersonSkillProvider personSkillProvider)
		{
			_personSkillProvider = personSkillProvider;
		}

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool HasItems()
		{
			return _dictionary.Count > 0;
		}

		public void AddResources(DateTimePeriod period, IPayload activity, IPerson person, DateOnly personDate,
		                         double resource)
		{
			IDictionary<string, double> resources;
			if (!_dictionary.TryGetValue(period, out resources))
			{
				resources = new Dictionary<string, double>();
				_dictionary.Add(period, resources);
			}

			var key = new ActivitySkillsCombination(activity, _personSkillProvider.SkillsOnPersonDate(person, personDate)).GenerateKey();
			double foundResource;
			if (resources.TryGetValue(key, out foundResource))
			{
				resources[key] = resource + foundResource;
			}
			else
			{
				resources.Add(key, resource);
			}
		}

		public double SkillResources(ISkill skill, DateTimePeriod period)
		{
			var skillKey = skill.Id.GetValueOrDefault().ToString();
			var activityKey = string.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault().ToString();
			}

			double resource = 0;
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(15));
			foreach (var dateTimePeriod in periodSplit)
			{
				IDictionary<string,double> interval;
				if (_dictionary.TryGetValue(dateTimePeriod, out interval))
				{
					foreach (var pair in interval)
					{
						if (!string.IsNullOrEmpty(activityKey) && !pair.Key.StartsWith(activityKey)) continue;
						if (!pair.Key.Contains(skillKey)) continue;

						resource += pair.Value;
					}
				}
			}
			return resource;
		}

		public bool AllIsSingleSkill()
		{
			return !_dictionary.Values.Any(v => v.Keys.Any(k => k.Contains("_")));
		}

		public IDictionary<string, double> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<string, double>();

			var activityKey = activity.Id.GetValueOrDefault().ToString();
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(15));
			foreach (var dateTimePeriod in periodSplit)
			{
				IDictionary<string, double> interval;
				if (_dictionary.TryGetValue(dateTimePeriod, out interval))
				{
					foreach (var pair in interval)
					{
						if (!pair.Key.StartsWith(activityKey)) continue;

						if (result.ContainsKey(pair.Key))
						{
							result[pair.Key] += pair.Value;
						}
						else
						{
							result.Add(pair.Key, pair.Value);
						}
					}
				}
			}
			return result;
		}
	}

	public interface IPersonSkillProvider
	{
		ISkill[] SkillsOnPersonDate(IPerson person, DateOnly date);
	}

	public class PersonSkillProvider : IPersonSkillProvider
	{
		public ISkill[] SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			IPersonPeriod personPeriod = person.Period(date);
			if (personPeriod == null) return new ISkill[0];

			var skills = personPeriod.PersonSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0).Select(s => s.Skill);
			if (personPeriod.Team.Site.MaxSeatSkill != null)
			{
				skills = skills.Concat(new[] {personPeriod.Team.Site.MaxSeatSkill});
			}

			return skills.ToArray();
		}
	}
}

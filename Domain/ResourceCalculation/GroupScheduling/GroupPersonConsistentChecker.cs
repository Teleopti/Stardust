﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPersonConsistentChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		bool AllPersonsHasSameOrNoneScheduled(IScheduleDictionary scheduleDictionary, IList<IPerson> persons, 
		                                                      DateOnly dateOnly, ISchedulingOptions schedulingOptions);

		PossibleStartEndCategory CommonPossibleStartEndCategory { get; }
	}

	public class GroupPersonConsistentChecker : IGroupPersonConsistentChecker
	{
		private HashSet<PossibleStartEndCategory> _possible= new HashSet<PossibleStartEndCategory>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public bool AllPersonsHasSameOrNoneScheduled(IScheduleDictionary scheduleDictionary, IList<IPerson> persons, 
			DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
			InParameter.NotNull("schedulingOptions", schedulingOptions);
			InParameter.NotNull("persons",persons);
			InParameter.NotNull("scheduleDictionary", scheduleDictionary);
			_possible = new HashSet<PossibleStartEndCategory>();

			foreach (var person in persons)
			{
                if(!person.VirtualSchedulePeriod(dateOnly).IsValid)
                    continue;
				var range = scheduleDictionary[person];
				var day = range.ScheduledDay(dateOnly);
				if (day.SignificantPart().Equals(SchedulePartView.MainShift))
				{
					var poss = new PossibleStartEndCategory();
					var shift = day.AssignmentHighZOrder().MainShift;
					if(schedulingOptions.UseGroupSchedulingCommonStart	 || schedulingOptions.UseGroupSchedulingCommonEnd)
					{
						var period = shift.ProjectionService().CreateProjection().Period().Value;						
						if (schedulingOptions.UseGroupSchedulingCommonStart)
							poss.StartTime = period.LocalStartDateTime.TimeOfDay;
						if (schedulingOptions.UseGroupSchedulingCommonEnd)
							poss.EndTime = period.LocalEndDateTime.TimeOfDay;
					}

					if (schedulingOptions.UseGroupSchedulingCommonCategory)
						poss.ShiftCategory = shift.ShiftCategory;

					_possible.Add(poss);
				}
			}
			return _possible.Count < 2;
		}

		public PossibleStartEndCategory CommonPossibleStartEndCategory
		{
			get { 
				if(_possible.Count != 1)
				return null;

				return _possible.First();
			}
		}
	}
}
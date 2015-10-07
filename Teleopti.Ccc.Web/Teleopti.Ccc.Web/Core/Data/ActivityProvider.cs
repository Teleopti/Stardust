using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class ActivityProvider : IActivityProvider
	{
		private readonly IActivityRepository _activityRepository;

		public ActivityProvider(IActivityRepository activityRepository)
		{
			_activityRepository = activityRepository;
		}

		public IEnumerable<ActivityViewModel> GetAll()
		{
			var activities = new List<ActivityViewModel>();

			foreach (var activity in _activityRepository.LoadAll())
			{
				if (activity.RequiresSkill) activities.Add(new ActivityViewModel {Id = activity.Id, Name = activity.Name});
			}

			return activities;
		}
	}
}
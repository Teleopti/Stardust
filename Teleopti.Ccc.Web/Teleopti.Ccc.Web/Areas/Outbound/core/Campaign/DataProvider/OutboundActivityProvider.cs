using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class OutboundActivityProvider : IOutboundActivityProvider
	{
		private readonly IActivityRepository _activityRepository;

		public OutboundActivityProvider(IActivityRepository activityRepository)
		{
			_activityRepository = activityRepository;
		}

		public IEnumerable<ActivityViewModel> GetAll()
		{
			var activities = new List<ActivityViewModel>();

			foreach (var activity in _activityRepository.LoadAll())
			{
				if (activity.RequiresSkill) activities.Add(new ActivityViewModel(){Id = activity.Id, Name = activity.Name});
			}

			return activities;
		}
	}
}
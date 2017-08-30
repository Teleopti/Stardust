using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeActivityRepository : IActivityRepository, IProxyForId<IActivity>
	{
		private readonly IList<IActivity> _activities;

		public FakeActivityRepository()
		{
			_activities = new List<IActivity>();
		}

		public void Has(IActivity activity)
		{
			Add(activity);
		}

		public IActivity Has(string name)
		{
			var newActivity = new Activity(name)
			{
				InWorkTime = true,
				InContractTime = true,
				RequiresSkill = true
			}.WithId();
			_activities.Add(newActivity);
			return newActivity;
		}
		
		public void Add(IActivity activity)
		{
			_activities.Add(activity);
		}

		public void Remove(IActivity entity)
		{
			throw new NotImplementedException();
		}

		public IActivity Get(Guid id)
		{
			return _activities.FirstOrDefault(a => id == a.Id);
		}

		public IList<IActivity> LoadAll()
		{
			return _activities;
		}

		public IActivity Load(Guid id)
		{
			return _activities.First(x => x.Id.GetValueOrDefault() == id);
		}

		public IList<IActivity> LoadAllSortByName()
		{
			return _activities.OrderBy(x => x.Name).ToList();
		}
	}
}
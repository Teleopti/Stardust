using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeActivityRepository : IActivityRepository
	{
		private readonly IList<IActivity> _activities;
		private bool withoutAdd = true;
		private readonly IActivity activity = ActivityFactory.CreateActivity("phone");

		public FakeActivityRepository()
		{
			_activities = new List<IActivity>();
		}

		public IActivity Has(string name)
		{
			var newActivity = new Activity(name)
			{
				InWorkTime = true,
				InContractTime = true,
				RequiresSkill = true
			};
			_activities.Add(newActivity);
			return newActivity;
		}

		public void SetOutboundActivity(bool isOutboundActivity)
		{
			activity.IsOutboundActivity = isOutboundActivity;
		}

		public void Add(IActivity activity)
		{
			withoutAdd = false;
			_activities.Add(activity);
		}

		public void Remove(IActivity entity)
		{
			throw new NotImplementedException();
		}

		public IActivity Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IActivity> LoadAll()
		{
			return withoutAdd ? new List<IActivity> { activity } : _activities;
		}

		public IActivity Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IActivity> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IActivity> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for person rotations
	/// </summary>
	public class PreferenceDayRepository : Repository<IPreferenceDay>, IPreferenceDayRepository
	{
		public PreferenceDayRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			var result = new List<IPreferenceDay>();

			if (!persons.Any()) return result;

			foreach (var personList in persons.Batch(400))
			{
				ICriteria crit = FilterByPeriod(period)
					.Add(personCriterion(personList.ToArray()))
					.Fetch("Restriction")
					.Fetch("Restriction.ActivityRestrictionCollection")
					.SetResultTransformer(Transformers.DistinctRootEntity);
				result.AddRange(crit.List<IPreferenceDay>());
			}

			InitializePreferenceDays(result);
			return result;
		}

		public IList<IPreferenceDay> Find(DateOnly dateOnly, IPerson person)
		{
			ICriteria crit = Session.CreateCriteria(typeof(PreferenceDay))
				.Add(Restrictions.Eq("RestrictionDate", dateOnly))
				.Add(Restrictions.Eq("Person", person))
				.Fetch("Restriction");
			IList<IPreferenceDay> retList = crit.List<IPreferenceDay>();

			InitializePreferenceDays(retList);
			return retList;
		}

		public IList<IPreferenceDay> FindAndLock(DateOnly dateOnly, IPerson person)
		{
			// lock mode set to upgrade
			ICriteria crit = Session.CreateCriteria(typeof(PreferenceDay))
				.Add(Restrictions.Eq("RestrictionDate", dateOnly))
				.Add(Restrictions.Eq("Person", person))
				.SetLockMode(LockMode.Upgrade)
				.Fetch("Restriction");
			IList<IPreferenceDay> retList = crit.List<IPreferenceDay>();

			InitializePreferenceDays(retList);
			return retList;
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IPerson person)
		{
			// lock mode set to upgrade
			var result = new List<IPreferenceDay>();
			ICriteria crit = FilterByPeriod(period)
				.Add(Restrictions.Eq("Person", person))
				.SetLockMode(LockMode.Upgrade)
				.Fetch("Restriction")
				.Fetch("Restriction.ActivityRestrictionCollection")
				.SetResultTransformer(Transformers.DistinctRootEntity);
			result.AddRange(crit.List<IPreferenceDay>());

			InitializePreferenceDays(result);
			return result;
		}

		public IList<IPreferenceDay> FindNewerThan(DateTime newerThan)
		{
			var crit = Session.CreateCriteria(typeof(PreferenceDay))
				.Add(Restrictions.Gt("UpdatedOn", newerThan))
				.Fetch("Restriction");
			var retList = crit.List<IPreferenceDay>();

			InitializePreferenceDays(retList);
			return retList;
		}

		public IPreferenceDay Find(Guid preferenceId)
		{
			ICriteria crit = Session.CreateCriteria(typeof(PreferenceDay))
				.Add(Restrictions.Eq("Id", preferenceId))
				.Fetch("Restriction");
			IList<IPreferenceDay> retList = crit.List<IPreferenceDay>();

			InitializePreferenceDays(retList);

			if (retList.IsEmpty())
				return null;
			return retList.First();
		}

		private ICriteria FilterByPeriod(DateOnlyPeriod period)
		{
			return Session.CreateCriteria(typeof(PreferenceDay))
				 .Add(Restrictions.Between("RestrictionDate", period.StartDate, period.EndDate))
				 .AddOrder(Order.Asc("Person"))
				 .AddOrder(Order.Asc("RestrictionDate"));
		}

		private static void InitializePreferenceDays(IEnumerable<IPreferenceDay> preferenceDays)
		{
			foreach (IPreferenceDay day in preferenceDays)
			{
				InitializePreferenceDay(day);
			}
		}

		private static void InitializePreferenceDay(IPreferenceDay day)
		{
			if (!LazyLoadingManager.IsInitialized(day.Person))
				LazyLoadingManager.Initialize(day.Person);
			if (!LazyLoadingManager.IsInitialized(day.Restriction.ShiftCategory))
				LazyLoadingManager.Initialize(day.Restriction.ShiftCategory);
			if (!LazyLoadingManager.IsInitialized(day.Restriction.DayOffTemplate))
				LazyLoadingManager.Initialize(day.Restriction.DayOffTemplate);
			if (!LazyLoadingManager.IsInitialized(day.Restriction.ActivityRestrictionCollection))
				LazyLoadingManager.Initialize(day.Restriction.ActivityRestrictionCollection);
			foreach (var activityRestriction in day.Restriction.ActivityRestrictionCollection)
			{
				if (!LazyLoadingManager.IsInitialized(activityRestriction.Activity))
					LazyLoadingManager.Initialize(activityRestriction.Activity);
			}
		}

		private static ICriterion personCriterion(IList<IPerson> personCollection)
		{
			ICriterion ret;
			if (personCollection.Count > 1)
				ret = Restrictions.InG("Person", personCollection);
			else
				ret = Restrictions.Eq("Person", personCollection[0]);
			return ret;
		}

		/// <summary>
		/// Loads the aggregate.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-03-09
		/// </remarks>
		public IPreferenceDay LoadAggregate(Guid id)
		{
			PreferenceDay retObj = Session.CreateCriteria(typeof(PreferenceDay))
							.Add(Restrictions.IdEq(id))
							.UniqueResult<PreferenceDay>();
			if (retObj != null)
				InitializePreferenceDay(retObj);
			return retObj;
		}
	}
}
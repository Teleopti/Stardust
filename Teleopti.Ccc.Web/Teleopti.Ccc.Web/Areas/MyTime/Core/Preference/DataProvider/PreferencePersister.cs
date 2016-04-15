using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePersister : IPreferencePersister
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IMappingEngine _mapper;
		private readonly IMustHaveRestrictionSetter _mustHaveRestrictionSetter;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly INow _now;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IEventPublisher _publisher;

		public PreferencePersister(
			IPreferenceDayRepository preferenceDayRepository, 
			IMappingEngine mapper,  
			IMustHaveRestrictionSetter mustHaveRestrictionSetter,
			ILoggedOnUser loggedOnUser, 
			ICurrentUnitOfWork currentUnitOfWork, 
			ICurrentBusinessUnit businessUnitProvider, 
			INow now, 
			ICurrentDataSource currentDataSource,
			IEventPublisher publisher)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_mapper = mapper;
			_mustHaveRestrictionSetter = mustHaveRestrictionSetter;
			_loggedOnUser = loggedOnUser;
			_currentUnitOfWork = currentUnitOfWork;
			_businessUnitProvider = businessUnitProvider;
			_now = now;
			_currentDataSource = currentDataSource;
			_publisher = publisher;
		}

		public PreferenceDayViewModel Persist(PreferenceDayInput input)
		{
			var preferenceDays = _preferenceDayRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			preferenceDays = DeleteOrphanPreferenceDays(preferenceDays);
			var preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			if (preferenceDay == null)
			{
				preferenceDay = _mapper.Map<PreferenceDayInput, IPreferenceDay>(input);
				_preferenceDayRepository.Add(preferenceDay);
			}
			else
			{
				ClearExtendedAndMustHave(preferenceDay);
				_mapper.Map(input, preferenceDay);
			}

			if (_currentUnitOfWork != null && input.PreferenceId.HasValue)
			{
				var message = new PreferenceChangedEvent
				{
					LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					LogOnDatasource = _currentDataSource.Current().DataSourceName,
					PreferenceId = input.PreferenceId.Value,
					Timestamp = _now.UtcDateTime()
				};
				_currentUnitOfWork.Current().AfterSuccessfulTx(() => _publisher.Publish(message));
			}

			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}

		public bool MustHave(MustHaveInput input)
		{
			return _mustHaveRestrictionSetter.SetMustHave(input.Date, _loggedOnUser.CurrentUser(), input.MustHave);
		}

		private static void ClearExtendedAndMustHave(IPreferenceDay preferenceDay)
		{
			if (preferenceDay.Restriction != null)
			{
				preferenceDay.Restriction.StartTimeLimitation = new StartTimeLimitation();
				preferenceDay.Restriction.EndTimeLimitation = new EndTimeLimitation();
				preferenceDay.Restriction.WorkTimeLimitation = new WorkTimeLimitation();
				preferenceDay.Restriction.MustHave = false;
				preferenceDay.TemplateName = null;
			}
		}


		private IList<IPreferenceDay> DeleteOrphanPreferenceDays(IList<IPreferenceDay> preferenceDays)
		{
			preferenceDays = preferenceDays != null
				                 ? preferenceDays.OrderBy(k => k.UpdatedOn).ToList()
				                 : new List<IPreferenceDay>();
			while (preferenceDays.Count > 1)
			{
				_preferenceDayRepository.Remove(preferenceDays.First());
				preferenceDays.Remove(preferenceDays.First());
			}
			return preferenceDays;
		}

		public PreferenceDayViewModel Delete(IList<IPreferenceDay> preferences)
		{
			foreach (var preferenceDay in preferences)
			{
				_preferenceDayRepository.Remove(preferenceDay);
			}
			return new PreferenceDayViewModel { Color = "" };
		}


		public IEnumerable<PreferenceDayViewModel> Delete(List<DateOnly> dates)
		{
			return dates.Select (date => _preferenceDayRepository.FindAndLock (date, _loggedOnUser.CurrentUser()))
				.Where (preferences => !preferences.IsEmpty())
				.Select (Delete).ToList();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMapper : IShiftTradeScheduleViewModelMapper
	{
		private readonly IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeAddPersonScheduleViewModelMapper _shiftTradePersonScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradeSiteOpenHourFilter _shiftTradeSiteOpenHourFilter;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;
		private readonly IPermissionProvider _permissionProvider;

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider,
			IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
			IShiftTradeAddPersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper,
			IPersonRequestRepository personRequestRepository,
			IScheduleProvider scheduleProvider,
			ILoggedOnUser loggedOnUser, IShiftTradeSiteOpenHourFilter shiftTradeSiteOpenHourFilter, IProjectionChangedEventBuilder builder, IPersonRepository personRepository, IPersonNameProvider personNameProvider, IShiftTradeAddScheduleLayerViewModelMapper layerMapper, IPermissionProvider permissionProvider)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradePersonScheduleViewModelMapper = shiftTradePersonScheduleViewModelMapper;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
			_personRequestRepository = personRequestRepository;
			_scheduleProvider = scheduleProvider;
			_loggedOnUser = loggedOnUser;
			_shiftTradeSiteOpenHourFilter = shiftTradeSiteOpenHourFilter;
			_builder = builder;
			_personRepository = personRepository;
			_personNameProvider = personNameProvider;
			_layerMapper = layerMapper;
			_permissionProvider = permissionProvider;
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data)
		{
			if (data.Paging.Equals(Paging.Empty) || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			return getShiftTradeScheduleViewModel(data.Paging, data.ShiftTradeDate, data.TimeFilter, data.TimeSortOrder, possibleTradePersons);
		}

		private IEnumerable<IShiftExchangeOffer> getMatchShiftExchangeOffers(DateOnly shiftTradeDate)
		{
			var myScheduleDay =
				_scheduleProvider.GetScheduleForPersons(shiftTradeDate, new List<IPerson> { _loggedOnUser.CurrentUser() })
					.FirstOrDefault();

			if (myScheduleDay == null)
			{
				return new List<IShiftExchangeOffer>();
			}
			var result = _personRequestRepository.FindShiftExchangeOffersForBulletin(shiftTradeDate);
			return result.Where(x => x.IsWantedSchedule(myScheduleDay));
		}

		private IScheduleDay getScheduleDayForShiftExchangeOffer(IShiftExchangeOffer offer)
		{
			IEnumerable<IPerson> persons = new List<IPerson>() { offer.Person };
			return _scheduleProvider.GetScheduleForPersons(offer.Date, persons).FirstOrDefault();
		}

		private IEnumerable<IShiftExchangeOffer> getMatchShiftExchangeOffers(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers, TimeFilterInfo timeFilter)
		{
			var possibleTargets = shiftExchangeOffers.Select(x => new
			{
				ShiftExchangeOffer = x,
				ScheduleDay = getScheduleDayForShiftExchangeOffer(x)
			});

			Func<IScheduleDay, bool> filterFunc = s =>
			{
				if (s != null && timeFilter.IsDayOff && s.HasDayOff())
				{
					return true;
				}

				var period = s?.ProjectionService().CreateProjection().Period();
				if (period.HasValue)
				{
					bool startMatch = timeFilter.StartTimes.IsEmpty() ||
									  !timeFilter.StartTimes.Where(start => start.Contains(period.Value.StartDateTime)).IsEmpty();
					bool endMatch = timeFilter.EndTimes.IsEmpty() ||
									!timeFilter.EndTimes.Where(end => end.Contains(period.Value.EndDateTime)).IsEmpty();
					return startMatch && endMatch;
				}
				return s != null && !s.HasDayOff() && timeFilter.IsEmptyDay;
			};

			return possibleTargets.Where(x => filterFunc(x.ScheduleDay)).Select(x => x.ShiftExchangeOffer);
		}


		public ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelData data)
		{
			if (data.Paging.Equals(Paging.Empty) || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var filteredShiftExchangeOffers = getMatchShiftExchangeOffers(data.ShiftTradeDate);
			var ids = new HashSet<Guid>();
			var shiftExchangeOffers = filteredShiftExchangeOffers as IShiftExchangeOffer[] ?? filteredShiftExchangeOffers.ToArray();
			foreach (var request in shiftExchangeOffers)
			{
				if (request.Person.Id != null) ids.Add(request.Person.Id.Value);
			}

			var personDictionary = _possibleShiftTradePersonsProvider.RetrievePersons(data, ids.ToArray())
								.Persons.Distinct()
								.ToDictionary(p => p.Id);

			filteredShiftExchangeOffers = filteredShiftExchangeOffers.Where(a => personDictionary.ContainsKey(a.Person.Id.Value));
			if (data.TimeFilter != null)
			{
				filteredShiftExchangeOffers = getMatchShiftExchangeOffers(filteredShiftExchangeOffers, data.TimeFilter);
			}

			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var myScheduleViewModel = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true, false);

			if (myScheduleViewModel != null)
			{
				filteredShiftExchangeOffers =
					filteredShiftExchangeOffers.Where(
						shiftExchangeOffer =>
							_shiftTradeSiteOpenHourFilter.FilterShiftExchangeOffer(shiftExchangeOffer, myScheduleViewModel));
			}

			var possibleTradeSchedule = getBulletinSchedules(filteredShiftExchangeOffers, data.Paging);

			return getShiftTradeScheduleViewModel(data.Paging, myScheduleViewModel, possibleTradeSchedule, data.ShiftTradeDate);
		}

		public ShiftTradeMultiSchedulesViewModel GetMeAndPersonToSchedules(DateOnlyPeriod period, Guid personToId)
		{
			var viewModel = new ShiftTradeMultiSchedulesViewModel
			{
				MySchedules = new List<ShiftTradeAddPersonScheduleViewModel>(),
				PersonToSchedules = new List<ShiftTradeAddPersonScheduleViewModel>()
			};
			var fixedPeriod = fixPeriodForUnpublishedSchedule(period);
			if (fixedPeriod == null) return viewModel;

			var personTo = _personRepository.Get(personToId);
			var allSchedules = _shiftTradeRequestProvider.RetrieveTradeMultiSchedules(period,
				new List<IPerson> {_loggedOnUser.CurrentUser(), personTo });
			if (allSchedules == null) return viewModel;

			allSchedules.TryGetValue(_loggedOnUser.CurrentUser(), out var myScheduleRange);
			allSchedules.TryGetValue(personTo, out var personToScheduleRange);
			var mySchedules = new List<ShiftTradeAddPersonScheduleViewModel>();
			var personToSchedules = new List<ShiftTradeAddPersonScheduleViewModel>();
			foreach (var dateOnly in fixedPeriod.Value.DayCollection())
			{
				var mySchedule = mapToShiftTradeAddPersonScheduleViewModel(myScheduleRange, dateOnly, _loggedOnUser.CurrentUser());
				if (mySchedule != null) mySchedules.Add(mySchedule);
				var personToSchedule = mapToShiftTradeAddPersonScheduleViewModel(personToScheduleRange, dateOnly, personTo);
				if (personToSchedule != null) personToSchedules.Add(personToSchedule);
			}

			return new ShiftTradeMultiSchedulesViewModel
			{
				MySchedules = mySchedules,
				PersonToSchedules = personToSchedules
			};
		}

		private DateOnlyPeriod? fixPeriodForUnpublishedSchedule(DateOnlyPeriod periodInput)
		{
			var dateTimePeriod = periodInput.ToDateTimePeriod(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)) return periodInput;

			var publishedToDate = _loggedOnUser.CurrentUser().WorkflowControlSet.SchedulePublishedToDate;
			if (!publishedToDate.HasValue) return null;

			var startTime = periodInput.StartDate;
			var endTime = periodInput.EndDate;
			if (publishedToDate >= dateTimePeriod.StartDateTime && publishedToDate < dateTimePeriod.EndDateTime) endTime = new DateOnly(publishedToDate.Value);
			if (publishedToDate < dateTimePeriod.StartDateTime) return null;

			return new DateOnlyPeriod(startTime, endTime);
		}

		private ShiftTradeAddPersonScheduleViewModel mapToShiftTradeAddPersonScheduleViewModel(IScheduleRange scheduleRange, DateOnly date, IPerson person)
		{
			var scheduleDay = scheduleRange?.ScheduledDay(date);
			if (scheduleDay == null) return null;

			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			var layers = new List<SimpleLayer>();
			if (eventScheduleDay.Shift != null)
			{
				var ls = from layer in eventScheduleDay.Shift.Layers
					select new SimpleLayer
					{
						Color = ColorTranslator.ToHtml(Color.FromArgb(layer.DisplayColor)),
						Description = layer.Name,
						Start = layer.StartDateTime,
						End = layer.EndDateTime,
						Minutes = (int)layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes,
						IsAbsenceConfidential = layer.IsAbsenceConfidential
					};

				layers.AddRange(ls);
			}

			var shiftCategory = scheduleDay.PersonAssignment()?.ShiftCategory;
			var isDayOff = eventScheduleDay.DayOff != null;
			var isFulldayAbsence = eventScheduleDay.IsFullDayAbsence;
			var startTimeUtc = date.Date;
			var categoryName = shiftCategory?.Description.Name;
			string displayColor = null;
			if (shiftCategory != null) displayColor = ColorTranslator.ToHtml(Color.FromArgb(shiftCategory.DisplayColor.ToArgb()));   //$"rgb({shiftCategory.DisplayColor.R},{shiftCategory.DisplayColor.G},{shiftCategory.DisplayColor.B})";
			if (eventScheduleDay.Shift != null) startTimeUtc = eventScheduleDay.Shift.StartDateTime;
			if (isDayOff) startTimeUtc = eventScheduleDay.DayOff.StartDateTime;
			if (isFulldayAbsence)
			{
				startTimeUtc = scheduleDay.PersonAbsenceCollection()[0].Layer.Period.StartDateTime;
				categoryName =  scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDescription(person).Name;
				var absenceColor = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.DisplayColor;
				displayColor = ColorTranslator.ToHtml(Color.FromArgb(absenceColor.ToArgb()));//$"rgb({absenceColor.R},{absenceColor.G},{absenceColor.B})";
			}
			return new ShiftTradeAddPersonScheduleViewModel
			{
				ContractTimeInMinute = eventScheduleDay.ContractTime.TotalMinutes,
				DayOffName = eventScheduleDay.Name,
				IsDayOff = isDayOff,
				IsFullDayAbsence = isFulldayAbsence,
				IsNotScheduled = eventScheduleDay.Shift == null && !isDayOff && !isFulldayAbsence,
				MinStart = eventScheduleDay.Shift?.StartDateTime,
				StartTimeUtc = startTimeUtc,
				Name = _personNameProvider.BuildNameFromSetting(scheduleDay.Person.Name.FirstName, scheduleDay.Person.Name.LastName),
				PersonId = scheduleDay.Person.Id.GetValueOrDefault(),
				ScheduleLayers = _layerMapper.Map(layers, person.Id == _loggedOnUser.CurrentUser().Id),
				ShiftExchangeOfferId = null,
				CategoryName = categoryName,
				DisplayColor = displayColor
			};
		}

		private ShiftTradeScheduleViewModel getShiftTradeScheduleViewModel(Paging paging,
			ShiftTradeAddPersonScheduleViewModel mySchedule, IEnumerable<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule, DateOnly shiftTradeDate)
		{
			var possibleTradeScheduleNum = possibleTradeSchedule.Any()
				? possibleTradeSchedule.First().Total
				: 0;
			var pageCount = possibleTradeScheduleNum % paging.Take != 0
				? possibleTradeScheduleNum / paging.Take + 1
				: possibleTradeScheduleNum / paging.Take;

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule,
				shiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount,
			};
		}

		private ShiftTradeScheduleViewModel getShiftTradeScheduleViewModel(Paging paging, DateOnly shiftTradeDate,
			TimeFilterInfo timeFilter, string timeSortOrder, DatePersons possibleTradePersons)
		{
			var ret = new ShiftTradeScheduleViewModel();
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(shiftTradeDate);
			var mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true, false);
			ret.MySchedule = mySchedule;
			var possibleTradeSchedule = new List<ShiftTradeAddPersonScheduleViewModel>();
			if (mySchedule == null || !mySchedule.IsFullDayAbsence)
			{
				possibleTradeSchedule = timeFilter == null
				? getPossibleTradeSchedules(possibleTradePersons, paging, timeSortOrder).ToList()
				: getFilteredTimesPossibleTradeSchedules(possibleTradePersons, paging, timeFilter, timeSortOrder).ToList();
			}

			possibleTradeSchedule =
				_shiftTradeSiteOpenHourFilter.FilterScheduleView(possibleTradeSchedule, mySchedule, possibleTradePersons).ToList();

			var possibleTradeScheduleNum = possibleTradeSchedule.Any()
				? possibleTradeSchedule.First().Total
				: 0;
			var pageCount = possibleTradeScheduleNum % paging.Take != 0
				? possibleTradeScheduleNum / paging.Take + 1
				: possibleTradeScheduleNum / paging.Take;

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule,
				shiftTradeDate);

			ret.PageCount = pageCount;
			ret.PossibleTradeSchedules = possibleTradeSchedule;
			ret.TimeLineHours = timeLineHours;

			return ret;
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedules(IEnumerable<IShiftExchangeOffer> offers, Paging paging)
		{
			var offerList = offers.ToList();
			if (offerList.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedules(offerList.Select(x => x.ShiftExchangeOfferId), paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons,
			Paging paging, string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons,
					paging, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getFilteredTimesPossibleTradeSchedules(
			DatePersons datePersons, Paging paging, TimeFilterInfo timeFilter, string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedulesWithFilteredTimes(datePersons.Date,
					datePersons.Persons, paging, timeFilter, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;

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
		private readonly IShiftTradeMultiSchedulesSelectableChecker _selectableChecker;
		private readonly INow _now;

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider,
			IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
			IShiftTradeAddPersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper,
			IPersonRequestRepository personRequestRepository,
			IScheduleProvider scheduleProvider,
			ILoggedOnUser loggedOnUser, IShiftTradeSiteOpenHourFilter shiftTradeSiteOpenHourFilter, IProjectionChangedEventBuilder builder, IPersonRepository personRepository, IPersonNameProvider personNameProvider, IShiftTradeAddScheduleLayerViewModelMapper layerMapper, IPermissionProvider permissionProvider, IShiftTradeMultiSchedulesSelectableChecker selectableChecker, INow now)
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
			_selectableChecker = selectableChecker;
			_now = now;
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

		private static IEnumerable<IVirtualSchedulePeriod> extractVirtualPeriods(IPerson person, DateOnlyPeriod period)
		{
			if (person == null)
				throw new ArgumentNullException(nameof(person));

			var virtualPeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var dateOnly in period.DayCollection())
			{
				virtualPeriods.Add(person.VirtualSchedulePeriod(dateOnly));
			}
			return virtualPeriods;
		}

		private IList<ContractTimeInfoViewModel> getContractInfos(IPerson person, IEnumerable<IVirtualSchedulePeriod> virtualSchedulePeriods, IScheduleDictionary allSchedules)
		{
			var shiftTradeTargetTimeFlexibility = person.WorkflowControlSet.ShiftTradeTargetTimeFlexibility.TotalMinutes;

			return (from schedulePeriod in virtualSchedulePeriods
				let contractNegativeWorkTimeTolerance = schedulePeriod.Contract?.NegativePeriodWorkTimeTolerance.TotalMinutes ?? 0
				let contracePositiveWorkTimeTolerance = schedulePeriod.Contract?.PositivePeriodWorkTimeTolerance.TotalMinutes ?? 0
				let actualContractTime = getCurrentWorkingContractTime(schedulePeriod.DateOnlyPeriod, person, allSchedules)
				let contractDiff = actualContractTime - getTotalContractTime(schedulePeriod)
				let negative = contractDiff < 0 ? -contractDiff : 0
				let positive = contractDiff > 0 ? contractDiff : 0
				select new ContractTimeInfoViewModel
				{
					PeriodStart = schedulePeriod.DateOnlyPeriod.StartDate.Date.ToString("yyyy-MM-dd"),
					PeriodEnd = schedulePeriod.DateOnlyPeriod.EndDate.Date.ToString("yyyy-MM-dd"),
					ContractTimeMinutes = actualContractTime,
					NegativeToleranceMinutes = contractNegativeWorkTimeTolerance + shiftTradeTargetTimeFlexibility,
					PositiveToleranceMinutes = contracePositiveWorkTimeTolerance + shiftTradeTargetTimeFlexibility,
					RealScheduleNegativeGap = negative,
					RealSchedulePositiveGap = positive
				}).ToList();
		}

		private double getTotalContractTime(IVirtualSchedulePeriod schedulePeriod)
		{
			var totalWorkDays = schedulePeriod.DateOnlyPeriod.DayCount() - schedulePeriod.DaysOff();
			var totalContractTime = schedulePeriod.AverageWorkTimePerDay.TotalMinutes * totalWorkDays;
			totalContractTime = totalContractTime - schedulePeriod.BalanceIn.TotalMinutes;
			totalContractTime = totalContractTime + schedulePeriod.BalanceOut.TotalMinutes;
			return totalContractTime;
		}

		private double getCurrentWorkingContractTime(DateOnlyPeriod period, IPerson person, IScheduleDictionary allSchedules)
		{
			var scheduleRange = allSchedules[person];
			var contractTime = TimeSpan.Zero;
			foreach (var date in period.DayCollection())
			{
				var projection = scheduleRange.ScheduledDay(date).ProjectionService().CreateProjection();
				contractTime += projection.ContractTime();
			}

			return contractTime.TotalMinutes;
		}

		public DateOnlyPeriod GetShiftTradeOpenPeriod(IPerson person)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var agentToday = _now.CurrentLocalDate(timeZone);
			var openPeriodStart = agentToday.AddDays(person.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum);
			var openPeriodEnd = agentToday.AddDays(person.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum);
			return new DateOnlyPeriod(openPeriodStart, openPeriodEnd);
		}

		public ShiftTradeToleranceInfoViewModel GetToleranceInfo(Guid personToId)
		{
			var personTo = _personRepository.Get(personToId);
			var currentUser = _loggedOnUser.CurrentUser();
			var myShiftTradeOpenPeriod = GetShiftTradeOpenPeriod(currentUser);
			var personToShiftTradeOpenPeriod = GetShiftTradeOpenPeriod(personTo);
			var myVirtualSchedulePeriods = extractVirtualPeriods(currentUser, myShiftTradeOpenPeriod).ToList();
			var personToVirtualSchedulePeriods = extractVirtualPeriods(personTo, personToShiftTradeOpenPeriod).ToList();

			var starts = myVirtualSchedulePeriods.Select(x => x.DateOnlyPeriod.StartDate).ToList();
			starts.AddRange(personToVirtualSchedulePeriods.Select(x=>x.DateOnlyPeriod.StartDate));
			var end = myVirtualSchedulePeriods.Select(x => x.DateOnlyPeriod.EndDate).ToList();
			end.AddRange(personToVirtualSchedulePeriods.Select(x=>x.DateOnlyPeriod.EndDate));
			var minStart = starts.Min().Date == DateTime.MinValue ? starts.Min().AddWeeks(52) : starts.Min();
			var maxEnd = end.Max().Date == DateTime.MinValue ? new DateOnly(DateTime.MaxValue.AddYears(-1)) : end.Max();
			var period = new DateOnlyPeriod(minStart, maxEnd);
			var allSchedules = _shiftTradeRequestProvider.RetrieveTradeMultiSchedules(period, new List<IPerson> { currentUser, personTo });

			return new ShiftTradeToleranceInfoViewModel
			{
				IsNeedToCheck = _selectableChecker.IsNeedCheckTolerance(),
				MyInfos = getContractInfos(currentUser, myVirtualSchedulePeriods, allSchedules),
				PersonToInfos = getContractInfos(personTo, personToVirtualSchedulePeriods, allSchedules)
			};
		}

		public ShiftTradeMultiSchedulesViewModel GetMeAndPersonToSchedules(DateOnlyPeriod period, Guid personToId)
		{
			var viewModel = new ShiftTradeMultiSchedulesViewModel
			{
				MultiSchedulesForShiftTrade = new List<ShiftTradeMultiScheduleViewModel>()
			};
			var fixedPeriod = fixPeriodForUnpublishedSchedule(period);
			if (fixedPeriod == null) return viewModel;

			var personTo = _personRepository.Get(personToId);
			var currentUser = _loggedOnUser.CurrentUser();
			var allSchedules = _shiftTradeRequestProvider.RetrieveTradeMultiSchedules(period, new List<IPerson> {currentUser, personTo });
			if (allSchedules == null) return viewModel;

			allSchedules.TryGetValue(currentUser, out var myScheduleRange);
			allSchedules.TryGetValue(personTo, out var personToScheduleRange);
			var multiSchedules = new List<ShiftTradeMultiScheduleViewModel>();
			foreach (var dateOnly in fixedPeriod.Value.DayCollection())
			{
				var mySchedule = mapToShiftTradeAddPersonScheduleViewModel(myScheduleRange, dateOnly, currentUser);
				var personToSchedule = mapToShiftTradeAddPersonScheduleViewModel(personToScheduleRange, dateOnly, personTo);
				var isSelectable = _selectableChecker.CheckSelectable(hasAbsence(mySchedule, personToSchedule), 
					myScheduleRange?.ScheduledDay(dateOnly), personToScheduleRange?.ScheduledDay(dateOnly), dateOnly, personTo, out var reason);
				multiSchedules.Add(new ShiftTradeMultiScheduleViewModel
				{
					Date = dateOnly.Date.ToString("yyyy-MM-dd"), MySchedule = mySchedule, PersonToSchedule = personToSchedule,
					IsSelectable = isSelectable, UnselectableReason = reason
				});
			}

			return new ShiftTradeMultiSchedulesViewModel
			{
				MultiSchedulesForShiftTrade = multiSchedules
			};
		}

		private bool hasAbsence(ShiftTradeAddPersonScheduleViewModel myViewModel, ShiftTradeAddPersonScheduleViewModel personToViewModle)
		{
			if (myViewModel == null)
			{
				if (personToViewModle == null) return false;
				return personToViewModle.IsIntradayAbsence || personToViewModle.IsFullDayAbsence;
			}

			if (personToViewModle == null) return myViewModel.IsFullDayAbsence || myViewModel.IsIntradayAbsence;

			return myViewModel.IsFullDayAbsence || myViewModel.IsIntradayAbsence|| personToViewModle.IsFullDayAbsence || personToViewModle.IsIntradayAbsence;
		}

		private DateOnlyPeriod? fixPeriodForUnpublishedSchedule(DateOnlyPeriod periodInput)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var dateTimePeriod = periodInput.ToDateTimePeriod(currentUser.PermissionInformation.DefaultTimeZone());
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)) return periodInput;

			var publishedToDate = currentUser.WorkflowControlSet.SchedulePublishedToDate;
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
			var shiftCategory = scheduleDay.PersonAssignment()?.ShiftCategory;
			var isDayOff = eventScheduleDay.DayOff != null;
			var isFulldayAbsence = eventScheduleDay.IsFullDayAbsence;
			var categoryName = shiftCategory?.Description.Name;
			var shortName = shiftCategory?.Description.ShortName;
			string displayColor = null;
			if (shiftCategory != null) displayColor = mapColor(shiftCategory.DisplayColor.ToArgb());
			if (isFulldayAbsence)
			{
				var payload = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload;
				categoryName = payload.ConfidentialDescription(person).Name;
				var absenceColor = payload.ConfidentialDisplayColor(person);
				displayColor = mapColor(absenceColor.ToArgb());
				shortName = payload.Description.ShortName;
			}
			var isIntradayAbsence = !isFulldayAbsence && scheduleDay.PersonAbsenceCollection().Any();
			var intradayCategory = new IntradayAbsenceCategoryViewModel();
			if (isIntradayAbsence)
			{
				var payload = scheduleDay.PersonAbsenceCollection().First().Layer.Payload;
				intradayCategory.ShortName = payload.ConfidentialDescription(person).ShortName;
				intradayCategory.Color = mapColor(payload.ConfidentialDisplayColor(person).ToArgb());
			}

			return new ShiftTradeAddPersonScheduleViewModel
			{
				ContractTimeInMinute = eventScheduleDay.ContractTime.TotalMinutes,
				DayOffName = eventScheduleDay.Name,
				IsDayOff = isDayOff,
				IsFullDayAbsence = isFulldayAbsence,
				IsNotScheduled = eventScheduleDay.Shift == null && !isDayOff && !isFulldayAbsence,
				MinStart = date.Date,
				Name = _personNameProvider.BuildNameFromSetting(scheduleDay.Person.Name.FirstName, scheduleDay.Person.Name.LastName),
				PersonId = scheduleDay.Person.Id.GetValueOrDefault(),
				ScheduleLayers = getScheduleLayers(eventScheduleDay, scheduleDay.PersonAssignment(), person.Id == _loggedOnUser.CurrentUser().Id),
				ShiftExchangeOfferId = null,
				ShiftCategory = new ShiftCategoryViewModel { Name = categoryName, ShortName = shortName, DisplayColor = displayColor},
				IsIntradayAbsence = isIntradayAbsence,
				IntradayAbsenceCategory = intradayCategory
			};
		}

		private string mapColor(int argb)
		{
			return ColorTranslator.ToHtml(Color.FromArgb(argb));
		}

		private IList<SimpleLayer> mapLayers(ProjectionChangedEventScheduleDay eventScheduleDay)
		{
			var layers = new List<SimpleLayer>();
			if (eventScheduleDay.Shift == null) return layers;

			var ls = from layer in eventScheduleDay.Shift.Layers
				select new SimpleLayer
				{
					Color = mapColor(layer.DisplayColor),
					Description = layer.Name,
					Start = layer.StartDateTime,
					End = layer.EndDateTime,
					Minutes = (int)layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes,
					IsAbsenceConfidential = layer.IsAbsenceConfidential
				};

			layers.AddRange(ls);
			return layers;
		}

		private TeamScheduleLayerViewModel[] getScheduleLayers( ProjectionChangedEventScheduleDay eventScheduleDay, IPersonAssignment personAssignment, bool isMySchedule)
		{
			var layers = mapLayers(eventScheduleDay);
			return _layerMapper.Map(layers, personAssignment?.OvertimeActivities(), isMySchedule);
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

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule, shiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount
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
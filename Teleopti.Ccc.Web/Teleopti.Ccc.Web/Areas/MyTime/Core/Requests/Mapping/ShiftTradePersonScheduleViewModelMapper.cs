using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePersonScheduleViewModelMapper : IShiftTradePersonScheduleViewModelMapper
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradePersonScheduleProvider _personScheduleProvider;
		private readonly TeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeSiteOpenHourFilter _shiftTradeSiteOpenHourFilter;
		private readonly IProjectionProvider _projectionProvider;

		public ShiftTradePersonScheduleViewModelMapper(IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser
			, IShiftTradePersonScheduleProvider personScheduleProvider, TeamScheduleShiftViewModelProvider shiftViewModelProvider
			, IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider, IPersonRequestRepository personRequestRepository
			, IShiftTradeSiteOpenHourFilter shiftTradeSiteOpenHourFilter, IProjectionProvider projectionProvider)
		{
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_personScheduleProvider = personScheduleProvider;
			_shiftViewModelProvider = shiftViewModelProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_personRequestRepository = personRequestRepository;
			_shiftTradeSiteOpenHourFilter = shiftTradeSiteOpenHourFilter;
			_projectionProvider = projectionProvider;
		}

		public ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(inputData.ShiftTradeDate,
				currentUser) || _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
				? _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] { currentUser }).SingleOrDefault()
				: null;
			var myScheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, currentUser, myScheduleDay, true);
			return new ShiftTradeAddPersonScheduleViewModel(myScheduleViewModel);
		}

		public IList<ShiftTradeAddPersonScheduleViewModel> MakePossibleShiftTradeAddPersonScheduleViewModels(
			ShiftTradeScheduleViewModelData inputData, ShiftTradeAddPersonScheduleViewModel myScheduleView, out int pageCount)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var myScheduleDay = _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new List<IPerson>
				{currentUser}).FirstOrDefault();


			var shiftTradeRequests = _personRequestRepository.FindShiftExchangeOffersForBulletin(inputData.ShiftTradeDate).Where(x => x.IsWantedSchedule(myScheduleDay));

			var ids = new HashSet<Guid>();
			var shiftExchangeOffers = shiftTradeRequests as IShiftExchangeOffer[] ?? shiftTradeRequests.ToArray();
			foreach (var request in shiftExchangeOffers)
			{
				if (request.Person.Id != null) ids.Add(request.Person.Id.Value);
			}

			var personDictionary = _possibleShiftTradePersonsProvider.RetrievePersons(inputData, ids.ToArray())
							.Persons.Distinct()
							.ToDictionary(p => p.Id);

			var allPossibleScheduleDayTuples = shiftExchangeOffers.Select(
				req =>
				{
					var person = req.Person;
					if (!personDictionary.ContainsKey(person.Id))
					{
						return null;
					}
					var scheduleDay =
						_personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] { person }).SingleOrDefault();
					if (scheduleDay.IsFullDayAbsence() ||
						_shiftViewModelProvider.IsOvertimeOnDayOff(scheduleDay))
					{
						return null;
					}
					var tupleValue = new Tuple<IScheduleDay, IShiftExchangeOffer>(scheduleDay, req);
					return _shiftTradeSiteOpenHourFilter.FilterSchedule(scheduleDay, myScheduleView) ? tupleValue : null;
				}).Where(scheduleDay => scheduleDay != null)
				.OrderBy(scheduleDayDateTimeOrder).ToList();

			var allPossibleExchangedScheduleViews = new List<ShiftTradeAddPersonScheduleViewModel>();
			foreach (var tuple in allPossibleScheduleDayTuples)
			{
				var agent = tuple.Item2.Person;
				var scheduleVm = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, agent, tuple.Item1, true);

				// Agent may create multiple shift trade post for same day, only return first one
				if (allPossibleExchangedScheduleViews.Any(x => x.PersonId == agent.Id && x.StartTimeUtc == scheduleVm.StartTimeUtc))
				{
					continue;
				}

				allPossibleExchangedScheduleViews.Add(new ShiftTradeAddPersonScheduleViewModel(scheduleVm)
				{
					ShiftExchangeOfferId = new Guid(tuple.Item2.ShiftExchangeOfferId)
				});
			}

			pageCount = (int)Math.Ceiling((double)allPossibleExchangedScheduleViews.Count / inputData.Paging.Take);
			return allPossibleExchangedScheduleViews.Skip(inputData.Paging.Skip).Take(inputData.Paging.Take).ToList();
		}

		private DateTime scheduleDayDateTimeOrder(Tuple<IScheduleDay, IShiftExchangeOffer> tuple)
		{
			if (tuple.Item1 == null)
				return default(DateTime);

			var projection = _projectionProvider.Projection(tuple.Item1);
			var period = projection.Period();
			if (!period.HasValue)
				return default(DateTime);

			return period.Value.StartDateTime;
		}
	}

	public interface IShiftTradePersonScheduleViewModelMapper
	{
		ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData);

		IList<ShiftTradeAddPersonScheduleViewModel> MakePossibleShiftTradeAddPersonScheduleViewModels(
			ShiftTradeScheduleViewModelData inputData, ShiftTradeAddPersonScheduleViewModel myScheduleView, out int pageCount);
	}
}
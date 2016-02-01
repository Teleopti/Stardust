using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePersonScheduleViewModelMapper : IShiftTradePersonScheduleViewModelMapper
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradePersonScheduleProvider _personScheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IPersonRequestRepository _personRequestRepository;
		

		public ShiftTradePersonScheduleViewModelMapper(IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser, IShiftTradePersonScheduleProvider personScheduleProvider, ITeamScheduleProjectionProvider projectionProvider, IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider, IPersonRequestRepository personRequestRepository)
		{
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_personScheduleProvider = personScheduleProvider;
			_projectionProvider = projectionProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_personRequestRepository = personRequestRepository;
		}

		public ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(inputData.ShiftTradeDate,
				_loggedOnUser.CurrentUser())
				? _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] { _loggedOnUser.CurrentUser() }).SingleOrDefault()
				: null;
			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(_loggedOnUser.CurrentUser(), myScheduleDay, true);
			return new ShiftTradeAddPersonScheduleViewModel(myScheduleViewModel);
		}

		public IList<ShiftTradeAddPersonScheduleViewModel> MakePossibleShiftTradeAddPersonScheduleViewModels(ShiftTradeScheduleViewModelData inputData, out int pageCount)
		{
			var myScheduleDay =
				_personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new List<IPerson> { _loggedOnUser.CurrentUser() })
					.FirstOrDefault();

			var persons = _possibleShiftTradePersonsProvider.RetrievePersons(inputData);
			var shiftTradeRequests = _personRequestRepository.FindShiftExchangeOffersForBulletin(persons.Persons, inputData.ShiftTradeDate)
				.Where(x => x.IsWantedSchedule(myScheduleDay));

			var possibleExchangedSchedules = shiftTradeRequests.Select(
				req =>
				{
					var person = req.Person;
					var scheduleDay =
						_personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] {person}).SingleOrDefault();
					var shiftExchangeOfferId = req.ShiftExchangeOfferId;
					return new ShiftTradeAddPersonScheduleViewModel(_projectionProvider.MakeScheduleReadModel(person, scheduleDay, true))
						{
							ShiftExchangeOfferId = new Guid(shiftExchangeOfferId)
						};

				}).Where(vm => vm != null && !vm.IsFullDayAbsence);
			pageCount = (int)Math.Ceiling((double)possibleExchangedSchedules.Count()/inputData.Paging.Take);

			return possibleExchangedSchedules.OrderBy(vm => vm.StartTimeUtc).Skip(inputData.Paging.Skip).Take(inputData.Paging.Take).ToList();
		}
	}

	public interface IShiftTradePersonScheduleViewModelMapper
	{
		ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData);

		IList<ShiftTradeAddPersonScheduleViewModel> MakePossibleShiftTradeAddPersonScheduleViewModels(
			ShiftTradeScheduleViewModelData inputData, out int pageCount);
	}
}
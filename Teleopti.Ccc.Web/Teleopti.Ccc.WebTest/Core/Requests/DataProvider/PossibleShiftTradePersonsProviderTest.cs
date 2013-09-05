using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class PossibleShiftTradePersonsProviderTest
	{
		private IPossibleShiftTradePersonsProvider target;
		private IShiftTradeLightValidator shiftTradeValidator;
		private IPersonRepository personRepository;
		private IPermissionProvider permissionProvider;
		private IPerson currentUser;

		[SetUp]
		public void Setup()
		{
			currentUser = new Person();
			shiftTradeValidator = MockRepository.GenerateMock<IShiftTradeLightValidator>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(l => loggedOnUser.CurrentUser()).Return(currentUser);

			target = new PossibleShiftTradePersonsProvider(personRepository, shiftTradeValidator, loggedOnUser, permissionProvider);
		}

		[Test]
		public void ShouldReturnPossiblePersonsToShiftTradeWith()
		{
			var validAgent = new Person();
			var invalidAgent = new Person();
			var date = new DateOnly(2000, 1, 1);
			var data = new ShiftTradeScheduleViewModelData {ShiftTradeDate = date};

			personRepository.Expect(rep => rep.FindPossibleShiftTrades(currentUser, data.LoadOnlyMyTeam, data.ShiftTradeDate))
			                .Return(new List<IPerson> {validAgent, invalidAgent});
			permissionProvider.Expect(c => c.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, date, validAgent)).Return(true);
			permissionProvider.Expect(c => c.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, date, invalidAgent)).Return(true);

			var resultPersonValid = new ShiftTradeAvailableCheckItem(date, currentUser, validAgent);
			var resultPersonInvalid = new ShiftTradeAvailableCheckItem(date, currentUser, invalidAgent);

			shiftTradeValidator.Expect(val => val.Validate(resultPersonInvalid)).Return(new ShiftTradeRequestValidationResult(false));
			shiftTradeValidator.Expect(val => val.Validate(resultPersonValid)).Return(new ShiftTradeRequestValidationResult(true));

			target.RetrievePersons(data).Should().Have.SameValuesAs(validAgent);
		}

		[Test]
		public void ShouldFilterPersonsWithNoPermissionTo()
		{
			var validAgent = new Person();
			var invalidAgent = new Person();
			var date = new DateOnly(2000, 1, 1);
			var data = new ShiftTradeScheduleViewModelData {ShiftTradeDate = date};

			personRepository.Expect(rep => rep.FindPossibleShiftTrades(currentUser, data.LoadOnlyMyTeam, data.ShiftTradeDate))
			                .Return(new List<IPerson> {validAgent, invalidAgent});
			permissionProvider.Expect(c => c.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, date, validAgent)).Return(true);
			permissionProvider.Expect(c => c.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, date, invalidAgent)).Return(false);

			var resultPersonValid = new ShiftTradeAvailableCheckItem(date, currentUser, validAgent);
			var resultPersonInvalid = new ShiftTradeAvailableCheckItem(date, currentUser, invalidAgent);

			shiftTradeValidator.Expect(val => val.Validate(resultPersonInvalid)).Return(new ShiftTradeRequestValidationResult(true));
			shiftTradeValidator.Expect(val => val.Validate(resultPersonValid)).Return(new ShiftTradeRequestValidationResult(true));

			target.RetrievePersons(data).Should().Have.SameValuesAs(validAgent);
		}
	}
}
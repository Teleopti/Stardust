using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
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
		private IPerson currentUser;

		[SetUp]
		public void Setup()
		{
			currentUser = new Person();
			shiftTradeValidator = MockRepository.GenerateMock<IShiftTradeLightValidator>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(l => loggedOnUser.CurrentUser()).Return(currentUser);

			target = new PossibleShiftTradePersonsProvider(personRepository, shiftTradeValidator, loggedOnUser);
		}

		[Test]
		public void ShouldReturnPossiblePersonsToShiftTradeWith()
		{
			var validAgent = new Person();
			var invalidAgent = new Person();
			var date = new DateOnly(2000, 1, 1);
			personRepository.Expect(rep => rep.LoadAll()).Return(new List<IPerson>{validAgent, invalidAgent});

			var resultPersonValid = new ShiftTradeAvailableCheckItem {DateOnly = date, PersonFrom = currentUser, PersonTo = validAgent};
			var resultPersonInvalid = new ShiftTradeAvailableCheckItem {DateOnly = date, PersonFrom = currentUser, PersonTo = invalidAgent};

			shiftTradeValidator.Expect(val => val.Validate(resultPersonInvalid)).Return(new ShiftTradeRequestValidationResult(false));
			shiftTradeValidator.Expect(val => val.Validate(resultPersonValid)).Return(new ShiftTradeRequestValidationResult(true));

			target.RetrievePersons(date).Should().Have.SameValuesAs(validAgent);
		}
	}
}
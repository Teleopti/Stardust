using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	public class ShiftTradeRequestHandlerNoMockTest
	{
		private ShiftTradeRequestHandler _target;
		private IPersonRequestRepository _personRequestRepository;
		private ICurrentScenario _scenarioRepository;
		private ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IRequestFactory _requestFactory;
		private IShiftTradeValidator _validator;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonAssignment _personAssignment;
		private IBusinessRuleProvider _businessRuleProvider;
		private INewBusinessRuleCollection _businessRuleCollection;


		[SetUp]
		public void Setup()
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_scenarioRepository = new FakeCurrentScenario();
			_schedulingResultStateHolder = new FakeSchedulingResultStateHolder();
			_personRepository = new FakePersonRepository();
			_requestFactory = new FakeRequestFactory();
			_scheduleStorage = new FakeScheduleStorage();
			_businessRuleProvider = new FakeBusinessRuleProvider();
			_businessRuleCollection = new FakeNewBusinessRuleCollection();
			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeValidatorTest.ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification")
			};
			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), shiftTradeSpecifications);
		}

		[Test]
		public void ShouldGetAndHandleBrokenBusinessRules()
		{
			var personTo = PersonFactory.CreatePersonWithId();
			var personFrom = PersonFactory.CreatePersonWithId();
			setPersonAssignment(personTo);
			var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
			var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));

			prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);

			var acceptShiftTradeEvent = getAcceptShiftTradeEvent(personTo);

			_personRepository.Add(personTo);
			var personRequest = prepareAndGetPersonRequest(personFrom, personTo, acceptShiftTradeEvent.PersonRequestId);
		    var approvalService = new ApprovalServiceForTest();
		    approvalService.SetBusinessRuleResponse(ruleResponse1, ruleResponse2);
            ((FakeRequestFactory) _requestFactory).setRequestApprovalService(approvalService);
            _target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
			 _scenarioRepository, _personRequestRepository, _scheduleStorage, _personRepository
			  , null, null,
			 _loadSchedulingDataForRequestWithoutResourceCalculation, null, null, _businessRuleProvider);
			_target.Handle(acceptShiftTradeEvent);
			Assert.IsTrue((personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.MinWeeklyRestRule)));
			Assert.IsTrue((personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule)));

		}
		private IPersonRequest prepareAndGetPersonRequest(IPerson personFrom, IPerson personTo, Guid id)
		{
			var personRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, DateOnly.Today);
			personRequest.SetId(id);
			_personRequestRepository.Add(personRequest);
			return personRequest;
		}
		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			((FakeNewBusinessRuleCollection)_businessRuleCollection).SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(_businessRuleCollection);

		}
		private void setPersonAssignment(IPerson person)
		{
			_personAssignment = new PersonAssignment(person, _scenarioRepository.Current(), DateOnly.Today);
			_loadSchedulingDataForRequestWithoutResourceCalculation
			 = new LoadSchedulesForRequestWithoutResourceCalculation(new FakePersonAbsenceAccountRepository(),
			  new FakePersonAssignmentReadScheduleStorage(_personAssignment));
		}
		private static AcceptShiftTradeEvent getAcceptShiftTradeEvent(IPerson personTo)
		{
			var ast = new AcceptShiftTradeEvent();
			ast.LogOnDatasource = "V7Config";
			ast.LogOnBusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA");
			ast.Timestamp = DateTime.UtcNow;
			ast.PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585");
			ast.Message = "I want to trade!";
			ast.AcceptingPersonId = personTo.Id.GetValueOrDefault();
			return ast;
		}
	}
}

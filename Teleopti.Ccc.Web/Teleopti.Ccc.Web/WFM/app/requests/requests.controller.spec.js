'use strict';
describe('RequestsControllerTests', function () {
	var $rootScope,
		$controller,
		requestCommandParamsHolder,
		$q,
		requestsDataService,
		toggleObject = {
			Wfm_Requests_People_Search_36294: true,
		};

	var absenceRequestTabIndex = 0;
	var shiftTradeRequestTabIndex = 1;

	beforeEach(function() {
		module('wfm.groupPage');
		module('wfm.requests');

		requestCommandParamsHolder = new fakeRequestCommandParamsHolder();
		requestsDataService = new FakeRequestsDataService();

		module(function($provide) {
			$provide.service('Toggle', function() {
				toggleObject.togglesLoaded = $q(function(resolve, reject) {
					resolve();
				});
				return toggleObject;
			});
			$provide.service('requestCommandParamsHolder', function() {
				return requestCommandParamsHolder;
			});
			$provide.service('requestsDataService', function() {
				return requestsDataService;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$controller_, _$q_) {
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		$q = _$q_;
	}));

	function setUpTarget() {
		var scope = $rootScope.$new();

		var target = $controller('requestsOriginController', {
			$scope: scope
		});

		return { target: target, scope: scope };
	}

	function fakeRequestCommandParamsHolder() {
		var requestIds = [];
		this.setSelectedRequestsIds = function (ids) {
			requestIds = ids;
		};
		this.getSelectedRequestsIds = function () {
			return requestIds;
		};
		this.resetSelectedRequestIds =function(){
			requestIds = [];
		};
	}

	function FakeRequestsDataService() {
		this.hierarchy = function () {
			return $q(function (resolve) {
				resolve({ Children: [] });
			});
		};
	}

	xit('should keep different date range', function () {
		var test = setUpTarget();
		test.scope.$digest();

		var periodForAbsenceRequest = {
			startDate: new Date(2016, 1 - 1, 1, 0, 0, 0, 0),
			endDate: new Date(2016, 4 - 1, 1, 0, 0, 0, 0)
		};

		var periodForShiftTradeRequest = {
			startDate: new Date(2016, 5 - 1, 1, 0, 0, 0, 0),
			endDate: new Date(2016, 6 - 1, 1, 0, 0, 0, 0)
		};

		var controller = test.target;
		controller.selectedTabIndex = absenceRequestTabIndex;
		test.scope.$digest();
		controller.absencePeriod = periodForAbsenceRequest;

		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		test.scope.$digest();
		controller.shiftTradePeriod = periodForShiftTradeRequest;

		controller.selectedTabIndex = absenceRequestTabIndex;
		test.scope.$digest();
		expect(controller.period).toEqual(periodForAbsenceRequest);

		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		test.scope.$digest();
		expect(controller.period).toEqual(periodForShiftTradeRequest);
	});

	it('should active search status after selected teams changed', function(){
		var target = setUpTarget().target;

		target.agentSearchOptions = {
			focusingSearch: false
		};

		target.changeSelectedTeams(['fakeTeamId']);

		expect(target.agentSearchOptions.focusingSearch).toEqual(true);
	});

	it('should deactive search status after applying favorite', function(){
		var target = setUpTarget().target;

		target.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false,
				focusingSearch: true,
				searchFields: [
					'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
					'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
				]
			};

		target.applyFavorite({
			Name: 'fakeFavorite',
			SearchTerm: 'a',
			TeamIds: ['fakeTeam1Id']
		});

		expect(target.agentSearchOptions.focusingSearch).toEqual(false);
	});

	it('should deactive search status after search term changed and enter pressed', function(){
		var target = setUpTarget().target;

		target.agentSearchOptions = {
			focusingSearch: true
		};

		target.onSearchTermChangedCallback();

		expect(target.agentSearchOptions.focusingSearch).toEqual(false);
	});

	it('should deactive search status after period changed', function(){
		var test = setUpTarget();
		var target = test.target;

		target.agentSearchOptions = {
			focusingSearch: true
		};

		target.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};

		test.scope.$digest();

		expect(target.agentSearchOptions.focusingSearch).toEqual(false);
	});

	it('should clear the selection after selected period changed', function(){
		var test = setUpTarget();
		var target = test.target;

		requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

		target.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};

		test.scope.$digest();
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
	});

	it('should clear the selection after search term changed and enter press', function(){
		var target = setUpTarget().target;

		target.agentSearchOptions = {
			focusingSearch: false
		};

		requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

		target.onSearchTermChangedCallback();
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
	});

	it('should clear the selection after selected teams changed', function(){
		var target = setUpTarget().target;

		target.agentSearchOptions = {
			focusingSearch: false
		};

		requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

		target.changeSelectedTeams(['fakeTeamId']);
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
	});

	it('should clear the selection after applying favorite', function(){
		var target = setUpTarget().target;

		requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

		target.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false,
				searchFields: [
					'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBags',
					'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
				]
			};

		target.applyFavorite({
			Name: 'fakeFavorite',
			SearchTerm: 'a',
			TeamIds: ['fakeTeam1Id']
		});
		expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
	});
});
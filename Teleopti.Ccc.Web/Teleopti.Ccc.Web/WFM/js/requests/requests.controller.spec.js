'use strict';
describe('RequestsControllerTests', function () {
	var $rootScope,
		$controller,
		requestCommandParamsHolder;

	var absenceRequestTabIndex = 0;
	var shiftTradeRequestTabIndex = 1;

	beforeEach(function () {
		module('wfm.requests');

		requestCommandParamsHolder = new fakeRequestCommandParamsHolder();

		module(function ($provide) {
			$provide.service('Toggle', function() {
				return {
					Wfm_Requests_Basic_35986: true,
					Wfm_Requests_People_Search_36294: true,
					Wfm_Requests_Performance_36295: true,
					Wfm_Requests_ApproveDeny_36297: true,
					Wfm_Requests_ApproveDeny_ShiftTrade_38494: true,
					togglesLoaded: {
						then: function(cb) { cb(); }
					}
				}
			});
			$provide.service('requestCommandParamsHolder', function () {
				return requestCommandParamsHolder;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$controller_) {
		$rootScope = _$rootScope_;
		$controller = _$controller_;
	}));
	
	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('RequestsCtrl', {
			$scope: scope
		});
		return { target: target, scope: scope };
	}

	function fakeRequestCommandParamsHolder() {
		var requestIds;
		this.setSelectedRequestsIds = function (ids) {
			requestIds = ids;
		}
		this.getSelectedRequestsIds = function () {
			return requestIds;
		}
	}

	it('should display approve deny command', function () {
		var test = setUpTarget();
		test.scope.$digest();

		test.target.selectedTabIndex = 0;
		expect(test.target.canApproveOrDenyRequest()).toEqual(true);

		test.target.selectedTabIndex = 1;
		expect(test.target.canApproveOrDenyRequest()).toEqual(true);
	});

	it('should keep different date range', function () {
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
		controller.period = periodForAbsenceRequest;

		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		test.scope.$digest();
		controller.period = periodForShiftTradeRequest;

		controller.selectedTabIndex = absenceRequestTabIndex;
		test.scope.$digest();
		expect(controller.period).toEqual(periodForAbsenceRequest);

		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		test.scope.$digest();
		expect(controller.period).toEqual(periodForShiftTradeRequest);
	});

	it('not allow date range longer than 60 days for shift trade request', function () {
		var test = setUpTarget();
		test.scope.$digest();

		var startDate = moment();
		var defaultPeriod = {
			startDate: startDate.toDate(),
			endDate: startDate.add(90, 'day').toDate()
		};

		var periodLongerThen60Days = {
			startDate: startDate.toDate(),
			endDate: startDate.add(61, 'day').toDate()
		};

		var controller = test.target;
		controller.selectedTabIndex = absenceRequestTabIndex;
		controller.period = defaultPeriod;
		test.scope.$digest();

		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		controller.period = defaultPeriod;
		test.scope.$digest();
		
		controller.selectedTabIndex = absenceRequestTabIndex;
		controller.period = periodLongerThen60Days;
		test.scope.$digest();
		expect(controller.period).toEqual(periodLongerThen60Days);
		
		controller.selectedTabIndex = shiftTradeRequestTabIndex;
		controller.period = periodLongerThen60Days;
		test.scope.$digest();
		expect(controller.period).toEqual(defaultPeriod);
	});
});

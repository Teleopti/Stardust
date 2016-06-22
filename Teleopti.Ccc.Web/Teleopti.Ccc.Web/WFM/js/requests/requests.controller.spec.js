'use strict';
describe('RequestsControllerTests', function () {
	var $rootScope,
		$controller,
		requestCommandParamsHolder;

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
});

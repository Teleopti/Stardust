'use strict';
describe('Requests - Refactor(remove later) controller controllers',
	function () {

		var $rootScope,
			$controller,
			$q,
			$httpBackend,
			requestsDataService,
			requestCommandParamsHolder,
			fakeStateParams,
			fakeState = {
				current: {
					url: ''
				},
				go: function (url, params) {
					this.current.url = url;
					fakeStateParams = params;
				}
			};

		beforeEach(function () {
			module('wfm.requests');

			requestsDataService = new fakeRequestsDataService();
			requestCommandParamsHolder = new fakeRequestCommandParamsHolder();

			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_Basic_35986: true,
						Wfm_Requests_People_Search_36294: true,
						Wfm_Requests_Performance_36295: true,
						Wfm_Requests_ApproveDeny_36297: true,
						Wfm_Requests_ApproveDeny_ShiftTrade_38494: true,
						Wfm_Requests_SaveFavoriteSearches_42578: true,
						Wfm_Requests_Filtering_37748: true,
						togglesLoaded: $q(function (resolve, reject) {
							resolve();
						})
					}
				});
				$provide.service('requestsDataService', function () { return requestsDataService; });
				$provide.service('requestCommandParamsHolder', function () { return requestCommandParamsHolder; });
			});

		});

		beforeEach(inject(function (_$rootScope_, _$controller_, _$q_, _$httpBackend_) {
			$rootScope = _$rootScope_;
			$controller = _$controller_;
			$q = _$q_;
			$httpBackend = _$httpBackend_;

			$httpBackend.whenGET('app/requests/html/requests.refactor.html').respond(function () {
				return [200, true];
			});
		}));

		it('should display approve deny command', function () {
			var controller = setUpTarget();
			controller.scope.$digest();

			controller.controller.selectedTabIndex = 0;
			expect(controller.controller.canApproveOrDenyRequest()).toEqual(true);

			controller.controller.selectedTabIndex = 1;
			expect(controller.controller.canApproveOrDenyRequest()).toEqual(true);
		});

		it('should keep different date range', function () {
			var controller = setUpTarget();
			controller.scope.$digest();

			var periodForAbsenceRequest = {
				startDate: new Date(2016, 1 - 1, 1, 0, 0, 0, 0),
				endDate: new Date(2016, 4 - 1, 1, 0, 0, 0, 0)
			};

			var periodForShiftTradeRequest = {
				startDate: new Date(2016, 5 - 1, 1, 0, 0, 0, 0),
				endDate: new Date(2016, 6 - 1, 1, 0, 0, 0, 0)
			};

			var controller = controller.controller;
			controller.absencePeriod = periodForAbsenceRequest;
			controller.activeAbsenceAndTextTab();

			controller.shiftTradePeriod = periodForShiftTradeRequest;
			controller.activeShiftTradeTab();

			controller.activeAbsenceAndTextTab();
			expect(controller.period).toEqual(periodForAbsenceRequest);

			controller.activeShiftTradeTab();
			expect(controller.period).toEqual(periodForShiftTradeRequest);
		});

		it('should active search status after selected teams changed', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: false
			};

			controller.changeSelectedTeams(['fakeTeamId']);

			expect(controller.agentSearchOptions.focusingSearch).toEqual(true);
		});

		it('should deactive search status after applying favorite', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false,
				focusingSearch: true,
				searchFields: [
					'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
					'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
				]
			};

			controller.applyFavorite({
				Name: 'fakeFavorite',
				SearchTerm: 'a',
				TeamIds: ['fakeTeam1Id']
			});

			expect(controller.agentSearchOptions.focusingSearch).toEqual(false);
		});

		it('should deactive search status after search term changed and enter pressed', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: true
			};

			controller.onSearchTermChangedCallback();

			expect(controller.agentSearchOptions.focusingSearch).toEqual(false);
		});

		it('should deactive search status after period changed', function () {
			var target = setUpTarget();
			var controller = target.controller;

			controller.agentSearchOptions = {
				focusingSearch: true
			};

			controller.period = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			controller.onFavoriteSearchInitDefer.resolve();
			target.scope.$digest();

			expect(controller.agentSearchOptions.focusingSearch).toEqual(false);
		});

		it('should clear the selection after selected period changed', function () {
			var target = setUpTarget();
			var controller = target.controller;

			requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);
			controller.onFavoriteSearchInitDefer.resolve();

			controller.period = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			
			target.scope.$digest();
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
		});

		it('should clear the selection after search term changed and enter press', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: false
			};

			requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

			controller.onSearchTermChangedCallback();
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
		});

		it('should broadcast reload.requests.with.selection when search term changed', function () {
			var target = setUpTarget();
			var controller = target.controller;
			var eventData;

			target.scope.$on('reload.requests.with.selection', function (event, data) {
				eventData = data;
			});

			controller.selectedGroups.groupIds.push('123');
			controller.agentSearchOptions.keyword = 'FirstName:aaa';
			controller.onSearchTermChangedCallback();

			expect(eventData.selectedGroupIds[0]).toEqual('123');
			expect(eventData.agentSearchTerm).toEqual('FirstName:aaa');
		});

		it('should clear the selection after selected teams changed', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: false
			};

			requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

			controller.changeSelectedTeams(['fakeTeamId']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
		});

		it('should clear the selection after applying favorite', function () {
			var controller = setUpTarget().controller;

			requestCommandParamsHolder.setSelectedRequestsIds(['selectedIds']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);

			controller.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false,
				searchFields: [
					'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBags',
					'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
				]
			};

			controller.applyFavorite({
				Name: 'fakeFavorite',
				SearchTerm: 'a',
				TeamIds: ['fakeTeam1Id']
			});
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
		});

		it('should get correct url when go to absence and text requests tab', function () {
			var controller = setUpTarget().controller;
			controller.activeAbsenceAndTextTab();
			expect(fakeStateParams != undefined).toEqual(true);
			expect(fakeState.current.url).toEqual('requestsRefactor-absenceAndText');
		});

		it('should get correct url when go to shift trade requests tab', function () {
			var controller = setUpTarget().controller;
			controller.activeShiftTradeTab();
			expect(fakeStateParams != undefined).toEqual(true);
			expect(fakeState.current.url).toEqual('requestsRefactor-shiftTrade');
		});

		it('should pass correct params to absence and text requests', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions.keyword = 'a';
			controller.selectedGroups.groupIds.push('123');
			controller.selectedGroups.groupIds.push('456');
			controller.filterEnabled = true;
			controller.paging.pageSize = 100;
			controller.isUsingRequestSubmitterTimeZone = true;
			controller.init();

			controller.activeAbsenceAndTextTab();

			expect(fakeStateParams != undefined).toEqual(true);
			expect(fakeStateParams.selectedGroupIds[0]).toEqual('123');
			expect(fakeStateParams.selectedGroupIds[1]).toEqual('456');
			expect(fakeStateParams.filterEnabled).toEqual(true);
			expect(fakeStateParams.paging.pageSize).toEqual(100);
			expect(fakeStateParams.isUsingRequestSubmitterTimeZone).toEqual(true);
			expect(fakeStateParams.getPeriod().startDate).toEqual(moment().startOf('week')._d);
			expect(fakeStateParams.getPeriod().endDate).toEqual(moment().endOf('week')._d);
		});

		it('should pass correct params to shift trade requests', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions.keyword = 'a';
			controller.selectedGroups.groupIds.push('123');
			controller.selectedGroups.groupIds.push('456');
			controller.filterEnabled = true;
			controller.paging.pageSize = 100;
			controller.isUsingRequestSubmitterTimeZone = true;
			controller.init();

			controller.activeShiftTradeTab();

			expect(fakeStateParams != undefined).toEqual(true);
			expect(fakeStateParams.selectedGroupIds[0]).toEqual('123');
			expect(fakeStateParams.selectedGroupIds[1]).toEqual('456');
			expect(fakeStateParams.filterEnabled).toEqual(true);
			expect(fakeStateParams.paging.pageSize).toEqual(100);
			expect(fakeStateParams.isUsingRequestSubmitterTimeZone).toEqual(true);
			expect(fakeStateParams.getPeriod().startDate).toEqual(moment().startOf('week')._d);
			expect(fakeStateParams.getPeriod().endDate).toEqual(moment().endOf('week')._d);
		});

		it('should broadcast filterEnabled changed event', function () {
			var target = setUpTarget();
			var controller = target.controller;
			var filterEnabledChanged = false;

			target.scope.$on('requests.filterEnabled.changed', function () {
				filterEnabledChanged = true;
			});

			controller.init();
			controller.onFavoriteSearchInitDefer.resolve();

			controller.filterEnabled = false;
			target.scope.$apply();

			controller.filterEnabled = true;
			target.scope.$apply();

			expect(filterEnabledChanged).toEqual(true);
		});

		it('should broadcast isUsingRequestSubmitterTimeZone changed event', function () {
			var target = setUpTarget();
			var controller = target.controller;
			var isUsingRequestSubmitterTimeZoneChanged = false;

			target.scope.$on('requests.isUsingRequestSubmitterTimeZone.changed', function () {
				isUsingRequestSubmitterTimeZoneChanged = true;
			});

			controller.init();
			controller.onFavoriteSearchInitDefer.resolve();

			controller.isUsingRequestSubmitterTimeZone = false;
			target.scope.$apply();

			controller.isUsingRequestSubmitterTimeZone = true;
			target.scope.$apply();

			expect(isUsingRequestSubmitterTimeZoneChanged).toEqual(true);
		});

		function setUpTarget() {
			var scope = $rootScope.$new();

			var controller = $controller('RequestsRefactorCtrl', {
				$scope: scope,
				$state: fakeState,
				$q: $q
			});

			return { controller: controller, scope: scope };
		}

		function fakeRequestsDataService() {
			this.hierarchy = function () {
				return $q(function (resolve) {
					resolve({ Children: [] });
				});
			};
		}

		function fakeRequestCommandParamsHolder() {
			var requestIds = [];
			this.setSelectedRequestsIds = function (ids) {
				requestIds = ids;
			};
			this.getSelectedRequestsIds = function () {
				return requestIds;
			};
			this.resetSelectedRequestIds = function () {
				requestIds = [];
			};
		}
	});

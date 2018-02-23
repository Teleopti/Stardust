﻿'use strict';
describe('Requests controller tests',
	function () {

		var $rootScope,
			$controller,
			$q,
			$httpBackend,
			requestsDataService,
			requestCommandParamsHolder,
			getParamsFn,
			fakeState = {
				current: {
					name: ''
				},
				go: function (name, getParams) {
					this.current.name = name;
					getParamsFn = getParams.getParams;
				}
			};

		beforeEach(function () {
			module('wfm.requests');

			requestsDataService = new fakeRequestsDataService();

			module(function ($provide) {
				$provide.service('$state', function () { return fakeState; });
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_People_Search_36294: true,
						togglesLoaded: $q(function (resolve, reject) {
							resolve();
						})
					}
				});
				$provide.service('requestsDataService', function () { return requestsDataService; });
				$provide.service('$timeout', function() {
					return function(callback) {
						callback();
					}
				});
			});

		});

		beforeEach(inject(function (_$rootScope_, _$controller_, _$q_, _$httpBackend_, _requestCommandParamsHolder_) {
			$rootScope = _$rootScope_;
			$controller = _$controller_;
			$q = _$q_;
			$httpBackend = _$httpBackend_;
			requestCommandParamsHolder = _requestCommandParamsHolder_;

			$httpBackend.whenGET('app/requests/html/requests.html').respond(function () {
				return [200, true];
			});
		}));

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

			var controller = test.controller;
			controller.absenceAndOvertimePeriod = periodForAbsenceRequest;
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

			requestCommandParamsHolder.setSelectedRequestIds(['selectedIds']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(1);
			controller.onFavoriteSearchInitDefer.resolve();

			controller.period = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};

			target.scope.$digest();
			expect(requestCommandParamsHolder.getSelectedRequestsIds().length).toEqual(0);
		});

		it('should keep search tearms in params after selected period changed', function () {
			var target = setUpTarget();
			var controller = target.controller;
			
			controller.onFavoriteSearchInitDefer.resolve();

			controller.agentSearchOptions.keyword = 'keyword';
			target.scope.$digest();
			expect(getParamsFn().agentSearchTerm).toEqual('keyword');

			controller.period.startDate = new Date('2017-11-10');
			controller.period.endDate = new Date('2017-11-30');
			
			target.scope.$digest();
			expect(getParamsFn().agentSearchTerm).toEqual('keyword');
		});

		it('should clear the selection after search term changed and enter press', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: false
			};

			controller.activeShiftTradeTab();
			requestCommandParamsHolder.setSelectedRequestIds(['selectedIds'], true);
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(1);

			controller.onSearchTermChangedCallback();
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(0);
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

		it('should broadcast reload.requests.without.selection onCommandSuccess', function () {
			var target = setUpTarget();
			var controller = target.controller;
			var hasSentReloadBroadcast;

			target.scope.$on('reload.requests.without.selection', function (event, data) {
				hasSentReloadBroadcast = true;
			});

			controller.onCommandSuccess();

			expect(hasSentReloadBroadcast).toBeTruthy();
		});

		it('should clear the selection after selected teams changed', function () {
			var controller = setUpTarget().controller;

			controller.agentSearchOptions = {
				focusingSearch: false
			};

			controller.activeShiftTradeTab();
			requestCommandParamsHolder.setSelectedRequestIds(['selectedIds'], true);
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(1);

			controller.changeSelectedTeams(['fakeTeamId']);
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(0);
		});

		it('should clear the selection after applying favorite', function () {
			var controller = setUpTarget().controller;

			controller.activeShiftTradeTab();
			requestCommandParamsHolder.setSelectedRequestIds(['selectedIds'], true);
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(1);

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
			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(0);
		});

		it('should get correct url when go to absence and text requests tab', function () {
			var controller = setUpTarget().controller;
			controller.activeAbsenceAndTextTab();

			expect(getParamsFn != undefined).toEqual(true);
			expect(getParamsFn() != undefined).toEqual(true);
			expect(fakeState.current.name).toEqual('requests.absenceAndText');
		});

		it('should get correct url when go to shift trade requests tab', function () {
			var controller = setUpTarget().controller;
			controller.activeShiftTradeTab();
			expect(getParamsFn() != undefined).toEqual(true);
			expect(fakeState.current.name).toEqual('requests.shiftTrade');
		});

		it('should get isShiftTradeViewActive according to current state name', function () {
			var controller = setUpTarget().controller;

			fakeState.current.name = 'requests';
			expect(controller.isShiftTradeViewActive()).toBeFalsy();
			controller.activeShiftTradeTab();

			expect(controller.isShiftTradeViewActive()).toBeTruthy();
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

			expect(getParamsFn() != undefined).toEqual(true);
			expect(getParamsFn().selectedGroupIds[0]).toEqual('123');
			expect(getParamsFn().selectedGroupIds[1]).toEqual('456');
			expect(getParamsFn().filterEnabled).toEqual(true);
			expect(getParamsFn().paging.pageSize).toEqual(100);
			expect(getParamsFn().isUsingRequestSubmitterTimeZone).toEqual(true);
			expect(getParamsFn().getPeriod().startDate).toEqual(moment().startOf('week')._d);
			expect(getParamsFn().getPeriod().endDate).toEqual(moment().endOf('week')._d);
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

			expect(getParamsFn() != undefined).toEqual(true);
			expect(getParamsFn().selectedGroupIds[0]).toEqual('123');
			expect(getParamsFn().selectedGroupIds[1]).toEqual('456');
			expect(getParamsFn().filterEnabled).toEqual(true);
			expect(getParamsFn().paging.pageSize).toEqual(100);
			expect(getParamsFn().isUsingRequestSubmitterTimeZone).toEqual(true);
			expect(getParamsFn().getPeriod().startDate).toEqual(moment().startOf('week')._d);
			expect(getParamsFn().getPeriod().endDate).toEqual(moment().endOf('week')._d);
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

		it('should clear selected requests ids after changing tab', function () {

			var target = setUpTarget();
			var controller = target.controller;
			controller.init();

			requestCommandParamsHolder.setSelectedRequestIds([1], true);
			requestCommandParamsHolder.setSelectedRequestIds([2], false);
			requestCommandParamsHolder.setOvertimeSelectedRequestIds([3]);

			controller.activeShiftTradeTab();

			expect(requestCommandParamsHolder.getSelectedRequestsIds(true).length).toEqual(0);
			expect(requestCommandParamsHolder.getSelectedRequestsIds(false).length).toEqual(0);
			expect(requestCommandParamsHolder.getOvertimeSelectedRequestIds().length).toEqual(0);
		});

		it('should reset pageNumber after chaning tab', function () {
			var target = setUpTarget();
			var controller = target.controller;
			controller.init();

			controller.activeShiftTradeTab();
			controller.paging.pageNumber = 2;

			controller.activeAbsenceAndTextTab();

			expect(controller.paging.pageNumber).toEqual(1);
		});

		function setUpTarget() {
			var scope = $rootScope.$new();

			var controller = $controller('requestsController', {
				$scope: scope,
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
			this.getOvertimeLicense = function (successCallback) {
				return { then: function() {
					successCallback && successCallback();
				}};
			};
		}
	});

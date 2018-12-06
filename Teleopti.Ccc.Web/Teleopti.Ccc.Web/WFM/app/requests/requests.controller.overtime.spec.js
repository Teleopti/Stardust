'use strict';
describe('Requests - overtime controller tests', function() {
	var $rootScope,
		$filter,
		$compile,
		$controller,
		requestsDataService,
		requestsDefinitions,
		requestsNotificationService,
		currentUserInfo,
		requestsFilterSvc,
		requestGridStateSvc,
		uiGridConstants,
		requestsTabNames,
		requestCommandParamsHolder;

	var period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		},
		params = {
			agentSearchTerm: '',
			selectedGroupIds: [],
			filterEnabled: false,
			onInitCallBack: undefined,
			paging: {},
			isUsingRequestSubmitterTimeZone: undefined,
			getPeriod: function() {
				return period;
			}
		},
		fakeStateParams = {
			getParams: function() {
				return params;
			}
		};

	var controller, scope;

	beforeEach(function() {
		module('wfm.templates');
		module('wfm.requests');
		requestsDataService = new FakeRequestsDataService();
		requestsNotificationService = new FakeRequestsNotificationService();
		currentUserInfo = new FakeCurrentUserInfo();

		module(function($provide) {
			$provide.service('Toggle', function() {
				return {
					Wfm_Requests_People_Search_36294: true,
					togglesLoaded: {
						then: function(cb) {
							cb();
						}
					}
				};
			});
			$provide.service('requestsDataService', function() {
				return requestsDataService;
			});
			$provide.service('requestsNotificationService', function() {
				return requestsNotificationService;
			});
			$provide.service('CurrentUserInfo', function() {
				return currentUserInfo;
			});
		});
	});

	beforeEach(inject(function(
		_$filter_,
		_$compile_,
		_$rootScope_,
		_$controller_,
		_requestsDefinitions_,
		_RequestsFilter_,
		_RequestGridStateService_,
		_uiGridConstants_,
		REQUESTS_TAB_NAMES,
		_requestCommandParamsHolder_
	) {
		$filter = _$filter_;
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		requestsDefinitions = _requestsDefinitions_;
		requestsFilterSvc = _RequestsFilter_;
		requestGridStateSvc = _RequestGridStateService_;
		uiGridConstants = _uiGridConstants_;
		requestsTabNames = REQUESTS_TAB_NAMES;
		requestCommandParamsHolder = _requestCommandParamsHolder_;

		setUpTarget();
	}));

	it('should see UI Grid', function() {
		scope.requests = [
			{
				Id: 1,
				PeriodStartTime: '2016-01-05T00:00:00',
				PeriodEndTime: '2016-01-07T23:59:00',
				CreatedTime: '2016-01-05T03:29:37',
				TimeZone: 'Europe/Berlin',
				UpdatedTime: '2016-01-05T03:29:37'
			}
		];

		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();
		var results = Array.from(element.children());

		expect(
			results.some(function(item) {
				return angular.element(item).hasClass('ui-grid-contents-wrapper');
			})
		).toBeTruthy();
	});

	it('see table rows for each request', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.OVERTIME,
			Payload: { Name: 'overtime paid' }
		};
		requestsDataService.setRequests([request, request]);

		params.selectedGroupIds = ['team'];
		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		var targets = element[0].querySelectorAll('.ui-grid-render-container-body .ui-grid-row');

		expect(targets.length).toEqual(2);
	});

	it('should save ui-grid state', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.OVERTIME,
			Payload: {
				Name: 'overtime paid'
			}
		};
		requestsDataService.setRequests([request, request]);

		params.selectedGroupIds = ['team'];
		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		requestGridStateSvc.setupGridEventHandlers(scope, controller, requestsDefinitions.REQUEST_TYPES.OVERTIME);

		var gridScope = angular.element(element[0].querySelectorAll('.ui-grid-contents-wrapper')).scope();
		gridScope.grid.api.core.raise.columnVisibilityChanged(gridScope.grid.columns[0]);
		controller.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

		scope.$digest();

		expect(requestGridStateSvc.hasSavedState(requestsDefinitions.REQUEST_TYPES.OVERTIME)).toBeTruthy();
	});

	it('should exclude the invalid selection object when restoring ui-grid state', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.OVERTIME,
			Payload: {
				Name: 'overtime paid'
			}
		};
		requestsDataService.setRequests([request, request]);

		params.selectedGroupIds = ['team'];
		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		requestGridStateSvc.setupGridEventHandlers(scope, controller, requestsDefinitions.REQUEST_TYPES.OVERTIME);

		var gridScope = angular.element(element[0].querySelectorAll('.ui-grid-contents-wrapper')).scope();
		gridScope.grid.api.core.raise.columnVisibilityChanged(gridScope.grid.columns[0]);
		controller.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

		scope.$digest();

		expect(requestGridStateSvc.hasSavedState(requestsDefinitions.REQUEST_TYPES.OVERTIME)).toBeTruthy();
		expect(
			requestGridStateSvc.restoreState(controller, requestsDefinitions.REQUEST_TYPES.OVERTIME).selection
		).toBeFalsy();
	});

	it('populate requests data from requests data service', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.OVERTIME,
			Payload: { Name: 'overtime paid' }
		};
		requestsDataService.setRequests([request]);

		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(controller.requests.length).toEqual(1);
		expect(controller.requests[0]).toEqual(request);
	});

	it('should not populate requests data from requests data service when no team selected and organization picker is on', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.OVERTIME,
			Payload: { Name: 'overtime paid' }
		};
		requestsDataService.setRequests([request]);

		params.selectedGroupIds = [];
		compileUIGridHtml(scope, controller.gridOptions);

		expect(requestsDataService.getHasSentRequests()).toBeFalsy();
		expect(controller.requests.length).toEqual(0);
	});

	it('should not request data when period is invalid', function() {
		requestsDataService.setRequests([]);
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		requestsDataService.reset();
		period = {
			startDate: moment()
				.add(1, 'day')
				.toDate(),
			endDate: new Date()
		};
		scope.$digest();

		expect(requestsDataService.getHasSentRequests()).toBeFalsy();
	});

	it('should request data when period is valid', function() {
		requestsDataService.setRequests([]);
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		requestsDataService.reset();
		period = {
			startDate: new Date(),
			endDate: moment()
				.add(2, 'day')
				.toDate()
		};
		scope.$digest();

		expect(requestsDataService.getHasSentRequests()).toBeTruthy();
	});

	it('should request data when search term changed', function() {
		requestsDataService.setRequests([]);
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		requestsDataService.reset();

		controller.agentSearchTerm = 'search term';
		scope.$digest();
		expect(requestsDataService.getHasSentRequests()).toBeTruthy();
		expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual('search term');
	});

	it('should set isLoading to false after reload requests action finished', function() {
		requestsDataService.setRequests([]);

		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		requestsDataService.reset();

		controller.isLoading = true;
		controller.agentSearchTerm = 'search term';
		scope.$digest();
		expect(controller.isLoading).toBeFalsy();
	});

	it('should show pending overtime requests only by default', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(controller.filters[0].Status).toEqual('0');
	});

	it('should not call data service more than once after initialized', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		period = {
			startDate: moment().add(-1, 'd')._d,
			endDate: moment().add(1, 'd')._d
		};
		scope.$digest();

		expect(requestsDataService.getCallCounts()).toEqual(1);
	});

	it('should display error when searching person count is exceeded', function() {
		requestsDataService.setGetAllRequetsCallbackData({
			IsSearchPersonCountExceeded: true,
			MaxSearchPersonCount: 5000
		});

		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(controller.requests.length).toEqual(0);
		expect(requestsNotificationService.getNotificationResult()).toEqual(5000);
	});

	it('startTime, endTime, createdTime and updatedTime columns should shown in the same timezone as backend says', function() {
		params.selectedGroupIds = ['team'];
		controller.isUsingRequestSubmitterTimeZone = true;
		compileUIGridHtml(scope, controller.gridOptions);

		var requests = [
			{
				Id: 1,
				PeriodStartTime: '2016-01-05T00:00:00',
				PeriodEndTime: '2016-01-07T23:59:00',
				CreatedTime: '2016-01-05T03:29:37',
				TimeZone: 'Europe/Berlin',
				UpdatedTime: '2016-01-05T03:29:37',
				Payload: { Name: 'overtime paid' }
			}
		];
		requestsDataService.setRequests(requests);

		scope.$digest();

		var startTime = controller.requests[0].FormatedPeriodStartTime();
		var endTime = controller.requests[0].FormatedPeriodEndTime();
		var createdTime = controller.requests[0].FormatedCreatedTime();
		var updatededTime = controller.requests[0].FormatedUpdatedTime();

		expect(startTime).toEqual(toDateString('2016-01-05T00:00:00'));
		expect(endTime).toEqual(toDateString('2016-01-07T23:59:00'));
		expect(createdTime).toEqual(toDateString('2016-01-05T03:29:37'));
		expect(updatededTime).toEqual(toDateString('2016-01-05T03:29:37'));
	});

	it('should be able to switch between user timezone and request submitter timezone', function() {
		currentUserInfo.setCurrentUserInfo('Europe/Berlin');
		setUpTarget();

		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		var requests = [
			{
				Id: 1,
				PeriodStartTime: '2016-01-06T14:00:00',
				PeriodEndTime: '2016-01-09T20:00:00',
				CreatedTime: '2016-01-06T10:17:31',
				TimeZone: 'Pacific/Port_Moresby',
				UpdatedTime: '2016-01-06T10:17:31',
				IsFullDay: false,
				Payload: { Name: 'overtime paid' }
			}
		];
		requestsDataService.setRequests(requests);
		scope.$digest();

		scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', false);
		expect(controller.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T05:00:00'));

		scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', true);
		expect(controller.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T14:00:00'));
	});

	it('should select default status filter', function() {
		compileUIGridHtml(scope, controller.gridOptions);

		var status0 = '1';
		var status1 = '2';
		var status2 = '3';
		controller.filters = [{ Status: status0 + ' ' + status1 + ' ' + status2 }];
		scope.$digest();

		var selectedStatus = controller.selectedRequestStatuses;

		expect(selectedStatus.length).toEqual(3);
		expect(selectedStatus[0].Id).toEqual(status0.trim());
		expect(selectedStatus[1].Id).toEqual(status1.trim());
		expect(selectedStatus[2].Id).toEqual(status2.trim());
	});

	it('should select types', function() {
		compileUIGridHtml(scope, controller.gridOptions);

		var type1 = '29f7ece8-d340-408f-be40-9bb900b8a4cb';
		var type2 = '9019d62f-0086-44b1-a977-9bb900b8c361';
		controller.filters = [
			{
				Type: type1 + ' ' + type2
			}
		];

		controller.setSelectedTypes();
		scope.$digest();

		expect(controller.selectedTypes.length).toEqual(2);
		expect(controller.selectedTypes[0].Id).toEqual(type1.trim());
		expect(controller.selectedTypes[1].Id).toEqual(type2.trim());
	});
	it('should save the filters data in RequestsFilter service for overtime', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(requestsFilterSvc.filters[requestsTabNames.overtime]).not.toBe(null);

		var status0 = '79',
			status1 = '86',
			status2 = '93',
			filterName = 'Status',
			expectedFilters = [{}];

		expectedFilters[0][filterName] = status0 + ' ' + status1 + ' ' + status2;

		controller.selectedRequestStatuses = [
			{
				Id: status0
			},
			{
				Id: status1
			},
			{
				Id: status2
			}
		];
		controller.statusFilterClose();

		var overtimeFilters = requestsFilterSvc.filters[requestsTabNames.overtime];
		expect(overtimeFilters.length).toEqual(expectedFilters.length);
		expect(Object.keys(overtimeFilters[0])[0]).toEqual(Object.keys(expectedFilters[0])[0]);
		expect(overtimeFilters[0][filterName]).toEqual(expectedFilters[0][filterName]);
	});

	it('should clear the filters data in RequestsFilter service for overtime', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(requestsFilterSvc.filters[requestsTabNames.overtime]).not.toBe(null);

		var status0 = '79',
			status1 = '86',
			status2 = '93',
			filterName = 'Status',
			expectedFilters = [{}];

		expectedFilters[0][filterName] = status0 + ' ' + status1 + ' ' + status2;

		controller.selectedRequestStatuses = [
			{
				Id: status0
			},
			{
				Id: status1
			},
			{
				Id: status2
			}
		];

		controller.statusFilterClose();

		var overtimeFilters = requestsFilterSvc.filters[requestsTabNames.overtime];
		expect(overtimeFilters[0][filterName]).toEqual(expectedFilters[0][filterName]);

		controller.clearAllFilters();
		expect(overtimeFilters.length).toEqual(0);
	});

	it('should display correct time for DST', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		var requests = [
			{
				Id: 1,
				PeriodStartTime: '2016-10-27T17:00:00',
				PeriodEndTime: '2016-10-27T18:00:00',
				CreatedTime: '2016-11-07T10:17:31',
				TimeZone: 'Europe/Berlin',
				UpdatedTime: '2016-11-07T10:17:31',
				IsFullDay: false,
				Payload: { Name: 'overtime paid' }
			}
		];
		requestsDataService.setRequests(requests);

		scope.$digest();
		scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', true);

		var request = controller.requests[0];
		expect(request.FormatedPeriodStartTime()).toEqual(toDateString('2016-10-27T17:00:00'));
		expect(request.FormatedPeriodEndTime()).toEqual(toDateString('2016-10-27T18:00:00'));
	});

	it('should clear subject and message filters', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		controller.subjectFilter = ['a'];
		controller.messageFilter = ['b'];
		scope.$digest();

		controller.clearAllFilters();
		expect(controller.subjectFilter).toEqual(undefined);
		expect(controller.messageFilter).toEqual(undefined);
	});

	it('should toggle filters status', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		scope.$digest();
		scope.$broadcast('requests.filterEnabled.changed', true);

		expect(controller.gridOptions.enableFiltering).toBeTruthy();
		expect(controller.gridOptions.useExternalFiltering).toBeTruthy();

		scope.$broadcast('requests.filterEnabled.changed', false);

		expect(controller.gridOptions.enableFiltering).toBeFalsy();
		expect(controller.gridOptions.useExternalFiltering).toBeFalsy();
	});

	it('should get overtime types', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(controller.overtimeTypes.length).toEqual(2);
		expect(controller.overtimeTypes[0].Name).toEqual('overtime paid');
		expect(controller.overtimeTypes[1].Name).toEqual('overtime time');
	});

	it('should list overtime types', function() {
		params.selectedGroupIds = ['team'];

		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();
		var results = Array.from(element.children());

		expect(
			results.some(function(item) {
				return angular
					.element(item)
					.html()
					.indexOf('overtime paid');
			})
		).toBeTruthy();

		expect(
			results.some(function(item) {
				return angular
					.element(item)
					.html()
					.indexOf('overtime time');
			})
		).toBeTruthy();
	});

	it('should uncheck selectAll checkbox after loading requests', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		controller.gridApi.grid.selection.selectAll = true;
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupIds: 'group ids'
		});

		expect(controller.gridApi.grid.selection.selectAll).toEqual(false);
	});

	it('should clear selection after reloading without selection', function() {
		requestCommandParamsHolder.setOvertimeSelectedRequestIds([1, 2]);

		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		scope.$broadcast('reload.requests.without.selection');

		expect(requestCommandParamsHolder.getOvertimeSelectedRequestIds().length).toEqual(0);
	});

	function setUpTarget() {
		scope = $rootScope.$new();
		controller = $controller('requestsOvertimeController', {
			$scope: scope,
			$stateParams: fakeStateParams
		});
	}

	function compileUIGridHtml(scope, gridOptions) {
		var html =
			'<div ui-grid="gridOptions" class="ui-grid requests-uigrid-table" ui-grid-auto-resize ui-grid-selection ui-grid-resize-columns ui-grid-pinning ui-grid-save-state ui-grid-disable-animation></div>';

		scope.gridOptions = gridOptions;
		return $compile(html)(scope);
	}

	function FakeRequestsDataService() {
		var _requests = [],
			_hasSentRequests,
			_lastRequestParameters,
			_callCounts = 0,
			_getAllRequetsCallbackData = {
				Requests: _requests,
				TotalCount: _requests.length
			};

		this.reset = function() {
			_requests = [];
			_hasSentRequests = false;
			_lastRequestParameters = null;
			_callCounts = 0;
			_getAllRequetsCallbackData = {
				Requests: _requests,
				TotalCount: _requests.length
			};
		};

		this.setRequests = function(requests) {
			_requests = requests;
			_getAllRequetsCallbackData = {
				Requests: _requests,
				TotalCount: _requests.length
			};
		};

		this.setGetAllRequetsCallbackData = function(data) {
			_getAllRequetsCallbackData = data;
		};

		this.getHasSentRequests = function() {
			return _hasSentRequests;
		};
		this.getLastRequestParameters = function() {
			return _lastRequestParameters;
		};

		this.getOvertimeRequestsPromise = function() {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function(cb) {
					_callCounts++;
					cb({
						data: _getAllRequetsCallbackData
					});
				}
			};
		};

		this.getOvertimeTypes = function() {
			return {
				then: function(cb) {
					cb({
						data: [{ Id: '00', Name: 'overtime paid' }, { Id: '01', Name: 'overtime time' }]
					});
				}
			};
		};

		this.getOvertimeRequestsStatuses = function() {
			return [{ Id: 0, Name: 'Status0' }, { Id: 1, Name: 'Status1' }];
		};

		this.getCallCounts = function() {
			return _callCounts;
		};
	}

	function FakeRequestsNotificationService() {
		var _notificationResult;
		this.notifyMaxSearchPersonCountExceeded = function(maxSearchPersonCount) {
			_notificationResult = maxSearchPersonCount;
		};

		this.getNotificationResult = function() {
			return _notificationResult;
		};
	}

	function FakeCurrentUserInfo() {
		var defaultTimeZone = 'Atlantic/Reykjavik';
		return {
			CurrentUserInfo: function() {
				return {
					DefaultTimeZone: defaultTimeZone
				};
			},
			setCurrentUserInfo: function(timezone) {
				defaultTimeZone = timezone;
			}
		};
	}

	function toDateString(date, timeZone) {
		var momentDate = moment(date);
		if (timeZone) {
			var _isNowDST = moment.tz(timeZone).isDST();
			var _dateTime = _isNowDST ? momentDate.add(1, 'h').toDate() : momentDate.toDate();
			return $filter('date')(_dateTime, 'short');
		} else {
			return $filter('date')(momentDate.toDate(), 'short');
		}
	}
});

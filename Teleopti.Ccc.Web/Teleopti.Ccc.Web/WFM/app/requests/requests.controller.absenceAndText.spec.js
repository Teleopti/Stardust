'use strict';
describe('Requests - absence and text controller tests: ', function() {
	var $rootScope,
		$filter,
		$compile,
		$controller,
		requestsDataService,
		requestsDefinitions,
		requestsNotificationService,
		currentUserInfo,
		requestGridStateSvc,
		requestsFilterSvc,
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
		_RequestGridStateService_,
		_RequestsFilter_,
		_uiGridConstants_,
		REQUESTS_TAB_NAMES,
		_requestCommandParamsHolder_
	) {
		$filter = _$filter_;
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		requestsDefinitions = _requestsDefinitions_;
		requestGridStateSvc = _RequestGridStateService_;
		requestsFilterSvc = _RequestsFilter_;
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
			Type: requestsDefinitions.REQUEST_TYPES.TEXT
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
			Type: requestsDefinitions.REQUEST_TYPES.ABSENCE,
			Payload: {
				Name: 'holiday'
			}
		};
		requestsDataService.setRequests([request, request]);

		params.selectedGroupIds = ['team'];
		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		requestGridStateSvc.setupGridEventHandlers(scope, controller, requestsDefinitions.REQUEST_TYPES.ABSENCE);

		var gridScope = angular.element(element[0].querySelectorAll('.ui-grid-contents-wrapper')).scope();
		gridScope.grid.api.core.raise.columnVisibilityChanged(gridScope.grid.columns[0]);
		controller.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

		scope.$digest();

		expect(requestGridStateSvc.hasSavedState(requestsDefinitions.REQUEST_TYPES.ABSENCE)).toBeTruthy();
	});

	it('should exclude the invalid selection object when restoring ui-grid state', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.ABSENCE,
			Payload: {
				Name: 'holiday'
			}
		};
		requestsDataService.setRequests([request, request]);

		params.selectedGroupIds = ['team'];
		var element = compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		requestGridStateSvc.setupGridEventHandlers(scope, controller, requestsDefinitions.REQUEST_TYPES.ABSENCE);

		var gridScope = angular.element(element[0].querySelectorAll('.ui-grid-contents-wrapper')).scope();
		gridScope.grid.api.core.raise.columnVisibilityChanged(gridScope.grid.columns[0]);
		controller.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

		scope.$digest();

		expect(requestGridStateSvc.hasSavedState(requestsDefinitions.REQUEST_TYPES.ABSENCE)).toBeTruthy();
		expect(
			requestGridStateSvc.restoreState(controller, requestsDefinitions.REQUEST_TYPES.ABSENCE).selection
		).toBeFalsy();
	});

	it('populate requests data from requests data service', function() {
		var request = {
			Id: 1,
			Type: requestsDefinitions.REQUEST_TYPES.TEXT
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
			Type: requestsDefinitions.REQUEST_TYPES.TEXT
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
		scope.$digest();

		scope.$broadcast('reload.requests.with.selection', {
			agentSearchTerm: 'search term'
		});

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

	it('should show pending and waitlisted absence requests only by default', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(controller.filters[0].Status).toEqual('0 5');
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
				UpdatedTime: '2016-01-05T03:29:37'
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
				IsFullDay: false
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

		var status0 = '79';
		var status1 = '86';
		var status2 = '93';
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

		var type1 = '47d9292f-ead6-40b2-ac4f-9b5e015ab330';
		var type2 = '041db668-3185-4d7a-8781-9b5e015ab330';
		controller.filters = [{ Type: type1 + ' ' + type2 }];

		controller.setSelectedTypes();
		scope.$digest();

		expect(controller.selectedTypes.length).toEqual(2);
		expect(controller.selectedTypes[0].Id).toEqual(type1.trim());
		expect(controller.selectedTypes[1].Id).toEqual(type2.trim());
	});

	it('should save the filters data in RequestsFilter service for absenceAndText', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(requestsFilterSvc.filters[requestsTabNames.absenceAndText]).not.toBe(null);

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

		var absenceAndTextFilters = requestsFilterSvc.filters[requestsTabNames.absenceAndText];
		expect(absenceAndTextFilters.length).toEqual(expectedFilters.length);
		expect(Object.keys(absenceAndTextFilters[0])[0]).toEqual(Object.keys(expectedFilters[0])[0]);
		expect(absenceAndTextFilters[0][filterName]).toEqual(expectedFilters[0][filterName]);
	});

	it('should clear the filters data in RequestsFilter service for absenceAndText', function() {
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		expect(requestsFilterSvc.filters[requestsTabNames.absenceAndText]).not.toBe(null);

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

		var absenceAndTextFilters = requestsFilterSvc.filters[requestsTabNames.absenceAndText];
		expect(absenceAndTextFilters[0][filterName]).toEqual(expectedFilters[0][filterName]);

		controller.clearAllFilters();
		expect(absenceAndTextFilters.length).toEqual(0);
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
				IsFullDay: false
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

	it('should update agent search term before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = '';
		scope.$digest();

		controller.agentSearchTerm = 'blabla';
		scope.$broadcast('reload.requests.with.selection', {
			agentSearchTerm: expectedResult
		});

		expect(controller.agentSearchTerm).toEqual(expectedResult);
	});

	it('should update selected group Ids before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = 'other team';
		scope.$digest();

		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupIds: expectedResult
		});

		expect(controller.selectedGroupIds).toEqual(expectedResult);
	});

	it('should update selected group page Id before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = '';
		scope.$digest();

		controller.selectedGroupPageId = 'page id';
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupPageId: expectedResult
		});

		expect(controller.selectedGroupPageId).toEqual(expectedResult);
	});

	it('should update paging before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = {};
		scope.$digest();

		controller.paging = { PageCount: 1 };
		scope.$broadcast('reload.requests.with.selection', {
			paging: expectedResult
		});

		expect(controller.paging).toEqual(expectedResult);
	});

	it('should not selected search term if it is undefined before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = 'John';
		scope.$digest();

		controller.agentSearchTerm = expectedResult;
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupIds: 'group ids'
		});

		expect(controller.agentSearchTerm).toEqual(expectedResult);
	});

	it('should not selected group page id if it is undefined before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = 'page id';
		scope.$digest();

		controller.selectedGroupPageId = expectedResult;
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupIds: 'group ids'
		});

		expect(controller.selectedGroupPageId).toEqual(expectedResult);
	});

	it('should not selected group ids if it is undefined before loading requests', function() {
		var expectedResult = ['team'];
		params.selectedGroupIds = expectedResult;
		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		controller.paging = expectedResult;
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupPageId: 'group ids'
		});

		expect(controller.selectedGroupIds).toEqual(expectedResult);
	});

	it('should not update paging if it is undefined before loading requests', function() {
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);
		var expectedResult = { PageCount: 1 };
		scope.$digest();

		controller.paging = expectedResult;
		scope.$broadcast('reload.requests.with.selection', {
			selectedGroupIds: 'group ids'
		});

		expect(controller.paging).toEqual(expectedResult);
	});

	it('should reset filters in the first time', function() {
		requestsFilterSvc.setFilter('Status', '0 4 5', 'absenceAndText');
		compileUIGridHtml(scope, controller.gridOptions);

		scope.$apply();
		expect(requestsFilterSvc.filters['absenceAndText'][0]['Status']).toEqual('0 5');
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
		requestCommandParamsHolder.setSelectedRequestIds([1, 2], false);

		compileUIGridHtml(scope, controller.gridOptions);
		scope.$digest();

		scope.$broadcast('reload.requests.without.selection');

		expect(requestCommandParamsHolder.getSelectedRequestsIds(false).length).toEqual(0);
	});

	it('should not load requests when period is undefined', function() {
		requestsDataService.setRequests([]);
		params.selectedGroupIds = ['team'];
		compileUIGridHtml(scope, controller.gridOptions);

		requestsDataService.reset();
		period = undefined;
		scope.$digest();

		expect(requestsDataService.getHasSentRequests()).toBeFalsy();
	});

	it('should build absence schedule container style and show when toggled absence schedules', function() {
		var absence = {
			IsFullDay: false,
			PersonAccountSummaryViewModel: {
				PersonAccountSummaryDetails: [
					{
						StartDate: '2017-01-01T02:00:00',
						RemainingDescription: '19',
						TrackingTypeDescription: 'Days',
						EndDate: '2026-11-10T02:00:00'
					}
				]
			},
			Shifts: [
				{
					Name: 'Ashley Andeen',
					Periods: [
						{
							Title: 'Social Media',
							TimeSpan: '8:00 AM - 10:15 AM',
							Color: '30,144,255',
							StartTime: '2018-11-23T08:00:00',
							EndTime: '2018-11-23T10:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.0,
							EndPositionPercentage: 0.25,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '10:15 AM - 10:30 AM',
							Color: '255,0,0',
							StartTime: '2018-11-23T10:15:00',
							EndTime: '2018-11-23T10:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.25,
							EndPositionPercentage: 0.2778,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '10:30 AM - 12:30 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T10:30:00',
							EndTime: '2018-11-23T12:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.2778,
							EndPositionPercentage: 0.5,
							Meeting: null
						},
						{
							Title: 'Lunch',
							TimeSpan: '12:30 PM - 1:30 PM',
							Color: '255,255,0',
							StartTime: '2018-11-23T12:30:00',
							EndTime: '2018-11-23T13:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.5,
							EndPositionPercentage: 0.6111,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '1:30 PM - 3:15 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T13:30:00',
							EndTime: '2018-11-23T15:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.6111,
							EndPositionPercentage: 0.8056,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '3:15 PM - 3:30 PM',
							Color: '255,0,0',
							StartTime: '2018-11-23T15:15:00',
							EndTime: '2018-11-23T15:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8056,
							EndPositionPercentage: 0.8333,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '3:30 PM - 5:00 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T15:30:00',
							EndTime: '2018-11-23T17:00:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8333,
							EndPositionPercentage: 1.0,
							Meeting: null
						}
					],
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					ShiftCategory: {
						Id: null,
						ShortName: 'PM',
						Name: 'Late',
						DisplayColor: '#000000'
					},
					BelongsToDate: '2018-11-23T00:00:00'
				}
			],
			Subject: 'subject',
			Message: '',
			AgentName: 'Ashley Andeen',
			PersonId: '11610fe4-0130-4568-97de-9b5e015b2564',
			TimeZone: 'Asia/Amman',
			Id: '7cfc4367-0fdd-4c31-981f-a9a1003bde07',
			Seniority: 23,
			PeriodStartTime: '2018-11-23T09:00:00',
			PeriodEndTime: '2018-11-23T18:00:00',
			UpdatedTime: '2018-11-23T05:37:58.563',
			CreatedTime: '2018-11-23T05:37:58.21',
			Type: 1,
			TypeText: 'Absence',
			Status: 1,
			StatusText: 'Pending',
			Payload: {
				Name: 'Holiday',
				ShortName: 'HO'
			},
			Team: 'London/Team Preferences',
			DenyReason: ''
		};
		var clickEvent = new MouseEvent('click');

		controller.toggleAbsenceSchedules(absence, clickEvent);

		expect(controller.showingAbsenceSchedules).toEqual(true);
		expect(controller.schedulesContainerStyle.width).toEqual('762px');
		expect(controller.schedulesContainerStyle.height).toEqual('102px');
		expect(controller.schedulesContainerStyle['max-height']).toEqual('230px');
	});

	it('should get absence shift start and end time when toggled absence schedules', function() {
		var absence = {
			IsFullDay: false,
			PersonAccountSummaryViewModel: {
				PersonAccountSummaryDetails: [
					{
						StartDate: '2017-01-01T02:00:00',
						RemainingDescription: '19',
						TrackingTypeDescription: 'Days',
						EndDate: '2026-11-10T02:00:00'
					}
				]
			},
			Shifts: [
				{
					Name: 'Ashley Andeen',
					Periods: [
						{
							Title: 'Social Media',
							TimeSpan: '8:00 AM - 10:15 AM',
							Color: '30,144,255',
							StartTime: '2018-11-23T08:00:00',
							EndTime: '2018-11-23T10:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.0,
							EndPositionPercentage: 0.25,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '10:15 AM - 10:30 AM',
							Color: '255,0,0',
							StartTime: '2018-11-23T10:15:00',
							EndTime: '2018-11-23T10:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.25,
							EndPositionPercentage: 0.2778,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '10:30 AM - 12:30 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T10:30:00',
							EndTime: '2018-11-23T12:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.2778,
							EndPositionPercentage: 0.5,
							Meeting: null
						},
						{
							Title: 'Lunch',
							TimeSpan: '12:30 PM - 1:30 PM',
							Color: '255,255,0',
							StartTime: '2018-11-23T12:30:00',
							EndTime: '2018-11-23T13:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.5,
							EndPositionPercentage: 0.6111,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '1:30 PM - 3:15 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T13:30:00',
							EndTime: '2018-11-23T15:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.6111,
							EndPositionPercentage: 0.8056,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '3:15 PM - 3:30 PM',
							Color: '255,0,0',
							StartTime: '2018-11-23T15:15:00',
							EndTime: '2018-11-23T15:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8056,
							EndPositionPercentage: 0.8333,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '3:30 PM - 5:00 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T15:30:00',
							EndTime: '2018-11-23T17:00:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8333,
							EndPositionPercentage: 1.0,
							Meeting: null
						}
					],
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					ShiftCategory: {
						Id: null,
						ShortName: 'PM',
						Name: 'Late',
						DisplayColor: '#000000'
					},
					BelongsToDate: '2018-11-23T00:00:00'
				}
			],
			Subject: 'subject',
			Message: '',
			AgentName: 'Ashley Andeen',
			PersonId: '11610fe4-0130-4568-97de-9b5e015b2564',
			TimeZone: 'Asia/Amman',
			Id: '7cfc4367-0fdd-4c31-981f-a9a1003bde07',
			Seniority: 23,
			PeriodStartTime: '2018-11-23T09:00:00',
			PeriodEndTime: '2018-11-23T18:00:00',
			UpdatedTime: '2018-11-23T05:37:58.563',
			CreatedTime: '2018-11-23T05:37:58.21',
			Type: 1,
			TypeText: 'Absence',
			Status: 1,
			StatusText: 'Pending',
			Payload: {
				Name: 'Holiday',
				ShortName: 'HO'
			},
			Team: 'London/Team Preferences',
			DenyReason: ''
		};
		var clickEvent = new MouseEvent('click');

		controller.toggleAbsenceSchedules(absence, clickEvent);
		expect(controller.shifts[0].Date).toEqual('11/23/18');
		expect(controller.shifts[0].ShiftStartTime).toEqual('8:00 AM');
		expect(controller.shifts[0].ShiftEndTime).toEqual('5:00 PM');
	});

	it('should get absence shift start on today and shift end on next day with +1 when toggled absence schedules', function() {
		var absence = {
			IsFullDay: false,
			PersonAccountSummaryViewModel: {
				PersonAccountSummaryDetails: [
					{
						StartDate: '2017-01-01T02:00:00',
						RemainingDescription: '19',
						TrackingTypeDescription: 'Days',
						EndDate: '2026-11-10T02:00:00'
					}
				]
			},
			Shifts: [
				{
					Name: 'Ashley Andeen',
					Periods: [
						{
							Title: 'Social Media',
							TimeSpan: '7:00 PM - 9:15 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T19:00:00',
							EndTime: '2018-11-23T21:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.0,
							EndPositionPercentage: 0.25,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '9:15 PM - 9:30 PM',
							Color: '255,0,0',
							StartTime: '2018-11-23T21:15:00',
							EndTime: '2018-11-23T21:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.25,
							EndPositionPercentage: 0.2778,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '9:30 PM - 11:30 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T21:30:00',
							EndTime: '2018-11-23T23:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.2778,
							EndPositionPercentage: 0.5,
							Meeting: null
						},
						{
							Title: 'Lunch',
							TimeSpan: '11:30 PM - 12:30 AM',
							Color: '255,255,0',
							StartTime: '2018-11-23T23:30:00',
							EndTime: '2018-11-24T00:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.5,
							EndPositionPercentage: 0.6111,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '12:30 AM - 2:15 AM',
							Color: '30,144,255',
							StartTime: '2018-11-24T00:30:00',
							EndTime: '2018-11-24T02:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.6111,
							EndPositionPercentage: 0.8056,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '2:15 AM - 2:30 AM',
							Color: '255,0,0',
							StartTime: '2018-11-24T02:15:00',
							EndTime: '2018-11-24T02:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8056,
							EndPositionPercentage: 0.8333,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '2:30 AM - 4:00 AM',
							Color: '30,144,255',
							StartTime: '2018-11-24T02:30:00',
							EndTime: '2018-11-24T04:00:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8333,
							EndPositionPercentage: 1.0,
							Meeting: null
						}
					],
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					ShiftCategory: {
						Id: null,
						ShortName: 'PM',
						Name: 'Late',
						DisplayColor: '#000000'
					},
					BelongsToDate: '2018-11-23T00:00:00'
				}
			],
			Subject: 'subject',
			Message: '',
			AgentName: 'Ashley Andeen',
			PersonId: '11610fe4-0130-4568-97de-9b5e015b2564',
			TimeZone: 'Asia/Amman',
			Id: '7cfc4367-0fdd-4c31-981f-a9a1003bde07',
			Seniority: 23,
			PeriodStartTime: '2018-11-23T19:00:00',
			PeriodEndTime: '2018-11-24T04:00:00',
			UpdatedTime: '2018-11-23T05:37:58.563',
			CreatedTime: '2018-11-23T05:37:58.21',
			Type: 1,
			TypeText: 'Absence',
			Status: 1,
			StatusText: 'Pending',
			Payload: {
				Name: 'Holiday',
				ShortName: 'HO'
			},
			Team: 'London/Team Preferences',
			DenyReason: ''
		};
		var clickEvent = new MouseEvent('click');

		controller.toggleAbsenceSchedules(absence, clickEvent);
		expect(controller.shifts[0].Date).toEqual('11/23/18');
		expect(controller.shifts[0].ShiftStartTime).toEqual('7:00 PM');
		expect(controller.shifts[0].ShiftEndTime).toEqual('4:00 AM+1');
	});

	it('should convert the shift start and end time to selected timezone when toggled absence schedules', function() {
		var absence = {
			IsFullDay: false,
			PersonAccountSummaryViewModel: {
				PersonAccountSummaryDetails: [
					{
						StartDate: '2017-01-01T02:00:00',
						RemainingDescription: '19',
						TrackingTypeDescription: 'Days',
						EndDate: '2026-11-10T02:00:00'
					}
				]
			},
			Shifts: [
				{
					Name: 'Ashley Andeen',
					Periods: [
						{
							Title: 'Social Media',
							TimeSpan: '7:00 PM - 9:15 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T19:00:00',
							EndTime: '2018-11-23T21:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.0,
							EndPositionPercentage: 0.25,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '9:15 PM - 9:30 PM',
							Color: '255,0,0',
							StartTime: '2018-11-23T21:15:00',
							EndTime: '2018-11-23T21:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.25,
							EndPositionPercentage: 0.2778,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '9:30 PM - 11:30 PM',
							Color: '30,144,255',
							StartTime: '2018-11-23T21:30:00',
							EndTime: '2018-11-23T23:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.2778,
							EndPositionPercentage: 0.5,
							Meeting: null
						},
						{
							Title: 'Lunch',
							TimeSpan: '11:30 PM - 12:30 AM',
							Color: '255,255,0',
							StartTime: '2018-11-23T23:30:00',
							EndTime: '2018-11-24T00:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.5,
							EndPositionPercentage: 0.6111,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '12:30 AM - 2:15 AM',
							Color: '30,144,255',
							StartTime: '2018-11-24T00:30:00',
							EndTime: '2018-11-24T02:15:00',
							IsOvertime: false,
							StartPositionPercentage: 0.6111,
							EndPositionPercentage: 0.8056,
							Meeting: null
						},
						{
							Title: 'Short break',
							TimeSpan: '2:15 AM - 2:30 AM',
							Color: '255,0,0',
							StartTime: '2018-11-24T02:15:00',
							EndTime: '2018-11-24T02:30:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8056,
							EndPositionPercentage: 0.8333,
							Meeting: null
						},
						{
							Title: 'Social Media',
							TimeSpan: '2:30 AM - 4:00 AM',
							Color: '30,144,255',
							StartTime: '2018-11-24T02:30:00',
							EndTime: '2018-11-24T04:00:00',
							IsOvertime: false,
							StartPositionPercentage: 0.8333,
							EndPositionPercentage: 1.0,
							Meeting: null
						}
					],
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					ShiftCategory: {
						Id: null,
						ShortName: 'PM',
						Name: 'Late',
						DisplayColor: '#000000'
					},
					BelongsToDate: '2018-11-23T00:00:00'
				}
			],
			Subject: 'subject',
			Message: '',
			AgentName: 'Ashley Andeen',
			PersonId: '11610fe4-0130-4568-97de-9b5e015b2564',
			TimeZone: 'Asia/Amman',
			Id: '7cfc4367-0fdd-4c31-981f-a9a1003bde07',
			Seniority: 23,
			PeriodStartTime: '2018-11-23T19:00:00',
			PeriodEndTime: '2018-11-24T04:00:00',
			UpdatedTime: '2018-11-23T05:37:58.563',
			CreatedTime: '2018-11-23T05:37:58.21',
			Type: 1,
			TypeText: 'Absence',
			Status: 1,
			StatusText: 'Pending',
			Payload: {
				Name: 'Holiday',
				ShortName: 'HO'
			},
			Team: 'London/Team Preferences',
			DenyReason: ''
		};
		var clickEvent = new MouseEvent('click');

		controller.isUsingRequestSubmitterTimeZone = true;
		controller.userTimeZone = 'Europe/Berlin';

		controller.toggleAbsenceSchedules(absence, clickEvent);
		expect(controller.shifts[0].Date).toEqual('11/23/18');
		expect(controller.shifts[0].ShiftStartTime).toEqual('8:00 PM');
		expect(controller.shifts[0].ShiftEndTime).toEqual('5:00 AM+1');
	});

	function setUpTarget() {
		scope = $rootScope.$new();
		controller = $controller('requestsAbsenceAndTextController', {
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

		this.getAbsenceAndTextRequestsPromise = function() {
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

		this.getHasSentRequests = function() {
			return _hasSentRequests;
		};

		this.getLastRequestParameters = function() {
			return _lastRequestParameters;
		};

		this.getRequestTypes = function() {
			return {
				then: function(cb) {
					cb({
						data: [
							{ Id: '00', Name: 'Absence0' },
							{ Id: '01', Name: 'Absence1' },
							{ Id: '0', Name: 'Text' }
						]
					});
				}
			};
		};

		this.getAbsenceAndTextRequestsStatuses = function() {
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

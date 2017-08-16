'use strict';
describe('Requests shift trade controller tests',
	function () {
		var $rootScope, $filter, $compile, $controller, $httpBackend, requestsDataService, requestsDefinitions, requestsNotificationService, currentUserInfo;

		var period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		},
			fakeStateParams = {
				agentSearchTerm: '',
				selectedTeamIds: [],
				filterEnabled: false,
				onInitCallBack: undefined,
				paging: {},
				isUsingRequestSubmitterTimeZone: undefined,
				getPeriod: function () {
					return period;
				}
			};

		var controller, scope;

		beforeEach(function () {
			module('wfm.templates');
			module('wfm.requests');
			requestsDataService = new FakeRequestsDataService();
			requestsNotificationService = new FakeRequestsNotificationService();
			currentUserInfo = new FakeCurrentUserInfo();

			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_Basic_35986: true,
						Wfm_Requests_People_Search_36294: true,
						Wfm_Requests_Performance_36295: true,
						Wfm_Requests_ApproveDeny_36297: true,
						Wfm_Requests_Filtering_37748: true,
						Wfm_Requests_Default_Status_Filter_39472: true,
						Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309: true,
						Wfm_Requests_Save_Grid_Columns_37976: true,
						Wfm_Requests_Show_Pending_Reasons_39473: true,
						togglesLoaded: {
							then: function (cb) { cb(); }
						}
					}
				});
				$provide.service('requestsDataService', function () {
					return requestsDataService;
				});
				$provide.service('requestsNotificationService', function () {
					return requestsNotificationService;
				});
				$provide.service('CurrentUserInfo', function () {
					return currentUserInfo;
				});
			});
		});

		beforeEach(inject(function (_$filter_, _$compile_, _$rootScope_, _$controller_, _$httpBackend_, _requestsDefinitions_) {
			$filter = _$filter_;
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			$controller = _$controller_;
			$httpBackend = _$httpBackend_;
			requestsDefinitions = _requestsDefinitions_;

			$httpBackend.whenGET('../api/Absence/GetRequestableAbsences').respond([{
				'Id': '47d9292f-ead6-40b2-ac4f-9b5e015ab330',
				'Name': 'Holiday',
				'ShortName': 'HO'
			},
			{
				'Id': '041db668-3185-4d7a-8781-9b5e015ab330',
				'Name': 'Time off in lieu',
				'ShortName': 'TL'
			}]);

			setUpTarget();
		}));

		it('populate requests data from requests data service', function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
			requestsDataService.setRequests([request]);

			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			scope.$digest();

			expect(controller.requests.length).toEqual(1);
			expect(controller.requests[0]).toEqual(request);
		});

		it('should not populate requests data from requests data service when no team selected and organization picker is on', function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
			requestsDataService.setRequests([request]);

			fakeStateParams.selectedTeamIds = [];
			compileUIGridHtml(scope, controller.gridOptions);

			expect(requestsDataService.getHasSentRequests()).toBeFalsy();
			expect(controller.requests.length).toEqual(0);
		});

		it('should not request data when period is invalid', function () {
			requestsDataService.setRequests([]);
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			requestsDataService.reset();
			period = {
				startDate: moment().add(1, 'day').toDate(),
				endDate: new Date()
			};
			scope.$digest();

			expect(requestsDataService.getHasSentRequests()).toBeFalsy();
		});

		it('should request data when period is valid', function () {
			requestsDataService.setRequests([]);
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			requestsDataService.reset();
			period = {
				startDate: new Date(),
				endDate: moment().add(2, 'day').toDate()
			};
			scope.$digest();

			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
		});

		it('should request data when search term changed', function () {
			requestsDataService.setRequests([]);
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			requestsDataService.reset();

			controller.agentSearchTerm = 'search term';
			scope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual('search term');
		});

		it('should set isLoading to false after reload requests action finished', function () {
			requestsDataService.setRequests([]);

			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			requestsDataService.reset();

			controller.isLoading = true;
			controller.agentSearchTerm = 'search term';
			scope.$digest();
			expect(controller.isLoading).toBeFalsy();
		});

		it('should show pending shift trade request only by default', function () {
			compileUIGridHtml(scope, controller.gridOptions);
			scope.$digest();

			expect(controller.filters[0].Status).toEqual('0');
		});

		it('should not call data service more than once after initialized', function () {
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			period = {
				startDate: moment().add(-1, 'd')._d,
				endDate: moment().add(1, 'd')._d
			};
			scope.$digest();

			expect(requestsDataService.getCallCounts()).toEqual(1);
		});

		it('should display error when searching person count is exceeded', function () {
			requestsDataService.setGetAllRequetsCallbackData({
				IsSearchPersonCountExceeded: true,
				MaxSearchPersonCount: 5000
			});

			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			scope.$digest();

			expect(controller.requests.length).toEqual(0);
			expect(requestsNotificationService.getNotificationResult()).toEqual(5000);
		});

		it('should see UI Grid', function () {
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

			expect(results.some(function (item) {
				return angular.element(item).hasClass('ui-grid-contents-wrapper');
			})).toBeTruthy();
		});

		it('see table rows for each request', function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
			requestsDataService.setRequests([request, request]);

			fakeStateParams.selectedTeamIds = ['team'];
			var element = compileUIGridHtml(scope, controller.gridOptions);
			scope.$digest();

			var targets = element[0].querySelectorAll('.ui-grid-render-container-body .ui-grid-row');

			expect(targets.length).toEqual(2);
		});

		it('startTime, endTime, createdTime and updatedTime columns should shown in the same timezone as backend says', function () {
			fakeStateParams.selectedTeamIds = ['team'];
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

		it('should be able to switch between user timezone and request submitter timezone', function () {
			currentUserInfo.setCurrentUserInfo('Europe/Berlin');
			setUpTarget();
			setUpShiftTradeRequestData();

			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			requestsDataService.setRequests(scope.requests);
			scope.$digest();

			scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', false);
			expect(controller.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T05:00:00'));

			scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', true);
			expect(controller.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T14:00:00'));
		});

		it("should be able to calculate columns for weeks using supplied period startofweek", function () {
			currentUserInfo.setCurrentUserInfo('Pacific/Port_Moresby');
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			requestsDataService.setRequests(scope.requests, dateSummary);

			scope.$digest();

			var dayViewModels = controller.shiftTradeDayViewModels;

			expect(dayViewModels[0].shortDate).toEqual(toShortDateString('2016-05-25T00:00:00'));
			expect(dayViewModels[dayViewModels.length - 1].shortDate).toEqual(toShortDateString('2016-06-02T00:00:00'));
			expect(dayViewModels[3].isWeekend).toEqual(true);
			expect(dayViewModels[4].isWeekend).toEqual(true);
			expect(dayViewModels[5].isStartOfWeek).toEqual(true);
		});

		it("should generate view models for shift trade days", function () {
			currentUserInfo.setCurrentUserInfo('Pacific/Port_Moresby');
			setUpShiftTradeRequestData();

			var dateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];

			compileUIGridHtml(scope, controller.gridOptions);
			requestsDataService.setRequests(scope.requests, dateSummary);
			scope.$digest();

			var shiftTradeDaysViewModels = controller.shiftTradeScheduleViewModels[1]; // using request ID '1'.

			expect(shiftTradeDaysViewModels[0].FromScheduleDayDetail.Type).toEqual(requestsDefinitions.SHIFT_OBJECT_TYPE.PersonAssignment);
			expect(shiftTradeDaysViewModels[1].ToScheduleDayDetail.Type).toEqual(requestsDefinitions.SHIFT_OBJECT_TYPE.DayOff);
			expect(shiftTradeDaysViewModels[1].ToScheduleDayDetail.IsDayOff).toEqual(true);

			expect(shiftTradeDaysViewModels[0].ToScheduleDayDetail.Name).toEqual('name-to-1');
			expect(shiftTradeDaysViewModels[1].FromScheduleDayDetail.Name).toEqual('name-from-2');

			expect(shiftTradeDaysViewModels[0].LeftOffset).toEqual(requestsDefinitions.SHIFTTRADE_COLUMN_WIDTH * 2 + 'px'); // starts two days after start of period.
			expect(shiftTradeDaysViewModels[1].LeftOffset).toEqual(requestsDefinitions.SHIFTTRADE_COLUMN_WIDTH * 3 + 'px');
		});

		it('should select default status filter', function () {
			compileUIGridHtml(scope, controller.gridOptions);

			var status0 = '79';
			var status1 = '86';
			var status2 = '93';
			controller.filters = [{ 'Status': status0 + ' ' + status1 + ' ' + status2 }];
			scope.$digest();

			var selectedStatus = controller.SelectedRequestStatuses;

			expect(selectedStatus.length).toEqual(3);
			expect(selectedStatus[0].Id).toEqual(status0.trim());
			expect(selectedStatus[1].Id).toEqual(status1.trim());
			expect(selectedStatus[2].Id).toEqual(status2.trim());
		});

		it('should get broken rules column', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			var brokenRules = ["Not allowed change", "Weekly rest time"];
			scope.requests[0].BrokenRules = brokenRules;
			requestsDataService.setRequests(scope.requests, dateSummary);
			scope.$digest();

			var columnDefs = controller.gridOptions.columnDefs;
			var existsBrokenRulesColmun;
			angular.forEach(columnDefs,
				function (columnDef) {
					if (columnDef.displayName === "BrokenRules") {
						existsBrokenRulesColmun = true;
					}
				});

			expect(existsBrokenRulesColmun).toEqual(true);
			expect(controller.requests[0].GetBrokenRules(), brokenRules.join(","));
		});

		it('should get shift trade schedule view with one of schedule day is empty', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			var shiftTradeDay = scope.requests[0].ShiftTradeDays[0];
			shiftTradeDay.ToScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };

			requestsDataService.setRequests(scope.requests, dateSummary);
			scope.$digest();

			expect(controller.shiftTradeScheduleViewModels[scope.requests[0].Id].length).toEqual(2);
		});

		it('should not get shift trade schedule view with both schedule days are empty', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);
			var shiftTradeDay = scope.requests[0].ShiftTradeDays[0];
			shiftTradeDay.ToScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };
			shiftTradeDay.FromScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };

			requestsDataService.setRequests(scope.requests, dateSummary);
			scope.$digest();

			expect(controller.shiftTradeScheduleViewModels[scope.requests[0].Id].length).toEqual(1);
		});

		it('should display correct time for DST', function () {
			fakeStateParams.selectedTeamIds = ['team'];
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

		it('should display time in shift trade day view according to logon user timezone', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			var submitterTimezone = 'Europe/Berlin';
			var  requests = [{ Id: 1, PeriodStartTime: '2017-01-09T00:00:00', PeriodEndTime: '2017-01-09T23:59:00', CreatedTime: '2017-01-03T05:54:12', TimeZone: submitterTimezone, UpdatedTime: '2017-01-03T05:54:50' }];

			requestsDataService.setRequests(requests, dateSummary);
			scope.$digest();

			var request = controller.requests[0];
			expect(request.FormatedPeriodStartTime()).toEqual(toDateString('2017-01-08T23:00:00'));
			expect(request.FormatedPeriodEndTime()).toEqual(toDateString('2017-01-09T22:59:00'));
		});

		it('should get dayViewModels according to logon user timezone', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			var submitterTimezone = 'Europe/Berlin';
			var requests = [
				{
					Id: 1,
					PeriodStartTime: '2017-01-09T00:00:00',
					PeriodEndTime: '2017-01-09T23:59:00',
					CreatedTime: '2017-01-03T05:54:12',
					TimeZone: submitterTimezone, UpdatedTime: '2017-01-03T05:54:50',
					ShiftTradeDays: [
						{
							Date: "2017-01-09T00:00:00",
							FromScheduleDayDetail: { Name: "Day", Type: 1, ShortName: "DY", Color: "#FFC080" },
							ToScheduleDayDetail: { Name: "Day", Type: 1, ShortName: "DY", Color: "#FFC080" }
						}
					]
				}];
			requestsDataService.setRequests(requests, dateSummary);
			scope.$digest();

			var shiftTradeDayViewModels = controller.shiftTradeDayViewModels;
			var shiftTradeScheduleViewModels = controller.shiftTradeScheduleViewModels;

			var expectedTimezone = "Atlantic/Reykjavik";
			var expectedDate = moment(controller.shiftTradeRequestDateSummary.Minimum);

			for (var i = 0; i < shiftTradeDayViewModels.length - 1; i++) {
				var dayViewModel = shiftTradeDayViewModels[i];
				expect(dayViewModel.originalDate).toEqual(expectedDate.toDate());
				expect(dayViewModel.targetTimezone).toEqual(expectedTimezone);
				expect(dayViewModel.dayNumber).toEqual("0" + (i + 1));
				expectedDate.add(1, "days");
			}

			var scheduleViewModel = shiftTradeScheduleViewModels[1][0];
			expect(scheduleViewModel.originalDate).toEqual(expectedDate.toDate());
			expect(scheduleViewModel.targetTimezone).toEqual(expectedTimezone);
			expect(scheduleViewModel.dayNumber).toEqual("08");
		});

		it('should get dayViewModels according to requester timezone', function () {
			setUpShiftTradeRequestData();
			var dateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			fakeStateParams.selectedTeamIds = ['team'];
			controller.isUsingRequestSubmitterTimeZone = true;
			compileUIGridHtml(scope, controller.gridOptions);

			var submitterTimezone = 'Europe/Berlin';
			var requests = [
				{
					Id: 1,
					PeriodStartTime: '2017-01-09T00:00:00',
					PeriodEndTime: '2017-01-09T23:59:00',
					CreatedTime: '2017-01-03T05:54:12',
					TimeZone: submitterTimezone,
					UpdatedTime: '2017-01-03T05:54:50',
					ShiftTradeDays: [
						{
							Date: "2017-01-09T00:00:00",
							FromScheduleDayDetail: { Name: "Day", Type: 1, ShortName: "DY", Color: "#FFC080" },
							ToScheduleDayDetail: { Name: "Day", Type: 1, ShortName: "DY", Color: "#FFC080" }
						}
					]
				}];
			requestsDataService.setRequests(requests, dateSummary);
			scope.$digest();
		
			var shiftTradeDayViewModels = controller.shiftTradeDayViewModels;
			var shiftTradeScheduleViewModels = controller.shiftTradeScheduleViewModels;

			var expectedTimezone = "Europe/Berlin";
			var expectedDate = moment(controller.shiftTradeRequestDateSummary.Minimum);

			for (var i = 0; i < shiftTradeDayViewModels.length - 1; i++) {
				var dayViewModel = shiftTradeDayViewModels[i];
				expect(dayViewModel.originalDate).toEqual(expectedDate.toDate());
				expect(dayViewModel.targetTimezone).toEqual(expectedTimezone);
				expect(dayViewModel.dayNumber).toEqual("0" + (i + 2));
				expectedDate.add(1, "days");
			}

			var scheduleViewModel = shiftTradeScheduleViewModels[1][0];
			expect(scheduleViewModel.originalDate).toEqual(expectedDate.toDate());
			expect(scheduleViewModel.targetTimezone).toEqual(expectedTimezone);
			expect(scheduleViewModel.dayNumber).toEqual("09");
		});

		it('should clear subject and message filters', function () {
			fakeStateParams.selectedTeamIds = ['team'];
			compileUIGridHtml(scope, controller.gridOptions);

			controller.subjectFilter = ['a'];
			controller.messageFilter = ['b'];
			scope.$digest();

			controller.clearAllFilters();
			expect(controller.subjectFilter).toEqual(undefined);
			expect(controller.messageFilter).toEqual(undefined);
		});

		function setUpShiftTradeRequestData() {
			var shiftTradeDays = [
				{
					Date: '2016-05-27T00:00:00',
					FromScheduleDayDetail: { Name: "name-from-1", ShortName: "shortname-from-1", Color: "red", Type: requestsDefinitions.SHIFT_OBJECT_TYPE.PersonAssignment },
					ToScheduleDayDetail: { Name: "name-to-1", ShortName: "shortname-to-1", Color: "red", Type: requestsDefinitions.SHIFT_OBJECT_TYPE.PersonAssignment }
				},
				{
					Date: '2016-05-28T00:00:00',
					FromScheduleDayDetail: { Name: "name-from-2", ShortName: "shortname-from-2", Color: "yellow", Type: requestsDefinitions.SHIFT_OBJECT_TYPE.PersonAssignment },
					ToScheduleDayDetail: { Name: "name-to-2", ShortName: "shortname-to-2", Color: "yellow", Type: requestsDefinitions.SHIFT_OBJECT_TYPE.DayOff }
				}
			];

			scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-06T14:00:00', PeriodEndTime: '2016-01-09T20:00:00', CreatedTime: '2016-01-06T10:17:31', TimeZone: 'Pacific/Port_Moresby', UpdatedTime: '2016-01-06T10:17:31', IsFullDay: false, ShiftTradeDays: shiftTradeDays }];
			scope.shiftTradeView = true;
		}

		function toShortDateString(dateString) {
			return $filter('date')(moment(dateString).toDate(), "shortDate");
		}

		function setUpTarget() {
			scope = $rootScope.$new();
			controller = $controller('requestsShiftTradeCtrl', {
				$scope: scope,
				$stateParams: fakeStateParams
			});
		}

		function compileUIGridHtml(scope, gridOptions) {
			var html = '<div ui-grid="gridOptions" class="ui-grid requests-uigrid-table" ui-grid-auto-resize ui-grid-selection ui-grid-resize-columns ui-grid-pinning ui-grid-save-state ui-grid-disable-animation></div>';

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

			this.reset = function () {
				_requests = [];
				_hasSentRequests = false;
				_lastRequestParameters = null;
				_callCounts = 0;
				_getAllRequetsCallbackData = {
					Requests: _requests,
					TotalCount: _requests.length
				}
			};

			this.setRequests = function (requests, dateSummary) {
				_requests = requests;
				_getAllRequetsCallbackData = {
					Requests: _requests,
					TotalCount: _requests.length,
					MinimumDateTime: dateSummary && dateSummary.Minimum,
					MaximumDateTime: dateSummary && dateSummary.Maximum,
					FirstDayOfWeek: dateSummary && dateSummary.FirstDayOfWeek
				}
			};

			this.setGetAllRequetsCallbackData = function (data) {
				_getAllRequetsCallbackData = data;
			};

			this.getHasSentRequests = function () { return _hasSentRequests; };
			this.getLastRequestParameters = function () { return _lastRequestParameters; };

			this.getAllRequestsPromise_old = function () {
				_hasSentRequests = true;
				_lastRequestParameters = arguments;
				return {
					then: function (cb) {
						cb({ data: _requests });
					}
				};
			};

			this.getAllRequestsPromise = function () {
				_hasSentRequests = true;
				_lastRequestParameters = arguments;
				return {
					then: function (cb) {
						_callCounts++;
						cb({
							data: _getAllRequetsCallbackData
						});
					}
				}
			};

			this.getShiftTradeRequestsPromise = function () {
				_hasSentRequests = true;
				_lastRequestParameters = arguments;
				return {
					then: function (cb) {
						_callCounts++;
						cb({
							data: _getAllRequetsCallbackData
						});
					}
				}
			}

			this.getRequestTypes = function () {
				return {
					then: function (cb) {
						cb({
							data: [
								{ Id: '00', Name: 'Absence0' },
								{ Id: '01', Name: 'Absence1' },
								{ Id: '0', Name: 'Text' }
							]
						});
					}
				}
			};

			this.getAllRequestStatuses = function () {
				return [
					{ Id: 0, Name: 'Status0' },
					{ Id: 1, Name: 'Status1' }
				];
			};

			this.getCallCounts = function () {
				return _callCounts;
			};
		}

		function FakeRequestsNotificationService() {
			var _notificationResult;
			this.notifyMaxSearchPersonCountExceeded = function (maxSearchPersonCount) {
				_notificationResult = maxSearchPersonCount;
			};

			this.getNotificationResult = function () {
				return _notificationResult;
			};
		}

		function FakeCurrentUserInfo() {
			var defaultTimeZone = 'Atlantic/Reykjavik';
			return {
				CurrentUserInfo: function () {
					return {
						DefaultTimeZone: defaultTimeZone
					};
				},
				setCurrentUserInfo: function (timezone) {
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
		};
	});
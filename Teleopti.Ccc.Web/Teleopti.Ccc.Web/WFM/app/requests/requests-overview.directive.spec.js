(function () {
	'use strict';

	describe('Requests overview directive', function () {
		var $compile, $rootScope, requestsDataService, requestsDefinitions, $injector, requestsFilterService;

		var targetElement, targetScope;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();

			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_Basic_35986: true,
						Wfm_Requests_People_Search_36294: true,
						Wfm_Requests_Performance_36295: true,
						Wfm_Requests_ApproveDeny_36297: true,
						Wfm_Requests_Filtering_37748: true,
						Wfm_Requests_Default_Status_Filter_39472: true,
						togglesLoaded: {
							then: function (cb) { cb(); }
						}
					}
				});

				$provide.service('requestsDataService', function () {
					return requestsDataService;
				});
			});
		});

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDataService_, _requestsDefinitions_, _$injector_, _RequestsFilter_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
			$injector = _$injector_;
			requestsFilterService = _RequestsFilter_;
		}));

		it("show requests table container", function () {
			requestsDataService.setRequests([]);
			targetElement = $compile('<requests-overview ></requests-overview>')(targetScope);
			targetScope.$digest();
			var targets = targetElement.find('requests-table-container');
			expect(targets.length).toEqual(1);
		});

		it("populate requests data from requests data service", function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};

			requestsDataService.setRequests([request]);
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			scope.requestsOverview.init();
			targetScope.$digest();
			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		it("should not populate requests data from requests data service when inactive", function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};

			requestsDataService.setRequests([request]);
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = false;
			scope.requestsOverview.init();
			targetScope.$digest();
			expect(scope.requestsOverview.requests.length).toEqual(0);
		});

		it("should not request data when filter contains error", function () {
			requestsDataService.setRequests([]);

			targetScope.period = {};
			targetElement = $compile('<requests-overview period="period"></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			requestsDataService.reset();
			targetScope.period = {
				startDate: moment().add(1, 'day').toDate(),
				endDate: new Date()
			};

			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeFalsy();
		});

		it("should request data when filter change to valid values", function () {
			requestsDataService.setRequests([]);
			targetScope.period = {};
			targetElement = $compile('<requests-overview period="period"></requests-overview>')(targetScope);

			targetScope.$digest();

			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;

			requestsDataService.reset();

			targetScope.period = {
				startDate: new Date(),
				endDate: moment().add(2, 'day').toDate()
			}

			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
		});

		it("should request data when search term changed", function () {
			requestsDataService.setRequests([]);
			targetScope.period = {};
			targetScope.agentSearchTerm = "";

			targetElement = $compile('<requests-overview period="period" agent-search-term="agentSearchTerm"></requests-overview>')(targetScope);

			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			requestsDataService.reset();

			targetScope.agentSearchTerm = "search term";
			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual("search term");
		});

		it('should show selected requests information when requests get selected and nothing vice verse', function () {
			var requestIds = [{ id: 1 }, { id: 2 }];
			requestsDataService.setRequests([]);

			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();

			var requestCommandParamsHolder = $injector.get('requestCommandParamsHolder');
			requestCommandParamsHolder.setSelectedRequestIds(requestIds, false);

			var vm = getInnerScope(targetElement).requestsOverview;

			vm.selectedRequestsInfoText = 'Selected {0} of {1} requests';
			vm.paging.totalRequestsCount = 10;
			targetScope.$digest();
			expect(vm.showSelectedRequestsInfo()).toEqual('Selected 2 of 10 requests');

			requestCommandParamsHolder.setSelectedRequestIds([]);
			targetScope.$digest();
			expect(vm.showSelectedRequestsInfo()).toEqual('');
		});

		it('should show pending and waitlisted absence requests only by default', function () {
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();

			var vm = getInnerScope(targetElement).requestsOverview;
			expect(vm.filters[0].Status).toEqual('0 5');
			expect(getStatusFilter()['Status']).toEqual(vm.filters[0].Status);
		});

		it('should show pending shift trade request only by default', function () {
			targetElement = $compile('<requests-overview shift-trade-view="true"></requests-overview>')(targetScope);
			targetScope.$digest();

			var vm = getInnerScope(targetElement).requestsOverview;
			expect(vm.filters[0].Status).toEqual('0');
			expect(getStatusFilter()['Status']).toEqual(vm.filters[0].Status);
		});

		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
		}

		function getStatusFilter() {
			return requestsFilterService.Filters.find(function(filter) {
				return filter['Status'] != undefined;
			});
		}
	});

	describe('requests table container directive', function () {
		var $compile, $rootScope, requestsDefinitions, $filter, teamSchedule;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();
			teamSchedule = new FakeTeamSchedule();
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_Basic_35986: true,
						Wfm_Requests_People_Search_36294: true,
						Wfm_Requests_Performance_36295: true,
						Wfm_Requests_ApproveDeny_36297: true,
						Wfm_Requests_Filtering_37748: true,
						Wfm_Requests_ShiftTrade_More_Relevant_Information_38492: true,
						Wfm_Requests_Default_Status_Filter_39472: true,
						Wfm_Requests_Show_Pending_Reasons_39473: true,
						togglesLoaded: {
							then: function (cb) { cb(); }
						}
					}
				});

				$provide.service('requestsDataService', function () {
					return requestsDataService;
				});

				$provide.service('TeamSchedule', function () {
					return teamSchedule;
				});
			});
		});

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDefinitions_, _$filter_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDefinitions = _requestsDefinitions_;
			$filter = _$filter_;
		}));

		it('should apply template', function () {
			var test = setUpTarget();
			test.scope.$digest();
			expect(test.target.html()).not.toEqual('');
		});

		it('see UI Grid', function () {
			var test = setUpTarget();

			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-05T00:00:00', PeriodEndTime: '2016-01-07T23:59:00', CreatedTime: '2016-01-05T03:29:37', TimeZone: 'Europe/Berlin', UpdatedTime: '2016-01-05T03:29:37' }];
			test.scope.$digest();

			var targets = Array.from(test.target.children());
			expect(targets.some(function (target) { return angular.element(target).hasClass('ui-grid'); })).toBeTruthy();
		});

		it("see table rows for each request", function () {
			var test = setUpTarget();
			test.scope.requests = [{ Id: 1 }, { Id: 2 }];
			test.scope.$digest();
			var targets = test.target[0].querySelectorAll('.ui-grid-render-container-body .ui-grid-row');
			expect(targets.length).toEqual(2);
		});

		it("startTime, endTime, createdTime and updatedTime columns should shown in the same timezone as backend says", function () {
			var test = setUpTarget();

			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-05T00:00:00', PeriodEndTime: '2016-01-07T23:59:00', CreatedTime: '2016-01-05T03:29:37', TimeZone: 'Europe/Berlin', UpdatedTime: '2016-01-05T03:29:37' }];
			test.scope.$digest();

			var startTime = test.scope.requests[0].FormatedPeriodStartTime();
			var endTime = test.scope.requests[0].FormatedPeriodEndTime();
			var createdTime = test.scope.requests[0].FormatedCreatedTime();
			var updatededTime = test.scope.requests[0].FormatedUpdatedTime();

			expect(startTime).toEqual(toDateString('2016-01-05T00:00:00', 'Europe/Berlin'));
			expect(endTime).toEqual(toDateString('2016-01-07T23:59:00', 'Europe/Berlin'));
			expect(createdTime).toEqual(toDateString('2016-01-05T03:29:37', 'Europe/Berlin'));
			expect(updatededTime).toEqual(toDateString('2016-01-05T03:29:37', 'Europe/Berlin'));
		});

		it("should be able to switch between user timezone and request submitter timezone", function () {
			var test = setUpTarget();

			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-06T14:00:00', PeriodEndTime: '2016-01-09T20:00:00', CreatedTime: '2016-01-06T10:17:31', TimeZone: 'Pacific/Port_Moresby', UpdatedTime: '2016-01-06T10:17:31', IsFullDay: false }];

			test.scope.$digest();
			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.userTimeZone = 'Europe/Berlin';
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = false;
			test.scope.$digest();

			expect(test.scope.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T05:00:00', 'Europe/Berlin'));

			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;
			test.scope.$digest();

			expect(test.scope.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T14:00:00'));
		});

		it("should be able to calculate columns for weeks using supplied period startofweek", function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};

			test.scope.$digest();
			var vm = test.target.isolateScope().requestsTableContainer;

			var dayViewModels = vm.shiftTradeDayViewModels;

			expect(dayViewModels[0].shortDate).toEqual(toShortDateString('2016-05-25T00:00:00'));
			expect(dayViewModels[dayViewModels.length - 1].shortDate).toEqual(toShortDateString('2016-06-02T00:00:00'));
			expect(dayViewModels[3].isWeekend).toEqual(true);
			expect(dayViewModels[4].isWeekend).toEqual(true);
			expect(dayViewModels[5].isStartOfWeek).toEqual(true);
		});

		it("should generate view models for shift trade days", function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};

			test.scope.$digest();
			var vm = test.target.isolateScope().requestsTableContainer;

			var shiftTradeDaysViewModels = vm.shiftTradeScheduleViewModels[1]; // using request ID '1'.

			expect(shiftTradeDaysViewModels[0].FromScheduleDayDetail.Type).toEqual(requestsDefinitions.SHIFT_OBJECT_TYPE.PersonAssignment);
			expect(shiftTradeDaysViewModels[1].ToScheduleDayDetail.Type).toEqual(requestsDefinitions.SHIFT_OBJECT_TYPE.DayOff);
			expect(shiftTradeDaysViewModels[1].ToScheduleDayDetail.IsDayOff).toEqual(true);

			expect(shiftTradeDaysViewModels[0].ToScheduleDayDetail.Name).toEqual('name-to-1');
			expect(shiftTradeDaysViewModels[1].FromScheduleDayDetail.Name).toEqual('name-from-2');

			expect(shiftTradeDaysViewModels[0].LeftOffset).toEqual(requestsDefinitions.SHIFTTRADE_COLUMN_WIDTH * 2 +'px'); // starts two days after start of period.
			expect(shiftTradeDaysViewModels[1].LeftOffset).toEqual(requestsDefinitions.SHIFTTRADE_COLUMN_WIDTH * 3 + 'px');

		});

		
		it('should select default status filter', function () {
			var test = setUpTarget();
			var status0 = " 79";
			var status1 = "86 ";
			var status2 = " 93 ";
			test.scope.filters = [{ "Status": status0 + " " + status1 + " " + status2 }];
			test.scope.$digest();

			var selectedStatus = test.target.isolateScope().requestsTableContainer.SelectedRequestStatuses;
			expect(selectedStatus.length).toEqual(3);
			expect(selectedStatus[0].Id).toEqual(status0.trim());
			expect(selectedStatus[1].Id).toEqual(status1.trim());
			expect(selectedStatus[2].Id).toEqual(status2.trim());
		});

		it('should get broken rules column', function () {
			var test = setUpTarget();
			setUpShiftTradeRequestData(test);
			var brokenRules = ["Not allowed change", "Weekly rest time"];
			test.scope.requests[0].BrokenRules = brokenRules;
			test.scope.$digest();

			var vm = test.target.isolateScope().requestsTableContainer;
			var columnDefs = vm.gridOptions.columnDefs;
			var existsBrokenRulesColmun;
			angular.forEach(columnDefs,
				function (columnDef) {
					if (columnDef.displayName === "BrokenRules") {
						existsBrokenRulesColmun = true;
					}
				});

			expect(existsBrokenRulesColmun).toEqual(true);
			expect(test.scope.requests[0].GetBrokenRules(), brokenRules.join(","));
		});

		it('should get shift trade schedule view with one of schedule day is empty', function () {
			var test = setUpTarget();
			setUpShiftTradeRequestData(test);

			var shiftTradeDay = test.scope.requests[0].ShiftTradeDays[0];
			shiftTradeDay.ToScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};

			test.scope.$apply();

			var vm = test.target.isolateScope().requestsTableContainer;
			expect(vm.shiftTradeScheduleViewModels[1].length).toEqual(2);
		});

		it('should not get shift trade schedule view with both schedule days are empty', function () {
			var test = setUpTarget();
			setUpShiftTradeRequestData(test);

			var shiftTradeDay = test.scope.requests[0].ShiftTradeDays[0];
			shiftTradeDay.ToScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };
			shiftTradeDay.FromScheduleDayDetail = { Color: null, Name: null, ShortName: null, Type: 0 };

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};

			test.scope.$apply();

			var vm = test.target.isolateScope().requestsTableContainer;
			expect(vm.shiftTradeScheduleViewModels[1].length).toEqual(1);
		});

		it('should not load absences for shif trade request', function() {
			var test = setUpTarget();
			test.scope.shiftTradeView = true;
			test.scope.$apply();
			var vm = test.target.isolateScope().requestsTableContainer;
			expect(angular.isDefined(vm.AllRequestableAbsences)).toEqual(false);
		});

		xit('should load schedules for shift trade request', function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				StartOfWeek: '2016-05-22T00:00:00',
				EndOfWeek: '2016-06-04T00:00:00'
			};

			test.scope.$digest();

			var vm = test.target.isolateScope().requestsTableContainer;
			vm.showShiftDetail({}, 1, 1, "2016-06-21T00:00:00");
			expect(teamSchedule.getSchedulesCallTimes()).toEqual(1);
		});

		function setUpShiftTradeRequestData(test) {
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

			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-06T14:00:00', PeriodEndTime: '2016-01-09T20:00:00', CreatedTime: '2016-01-06T10:17:31', TimeZone: 'Pacific/Port_Moresby', UpdatedTime: '2016-01-06T10:17:31', IsFullDay: false, ShiftTradeDays: shiftTradeDays }];
			test.scope.shiftTradeView = true;
		}

		function toShortDateString(dateString) {
			return $filter('date')(moment(dateString).toDate(), "shortDate");
		}

		function toDateString(date, timeZone) {
			var _isNowDST = moment.tz(timeZone).isDST();
			var _dateTime = _isNowDST ? moment(date).add(1, 'h').toDate() : moment(date).toDate();
			return $filter('date')(_dateTime, "short");
		};

		function setUpTarget() {
			var scope = $rootScope.$new();
			var directiveElem = getCompiledElement();

			function getCompiledElement() {
				var element = angular.element('<requests-table-container filters="filters" requests="requests" shift-trade-view="shiftTradeView" shift-trade-request-date-summary="shiftTradeRequestDateSummary" ></requests-table-container>');
				var compiledElement = $compile(element)(scope);
				//scope.$digest();
				return compiledElement;
			};

			return { scope: scope, target: directiveElem };
		}
	});

	function FakeTeamSchedule() {
		var searchScheduleCalledTimes = 0;
		this.getSchedules = function (date, agents) {
			return {
				then: function (cb) {
					searchScheduleCalledTimes = searchScheduleCalledTimes + 1;
				}
			}
		}
		this.getSchedulesCallTimes = function () {
			return searchScheduleCalledTimes;
		}
	}

	function FakeRequestsDataService() {
		var _requests;
		var _hasSentRequests;
		var _lastRequestParameters;

		this.reset = function () {
			_requests = [];
			_hasSentRequests = false;
			_lastRequestParameters = null;
		};

		this.setRequests = function (requests) {
			_requests = requests;
		};

		this.getHasSentRequests = function () { return _hasSentRequests; }
		this.getLastRequestParameters = function () { return _lastRequestParameters; }

		this.getAllRequestsPromise_old = function () {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function (cb) {
					cb({ data: _requests });
				}
			}
		};

		this.getAllRequestsPromise = function () {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function (cb) {
					cb({
						data: {
							Requests: _requests,
							TotalCount: _requests.length
						}
					});
				}
			}
		}

		this.getRequestableAbsences = function () {
			return {
				then: function (cb) {
					cb({
						data: [
							{ Id: "00", Name: "Absence0" },
							{ Id: "01", Name: "Absence1" }
						]
					});
				}
			}
		}

		this.getAllRequestStatuses = function () {
			return [
				{ Id: 0, Name: "Status0" },
				{ Id: 1, Name: "Status1" }
			];
		}
	}
})();

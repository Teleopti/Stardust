(function () {
	'use strict';

	describe('Requests overview directive', function () {
		var $compile, $rootScope, requestsDataService, requestsDefinitions, $injector;

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

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDataService_, _requestsDefinitions_, _$injector_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
			$injector = _$injector_;
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

		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
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

		it("should be able to calculate column categorys for weeks using supplied period startofweek", function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};


			test.scope.$digest();
			var vm = test.target.isolateScope().requestsTableContainer;

			var categories = vm.gridOptions.category;

			expect(categories[0].name).toEqual(toShortDateString('2016-05-23T00:00:00'));
			expect(categories[1].name).toEqual(toShortDateString('2016-05-30T00:00:00'));
		});

		it("should get columns representing the days involved in the shift trade, category should start Monday", function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 1
			};

			test.scope.$digest();

			var vm = test.target.isolateScope().requestsTableContainer;

			var columnDefs = vm.gridOptions.columnDefs;

			var columns = [];

			for (var i = 0; i < columnDefs.length; i++) {
				if (columnDefs[i].isShiftTradeDayColumn) {
					columns.push(columnDefs[i]);
				}
			}

			expect(columns.length).toEqual(9);

			expect(columns[0].displayName).toEqual('25');
			expect(columns[0].category).toEqual(toShortDateString('2016-05-23T00:00:00'));

			expect(columns[8].displayName).toEqual('02');
			expect(columns[8].category).toEqual(toShortDateString('2016-05-30T00:00:00'));
		});

		it("should get columns representing the days involved in the shift trade, category should start Sunday", function () {
			var test = setUpTarget();

			setUpShiftTradeRequestData(test);

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				FirstDayOfWeek: 7
			};

			test.scope.$digest();

			var vm = test.target.isolateScope().requestsTableContainer;
			var columnDefs = vm.gridOptions.columnDefs;
			var columns = [];

			for (var i = 0; i < columnDefs.length; i++) {
				if (columnDefs[i].isShiftTradeDayColumn) {
					columns.push(columnDefs[i]);
				}
			}

			expect(columns.length).toEqual(9);
			expect(columns[0].displayName).toEqual('25');
			expect(columns[0].category).toEqual(toShortDateString('2016-05-22T00:00:00'));
			expect(columns[1].category).toEqual(toShortDateString('2016-05-22T00:00:00'));
			expect(columns[8].category).toEqual(toShortDateString('2016-05-29T00:00:00'));
			expect(columns[8].displayName).toEqual('02');
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
					FromScheduleDayDetail: { Name: "name1", ShortName: "shortname1", Color: "red" }
				},
				{
					Date: '2016-05-28T00:00:00',
					FromScheduleDayDetail: { Name: "name2", ShortName: "shortname2", Color: "yellow" }
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
				var element = angular.element('<requests-table-container requests="requests" shift-trade-view="shiftTradeView" shift-trade-request-date-summary="shiftTradeRequestDateSummary" ></requests-table-container>');
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

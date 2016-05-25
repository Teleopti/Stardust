﻿(function () {
	'use strict';

	describe('Requests overview directive', function () {
		var $compile, $rootScope, requestsDataService, requestsDefinitions;

		var targetElement, targetScope;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();
			module(function ($provide) {
				$provide.service('requestsDataService', function () {
					return requestsDataService;
				});
				$provide.service('Toggle', function () {
					return new FakeToggleService();
				});
			});
		});

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDataService_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
		}));

		it("show requests table container", function () {
			requestsDataService.setRequests([]);
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
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
			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		it("should not request data when filter contains error", function () {
			requestsDataService.setRequests([]);

			targetScope.period = {};
			targetElement = $compile('<requests-overview period="period"></requests-overview>')(targetScope);
			targetScope.$digest();

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

			requestsDataService.reset();

			targetScope.agentSearchTerm = "search term";
			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual("search term");
		});

		it("should request data when pagination changed", function () {

			requestsDataService.setRequests([{ Id: 1 }, { Id: 2 }]);
			targetScope.period = {};
			targetScope.paging = {
				pageSize: 1,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 2
			};
			targetScope.isPaginationEnabled = true;


			targetElement = $compile('<requests-overview period="period"  paging="paging" toggle-pagination-enabled="isPaginationEnabled"></requests-overview>')(targetScope);

			targetScope.$digest();

			requestsDataService.reset();

			targetScope.paging.pageNumber = 2;

			targetScope.$broadcast('reload.requests.with.selection');

			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[2].pageNumber).toEqual(2);
		});

		

		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
		}
	});

	describe('requests table container directive', function () {
		var $compile, $rootScope, requestsDefinitions, $filter;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return new FakeToggleService();
				});
				$provide.service('requestsDataService', function () {
					return requestsDataService;
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
			expect(test.target.html()).not.toEqual('');
		});

		it('see UI Grid', function () {
			var test = setUpTarget();
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

		xit("should be able to calculate column categorys for weeks using supplied period startofweek", function () {
			var test = setUpTarget();

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				StartOfWeek: '2016-05-23T00:00:00'
			};

			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();

			var weeks = isolatedScope.requestsTableContainer.getWeekCategories();
			
			expect(weeks[0].name).toEqual(toShortDateString('2016-05-23T00:00:00'));
			expect(weeks[1].name).toEqual(toShortDateString('2016-05-30T00:00:00'));
			
		});

		
		xit("should get columns representing the days involved in the shift trade, starting on Monday", function () {
			var test = setUpTarget();

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				StartOfWeek: '2016-05-23T00:00:00',
				EndOfWeek: '2016-06-05T00:00:00'
			};

			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			var columns = isolatedScope.requestsTableContainer.getShiftTradeVisualisationDayColumns();

			expect(columns.length).toEqual(14);

			expect(columns[0].displayName).toEqual('Mon');
			expect(columns[0].category).toEqual(toShortDateString('2016-05-23T00:00:00'));
			
			expect(columns[13].displayName).toEqual('Sun');
			expect(columns[13].category).toEqual(toShortDateString('2016-05-30T00:00:00'));

		});


		xit("should get columns representing the days involved in the shift trade, starting on Sunday", function () {
			var test = setUpTarget();

			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2016-05-25T00:00:00',
				Maximum: '2016-06-02T00:00:00',
				StartOfWeek: '2016-05-22T00:00:00',
				EndOfWeek: '2016-06-04T00:00:00'
			};

			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			var columns = isolatedScope.requestsTableContainer.getShiftTradeVisualisationDayColumns();

			expect(columns.length).toEqual(14);

			expect(columns[0].displayName).toEqual('Sun');
			expect(columns[0].category).toEqual(toShortDateString('2016-05-22T00:00:00'));

			expect(columns[1].category).toEqual(toShortDateString('2016-05-22T00:00:00'));

			expect(columns[13].category).toEqual(toShortDateString('2016-05-29T00:00:00'));

			expect(columns[13].displayName).toEqual('Sat');


		});


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
				var element = angular.element('<requests-table-container requests="requests" shift-trade-request-date-summary="shiftTradeRequestDateSummary" ></requests-table-container>');
				var compiledElement = $compile(element)(scope);
				scope.$digest();
				return compiledElement;
			};

			return { scope: scope, target: directiveElem };
		}
	});

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

	function FakeToggleService() {
		this.Wfm_Requests_Filtering_37748 = true;
	}
})();
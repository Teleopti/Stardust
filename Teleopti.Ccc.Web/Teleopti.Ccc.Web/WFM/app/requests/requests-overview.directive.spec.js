(function () {
	'use strict';

	describe('[Requests overview directive]', function () {
		var $compile, $rootScope, requestsDataService, requestsDefinitions, $injector, requestsFilterService, requestsNotificationService;

		var targetElement, targetScope;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();
			requestsNotificationService = new FakeRequestsNotificationService();

			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_People_Search_36294: true,
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
			});
		});

		beforeEach(inject(function (
			_$compile_,
			_$rootScope_,
			_requestsDataService_,
			_requestsDefinitions_,
			_$injector_,
			_RequestsFilter_,
			_requestsNotificationService_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
			$injector = _$injector_;
			requestsFilterService = _RequestsFilter_;
			requestsNotificationService = _requestsNotificationService_;
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
			targetScope.initFooter = function () { };
			targetElement = $compile('<requests-overview ' +
				'on-init-call-back="initCallBack(count)"' +
				'period="absencePeriod"></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			scope.requestsOverview.selectedTeamIds = ["team"];
			scope.requestsOverview.init();
			targetScope.$digest();

			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: ''});

			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		it("should not populate requests data from requests data service when no team selected and organization picker is on", function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};

			requestsDataService.setRequests([request]);
			targetScope.agentSearchTerm = '';
			targetScope.selectedTeamIds = [];
			targetElement = $compile('<requests-overview agent-search-term="agentSearchTerm" selected-team-ids="selectedTeamIds"></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			
			targetScope.$broadcast('reload.requests.with.selection');

			expect(scope.requestsOverview.requests.length).toEqual(0);
		});

		it("should not populate requests data from requests data service when inactive", function () {
			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};

			requestsDataService.setRequests([request]);
			targetElement = $compile('<requests-overview ></requests-overview>')(targetScope);
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
			scope.requestsOverview.selectedTeamIds = ["team"];
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
			targetScope.initFooter = function () { };
			targetElement = $compile('<requests-overview on-init-call-back="initFooter(count)" period="period"></requests-overview>')(targetScope);
			targetScope.$digest();

			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			scope.requestsOverview.selectedTeamIds = ["team"];

			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: ''});

			requestsDataService.reset();

			targetScope.period = {
				startDate: new Date(),
				endDate: moment().add(2, 'day').toDate()
			};

			targetScope.$digest();

			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
		});

		it("should request data when search term changed", function () {
			requestsDataService.setRequests([]);
			targetScope.period = {};
			targetScope.agentSearchTerm = "";
			targetScope.initFooter = function () { };

			targetElement = $compile('<requests-overview on-init-call-back="initFooter(count)" period="period" agent-search-term="agentSearchTerm"></requests-overview>')(targetScope);

			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = true;
			scope.requestsOverview.selectedTeamIds = ["team"];
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: ''});
			targetScope.$digest();
			requestsDataService.reset();

			targetScope.agentSearchTerm = 'search term';
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: "search term"});

			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual("search term");
		});

		it("should set isLoading to false after reload requests action finished", function () {
			requestsDataService.setRequests([]);
			targetScope.period = {};
			targetScope.agentSearchTerm = "";
			targetScope.initFooter = function () { };
			targetElement = $compile('<requests-overview on-init-call-back="initFooter(count)" period="period" agent-search-term="agentSearchTerm"></requests-overview>')(targetScope);

			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isLoading = true;
			scope.requestsOverview.isActive = true;
			scope.requestsOverview.selectedTeamIds = ["team"];

			expect(scope.requestsOverview.isLoading).toBeTruthy();

			targetScope.agentSearchTerm = 'search term';
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: 'search term'});

			expect(scope.requestsOverview.isLoading).toBeFalsy();
		});

		it('should show pending and waitlisted absence requests only by default', function () {
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();

			var vm = getInnerScope(targetElement).requestsOverview;
			expect(vm.filters[0].Status).toEqual('0 5');
		});

		it('should show pending shift trade request only by default', function () {
			targetElement = $compile('<requests-overview shift-trade-view="true"></requests-overview>')(targetScope);
			targetScope.$digest();

			var vm = getInnerScope(targetElement).requestsOverview;
			expect(vm.filters[0].Status).toEqual('0');
		});

		it('should get agentSearchTerm and selectedTeamIds from event',function(){
			targetScope.agentSearchTerm = 'text';
			targetScope.onInitCallBack = function() {};
			targetScope.selectedGroupIds = ['1', '2', '3'];

			targetElement = $compile('<requests-overview shift-trade-view="true" agent-search-term="agentSearchTerm" selected-group-ids="selectedTeamIds" on-init-call-back="onInitCallBack"></requests-overview>')(targetScope);
			targetScope.$digest();

			var vm = getInnerScope(targetElement).requestsOverview;
			vm.isActive = true;
			targetScope.$broadcast('reload.requests.with.selection',{selectedGroupIds:['selectedTeamIds1','selectedTeamIds2'],agentSearchTerm:'testSearchTerm'});

			expect(vm.agentSearchTerm).toEqual('testSearchTerm');
			expect(vm.selectedGroupIds.length).toEqual(2);
			expect(vm.selectedGroupIds[0]).toEqual('selectedTeamIds1');
			expect(vm.selectedGroupIds[1]).toEqual('selectedTeamIds2');
		});

		it('should not call data service more than once in the first time', function() {
			requestsDataService.setRequests([]);
			targetScope.selectedTabIndex = 0;
			targetScope.absencePeriod = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			targetScope.selectedTeams = ["team"];
			targetScope.initFooter = function (count) { };

			targetElement = $compile('<requests-overview ' +
				'is-active="selectedTabIndex == 0" ' +
				'selected-team-ids="selectedTeams"' +
				'on-init-call-back="initFooter(count)"' +
				'period="absencePeriod"></requests-overview>')(targetScope);
			targetScope.$digest();

			targetScope.absencePeriod = {
				startDate: moment().add(-1, 'd')._d,
				endDate: moment().add(1, 'd')._d
			};
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: 'search term'});

			expect(requestsDataService.getCallCounts()).toEqual(1);
		});

		it('should not call data service more than once when switching tab with different date period', function () {
			requestsDataService.setRequests([]);
			targetScope.selectedTabIndex = 0;
			targetScope.absencePeriod = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			targetScope.shiftTradePeriod = {
				startDate: moment().add(-1, 'd')._d,
				endDate: moment().add(1, 'd')._d
			};
			targetScope.selectedTeams = ["team"];
			targetScope.initFooter = function () { };

			targetElement = $compile('<requests-overview on-init-call-back="initFooter(count)" is-active="selectedTabIndex == 0" period="absencePeriod"></requests-overview>' +
				'<requests-overview on-init-call-back="initFooter(count)" is-active="selectedTabIndex == 1" period="shiftTradePeriod" selected-team-ids="selectedTeams"></requests-overview>')(targetScope);
			targetScope.$digest();

			requestsDataService.reset();
			targetScope.selectedTabIndex = 1;
			targetScope.agentSearchTerm = 'search term';
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: 'search term'});
			targetScope.$digest();

			expect(requestsDataService.getCallCounts()).toEqual(1);
		});

		it('should display error when searching person count is exceeded', function () {
			requestsDataService.setGetAllRequetsCallbackData({
				IsSearchPersonCountExceeded: true,
				MaxSearchPersonCount: 5000
			});

			targetScope.initFooter = function () { };
			targetElement = $compile('<requests-overview ' +
				'on-init-call-back="initCallBack(count)"' +
				'period="absencePeriod"></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			scope.requestsOverview.isActive = false;
			scope.requestsOverview.selectedTeamIds = ["team"];
			scope.requestsOverview.init();
			targetScope.$digest();
			scope.requestsOverview.isActive = true;
			targetScope.$broadcast('reload.requests.with.selection', { selectedGroupIds: ['team'], agentSearchTerm: 'search term'});
			scope.$apply();
			expect(scope.requestsOverview.requests.length).toEqual(0);
			expect(requestsNotificationService.getNotificationResult()).toEqual(5000);
		});

		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
		}
	});

	describe('[requests table container directive]', function () {
		var $compile, $rootScope, requestsDefinitions, $filter, teamSchedule, currentUserInfo;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function () {
			var requestsDataService = new FakeRequestsDataService();
			teamSchedule = new FakeTeamSchedule();
			currentUserInfo = new FakeCurrentUserInfo();
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {
						Wfm_Requests_People_Search_36294: true,
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

				$provide.service('CurrentUserInfo', function () {
					return currentUserInfo;
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
			expect(startTime).toEqual(toDateString('2016-01-05T00:00:00'));
			expect(endTime).toEqual(toDateString('2016-01-07T23:59:00'));
			expect(createdTime).toEqual(toDateString('2016-01-05T03:29:37'));
			expect(updatededTime).toEqual(toDateString('2016-01-05T03:29:37'));
		});

		it("should be able to switch between user timezone and request submitter timezone", function () {
			var test = setUpTarget();

			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-01-06T14:00:00', PeriodEndTime: '2016-01-09T20:00:00', CreatedTime: '2016-01-06T10:17:31', TimeZone: 'Pacific/Port_Moresby', UpdatedTime: '2016-01-06T10:17:31', IsFullDay: false }];

			test.scope.$digest();
			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.userTimeZone = 'Europe/Berlin';
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = false;
			test.scope.$digest();
			expect(test.scope.requests[0].FormatedPeriodStartTime()).toEqual(toDateString('2016-01-06T05:00:00'));

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

			var selectedStatus = test.target.isolateScope().requestsTableContainer.selectedRequestStatuses;
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

		it('should not load absences for shift trade request', function() {
			var test = setUpTarget();
			test.scope.shiftTradeView = true;
			test.scope.$apply();
			var vm = test.target.isolateScope().requestsTableContainer;
			expect(angular.isDefined(vm.AllRequestableAbsences)).toEqual(false);
		});

		it('should display correct time for DST', function () {
			var timeZone = 'Europe/Berlin';

			var test = setUpTarget();
			test.scope.requests = [{ Id: 1, PeriodStartTime: '2016-10-27T17:00:00', PeriodEndTime: '2016-10-27T18:00:00', CreatedTime: '2016-11-07T10:17:31', TimeZone: timeZone, UpdatedTime: '2016-11-07T10:17:31', IsFullDay: false }];
			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;
			test.scope.$digest();

			var request = test.scope.requests[0];
			expect(request.FormatedPeriodStartTime()).toEqual(toDateString('2016-10-27T17:00:00'));
			expect(request.FormatedPeriodEndTime()).toEqual(toDateString('2016-10-27T18:00:00'));
		});

		it('should display time in shift trade day view according to logon user timezone', function () {
			var submitterTimezone = 'Europe/Berlin';
			var test = setUpTarget();
			test.scope.requests = [{ Id: 1, PeriodStartTime: '2017-01-09T00:00:00', PeriodEndTime: '2017-01-09T23:59:00', CreatedTime: '2017-01-03T05:54:12', TimeZone: submitterTimezone, UpdatedTime: '2017-01-03T05:54:50' }];
			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			test.scope.shiftTradeView = true;
			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = false;
			test.scope.$digest();

			var request = test.scope.requests[0];
			expect(request.FormatedPeriodStartTime()).toEqual(toDateString('2017-01-08T23:00:00'));
			expect(request.FormatedPeriodEndTime()).toEqual(toDateString('2017-01-09T22:59:00'));
		});

		it('should get dayViewModels according to logon user timezone', function () {
			var submitterTimezone = 'Europe/Berlin';
			var test = setUpTarget();
			test.scope.requests = [
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
			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			test.scope.shiftTradeView = true;
			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = false;
			test.scope.$digest();

			var shiftTradeDayViewModels = isolatedScope.requestsTableContainer.shiftTradeDayViewModels;
			var shiftTradeScheduleViewModels = isolatedScope.requestsTableContainer.shiftTradeScheduleViewModels;
			
			var expectedTimezone = "Atlantic/Reykjavik";
			var expectedDate = moment(test.scope.shiftTradeRequestDateSummary.Minimum);
			
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
			var submitterTimezone = 'Europe/Berlin';
			var test = setUpTarget();
			test.scope.requests = [
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
			test.scope.shiftTradeRequestDateSummary = {
				Minimum: '2017-01-02T00:00:00',
				Maximum: '2017-01-09T22:59:00',
				FirstDayOfWeek: 1
			};
			test.scope.shiftTradeView = true;
			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;
			test.scope.$digest();

			var shiftTradeDayViewModels = isolatedScope.requestsTableContainer.shiftTradeDayViewModels;
			var shiftTradeScheduleViewModels = isolatedScope.requestsTableContainer.shiftTradeScheduleViewModels;

			var expectedTimezone = "Europe/Berlin";
			var expectedDate = moment(test.scope.shiftTradeRequestDateSummary.Minimum);

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

		it('should clear subject and message filters', function() {
			var test = setUpTarget();
			test.scope.$digest();

			var isolatedScope = test.target.isolateScope();
			isolatedScope.requestsTableContainer.subjectFilter = ['a'];
			isolatedScope.requestsTableContainer.messageFilter = ['b'];
			test.scope.$digest();

			isolatedScope.requestsTableContainer.clearFilters();
			expect(isolatedScope.requestsTableContainer.subjectFilter).toEqual(undefined);
			expect(isolatedScope.requestsTableContainer.messageFilter).toEqual(undefined);
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
			var momentDate = moment(date);
			if (timeZone) {
				var _isNowDST = moment.tz(timeZone).isDST();
				var _dateTime = _isNowDST ? momentDate.add(1, 'h').toDate() : momentDate.toDate();
				return $filter('date')(_dateTime, "short");
			} else {
				return $filter('date')(momentDate.toDate(), "short");
			}
		};

		function setUpTarget() {
			var scope = $rootScope.$new();
			var directiveElem = getCompiledElement();

			function getCompiledElement() {
				var element = angular.element('<requests-table-container filters="filters" is-using-request-submitter-time-zone="true" requests="requests" shift-trade-view="shiftTradeView" shift-trade-request-date-summary="shiftTradeRequestDateSummary" ></requests-table-container>');
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

	function FakeCurrentUserInfo() {
		return{
			CurrentUserInfo: function() {
				return{
					DefaultTimeZone: "Atlantic/Reykjavik"
				};
			}
		};
	}

	function FakeRequestsDataService() {
		var _requests = [];
		var _hasSentRequests;
		var _lastRequestParameters;
		var _callCounts = 0;
		var _getAllRequetsCallbackData = {
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

		this.setRequests = function (requests) {
			_requests = requests;
			_getAllRequetsCallbackData = {
				Requests: _requests,
				TotalCount: _requests.length
			}
		};

		this.setGetAllRequetsCallbackData = function(data) {
			_getAllRequetsCallbackData = data;
		};

		this.getHasSentRequests = function () { return _hasSentRequests; }
		this.getLastRequestParameters = function () { return _lastRequestParameters; }

		this.getAbsenceAndTextRequestsPromise = function () {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function (cb) {
					_callCounts++;
					cb({
						data: _getAllRequetsCallbackData
					});
				}
			};
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
			};
		};

		this.getRequestTypes = function () {
			return {
				then: function (cb) {
					cb({
						data: [
							{ Id: "00", Name: "Absence0" },
							{ Id: "01", Name: "Absence1" },
							{ Id: "0", Name: "Text" }
						]
					});
				}
			}
		};

		this.getCallCounts = function() {
			return _callCounts;
		};

		this.getAbsenceAndTextRequestsStatuses = function () {
			var statuses = [];

			return getBasicStatuses().concat(statuses);
		};

		this.getShiftTradeRequestsStatuses = function () {
			return getBasicStatuses();
		};

		function getBasicStatuses(){
			// TODO: Should get this list in a better way
			// Refer to definition Teleopti.Ccc.Domain.AgentInfo.Requests.PersonRequest.personRequestState.CreateFromId()
			var basicStatuses = [
				{ Id: 0, Name: "Status0" },
				{ Id: 1, Name: "Status1" }
			];

			return basicStatuses;
		}
	}

	function FakeRequestsNotificationService() {
		var _notificationResult;
		this.notifyMaxSearchPersonCountExceeded = function (maxSearchPersonCount) {
			_notificationResult = maxSearchPersonCount;
		}
		this.getNotificationResult = function () {
			return _notificationResult;
		}
	}
})();

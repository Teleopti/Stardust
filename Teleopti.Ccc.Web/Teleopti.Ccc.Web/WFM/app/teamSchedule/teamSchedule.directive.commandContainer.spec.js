describe('team schedule command container test', function () {
	'use strict';

	var $compile,
		$rootScope,
		$httpBackend,
		fakeCommandCheckService,
		fakeTeamScheduleService;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(module('wfm.teamSchedule', function ($provide) {
		fakeCommandCheckService = new FakeCommandCheckService();

		$provide.factory('addPersonalActivityDirective', function () {
			return {};
		});

		$provide.service('CommandCheckService', function () {
			return fakeCommandCheckService;
		});

		$provide.service('TeamSchedule', function () {
			fakeTeamScheduleService = new FakeTeamScheduleService();
		});

	}));

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));

	it('container should render correctly', function () {
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = new Date();
		var target = $compile(html)(scope);
		scope.$apply();
		var result = target[0].querySelector('.teamschedule-command-container');
		expect(result).not.toBeNull();
	});

	it('container should respond to set and reset command events correctly', function () {
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = new Date();
		var target = $compile(html)(scope);
		scope.$apply();

		var innerScope = angular.element(target[0].querySelector('.teamschedule-command-container')).scope();

		scope.$broadcast('teamSchedule.init.command', {
			activeCmd: 'testCmd'
		});
		expect(innerScope.vm.activeCmd).toEqual('testCmd');

		scope.$broadcast('teamSchedule.reset.command');
		expect(innerScope.vm.activeCmd).toBeNull();
	});

	it('container should keep original active command and open command check sidenav when checking command', function () {
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = new Date();
		var target = $compile(html)(scope);
		scope.$apply();

		var innerScope = angular.element(target[0].querySelector('.teamschedule-command-container')).scope();

		scope.$broadcast('teamSchedule.init.command', {
			activeCmd: 'testCmd'
		});
		expect(innerScope.vm.activeCmd).toEqual('testCmd');

		fakeCommandCheckService.fakeIsCheckingCommand();

		scope.$apply();

		expect(innerScope.vm.activeCmd).toEqual('testCmd');

		var result = target[0].querySelector('.command-check');
		expect(result).not.toBeNull();
	});

	it('should not invoke getSchedule when the selected date is null', function () {
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = null;
		var target = $compile(html)(scope);
		scope.$apply();

		var innerScope = angular.element(target[0].querySelector('.teamschedule-command-container')).scope();

		scope.$broadcast('teamSchedule.init.command', {
			activeCmd: 'testCmd'
		});
		expect(innerScope.vm.activeCmd).toEqual('testCmd');
		expect(fakeTeamScheduleService.getCurrentDate()).toEqual(null);
	});

	function commonTestsInDifferentLocale() {
		it('should get correct date', function () {
			var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
			var scope = $rootScope.$new();
			scope.curDate = '2018-02-26';
			var target = $compile(html)(scope);
			scope.$apply();
			var innerScope = angular.element(target[0].querySelector('.teamschedule-command-container')).scope();

			expect(innerScope.vm.getDate()).toEqual('2018-02-26');
		});
	}

	commonTestsInDifferentLocale();
	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});


	function FakeCommandCheckService() {
		var fakeResponse = {
			data: []
		};
		var checkStatus = false,
			fakeOverlappingList = [];

		var fakeCheckConfig = {
			subject: 'ThisActivityWillIntersectExistingActivitiesThatDoNotAllowOverlapping',
			body: 'TheFollowingAgentsHaveAffectedActivities',
			actionOptions: ['MoveNonoverwritableActivityForTheseAgents', 'DoNotModifyForTheseAgents', 'OverrideForTheseAgents']
		};

		this.checkAddActivityOverlapping = function () {
			return {
				then: function (cb) {
					checkStatus = true;
					cb(fakeResponse);
				}
			}
		}

		this.getCommandCheckStatus = function () {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function () {
			checkStatus = false;
		}

		this.fakeIsCheckingCommand = function () {
			checkStatus = true;
		}

		this.getCheckFailedAgentList = function () {
			return fakeOverlappingList;
		}

		this.getCheckConfig = function () {
			return fakeCheckConfig;
		};
	}
	function FakeTeamScheduleService() {
		var currentDate = null;
		this.getSchedules = function (date, agents) {
			currentDate = date;
			return {
				then: function (cb) {
				}
			}
		}
		this.getCurrentDate = function () {
			return currentDate;
		}
	}
});
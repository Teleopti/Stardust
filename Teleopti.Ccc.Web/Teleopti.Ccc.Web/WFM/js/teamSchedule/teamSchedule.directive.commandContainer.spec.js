describe("team schedule command container test", function() {

	var $compile,
		$rootScope,
		fakeCommandCheckService;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(module('wfm.teamSchedule', function($provide) {
		fakeCommandCheckService = new FakeCommandCheckService();

		$provide.factory('addPersonalActivityDirective', function() {
			return {};
		});

		$provide.service('CommandCheckService', function() {
			return fakeCommandCheckService;
		});
	}));

	beforeEach(inject(function(_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
	}));

	it('container should render correctly', function() {
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = new Date();
		var target = $compile(html)(scope);
		scope.$apply();
		var result = target[0].querySelector('.teamschedule-command-container');
		expect(result).not.toBeNull();
	});

	it('container should respond to set and reset command events correctly', function() {
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

	it('container should keep original active command and open command check sidenav when checking command', function() {
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

	function FakeCommandCheckService() {
		var fakeResponse = {
			data: []
		};
		var checkStatus = false,
			fakeOverlappingList = [];

		this.checkOverlappingCertainActivities = function() {
			return {
				then: function(cb) {
					checkStatus = true;
					cb(fakeResponse);
				}
			}
		}

		this.getCommandCheckStatus = function() {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function() {
			checkStatus = false;
		}

		this.fakeIsCheckingCommand = function() {
			checkStatus = true;
		}

		this.getOverlappingAgentList = function() {
			return fakeOverlappingList;
		}
	}
});
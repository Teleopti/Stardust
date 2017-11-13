'use strict';
describe('RtaAgentsController', function () {

	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		vm,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		scope = $controllerBuilder.setup('RtaAgentsController46475');
		spyOn($state, 'go');
	}));

	afterEach(function () {
		$fakeBackend.clear();
		$sessionStorage.$reset();
	});

	it('should filter text', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(function () {
			vm.filterText = 'Charley';
		}).wait(5000);

		expect($fakeBackend.lastAgentStatesRequestParams.textFilter).toBe('Charley');
	});

	it('should not filter text immediately', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(function () {
			vm.filterText = 'Charley';
		});

		expect($fakeBackend.lastAgentStatesRequestParams.textFilter).toBeUndefined();
	});

	it('should not add textFilter to params when no filter', function () {
		$controllerBuilder.createController();

		expect($fakeBackend.lastAgentStatesRequestParams.textFilter).toBeUndefined();
	});

	it('should not add textFilter to params when removing text filter', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c
			.apply(function () {
				vm.filterText = 'Charley';
			})
			.wait(5000)
			.apply(function () {
				vm.filterText = '';
			})
			.wait(5000);

		expect($fakeBackend.lastAgentStatesRequestParams.textFilter).toBeUndefined();
	});

	it('should default sort by name ascending', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		c.apply(function () {
			vm.showInAlarm = false;
		});

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBe('Name');
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBe('asc');
	});

	it('should sort by name descending first time', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		c.apply(function () {
			vm.showInAlarm = false;
		});
		c.apply(function () {
			vm.sort("Name");
		});

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBe('Name');
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBe('desc');
	});

	it('should sort by name', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		c.apply(function () {
			vm.showInAlarm = false;
		});
		c.apply(function () {
			vm.sort("SiteAndTeamName");
		});
		c.apply(function () {
			vm.sort("Name");
		});

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBe('Name');
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBe('asc');
	});

	it('should sort by site name', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(function () {
			vm.showInAlarm = false;
		});
		c.apply(function () {
			vm.sort("SiteAndTeamName");
		});

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBe('SiteAndTeamName');
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBe('asc');
	});

	it('should not sort when viewing agents in alarm', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBeUndefined();
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBeUndefined();
	});

	it('should not sort when viewing agents in alarm after switching', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(function () {
			vm.showInAlarm = false;
		});
		c.apply(function () {
			vm.showInAlarm = true;
		});

		expect($fakeBackend.lastAgentStatesRequestParams.orderBy).toBeUndefined();
		expect($fakeBackend.lastAgentStatesRequestParams.direction).toBeUndefined();
	});

	it('should switch direction', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(function () {
			vm.showInAlarm = false;
		});
		c.apply(function () {
			vm.sort("State");
		});
		c.apply(function () {
			vm.sort("State");
		});

		expect(vm.direction).toEqual('desc');
	});

});

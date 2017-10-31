'use strict';
describe('RtaAgentsController46475', function() {

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

	beforeEach(function() {
		module(function($provide) {
			$provide.factory('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		scope = $controllerBuilder.setup('RtaAgentsController46475');
		spyOn($state, 'go');
	}));

	afterEach(function() {
		$fakeBackend.clear();
		$sessionStorage.$reset();
	});

	it('should filter by agent name', function() {
		$fakeBackend
			.withAgentState({
				Name: "Ashley Andeen"
			})
			.withAgentState({
				Name: "Charley Caper"
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(vm.showInAlarm = false);
		c.apply(vm.filterText = 'Charley');

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
	});

	it('should filter by site name', function() {
		$fakeBackend
			.withAgentState({
				Name: "Ashley Andeen",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "London"
			})
			.withAgentState({
				Name: "Charley Caper",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "London"
			})
			.withAgentState({
				Name: "Charley Smith",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab496",
				SiteName: "Paris"
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(vm.showInAlarm = false);
		c.apply(vm.filterText = "London");

		expect(vm.agentStates.length).toEqual(2);
		expect(vm.agentStates[0].SiteName).toEqual("London");
		expect(vm.agentStates[1].SiteName).toEqual("London");
	});

	it('should filter by two words separated by space', function() {
		stateParams.siteIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withAgentState({
				Name: "Ashley Andeen",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Hoth",
				TeamId: "11590a63-6331-4921-bc9f-9b5e015ab422",
				TeamName: "Rebels"
			})
			.withAgentState({
				Name: "Charley Caper",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Hoth",
				TeamId: "44590a63-6331-4921-bc9f-9b5e015ab433",
				TeamName: "Troopers"
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(vm.showInAlarm = false);
		c.apply(function() {
			vm.filterText = "Rebels Hoth";
		});

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Ashley Andeen");
	});

	it('should filter by two words separated by whitespace', function() {
		stateParams.siteIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withAgentState({
				Name: "Ashley Andeen",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Hoth",
				TeamId: "11590a63-6331-4921-bc9f-9b5e015ab422",
				TeamName: "Rebels"
			})
			.withAgentState({
				Name: "Charley Caper",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Hoth",
				TeamId: "44590a63-6331-4921-bc9f-9b5e015ab433",
				TeamName: "Troopers"
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;
		c.apply(vm.showInAlarm = false);
		c.apply(function() {
			vm.filterText = "Rebels 		 Hoth";
		});

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Ashley Andeen");
	});

});

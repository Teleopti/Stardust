'use strict';
describe('RtaAgentsController', function() {
	var $interval,
		$httpBackend,
		$state,
		$fakeBackend,
		$controllerBuilder,
		scope,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.factory('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsController');

		$fakeBackend.clear();
		spyOn($state, 'go');
	}));

	it('should get agent for skill and team', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];
		$fakeBackend
			.withAgentState({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid"
			})
			.withAgentState({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				TeamId: "teamRedGuid"
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("AshleyGuid");
		expect(vm.agentStates.length).toEqual(1);
	});

	it('should get agent for skill and site', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid"
			})
			.withAgentState({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("AshleyGuid");
		expect(vm.agentStates.length).toEqual(1);
	});

	it('should get agent states for teams and skills', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid",
				TeamName: "Team Red",
				SiteName: "London",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Rule: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].TeamName).toEqual("Team Red");
		expect(vm.agentStates[0].SiteName).toEqual("London");
		expect(vm.agentStates[0].State).toEqual("Ready");
		expect(vm.agentStates[0].Activity).toEqual("Phone");
		expect(vm.agentStates[0].NextActivity).toEqual("Short break");
		expect(vm.agentStates[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agentStates[0].Rule).toEqual("In Adherence");
		expect(vm.agentStates[0].Color).toEqual("#00FF00");
		expect(vm.agentStates[0].TimeInState).toEqual(15473);
	});

	it('should get agent states for sites and skills', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["londonGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "londonGuid",
				SiteName: "London",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Rule: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].SiteName).toEqual("London");
		expect(vm.agentStates[0].State).toEqual("Ready");
		expect(vm.agentStates[0].Activity).toEqual("Phone");
		expect(vm.agentStates[0].NextActivity).toEqual("Short break");
		expect(vm.agentStates[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agentStates[0].Rule).toEqual("In Adherence");
		expect(vm.agentStates[0].Color).toEqual("#00FF00");
		expect(vm.agentStates[0].TimeInState).toEqual(15473);
	});

	it('should display states in alarm for skill and team', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Ashley Andeen");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should display states in alarm for skill and site', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Ashley Andeen");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should take excluded states as stateParam for team and skill', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.es = ["StateGuid1"];

		$fakeBackend
			.withAgentState({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid",
				State: "StateGuid1",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid",
				State: "StateGuid2",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true)
			.wait(5000);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].PersonId).toEqual("AshleyGuid");
	});

	it('should take excluded states as stateParam for site and skill', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteParisGuid"];
		stateParams.es = ["StateGuid1"];

		$fakeBackend
			.withAgentState({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid",
				State: "StateGuid1",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid",
				State: "StateGuid2",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true)
			.wait(5000);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].PersonId).toEqual("AshleyGuid");
	});

	it('should get agent for skillArea and team', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid"
			})
			.withAgentState({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				TeamId: "teamRedGuid"
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			})
			.withAgentState({
				PersonId: "JohnGuid",
				SkillId: "invoiceGuid",
				TeamId: "teamRedGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("PierreGuid");
		expect(vm.agentStates[1].PersonId).toEqual("AshleyGuid");
		expect(vm.agentStates.length).toEqual(2);
	});

	it('should get agent for skillArea and site', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgentState({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid"
			})
			.withAgentState({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgentState({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgentState({
				PersonId: "JohnGuid",
				SkillId: "invoiceGuid",
				SiteId: "siteLondonGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("PierreGuid");
		expect(vm.agentStates[1].PersonId).toEqual("AshleyGuid");
		expect(vm.agentStates.length).toEqual(2);
	});

});

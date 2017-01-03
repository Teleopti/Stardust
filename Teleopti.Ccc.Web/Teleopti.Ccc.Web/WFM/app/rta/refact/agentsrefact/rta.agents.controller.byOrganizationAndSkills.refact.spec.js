'use strict';
xdescribe('RtaAgentsCtrl by organization and skills', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
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

		scope = $controllerBuilder.setup('RtaAgentsCtrlRefact');

		$fakeBackend.clear();

		spyOn($state, 'go');
	}));

	it('should get agent for skill and team', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];
		$fakeBackend
			.withAgent({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid"
			})
			.withAgent({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				TeamId: "teamRedGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("AshleyGuid");
		expect(vm.agents.length).toEqual(1);
	});

	it('should get agent for skill and site', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid"
			})
			.withAgent({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("AshleyGuid");
		expect(vm.agents.length).toEqual(1);
	});

	it('should get agent states for teams and skills', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid",
				TeamName: "Team Red",
				SiteName: "London"
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].TeamName).toEqual("Team Red");
		expect(vm.agents[0].SiteName).toEqual("London");
		expect(vm.agents[0].State).toEqual("Ready");
		expect(vm.agents[0].Activity).toEqual("Phone");
		expect(vm.agents[0].NextActivity).toEqual("Short break");
		expect(vm.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agents[0].Alarm).toEqual("In Adherence");
		expect(vm.agents[0].Color).toEqual("#00FF00");
		expect(vm.agents[0].TimeInState).toEqual(15473);
	});

	it('should get agent states for sites and skills', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["londonGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "londonGuid",
				SiteName: "London"
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents.length).toEqual(1);
		expect(vm.agents[0].SiteName).toEqual("London");
		expect(vm.agents[0].State).toEqual("Ready");
		expect(vm.agents[0].Activity).toEqual("Phone");
		expect(vm.agents[0].NextActivity).toEqual("Short break");
		expect(vm.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agents[0].Alarm).toEqual("In Adherence");
		expect(vm.agents[0].Color).toEqual("#00FF00");
		expect(vm.agents[0].TimeInState).toEqual(15473);
	});

	it('should display states in alarm for skill and team', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			})
			.withState({
				PersonId: "PierreGuid",
				State: "Break",
				TimeInAlarm: 0
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(vm.filteredData[0].State).toEqual("Break");
	});

	it('should display states in alarm for skill and site', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			})
			.withState({
				PersonId: "PierreGuid",
				State: "Break",
				TimeInAlarm: 0
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(vm.filteredData[0].State).toEqual("Break");
	});

	it('should take excluded states as stateParam for team and skill', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.es = ["StateGuid1"];

		$fakeBackend
			.withAgent({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			})
			.withState({
				PersonId: "PierreGuid",
				State: "StateGuid1",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "StateGuid2",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true)
			.wait(5000);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].PersonId).toEqual("AshleyGuid");
	});

	it('should take excluded states as stateParam for site and skill', function() {
		stateParams.skillIds = ["emailGuid"];
		stateParams.siteIds = ["siteParisGuid"];
		stateParams.es = ["StateGuid1"];

		$fakeBackend
			.withAgent({
				PersonId: "PierreGuid",
				Name: "Pierre Baldi",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				Name: "Ashley Andeen",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid"
			})
			.withState({
				PersonId: "PierreGuid",
				State: "StateGuid1",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withState({
				PersonId: "AshleyGuid",
				State: "StateGuid2",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true)
			.wait(5000);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].PersonId).toEqual("AshleyGuid");
	});

	it('should get agent for skillArea and team', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.teamIds = ["teamRedGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				TeamId: "teamGreenGuid"
			})
			.withAgent({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				TeamId: "teamRedGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				TeamId: "teamRedGuid"
			})
			.withAgent({
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
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("PierreGuid");
		expect(vm.agents[1].PersonId).toEqual("AshleyGuid");
		expect(vm.agents.length).toEqual(2);
	});

	it('should get agent for skillArea and site', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["siteLondonGuid"];

		$fakeBackend
			.withAgent({
				PersonId: "BillGuid",
				SkillId: "emailGuid",
				SiteId: "siteParisGuid"
			})
			.withAgent({
				PersonId: "PierreGuid",
				SkillId: "phoneGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgent({
				PersonId: "AshleyGuid",
				SkillId: "emailGuid",
				SiteId: "siteLondonGuid"
			})
			.withAgent({
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
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("PierreGuid");
		expect(vm.agents[1].PersonId).toEqual("AshleyGuid");
		expect(vm.agents.length).toEqual(2);
	});

});

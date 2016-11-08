'use strict';
describe('RtaAgentsCtrl by organization and skills', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();


		spyOn($state, 'go');
	}));

	it('should get agent for skill and team', function () {
		stateParams.skillId = "emailGuid";
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.showAllAgents = true;
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

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(1);
	});

	it('should get agent for skill and site', function () {
		stateParams.skillId = "emailGuid";
		stateParams.siteIds = ["siteLondonGuid"];
		stateParams.showAllAgents = true;

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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(1);
	});

	it('should get agent states for teams and skills', function() {
		stateParams.skillId = "emailGuid";
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.showAllAgents = true;
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

		$controllerBuilder.createController();

		expect(scope.agents[0].TeamName).toEqual("Team Red");
		expect(scope.agents[0].SiteName).toEqual("London");
		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should get agent states for sites and skills', function() {
		stateParams.skillId = "emailGuid";
		stateParams.siteIds = ["londonGuid"];
		stateParams.showAllAgents = true;
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

		$controllerBuilder.createController();

		expect(scope.agents.length).toEqual(1);
		expect(scope.agents[0].SiteName).toEqual("London");
		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should display states in alarm for skill and team', function () {
		stateParams.skillId = "emailGuid";
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.showAllAgents = false;
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

		$controllerBuilder.createController();

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(scope.filteredData[0].State).toEqual("Break");
	});

	it('should display states in alarm for skill and site', function () {
		stateParams.skillId = "emailGuid";
		stateParams.siteIds = ["siteLondonGuid"];
		stateParams.showAllAgents = false;
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

		$controllerBuilder.createController();

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(scope.filteredData[0].State).toEqual("Break");
	});

	it('should take excluded states as stateParam for team and skill', function () {
			stateParams.skillId = "emailGuid";
			stateParams.teamIds = ["teamRedGuid"];
			stateParams.es = ["StateGuid1"];
			stateParams.showAllAgents = false;
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

		$controllerBuilder.createController().wait(5000);

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].PersonId).toEqual("AshleyGuid");
	});

	it('should take excluded states as stateParam for site and skill', function () {
			stateParams.skillId = "emailGuid";
			stateParams.siteIds = ["siteParisGuid"];
			stateParams.es = ["StateGuid1"];
			stateParams.showAllAgents = false;
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

		$controllerBuilder.createController().wait(5000);

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].PersonId).toEqual("AshleyGuid");
	});

	it('should get agent for skillArea and team', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.teamIds = ["teamRedGuid"];
		stateParams.showAllAgents = true;
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

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("PierreGuid");
		expect(scope.agents[1].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(2);
	});

	it('should get agent for skillArea and site', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["siteLondonGuid"];
		stateParams.showAllAgents = true;
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

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("PierreGuid");
		expect(scope.agents[1].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(2);
	});

});

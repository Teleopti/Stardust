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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(1);
	});

	it('should get agent for skill and site', function () {
		stateParams.skillId = "emailGuid";
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(1);
	});

	xit('should display states in alarm only for skill and team', function () {
		stateParams.skillId = "emailGuid";
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
		
		$fakeBackend.onlyAllowGettingStatesByCombiningOrganizationAndSkillQueries()
		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(scope.filteredData[0].State).toEqual("Break");
	});


	it('should get agent for skillArea and team', function () {
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("PierreGuid");
		expect(scope.agents[1].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(2);
	});

	it('should get agent for skillArea and site', function () {
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("PierreGuid");
		expect(scope.agents[1].PersonId).toEqual("AshleyGuid");
		expect(scope.agents.length).toEqual(2);
	});

	//green with no effort :)
	
	it('should display states in alarm only for skill and team', function () {
		stateParams.skillId = "emailGuid";
		stateParams.teamIds = ["teamRedGuid"];
		$fakeBackend
		.withAgent({
			PersonId: "PierreGuid",
			SkillId: "emailGuid",
			TeamId: "teamRedGuid"
		})
		.withAgent({
			PersonId: "AshleyGuid",
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

			expect(scope.filteredData.length).toEqual(1);
			expect(scope.filteredData[0].State).toEqual("Break");
	});

});

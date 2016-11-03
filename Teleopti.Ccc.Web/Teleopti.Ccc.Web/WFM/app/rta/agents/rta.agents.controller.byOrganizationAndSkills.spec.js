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

});

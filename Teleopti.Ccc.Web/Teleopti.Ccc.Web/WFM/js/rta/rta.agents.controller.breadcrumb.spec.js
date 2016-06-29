'use strict';
describe('RtaAgentsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
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

	}));

	it('should set to team', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId : "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences"
		});

		$controllerBuilder.createController();

		expect(scope.teamName).toEqual("Team Preferences");
	});

	it('should set to multiple teams when selecting teams', function() {
		stateParams.teamIds = ["e5f968d7-6f6d-407c-81d5-9b5e015ab495", "d7a9c243-8cd8-406e-9889-9b5e015ab495"];

		$controllerBuilder.createController();

		expect(scope.multipleTeamsName).toEqual("Multiple Teams");
	});

	it('should set to multiple teams when selecting one site', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c"];

		$controllerBuilder.createController();

		expect(scope.multipleTeamsName).toEqual("Multiple Teams");
	});

	xit('should set site name when selecting one site', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId : "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			SiteName: "London"
		});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("London");
	});

	it('should set to multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "7a6c0754-4de8-48fb-8aee-a39a00b9d1c3"];

		$controllerBuilder.createController();

		expect(scope.multipleSitesName).toEqual("Multiple Sites");
	});

		it('should set team and site name', function() {
			stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
			stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
			$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId : "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamName: "Team Preferences",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				SiteName: "London"
			});

			$controllerBuilder.createController();

			expect(scope.teamName).toEqual("Team Preferences");
			expect(scope.siteName).toEqual("London");
		});
});

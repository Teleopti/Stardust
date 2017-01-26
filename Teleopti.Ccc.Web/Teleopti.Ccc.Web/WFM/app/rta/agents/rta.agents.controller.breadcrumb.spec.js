'use strict';
describe('RtaAgentsController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$translate,
		$fakeBackend,
		$controllerBuilder,
		scope,
		vm;
	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _$translate_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$translate = _$translate_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsController');

		$fakeBackend.clear();

		spyOn($state, 'go');
		//$fakeBackend.withToggle('RTA_QuicklyChangeAgentsSelection_40610');
		$fakeBackend.withToggle('RTA_FasterAgentsView_42039');
	}));

	it('should set to team', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences"
		})
			.withOrganization({
				Id: 'LondonSiteId',
				Name: 'London',
				Teams: [{
					Id: '34590a63-6331-4921-bc9f-9b5e015ab495',
					Name: 'Team Preferences'
				}]
			});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.teamName).toEqual("Team Preferences");
	});

	it('should set to site', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
			SiteName: "London",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
			.withOrganization({
				Id: 'LondonId',
				Name: 'London',
				Teams: [{
					Id: '34590a63-6331-4921-bc9f-9b5e015ab495',
					Name: 'Team Preferences'
				}]
			});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.siteName).toEqual("London");
	});

	it('should display skill name', function () {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend
			.withSkill({
				Name: "Phone",
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.skillName).toEqual("Phone");
	});

	it('should show name for skill area', function () {
		stateParams.skillAreaId = "bb8d75b3-fdb4-484a-ae4c-9f0800e2f753";

		$fakeBackend
			.withSkillAreas([{
				Id: "bb8d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "skill x"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.skillAreaName).toEqual("my skill area 2");
	});

	it('should set to multiple teams when selecting teams', function () {
		stateParams.teamIds = ["e5f968d7-6f6d-407c-81d5-9b5e015ab495", "d7a9c243-8cd8-406e-9889-9b5e015ab495"];
		$fakeBackend.withOrganization({
			Id: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c',
			Name: 'London',
			Teams: [{
				Id: '34590a63-6331-4921-bc9f-9b5e015ab495',
				Name: 'Team Preferences'
			}]
		});
		vm = $controllerBuilder.createController().vm;

		expect(vm.teamName).toEqual($translate.instant('MultipleTeams'));
	});

	it('should set to multiple teams when selecting one site', function () {
		stateParams.siteIds = ["4970a45a-90ff-4111-bfe1-9b5e015ab45c"];

		vm = $controllerBuilder.createController().vm;

		expect(vm.teamName).toEqual($translate.instant('MultipleTeams'));
	});

	xit('should set site name when selecting one site', function () {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			SiteName: "London"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.siteName).toEqual("London");
	});

	it('should set to multiple sites', function () {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "7a6c0754-4de8-48fb-8aee-a39a00b9d1c3"];

		vm = $controllerBuilder.createController().vm;

		expect(vm.siteName).toEqual($translate.instant('MultipleSites'));
	});

	it('should set to Multiple Teams when selecting site and team', function () {
		stateParams.teamIds = ["Team1Id"];
		stateParams.siteIds = ["LondonId"];
		$fakeBackend.withAgentState({
			PersonId: "AshleyId",
			TeamId: "TeamPreferencesId",
			TeamName: "Team Preferences",
			SiteId: "LondonId",
			SiteName: "London"
		})
			.withOrganization({
				Id: 'LondonId',
				Name: 'London',
				Teams: [{
					Id: 'TeamPreferencesId',
					Name: 'Team Preferences'
				}]
			})
			.withOrganization(
			{
				Id: 'ParisId',
				Name: 'Paris',
				Teams: [{
					Id: 'Team1Id',
					Name: 'Team 1'
				}]
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teamName).toEqual($translate.instant('MultipleTeams'));
	});

	it('should hide Breadcrumb', function () {
		stateParams.skillIds = ["3f15b334-22d1-4bc1-8e41-72359805d30c"];
		$fakeBackend.withSkills([{
			Id: "1f15b334-22d1-4bc1-8e41-72359805d30f",
			Name: "skill a"
		},
		{
			Id: "3f15b334-22d1-4bc1-8e41-72359805d30c",
			Name: "skill b"
		}
		]);

		vm = $controllerBuilder.createController().vm;
		expect(vm.showBreadcrumb).toEqual(false);
		expect(vm.skillName).toEqual("skill b");
	});

	it('should have site link when selected one team', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withOrganization({
				Id: '44590a63-6331-4921-bc9f-9b5e015ab495',
				Name: 'London',
				Teams: [{
					Id: '34590a63-6331-4921-bc9f-9b5e015ab495',
					Name: 'Team Preferences'
				}]
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.goBackToTeamsWithUrl).toContain("44590a63-6331-4921-bc9f-9b5e015ab495");
	});

	it('should have site link when viewing agents in team by skill', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		stateParams.skillIds = ["3d5dd51a-8713-42e9-9f33-9b5e015ab71b"];
		$fakeBackend
			.withOrganization({
				Id: '44590a63-6331-4921-bc9f-9b5e015ab495',
				Name: 'London',
				Teams: [{
					Id: '34590a63-6331-4921-bc9f-9b5e015ab495',
					Name: 'Team Preferences'
				}]
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.goBackToTeamsWithUrl).toContain("44590a63-6331-4921-bc9f-9b5e015ab495");
		expect(vm.goBackToTeamsWithUrl).toContain("3d5dd51a-8713-42e9-9f33-9b5e015ab71b");
	});

	it('should have site link when viewing agents in team by skillArea', function () {
		stateParams.teamIds = ["teamGuid"];
		stateParams.skillAreaId = "skillAreaGuid";
		$fakeBackend
			.withOrganization({
				Id: 'siteGuid',
				Teams: [{ Id: 'teamGuid' }]
			})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{ Id: "phoneGuid", }]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.goBackToTeamsWithUrl).toContain("siteGuid");
		expect(vm.goBackToTeamsWithUrl).toContain("skillAreaGuid");
	});
});

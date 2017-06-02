'use strict';
describe('RtaOverviewController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		NoticeService,
		rtaService,
		vm;

	var stateParams = {};
	var catchError = function (e) {
		console.error(e);
	}
	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_, _rtaService_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		NoticeService = _NoticeService_;
		rtaService = _rtaService_;

		scope = $controllerBuilder.setup('RtaOverviewController');

		$fakeBackend.clear();
		$fakeBackend.withToggle("RTA_MonitorAgentsInPermittedOrganizationOnly_40660");
	}));

	it('should display permitted site', function () {
		$fakeBackend
			.withSiteAdherence({
				Name: "London",
				Id: "londonGuid",
				NumberOfAgents: 11,
				OutOfAdherence: 1,
				Color: "good"
			})
			.withSiteAdherence({
				Name: "Paris",
				Id: "parisGuid",
				NumberOfAgents: 12,
				OutOfAdherence: 2,
				Color: "good"
			})
			.withPermittedSites(["londonGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Name).toEqual("London");
		expect(vm.sites[0].NumberOfAgents).toEqual(11);
		expect(vm.sites[0].OutOfAdherence).toEqual(1);
		expect(vm.sites[0].Color).toEqual("good");
	});

	it('should filter agents out of adherence in permitted site - service', function () {
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 1,
				Color: "good"
			})
			.withSiteAdherence({
				Id: "parisGuid",
				NumberOfAgents: 8,
				OutOfAdherence: 5,
				Color: "danger"
			})
			.withPermittedSites(["londonGuid"]);

		var siteAdherences = [];
		rtaService.getAdherenceForAllSites()
			.then(function (sa) { siteAdherences = sa; })
			.catch(catchError);

		$httpBackend.flush();
		expect(siteAdherences.length).toEqual(1);
		expect(siteAdherences[0].Id).toEqual("londonGuid");
		expect(siteAdherences[0].OutOfAdherence).toEqual(1);
		expect(siteAdherences[0].Color).toEqual("good");
	});

	it('should display permitted team for site', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeamAdherence({
			SiteId: "londonGuid",
			Id: "teamGreenGuid",
			NumberOfAgents: 10,
			Name: "teamGreen",
			OutOfAdherence: 1,
			Color: "good"
		})
			.withTeamAdherence({
				SiteId: "londonGuid",
				Id: "teamRedGuid",
				NumberOfAgents: 20,
				Name: "teamRed",
				OutOfAdherence: 10,
				Color: "warning"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams[0].Name).toEqual("teamGreen");
		expect(vm.teams[0].NumberOfAgents).toEqual(10);
		expect(vm.teams[0].OutOfAdherence).toEqual(1);
		expect(vm.teams[0].Color).toEqual("good");
	});


	it('should filter agents out of adherence in permitted team - service', function () {
		$fakeBackend.withTeamAdherence({
			SiteId: "londonGuid",
			Id: "teamGreenGuid",
			NumberOfAgents: 10,
			Name: "teamGreen",
			OutOfAdherence: 1,
			Color: "good"
		})
			.withTeamAdherence({
				SiteId: "londonGuid",
				Id: "teamRedGuid",
				NumberOfAgents: 20,
				Name: "teamRed",
				OutOfAdherence: 10,
				Color: "warning"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		var teamAdherences = [];

		rtaService.getAdherenceForTeamsOnSite({ siteId: "londonGuid" })
			.then(function (ta) { teamAdherences = ta; })
			.catch(catchError);

		$httpBackend.flush();
		expect(teamAdherences[0].OutOfAdherence).toEqual(1);
		expect(teamAdherences[0].NumberOfAgents).toEqual(10);
		expect(teamAdherences[0].Color).toEqual("good");
	});

	it('should filter agents out of adherence in permitted sites for preselected skill - service', function () {
		$fakeBackend
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "emailGuid",
				Color: "warning"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				NumberOfAgents: 12,
				OutOfAdherence: 2,
				SkillId: "emailGuid",
				Color: "good"
			})
			.withPermittedSites(["parisGuid"]);

		var sites = [];

		rtaService.getAdherenceForSitesBySkills(["emailGuid"])
			.then(function (sa) { sites = sa; })
			.catch(catchError);

		$httpBackend.flush();
		expect(sites.length).toEqual(1);
		expect(sites[0].Id).toEqual("parisGuid");
		expect(sites[0].OutOfAdherence).toEqual(5);
		expect(sites[0].NumberOfAgents).toEqual(10);
		expect(sites[0].Color).toEqual("warning");
	});

	it('should display agents out of adherence in permitted sites for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		$fakeBackend
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "emailGuid",
				Color: "warning"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				NumberOfAgents: 20,
				OutOfAdherence: 5,
				SkillId: "emailGuid",
				Color: "good"
			})
			.withPermittedSites(["parisGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual("parisGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(5);
		expect(vm.sites[0].NumberOfAgents).toEqual(10);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should display agents out of adherence in permitted team for preselected skill - service', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "emailGuid",
				Color: "warning"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamRedGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 7,
				SkillId: "emailGuid",
				Color: "danger"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		var teamAdherences = [];

		rtaService.getAdherenceForTeamsBySkills({ siteIds: "parisGuid", skillIds: "emailGuid" })
			.then(function (ta) { teamAdherences = ta; })
			.catch(catchError);

		$httpBackend.flush();

		expect(teamAdherences.length).toEqual(1);
		expect(teamAdherences[0].Id).toEqual("teamGreenGuid");
		expect(teamAdherences[0].OutOfAdherence).toEqual(5);
		expect(teamAdherences[0].NumberOfAgents).toEqual(10);
		expect(teamAdherences[0].Color).toEqual("warning");
	});

	it('should display agents out of adherence in permitted team for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "emailGuid",
				Color: "warning"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamRedGuid",
				NumberOfAgents: 20,
				OutOfAdherence: 7,
				SkillId: "emailGuid",
				Color: "good"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(1);
		expect(vm.teams[0].Id).toEqual("teamGreenGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(5);
		expect(vm.teams[0].NumberOfAgents).toEqual(10);
		expect(vm.teams[0].Color).toEqual("warning");
	});

});

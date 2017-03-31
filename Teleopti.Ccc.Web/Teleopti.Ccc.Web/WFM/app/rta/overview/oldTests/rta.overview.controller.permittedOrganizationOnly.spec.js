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
	var catchError = function(e){
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
			.withSite({
				Name: "London",
				Id: "londonGuid",
				NumberOfAgents: 11
			})
			.withSite({
				Name: "Paris",
				Id: "parisGuid",
				NumberOfAgents: 12
			})
			.withPermittedSites(["londonGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Name).toEqual("London");
		expect(vm.sites[0].NumberOfAgents).toEqual(11);
	});

	it('should filter agents out of adherence in permitted site - service', function () {
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				OutOfAdherence: 1
			})
			.withSiteAdherence({
				Id: "parisGuid",
				OutOfAdherence: 5,
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
	});

	it('should display agents out of adherence in permitted site', function () {
		$fakeBackend.withSite({
			Id: "londonGuid",
			Name: "London",
		})
			.withSite({
				Id: "parisGuid",
				Name: "Paris",
			})
			.withSiteAdherence({
				Id: "londonGuid",
				OutOfAdherence: 1
			})
			.withSiteAdherence({
				Id: "parisGuid",
				OutOfAdherence: 5,
			})
			.withPermittedSites(["londonGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].OutOfAdherence).toEqual(1);
	});

	it('should display permitted team for site', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeam({
			SiteId: "londonGuid",
			Id: "teamGreenGuid",
			Name: "teamGreen",
			NumberOfAgents: 1
		})
			.withTeam({
				SiteId: "londonGuid",
				Id: "teamRedGuid",
				Name: "teamRed",
				NumberOfAgents: 3
			})
			.withPermittedTeams(["teamGreenGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams[0].Name).toEqual("teamGreen");
		expect(vm.teams[0].NumberOfAgents).toEqual(1);
	});


	it('should filter agents out of adherence in permitted team - service', function () {
		$fakeBackend.withTeam({
			SiteId: "londonGuid",
			Id: "teamGreenGuid",
			NumberOfAgents: 1
		})
		.withTeam({
			SiteId: "londonGuid",
			Id: "teamRedGuid",
			NumberOfAgents: 3
		})
		.withTeamAdherence({
			Id: "teamGreenGuid",
			OutOfAdherence: 1
		})
		.withTeamAdherence({
			Id: "teamRedGuid",
			OutOfAdherence: 5,
		})
		.withPermittedTeams(["teamGreenGuid"]);

		var teamAdherences = [];

		rtaService.getAdherenceForTeamsOnSite({ siteId: "londonGuid" })
			.then(function (ta) { teamAdherences = ta; })
			.catch(catchError);

		$httpBackend.flush();
		expect(teamAdherences[0].OutOfAdherence).toEqual(1);
	});

	it('should display agents out of adherence in permitted team', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeam({
			SiteId: "londonGuid",
			Id: "teamGreenGuid",
			NumberOfAgents: 1
		})
		.withTeam({
			SiteId: "londonGuid",
			Id: "teamRedGuid",
			NumberOfAgents: 3
		})
		.withTeamAdherence({
			Id: "teamGreenGuid",
			OutOfAdherence: 1
		})
		.withTeamAdherence({
			Id: "teamRedGuid",
			OutOfAdherence: 5,
		})
		.withPermittedTeams(["teamGreenGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(1);
		expect(vm.teams[0].Id).toEqual("teamGreenGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(1);
	});

	it('should filter agents out of adherence in permitted sites for preselected skill - service', function() {
			$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withPermittedSites(["parisGuid"]);
		
		var sites = [];

		rtaService.getSitesForSkills(["emailGuid"])
			.then(function (s) { sites = s; })
			.catch(catchError);

		$httpBackend.flush();
		expect(sites.length).toEqual(1);
		expect(sites[0].Id).toEqual("parisGuid");
	});

	it('should filter agents out of adherence in permitted sites for preselected skill - service', function() {
			$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withPermittedSites(["parisGuid"]);
		
		var siteAdherences = [];

		rtaService.getAdherenceForSitesBySkills(["emailGuid"])
			.then(function (sa) { siteAdherences = sa; })
			.catch(catchError);

		$httpBackend.flush();
		expect(siteAdherences.length).toEqual(1);
		expect(siteAdherences[0].Id).toEqual("parisGuid");
		expect(siteAdherences[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in permitted sites for preselected skill', function() {
		stateParams.skillIds = "emailGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withPermittedSites(["parisGuid"]);
		
		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual("parisGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in permitted team for preselected skill - service', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "teamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "teamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		var teamAdherences = [];

		rtaService.getAdherenceForTeamsBySkills({siteIds: "parisGuid", skillIds: "emailGuid"})
		.then(function(ta){ teamAdherences = ta; })
		.catch(catchError);

		$httpBackend.flush();

		expect(teamAdherences.length).toEqual(1);
		expect(teamAdherences[0].Id).toEqual("teamGreenGuid");
		expect(teamAdherences[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in permitted team for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "teamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "teamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "teamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			})
			.withPermittedTeams(["teamGreenGuid"]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(1);
		expect(vm.teams[0].Id).toEqual("teamGreenGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(5);
	});

});

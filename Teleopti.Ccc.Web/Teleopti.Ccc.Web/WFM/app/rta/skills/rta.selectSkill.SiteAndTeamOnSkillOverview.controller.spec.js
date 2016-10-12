'use strict';
xdescribe('RtaSiteAndTeamOnSkillOverviewCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout;
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

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaSiteAndTeamOnSkillOverviewCtrl');

		$fakeBackend.clear();

	}));

	it('should display agents out of adherence in site for selected skill', function() {
		$fakeBackend
			.withSite({
				Id: "LondonGuid",
				Name: "London",
			})
			.withSite({
				Id: "ParisGuid",
				Name: "Paris",
			})
			.withSiteAdherenceForSkill({
				Id: "LondonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 5,
				SkillId: "emailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 3,
				SkillId: "phoneSkillGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "emailSkillGuid"
				});
			});

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Name).toEqual("Paris");
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in sites for selected skillArea', function() {
		$fakeBackend
			.withSite({
				Id: "LondonGuid",
				Name: "London",
			})
			.withSite({
				Id: "ParisGuid",
				Name: "Paris",
			})
			.withSiteAdherenceForSkillArea({
				Id: "LondonGuid",
				OutOfAdherence: 1,
				SkillAreaId: "PhoneSkillGuid"
			})
			.withSiteAdherenceForSkillArea({
				Id: "ParisGuid",
				OutOfAdherence: 5,
				SkillAreaId: "EmailSkillGuid"
			})
			.withSiteAdherenceForSkillArea({
				Id: "AnotherGuid",
				OutOfAdherence: 3,
				SkillAreaId: "PhoneSkillGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "EmailSkillGuid"
				});
			});

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Name).toEqual("Paris");
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in sites for pre selected skill', function() {
		stateParams.skillId = "emailSkillGuid";
		$fakeBackend
			.withSite({
				Id: "LondonGuid",
				Name: "London",
			})
			.withSite({
				Id: "ParisGuid",
				Name: "Paris",
			})
			.withSiteAdherenceForSkill({
				Id: "LondonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 5,
				SkillId: "emailSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Id).toEqual("ParisGuid");
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in sites for pre selected skillArea', function() {
		stateParams.skillAreaId = "PhoneSkillGuid";
		$fakeBackend
			.withSite({
				Id: "LondonGuid",
				Name: "London",
			})
			.withSite({
				Id: "ParisGuid",
				Name: "Paris",
			})
			.withSiteAdherenceForSkillArea({
				Id: "LondonGuid",
				OutOfAdherence: 1,
				SkillAreaId: "PhoneSkillGuid"
			})
			.withSiteAdherenceForSkillArea({
				Id: "ParisGuid",
				OutOfAdherence: 5,
				SkillAreaId: "EmailSkillGuid"
			})
			.withSiteAdherenceForSkillArea({
				Id: "ParisGuid",
				OutOfAdherence: 3,
				SkillAreaId: "PhoneSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[1].Name).toEqual("Paris");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].OutOfAdherence).toEqual(3);
	});

//should include other site to test filtering it out?
	it('should display teams for pre selected skill and sites', function() {
		stateParams.skillId = "EmailSkillGuid";
		stateParams.siteIds = ["ParisGuid"];
		$fakeBackend
			.withTeam({
				Id: "ParisTeamGreenGuid",
				Name: "Paris Team Green",
				SiteId: "ParisGuid"
			})
			.withTeam({
				Id: "ParisTeamRedGuid",
				Name: "Paris Team Red",
				SiteId: "ParisGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("Paris Team Red");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display teams for pre selected skillArea and sites', function() {
		stateParams.skillAreaId = "EmailSkillGuid";
		stateParams.siteIds = ["LondonGuid"];
		$fakeBackend
			.withTeam({
				Id: "LondonTeamGuid",
				Name: "London Team",
				SiteId: "LondonGuid"
			})
			.withTeam({
				Id: "ParisTeamGuid",
				Name: "Paris Team",
				SiteId: "ParisGuid"
			})
			.withTeamAdherenceForSkillArea({
				Id: "LondonTeamGuid",
				OutOfAdherence: 1,
				SkillAreaId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkillArea({
				Id: "LondonTeamGuid",
				OutOfAdherence: 3,
				SkillAreaId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkillArea({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillAreaId: "EmailSkillGuid"
			});


		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("London Team");
		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});



	it('should display agents out of adherence in teams for pre selected skill and sites', function() {
		stateParams.skillId = "EmailSkillGuid";
		stateParams.siteIds = ["ParisGuid"];
		$fakeBackend
			.withTeam({
				Id: "LondonTeamGuid",
				Name: "London Team",
				SiteId: "LondonGuid"
			})
			.withTeam({
				Id: "ParisTeamGuid",
				Name: "Paris Team",
				SiteId: "ParisGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "LondonTeamGuid",
				OutOfAdherence: 1,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			});

		$controllerBuilder.createController()
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(5);
	});

	it('should display teams for pre selected skillArea and sites', function() {
		stateParams.skillAreaId = "EmailSkillGuid";
		stateParams.siteIds = ["ParisGuid"];
		$fakeBackend
			.withTeam({
				Id: "LondonTeamGuid",
				Name: "London Team",
				SiteId: "LondonGuid"
			})
			.withTeam({
				Id: "ParisTeamGuid",
				Name: "Paris Team",
				SiteId: "ParisGuid"
			})
			.withTeamAdherenceForSkillArea({
				Id: "LondonTeamGuid",
				OutOfAdherence: 1,
				SkillAreaId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkillArea({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillAreaId: "EmailSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("Paris Team");
		expect(scope.teams[0].OutOfAdherence).toEqual(5);
	});


});
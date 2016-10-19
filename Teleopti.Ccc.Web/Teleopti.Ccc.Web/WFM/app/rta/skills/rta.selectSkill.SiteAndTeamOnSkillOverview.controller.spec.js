'use strict';
describe('RtaSiteAndTeamOnSkillOverviewCtrl', function() {
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
			.withSiteAdherenceForSkill({
				Id: "LondonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 3,
				SkillId: "emailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 2,
				SkillId: "phoneSkillGuid"
			});
		var c = $controllerBuilder.createController();
		c.apply(function() {
				scope.skillAreas =[ {
					Id: "EmailAndPhoneGuid",
					Skills:[{Id:"phoneSkillGuid"},{Id:"emailSkillGuid"}]
				}];
		});
		c.wait(5000);
		c.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "EmailAndPhoneGuid"
				});
		});

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].Name).toEqual("Paris");
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
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
		stateParams.skillAreaId = "EmailAndPhoneGuid";
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
				OutOfAdherence: 3,
				SkillId: "emailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 2,
				SkillId: "phoneSkillGuid"
			});
		var c = $controllerBuilder.createController();
		c.apply(function() {
				scope.skillAreas =[ {
					Id: "EmailAndPhoneGuid",
					Skills:[{Id:"phoneSkillGuid"},{Id:"emailSkillGuid"}]
				}];
		});
		c.wait(5000);
		
		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].Name).toEqual("Paris");
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

		it('should display teams for selected skill', function() {
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
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid",
				Id: "ParisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "EmailSkillGuid"
				});
			});

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("Paris Team Red");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display agents out of adherence in teams for selected skillArea', function() {
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
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid",
				Id: "ParisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});
		var c = $controllerBuilder.createController();
		c.apply(function() {
				scope.skillAreas = [{
					Id: "EmailAndPhoneGuid",
					Skills:[{Id:"PhoneSkillGuid"},{Id:"EmailSkillGuid"}]
				}];
		});
		c.wait(5000);
		c.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "EmailAndPhoneGuid"
				});
		});
		expect(scope.teams.length).toEqual(2);
		expect(scope.teams[0].Name).toEqual("Paris Team Green");
		expect(scope.teams[1].Name).toEqual("Paris Team Red");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);		
		expect(scope.teams[1].OutOfAdherence).toEqual(7);
	});

//should include other site to test filtering it out?
	it('should display agents out of adherence in team for pre selected skill', function() {
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
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid",
				Id: "ParisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("Paris Team Red");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display agents out of adherence in teams for pre selected skillArea', function() {
		stateParams.skillAreaId = "EmailAndPhoneGuid";
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
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid" ,
				Id: "ParisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "ParisGuid",
				Id: "ParisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});
		var c = $controllerBuilder.createController();
		c.apply(function() {
				scope.skillAreas =[ {
					Id: "EmailAndPhoneGuid",
					Skills:[{Id:"PhoneSkillGuid"},{Id:"EmailSkillGuid"}]
				}];
		});
		c.wait(5000);
		
		expect(scope.teams.length).toEqual(2);
		expect(scope.teams[0].Name).toEqual("Paris Team Green");
		expect(scope.teams[1].Name).toEqual("Paris Team Red");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);		
		expect(scope.teams[1].OutOfAdherence).toEqual(7);	
	});

});
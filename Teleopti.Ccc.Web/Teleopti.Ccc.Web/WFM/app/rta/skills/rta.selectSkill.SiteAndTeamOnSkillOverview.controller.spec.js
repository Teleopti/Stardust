'use strict';
fdescribe('RtaSiteAndTeamOnSkillOverviewCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout;
	var stateParams = {};
	var skills1 = [{
		Id: "PhoneSkillGuid",
		Name: "PhoneSkill"
	}, {
		Id: "EmailSkillGuid",
		Name: "EmailSkill"
	}];
	var skills2 = [{
		Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
		Name: "skill x"
	}, {
		Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
		Name: "skill y"
	}];

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

	it('should display site with agents with selected skill', function() {
		$fakeBackend
			.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris",
			})
			.withSiteAdherenceForSkill({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1,
				SkillId: "phoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 5,
				SkillId: "emailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
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
	});

	it('should display site with agents with selected skillArea', function() {
		$fakeBackend
			.withSkillAreas([{
				Id: "EmailPhone",
				Name: "my skill area 1",
				Skills: skills1
			}, {
				Id: "SkillAreaGuid2",
				Name: "my skill area 2",
				Skills: skills2
			}])
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
				SkillId: "PhoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "ParisGuid",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 3,
				SkillId: "PhoneSkillGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "EmailPhone"
				});
			});

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[1].Name).toEqual("Paris");
	});

	it('should display agents out of adherence in sites for pre selected skill', function() {
		stateParams.skillId = "emailSkillGuid";
		$fakeBackend
			.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris",
			})
			.withSiteAdherenceForSkill({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1,
				SkillId: "phoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 5,
				SkillId: "emailSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});
	it('should display site with agents with pre selected skillArea', function() {
		stateParams.skillAreaId = "SkillAreaGuid1";
		$fakeBackend
			.withSkillAreas([{
				Id: "SkillAreaGuid1",
				Name: "my skill area 1",
				Skills: skills1
			}, {
				Id: "SkillAreaGuid2",
				Name: "my skill area 2",
				Skills: skills2
			}])
		$fakeBackend
			.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris",
			})
			.withSiteAdherenceForSkill({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1,
				SkillId: "PhoneSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 3,
				SkillId: "PhoneSkillGuid"
			});

		$controllerBuilder.createController().wait(5000);

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[1].Name).toEqual("Paris");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should display teams for pre selected skill and sites', function() {
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
			.withTeam({
				Id: "ParisTeamPreferenceGuid",
				Name: "Paris Team Preference",
				SiteId: "ParisGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "LondonTeamGuid",
				OutOfAdherence: 1,
				SkillId: "PhoneSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "LondonTeamGuid",
				OutOfAdherence: 2,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamPreferenceGuid",
				OutOfAdherence: 7,
				SkillId: "EmailSkillGuid"
			});

		$controllerBuilder.createController()
			.wait(5000);

		expect(scope.teams.length).toEqual(2);
		//expect(scope.teams[0].OutOfAdherence).toEqual(5);
		expect(scope.teams[0].Name).toEqual("Paris Team");
		expect(scope.teams[1].Name).toEqual("Paris Team Preference");
	});

	it('should display teams for pre selected skillArea and sites', function() {
		stateParams.skillAreaId = "SkillAreaGuid1";
		stateParams.siteIds = ["LondonGuid"];
		$fakeBackend
			.withSkillAreas([{
				Id: "SkillAreaGuid1",
				Name: "my skill area 1",
				Skills: skills1
			}, {
				Id: "SkillAreaGuid2",
				Name: "my skill area 2",
				Skills: skills2
			}])
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
				Id: "LondonTeamGuid",
				OutOfAdherence: 3,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			});


		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		//expect(scope.teams[0].OutOfAdherence).toEqual(5);
		expect(scope.teams[0].Name).toEqual("London Team");
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
		stateParams.skillAreaId = "SkillAreaGuid1";
		stateParams.siteIds = ["LondonGuid"];
		$fakeBackend
			.withSkillAreas([{
				Id: "SkillAreaGuid1",
				Name: "my skill area 1",
				Skills: skills1
			}, {
				Id: "SkillAreaGuid2",
				Name: "my skill area 2",
				Skills: skills2
			}])
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
				Id: "LondonTeamGuid",
				OutOfAdherence: 3,
				SkillId: "EmailSkillGuid"
			})
			.withTeamAdherenceForSkill({
				Id: "ParisTeamGuid",
				OutOfAdherence: 5,
				SkillId: "EmailSkillGuid"
			});


		$controllerBuilder.createController().wait(5000);

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Name).toEqual("London Team");
		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});

});

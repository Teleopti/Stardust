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

		spyOn($state, 'go');
		$fakeBackend.withToggle('RTA_SiteAndTeamOnSkillOverview_40817');
	}));

	it('should agents out of adherence in teams for selected skill', function() {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "emailGuid"
				});
			});

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Id).toEqual("parisTeamRedGuid");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display agents out of adherence in teams for selected skill area', function() {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
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
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "emailAndPhoneGuid"
				});
			});

		expect(scope.teams.length).toEqual(2);
		expect(scope.teams[0].Id).toEqual("parisTeamGreenGuid");
		expect(scope.teams[1].Id).toEqual("parisTeamRedGuid");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
		expect(scope.teams[1].OutOfAdherence).toEqual(7);
	});

	//should include other site to test filtering it out?
	it('should display agents out of adherence in team for preselected skill', function() {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			});

		$controllerBuilder.createController();

		expect(scope.teams.length).toEqual(1);
		expect(scope.teams[0].Id).toEqual("parisTeamRedGuid");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display agents out of adherence in teams for preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
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

		expect(scope.teams.length).toEqual(2);
		expect(scope.teams[0].Id).toEqual("parisTeamGreenGuid");
		expect(scope.teams[1].Id).toEqual("parisTeamRedGuid");
		expect(scope.teams[0].OutOfAdherence).toEqual(7);
		expect(scope.teams[1].OutOfAdherence).toEqual(7);
	});

	it('should update adherence for team and selected skill', function() {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "phoneGuid"
				});
			}).wait(5000)
			.apply(function() {
				$fakeBackend.clearTeamAdherencesForSkill()
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
			})
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for team and selected skill area', function() {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
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
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "emailAndPhoneGuid"
				});
			}).wait(5000)
			.apply(function() {
				$fakeBackend.clearTeamAdherencesForSkill()
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 1,
						SkillId: "emailGuid"
					})
			})
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(4);
	});

	it('should update adherence for team and preselected skill', function() {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				$fakeBackend
					.clearTeamAdherencesForSkill()
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
			})
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for site and preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 7,
				SkillId: "phoneGuid"
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
			.apply(function() {
				$fakeBackend.clearTeamAdherencesForSkill()
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
					.withTeamAdherenceForSkill({
						SiteId: "parisGuid",
						Id: "parisTeamGreenGuid",
						OutOfAdherence: 4,
						SkillId: "emailGuid"
					})
			})
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should have preselected skill in field for teams', function() {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		$controllerBuilder.createController();

		expect(scope.selectedSkill.Id).toEqual('phoneGuid');
		expect(scope.selectedSkill.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for teams', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["parisGuid"];
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		$controllerBuilder.createController();

		expect(scope.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(scope.selectedSkillArea.Name).toEqual('Email and phone');
	});

	it('should update url when changing from skill to skill area', function() {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);;

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "emailAndPhoneGuid"
				});
			})
			.wait(5000);

		expect($state.go).toHaveBeenCalledWith('rta.sites-by-skillArea', {
			skillAreaId: "emailAndPhoneGuid"
		});
	});

	it('should update url when changing from skill area to skill', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);;

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "phoneGuid"
				});
			})
			.wait(5000);

		expect($state.go).toHaveBeenCalledWith('rta.sites-by-skill', {
			skillIds: "phoneGuid"
		});
	});

	it('should set site name in breadcrumb', function() {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid",
				Name: "Paris"
			})
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("Paris");
	});


});

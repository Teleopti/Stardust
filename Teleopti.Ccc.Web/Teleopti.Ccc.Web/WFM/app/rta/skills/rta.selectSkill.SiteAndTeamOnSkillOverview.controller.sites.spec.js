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

	}));

	it('should display agents out of adherence in sites for selected skill', function() {
		$fakeBackend
			.withSite({
				Id: "parisGuid",
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "emailGuid"
				});
			});

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Id).toEqual("parisGuid");
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in sites for selected skill area', function() {
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 3,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 2,
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
				scope.selectedSkillAreaChange({
					Id: "emailAndPhoneGuid"
				});
			});

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Id).toEqual("londonGuid");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].Id).toEqual("parisGuid");
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

	/*is it even possible to have a preselected skill in the sites by skill view*/
	it('should display agents out of adherence in sites for preselected skill', function() {
		stateParams.skillIds = "emailGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			});

		$controllerBuilder.createController();

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Id).toEqual("parisGuid");
		expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	/*is it even possible to have a preselected skill in the sites by skill area view*/
	it('should display agents out of adherence in sites for preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 3,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 2,
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

		$controllerBuilder.createController();

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Id).toEqual("londonGuid");
		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].Id).toEqual("parisGuid");
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should update adherence for site and selected skill', function() {
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "phoneGuid"
				});
			})
			.wait(5000)
			.apply(function() {
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
			})
			.wait(5000);

		expect(scope.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for site and selected skill area', function() {
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillAreaId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
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
			})
			.wait(5000)
			.apply(function() {
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 4,
						SkillId: "emailGuid"
					})
			})
			.wait(5000);

		expect(scope.sites[0].OutOfAdherence).toEqual(7);
	});

	it('should update adherence for site and preselected skill', function() {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
			})
			.wait(5000);

		expect(scope.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for site and preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillAreaId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
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
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 4,
						SkillId: "emailGuid"
					})
			})
			.wait(5000);

		expect(scope.sites[0].OutOfAdherence).toEqual(7);
	});

	it('should have preselected skill in field for sites', function() {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
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

	it('should have preselected skill area in field for sites', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
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

	it('should not allow simoultaneous selection when new skill area', function() {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid",
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
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

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillAreaChange({
					Id: "emailAndPhoneGuid"
				});
			});

		expect($state.go).toHaveBeenCalledWith('rta.sites-by-skillArea', {
			skillAreaId: "emailAndPhoneGuid"
		});
	});

	it('should not allow simoultaneous selection when new skill', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid",
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
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

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "phoneGuid"
				});
			});

		expect($state.go).toHaveBeenCalledWith('rta.sites-by-skill', {
			skillIds: "phoneGuid"
		});
	});

	it('should update url when selecting skill', function() {
		$fakeBackend
			.withSite({
				Id: "parisGuid",
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedSkillChange({
					Id: "emailGuid"
				});
			});

		expect($state.go).toHaveBeenCalledWith('rta.sites-by-skill', {
			skillIds: "emailGuid"
		});
	});

});

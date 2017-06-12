'use strict';
describe('RtaFilterController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout,
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

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaFilterController');
		$fakeBackend.clear();
		spyOn($state, 'go');
	}));

	it('should go to sites by selected skill', function () {
		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "emailGuid"
			});
		});

		expect(vm.selectedSkill.Id).toEqual("emailGuid");
		expect($state.go).toHaveBeenCalledWith('rta.sites', { skillIds: "emailGuid", skillAreaId: undefined });
	});

	it('should go to sites by selected skill area', function () {
		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "emailAndPhoneGuid"
			});
		});

		expect(vm.selectedSkillArea.Id).toEqual("emailAndPhoneGuid");
		expect($state.go).toHaveBeenCalledWith('rta.sites', { skillIds: undefined, skillAreaId: "emailAndPhoneGuid" });
	});

	it('should not allow simoultaneous selection when new skill area', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "parisGuid",
				InAlarmCount: 5,
				SkillId: "emailGuid"
			})
			.withSiteAdherence({
				Id: "parisGuid",
				InAlarmCount: 3,
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "emailAndPhoneGuid"
			});
		});

		expect($state.go).toHaveBeenCalledWith('rta.sites', {
			skillAreaId: "emailAndPhoneGuid",
			skillIds: undefined
		});
	});

	it('should not allow simoultaneous selection when new skill', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "parisGuid",
				InAlarmCount: 5,
				SkillId: "emailGuid"
			})
			.withSiteAdherence({
				Id: "parisGuid",
				InAlarmCount: 3,
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneGuid"
			});
		});

		expect($state.go).toHaveBeenCalledWith('rta.sites', {
			skillIds: "phoneGuid",
			skillAreaId: undefined
		});
	});

	it('should have preselected skill in field for sites', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkill.Id).toEqual('phoneGuid');
		expect(vm.selectedSkill.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for sites', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
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

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});

	it('should go to teams for selected skill', function () {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneGuid"
			});
		})
			.wait(5000);

		expect(vm.selectedSkill.Id).toEqual("phoneGuid");
		expect($state.go).toHaveBeenCalledWith("rta.sites", { skillIds: "phoneGuid", skillAreaId: undefined });
	});

	it('should go to teams for selected skill area', function () {
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherence({
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "emailAndPhoneGuid"
			});
		})
			.wait(5000);

		expect(vm.selectedSkillArea.Id).toEqual("emailAndPhoneGuid");
		expect($state.go).toHaveBeenCalledWith("rta.sites", { skillAreaId: "emailAndPhoneGuid", skillIds: undefined });
	});

	it('should have preselected skill in field for teams', function () {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkill.Id).toEqual('phoneGuid');
		expect(vm.selectedSkill.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for teams', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["parisGuid"];
		$fakeBackend
			.withTeamAdherence({
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

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});

	it('should go to sites when on teams and changing from skill to skill area', function () {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = ["parisGuid"];
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
			}])
			.withOrganizationOnSkills({
				Id: 'parisGuid',
				Teams: [{
					Id: 'parisTeamGuid',
				}]
			}, "phoneGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "emailAndPhoneGuid"
			});
		});

		expect($state.go).toHaveBeenCalledWith('rta.sites', {
			skillAreaId: "emailAndPhoneGuid",
			skillIds: undefined
		});
	});

	it('should go to sites when on teams and changing from skill area to skill', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["parisGuid"];
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
			}])
			.withOrganizationOnSkills({
				Id: 'parisGuid',
				Teams: [{
					Id: 'parisTeamGuid',
				}]
			}, "phoneGuid, emailGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneGuid"
			});
		})
			.wait(5000);

		expect($state.go).toHaveBeenCalledWith('rta.sites', {
			skillIds: "phoneGuid",
			skillAreaId: undefined
		});
	});
});

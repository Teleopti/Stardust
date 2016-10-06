'use strict';
fdescribe('RtaSiteAndTeamOnSkillOverviewCtrl', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout;
	var stateParams = {};
	var skills = [{ Id: "5f15b334-22d1-4bc1-8e41-72359805d30f", Name: "skill x" }, { Id: "4f15b334-22d1-4bc1-8e41-72359805d30c", Name: "skill y" }];
	var skills2 = [{ Id: "1f15b334-22d1-4bc1-8e41-72359805d30f", Name: "skill a" }, { Id: "3f15b334-22d1-4bc1-8e41-72359805d30c", Name: "skill b" }];

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
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

		scope = $controllerBuilder.setup('RtaSiteAndTeamOnSkillOverviewCtrl');

		$fakeBackend.clear();

	}));

	it('should display site with agents with selected skill', function () {
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
				.apply(function () {
					scope.selectedSkillChange({
						Id: "emailSkillGuid"
					});
				});

		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Name).toEqual("Paris");
	});

	it('should display agents out of adherence in sites for selected skill', function () {
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

		$controllerBuilder.createController()
			.apply(function () {
				scope.selectedSkillChange({
					Id: "emailSkillGuid"
				});
			});
			expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});

	xit('should display agents out of adherence in sites for selected skill', function () {
		stateParams.skillId= "emailSkillGuid";
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

		$controllerBuilder.createController();
			expect(scope.sites[0].OutOfAdherence).toEqual(5);
	});
});

'use strict';
describe('RtaSelectSkillCtrl', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaSelectSkillCtrl');

		$fakeBackend.clear();

	}));

	it('should display skill', function () {
		$fakeBackend.withSkill({
			Name: "Channel Sales",
			Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
		});

		$controllerBuilder.createController();
		
		expect(scope.skills[0].Name).toEqual("Channel Sales");
		expect(scope.skills[0].Id).toEqual("f08d75b3-fdb4-484a-ae4c-9f0800e2f753");
	});

	it('should filter on skill name', function () {
		$fakeBackend
			.withSkills([{
				Name: "Channel Sales",
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			}, {
				Name: "Email",
				Id: "BC50FC19-C211-4E7A-8A1A-9F0801134E37"
			}]);

		$controllerBuilder.createController();
		var result =  scope.querySearch("Email", scope.skills);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("Email");
	});

	it('should filter on lowercased skill name', function () {
		$fakeBackend
			.withSkills([{
				Name: "Channel Sales",
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			}, {
				Name: "Email",
				Id: "BC50FC19-C211-4E7A-8A1A-9F0801134E37"
			}]);

		$controllerBuilder.createController();
		var result = scope.querySearch("EmAiL", scope.skills);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("Email");
	});

	it('should go to agents on skill', function () {
		$fakeBackend.withSkill({
			Name: "Channel Sales",
			Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
		});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function () {
				scope.selectedSkillChange({
					Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
				});
			});

		expect($state.go).toHaveBeenCalledWith('rta.agents-skill', {
			skillIds: ['f08d75b3-fdb4-484a-ae4c-9f0800e2f753']
		});
	});
	
});

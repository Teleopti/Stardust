'use strict';
describe('RtaSelectSkillCtrl', function () {
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

	xit('should go to agents on skill', function () {
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
		 $timeout(function() {
	           expect($state.go).toHaveBeenCalledWith('rta.agents-skill', {
				skillId: 'f08d75b3-fdb4-484a-ae4c-9f0800e2f753'
				});
        });
		
	});

	it('should display skill areas', function () {
		$fakeBackend.withSkillAreas([
			{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: skills
			},
			{
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: skills
			}
		]);

		$controllerBuilder.createController();
		expect(scope.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skillAreas[0].Name).toEqual("my skill area 1");
		expect(scope.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should filter on skill area name', function () {
		$fakeBackend
			.withSkillAreas([
			{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: skills
			},
			{
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: skills
			}
			]);

		$controllerBuilder.createController();
		var result = scope.querySearch("my skill area 1", scope.skillAreas);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("my skill area 1");
	});

	it('should filter on lowercased skill area name', function () {
		
		$fakeBackend
			.withSkillAreas([
			{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: skills
			},
			{
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: skills
			}
			]);

		$controllerBuilder.createController();
		var result = scope.querySearch("my SKill Area 1", scope.skillAreas);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("my skill area 1");
	});


	xit('should go to agents on skill areas', function () {

		$fakeBackend
			.withSkillAreas([
			{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: skills
			},
			{
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: skills2
			}
			]);

		
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function () {
				scope.selectedSkillAreaChange({
					Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4"
				});
			});

		$timeout(function () {
			expect($state.go).toHaveBeenCalledWith('rta.agents-skill-area', {
				skillAreaId: '836cebb6-cee8-41a1-bb62-729f4b3a63f4'
			});
		});

	});

});

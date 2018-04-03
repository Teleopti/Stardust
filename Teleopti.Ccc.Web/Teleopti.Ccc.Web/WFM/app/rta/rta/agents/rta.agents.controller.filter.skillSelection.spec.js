'use strict';
rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													$state,
													$controllerBuilder,
													stateParams) {
	var vm;

	it('should display skill', function (t) {
		t.backend.withSkill({
			Name: "Channel Sales",
			Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.skills[0].Name).toEqual("Channel Sales");
		expect(vm.skills[0].Id).toEqual("f08d75b3-fdb4-484a-ae4c-9f0800e2f753");
	});

	it('should filter on skill name', function (t) {
		t.backend
			.withSkill({
				Name: "Channel Sales",
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withSkill({
				Name: "Email",
				Id: "BC50FC19-C211-4E7A-8A1A-9F0801134E37"
			});

		vm = $controllerBuilder.createController().vm;

		var result = vm.filterSkills("Email", vm.skills);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("Email");
	});

	it('should filter on lowercased skill name', function (t) {
		t.backend
			.withSkill({
				Name: "Channel Sales",
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withSkill({
				Name: "Email",
				Id: "BC50FC19-C211-4E7A-8A1A-9F0801134E37"
			});

		vm = $controllerBuilder.createController().vm;
		var result = vm.filterSkills("EmAiL", vm.skills);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("Email");
	});

	it('should display skill areas', function (t) {
		t.backend.withSkillAreas([{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill area 1",
			Skills: [{
				Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				Name: "skill x"
			}, {
				Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
				Name: "skill y"
			}]
		}, {
			Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
			Name: "my skill area 2",
			Skills: [{
				Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				Name: "skill x"
			}, {
				Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
				Name: "skill y"
			}]
		}
		]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(vm.skillAreas[0].Name).toEqual("my skill area 1");
		expect(vm.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(vm.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should filter on skill area name', function (t) {
		t.backend
			.withSkillAreas([{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "skill x"
				}, {
					Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
					Name: "skill y"
				}]
			}, {
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "skill x"
				}, {
					Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
					Name: "skill y"
				}]
			}
			]);

		vm = $controllerBuilder.createController().vm;

		var result = vm.filterSkills("my skill area 1", vm.skillAreas);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("my skill area 1");
	});

	it('should filter on lowercased skill area name', function (t) {
		t.backend
			.withSkillAreas([{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill area 1",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "skill x"
				}, {
					Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
					Name: "skill y"
				}]
			}, {
				Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "skill x"
				}, {
					Id: "4f15b334-22d1-4bc1-8e41-72359805d30c",
					Name: "skill y"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;
		var result = vm.filterSkills("my SKill Area 1", vm.skillAreas);

		expect(result.length).toEqual(1);
		expect(result[0].Name).toEqual("my skill area 1");
	});
});

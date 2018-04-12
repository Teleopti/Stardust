'use strict';
rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													$state,
													$fakeBackend,
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

	it('should display skill areas', function (t) {
		t.backend.withSkillGroups([{
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

		expect(vm.skillGroups[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(vm.skillGroups[0].Name).toEqual("my skill area 1");
		expect(vm.skillGroups[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(vm.skillGroups[0].Skills[0].Name).toEqual("skill x");
	});

	it('should get agent for skill', function () {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill', function () {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			State: "Ready"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].State).toEqual("Ready");
	});

	it('should state in alarm for skill', function () {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			State: "Break",
			TimeInAlarm: 0
		})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should get agent for skill area', function () {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";

		$fakeBackend
			.withSkillGroups([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "d08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withAgentState({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("22610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill area', function () {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend
			.withSkillGroups([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].State).toEqual("Ready");
	});

	it('should get state in alarm for skill area', function () {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend
			.withSkillGroups([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Break"
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should have preselected skill in field for sites', function (t) {
		t.stateParams.skillIds = "phoneGuid";
		t.backend
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

	it('should have preselected skill area in field for sites', function (t) {
		t.stateParams.skillAreaId = "emailAndPhoneGuid";
		t.backend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkillGroups([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillGroup.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillGroup.Name).toEqual('Email and phone');
	});

	it('should have preselected skill in field for teams', function (t) {
		t.stateParams.skillIds = "phoneGuid";
		t.stateParams.siteIds = "parisGuid";
		t.backend
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

	it('should have preselected skill area in field for teams', function (t) {
		t.stateParams.skillAreaId = "emailAndPhoneGuid";
		t.stateParams.siteIds = ["parisGuid"];
		t.backend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkillGroups([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillGroup.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillGroup.Name).toEqual('Email and phone');
	});

	it('should go to agents with skill when on agents view', function (t) {
		$state.current.name = "rta-agents";
		t.backend.withSkill({
			Id: "phoneSkillGuid"
		})

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkill = {
				Id: "phoneSkillGuid"
			};
		});

		expect(t.lastGoParams.skillIds).toEqual(['phoneSkillGuid']);
		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents with skillGroup when on agents view', function (t) {
		$state.current.name = "rta-agents";
		t.backend.withSkillGroups([{
			Id: "phoneAndEmailGuid"
		}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillGroup = {
				Id: "phoneAndEmailGuid"
			}
		});

		expect(t.lastGoParams.skillIds).toEqual(undefined);
		expect(t.lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	it('should go to agents by skillGroup and clear skill from stateParams when on agents view', function (t) {
		$state.current.name = "rta-agents";
		stateParams.skillIds = ["phoneSkillGuid"];
		t.backend
			.withSkill({
				Id: "phoneSkillGuid"
			})
			.withSkillGroups([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneSkillGuid"
				}, {
					Id: "emailSkillGuid"
				},]
			}])

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillGroup = {
				Id: "phoneAndEmailGuid"
			}
		});

		expect(t.lastGoParams.skillIds).toEqual(undefined);
		expect(t.lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	it('should go to agents by skill and clear skillGroup from stateParams when on agents view', function (t) {
		$state.current.name = "rta-agents";
		stateParams.skillAreaId = "phoneAndEmailGuid";
		t.backend
			.withSkill({
				Id: "phoneSkillGuid"
			})
			.withSkillGroups([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneSkillGuid"
				}, {
					Id: "emailSkillGuid"
				},]
			}])

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkill = {
				Id: "phoneSkillGuid"
			}
		});

		expect(t.lastGoParams.skillIds).toEqual(['phoneSkillGuid']);
		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents by skill and clear site from stateParams when on agents view', function (t) {
		$state.current.name = "rta-agents";
		stateParams.skillAreaId = "phoneAndEmailGuid";
		stateParams.siteIds = ['londonGuid'];
		t.backend
			.withSkill({
				Id: "phoneGuid"
			})
			.withSkillGroups([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				},]
			}])
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid")
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid, emailGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkill = {
				Id: "phoneGuid"
			}
		});

		expect(t.lastGoParams.skillIds).toEqual(['phoneGuid']);
		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents by skill area and clear site from stateParams when on agents view', function (t) {
		$state.current.name = "rta-agents";
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = ['londonGuid'];
		t.backend
			.withSkill({
				Id: "phoneGuid"
			})
			.withSkillGroups([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				},]
			}])
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid")
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid, emailGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillGroup = {
				Id: "phoneAndEmailGuid"
			}
		});

		expect(t.lastGoParams.skillIds).toEqual(undefined);
		expect(t.lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	it('should filter skills', function (t) {
		t.backend
			.withSkill({
				Id: "a",
				Name: "aSkill"
			})
			.withSkill({
				Id: "b",
				Name: "bSkill"
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillPickerText = "a";
		});

		expect(vm.skills.length).toEqual(1);
		expect(vm.skills[0].Name).toEqual("aSkill");
	});

	it('should filter skills', function (t) {
		t.backend
			.withSkill({
				Id: "a",
				Name: "aSkill"
			})
			.withSkill({
				Id: "b",
				Name: "bSkill"
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillPickerText = "b";
		});

		expect(vm.skills.length).toEqual(1);
		expect(vm.skills[0].Name).toEqual("bSkill");
	});

	it('should display skills', function (t) {
		t.backend
			.withSkill({
				Id: "id"
			});
		var vm = t.createController();

		expect(vm.skills[0].Id).toEqual("id");
	});

	it('should select skill', function (t) {
		var skill = {
			Id: "id",
			Name: "skill"
		};
		t.backend
			.withSkill(skill);
		var vm = t.createController();

		t.apply(function () {
			vm.selectedSkill = skill;
		});

		expect(vm.selectedSkill.Id).toEqual("id");
		expect(vm.selectedSkill.Name).toEqual("skill");
		expect(vm.skillPickerText).toEqual("skill");
	});

	it('should display with picker closed', function (t) {
		var vm = t.createController();

		expect(vm.skillPickerOpen).toEqual(false);
	});

	it('should close picker when skill selected', function (t) {
		var skill = {
			Id: "id",
			Name: "skill"
		};
		t.backend
			.withSkill(skill);
		var vm = t.createController();

		t.apply(function () {
			vm.skillPickerOpen = true;
		});
		t.apply(function () {
			vm.selectedSkill = skill;
		});

		expect(vm.skillPickerOpen).toEqual(false);
	});

	it('should display selected skill from url', function (t) {
		t.stateParams.skillIds = ["id"];
		t.backend
			.withSkill({
				Id: "id",
				Name: "skill"
			});
		var vm = t.createController();

		expect(vm.skillPickerText).toEqual("skill");
	});

	it('should filter skill groups', function (t) {
		t.backend
			.withSkillGroup({
				Id: "a",
				Name: "aaaa"
			})
			.withSkillGroup({
				Id: "b",
				Name: "bbb"
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillGroupPickerText = "a";
		});

		expect(vm.skillGroups.length).toEqual(1);
		expect(vm.skillGroups[0].Name).toEqual("aaaa");
	});

	it('should filter skill groups', function (t) {
		t.backend
			.withSkillGroup({
				Id: "a",
				Name: "aaaa"
			})
			.withSkillGroup({
				Id: "b",
				Name: "bbbb"
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillGroupPickerText = "b";
		});

		expect(vm.skillGroups.length).toEqual(1);
		expect(vm.skillGroups[0].Name).toEqual("bbbb");
	});

	it('should display skill groupds', function (t) {
		t.backend
			.withSkillGroup({
				Id: "id"
			});
		var vm = t.createController();

		expect(vm.skillGroups[0].Id).toEqual("id");
	});

	it('should select skill group', function (t) {
		var skillGroup = {
			Id: "id",
			Name: "group"
		};
		t.backend
			.withSkillGroup(skillGroup);
		var vm = t.createController();

		t.apply(function () {
			vm.selectedSkillGroup = skillGroup;
		});

		expect(vm.selectedSkillGroup.Id).toEqual("id");
		expect(vm.selectedSkillGroup.Name).toEqual("group");
		expect(vm.skillGroupPickerText).toEqual("group");
	});

	it('should display with skill group picker closed', function (t) {
		var vm = t.createController();

		expect(vm.skillGroupPickerOpen).toEqual(false);
	});

	it('should close skill group picker when group selected', function (t) {
		var skillGroup = {
			Id: "id",
			Name: "skill"
		};
		t.backend
			.withSkillGroup(skillGroup);
		var vm = t.createController();

		t.apply(function () {
			vm.skillGroupPickerOpen = true;
		});
		t.apply(function () {
			vm.selectedSkillGroup = skillGroup;
		});

		expect(vm.skillGroupPickerOpen).toEqual(false);
	});

	it('should display selected skill group from url', function (t) {
		t.stateParams.skillAreaId = "id";
		t.backend
			.withSkillGroup({
				Id: "id",
				Name: "group"
			});
		var vm = t.createController();

		expect(vm.skillGroupPickerText).toEqual("group");
	});

	it('should clear skill text when selecting skill group', function (t) {
		var skill = {
			Id: "skill",
			Name: "skill"
		};
		var skillGroup = {
			Id: "group",
			Name: "group"
		};
		t.backend
			.withSkill(skill)
			.withSkillGroup(skillGroup);
		var vm = t.createController();

		t.apply(function () {
			vm.selectedSkill = skill;
		});
		t.apply(function () {
			vm.selectedSkillGroup = skillGroup;
		});

		expect(vm.skillPickerText).toBeUndefined();
	});

	it('should clear skill group text when selecting skill', function (t) {
		var skill = {
			Id: "skill",
			Name: "skill"
		};
		var skillGroup = {
			Id: "group",
			Name: "group"
		};
		t.backend
			.withSkill(skill)
			.withSkillGroup(skillGroup);
		var vm = t.createController();

		t.apply(function () {
			vm.selectedSkillGroup = skillGroup;
		});
		t.apply(function () {
			vm.selectedSkill = skill;
		});

		expect(vm.skillGroupPickerText).toBeUndefined();
	});
});

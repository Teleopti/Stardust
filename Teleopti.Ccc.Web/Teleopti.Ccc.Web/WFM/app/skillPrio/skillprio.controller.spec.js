'use strict';
fdescribe('skillPrioController', function () {
	var $httpBackend,
		$controller,
		fakeBackend;

	beforeEach(function () {
		module('wfm.skillPrio');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _fakeSkillPrioBackend_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		fakeBackend = _fakeSkillPrioBackend_;

		fakeBackend.clear();
	}));

	it('should get activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		});

		var vm = $controller('skillPrioController');
		$httpBackend.flush();


		expect(vm.activites.length).toEqual(1);
	});

	it('should get activities', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withActivity({
			ActivityGuid: "a98d2c45-a8f4-4c70-97f9-907ab364af75",
			ActivityName: "Lunch"

		});

		var vm = $controller('skillPrioController');
		$httpBackend.flush();

		expect(vm.activites.length).toEqual(2);
	});

	it('should be able to select activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.selectedActivity.ActivityGuid).toEqual(vm.activites[0].ActivityGuid)
	});

	it('should get skills for selected activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: 2,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.skills[0].ActivityGuid).toBe(vm.selectedActivity.ActivityGuid);
	});


	it('should get skill for selected activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.selectedActivity.ActivityGuid).toBe(vm.activitySkills[0].ActivityGuid);
	});

	it('should prioritize skill ', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[0], 1);


		expect(vm.prioritizedSkills.length).toBe(1);
		expect(vm.prioritizedSkills[0].Priority).toBe(1)
	});

	it('should remove skill from being prioritized', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[0], 1);
		vm.removeFromPrioritized(vm.prioritizedSkills[0]);

		expect(vm.prioritizedSkills.length).toBe(0);
		expect(vm.activitySkills.length).toBe(1);

	})
	it('should clear priority when removing', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[0], 1);
		vm.removeFromPrioritized(vm.prioritizedSkills[0]);

		expect(vm.activitySkills[0].Priority).toBe(null);

	});

	it('should check if no skills are prioritized', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		
		expect(vm.noPrioritiedSkills()).toBe(true);
		
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[0], 1);

		expect(vm.noPrioritiedSkills()).toBe(false);

	});
		xit('should save current prioritized skills', function(){
				fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "e56950c6-ecc1-4540-ae44-f84ad22d23cd",
				ActivityName: "Phone",
				Priority: 1,
				SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				SkillName: "Channel Sales"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		
		expect(vm.noPrioritiedSkills()).toBe(true);
		
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[0], 1);

		vm.save()
	});

	it('should put skill with same prio as sibling', function(){
				fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				ActivityName: "Phone",
				Priority: null,
				SkillGuid: "3f28a7b9-6e41-4ed7-a66d-b0a23ce621f5",
				SkillName: "Radio"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[1], 1);
		vm.prioritizeSkill(vm.activitySkills[0], 1);
		expect(vm.prioritizedSkills[0].sibling.length).toBe(1);
	});

		it('should remove skill with sibling and persist sibling', function(){
				fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				ActivityName: "Phone",
				Priority: null,
				SkillGuid: "3f28a7b9-6e41-4ed7-a66d-b0a23ce621f5",
				SkillName: "Radio"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[1], 1);
		vm.prioritizeSkill(vm.activitySkills[0], 1);

		vm.removeFromPrioritized(vm.prioritizedSkills[0].sibling[0])
		expect(vm.prioritizedSkills.length).toBe(1);
	});

			it('should remove skill with multiple siblings and persist siblings', function(){
				fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"

		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		})
			.withSkill({
				ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				ActivityName: "Phone",
				Priority: null,
				SkillGuid: "3f28a7b9-6e41-4ed7-a66d-b0a23ce621f5",
				SkillName: "Radio"
			})
				.withSkill({
				ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				ActivityName: "Phone",
				Priority: null,
				SkillGuid: "4bcc3af2-5210-41e2-a838-4bccce37c740",
				SkillName: "Klingon"
			});
		var vm = $controller('skillPrioController');
		$httpBackend.flush();
		
		vm.selectActivity(vm.activites[0]);
		vm.prioritizeSkill(vm.activitySkills[2], 1);
		vm.prioritizeSkill(vm.activitySkills[1], 1);
		vm.prioritizeSkill(vm.activitySkills[0], 1);

		vm.removeFromPrioritized(vm.prioritizedSkills[0].sibling[0])
		expect(vm.prioritizedSkills.length).toBe(1);
	});


});


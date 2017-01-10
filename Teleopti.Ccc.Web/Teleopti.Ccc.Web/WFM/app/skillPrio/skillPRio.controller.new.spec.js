'use strict';
describe('skillPrioControllerNew', function () {
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
		$httpBackend.whenGET("../ToggleHandler/AllToggles").respond(200, {});

		fakeBackend.clear();
	}));

	it('should get activities', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"
		}).withActivity({
			ActivityGuid: "a98d2c45-a8f4-4c70-97f9-907ab364af75",
			ActivityName: "Lunch"
		});

		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();

		expect(vm.activites.length).toEqual(2);
	});

  it('should get skills', function () {
    fakeBackend.withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: 2,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		}).withSkill({
			ActivityGuid: "9bbe70b5-6c58-4019-a7e7-57bb0e064207",
			ActivityName: "Sales",
			Priority: 2,
			SkillGuid: "59deabd0-3a95-4e25-ac8a-feb1a685a46f",
			SkillName: "Customer Email"
		});

    var vm = $controller('skillPrioControllerNew');
    $httpBackend.flush();

    expect(vm.skills.length).toEqual(2);
  });

	it('should be able to select activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"
		});
		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.selectedActivity.ActivityGuid).toEqual(vm.activites[0].ActivityGuid)
	});

	it('should get correct skill for selected activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"
		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: 2,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		}).withSkill({
			ActivityGuid: "9bbe70b5-6c58-4019-a7e7-57bb0e064207",
			ActivityName: "Sales",
			Priority: 2,
			SkillGuid: "59deabd0-3a95-4e25-ac8a-feb1a685a46f",
			SkillName: "Customer Email"
		});
		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.skills[0].ActivityGuid).toBe(vm.selectedActivity.ActivityGuid);
	});

	it('should get correct skills for selected activity', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"
		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: null,
			SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales"
		}).withSkill({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone",
			Priority: 3,
			SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
			SkillName: "BTS"
		}).withSkill({
			ActivityGuid: "9bbe70b5-6c58-4019-a7e7-57bb0e064207",
			ActivityName: "Sales",
			Priority: 2,
			SkillGuid: "59deabd0-3a95-4e25-ac8a-feb1a685a46f",
			SkillName: "Customer Email"
		});
		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();

		vm.selectActivity(vm.activites[0]);

		expect(vm.selectedActivity.ActivityGuid).not.toBe(vm.skills[2].ActivityGuid);
	});

	it('should get unsorted skills for selected activity', function () {
    fakeBackend.withActivity({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 3,
      SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
      SkillName: "BTS"
    });
		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();
		vm.selectActivity(vm.activites[0]);

		expect(vm.unsortedList.length).toBe(2);
	});

  it('should get cascade skills for selected activity', function () {
    fakeBackend.withActivity({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 4,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 3,
      SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
      SkillName: "BTS"
    });
		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();
		vm.selectActivity(vm.activites[0]);

		expect(vm.cascadeList.length).toBe(3); //autoNewRow
	});

  it('should remove skill from cascade skills and move it back to unsortedList', function () {
    fakeBackend.withActivity({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 4,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 3,
      SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
      SkillName: "BTS"
    });
    var vm = $controller('skillPrioControllerNew');
    $httpBackend.flush();
    vm.selectActivity(vm.activites[0]);
    vm.moveBackToUnsort(vm.cascadeList[0].Skills, vm.skills[2]);

    expect(vm.cascadeList.length).toBe(2);
    expect(vm.unsortedList.length).toBe(2);
  });

  it('should reset the priority for skill that move from cascadeList to unsortedList', function () {
    fakeBackend.withActivity({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 4,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 3,
      SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
      SkillName: "BTS"
    });
    var vm = $controller('skillPrioControllerNew');
    $httpBackend.flush();
    vm.selectActivity(vm.activites[0]);
    vm.moveBackToUnsort(vm.cascadeList[0].Skills, vm.skills[2]);
    vm.save();

    expect(vm.skills[2].SkillName).toBe("BTS");
    expect(vm.skills[2].Priority).toBe(null);
  });

  it('should reset the priority for skill that move from unsortedList to cascadeList', function () {
    fakeBackend.withActivity({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: null,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 4,
      SkillGuid: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
      SkillName: "Channel Sales"
    }).withSkill({
      ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
      ActivityName: "Phone",
      Priority: 3,
      SkillGuid: "2e4c372c-627b-4306-8d17-d92a50bc0605",
      SkillName: "BTS"
    });
    var vm = $controller('skillPrioControllerNew');
    $httpBackend.flush();
    vm.selectActivity(vm.activites[0]);
    vm.cascadeList[1].Skills.push(vm.skills[0]);
    vm.save();

    expect(vm.skills[0].SkillName).toBe("Channel Sales");
    expect(vm.skills[0].Priority).toBe(4);
  });
});

'use strict';
describe('StaffingController', function () {
  var $httpBackend,
  $controller,
  scope,
  UtilService,
  fakeBackend,
  vm;

  beforeEach(function () {
    module('wfm.staffing');
  });

  beforeEach(inject(function (_$rootScope_, _$httpBackend_, _$controller_, _UtilService_ ) {
    $httpBackend = _$httpBackend_;
    $controller = _$controller_;
    UtilService = _UtilService_;
    scope = _$rootScope_.$new();

    // $httpBackend.whenGET('../api/').respond(function (method, url, data, headers) {
    //   return [200, fakeChanges]
    // });

  }));

  it('should return mdi-alert if do display data is false', function () {
    var skill = { DoDisplayData: false, SkillType: 'SkillTypeChat' }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    expect(a).toBe('mdi mdi-alert');
  });

  it('should toggle overtime settings', function () {
    var vm = $controller('StaffingController', {
      $scope: scope
    });
    vm.toggleOverstaffSettings()

    expect(vm.showOverstaffSettings).toBe(true);
    vm.toggleOverstaffSettings()
    expect(vm.showOverstaffSettings).toBe(false);
  });

  it('should return multisite icon if multisite', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: true }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    expect(a).toBe('mdi mdi-hexagon-multiple');
  });

  it('should return text icon if chat', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat' }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    expect(a).toBe('mdi mdi-message-text-outline');
  });

  it('should update chart when changing skill', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  };
    var skill2 = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'321'  };

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    vm.selectedSkill = skill;
    vm.selectedSkillChange(skill2);

    expect(vm.selectedSkill.Id).toBe('321');
  });


  it('should be able to use shrinkage', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    vm.selectedSkill = skill;
    vm.useShrinkageForStaffing();

    expect(vm.useShrinkage).toBe(true);
    expect(vm.hasSuggestionData).toBe(false);
  });

  it('should be able to view staffing for selected date ', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    spyOn(vm, 'generateChart').and.callThrough();
    vm.selectedSkill = skill;
    vm.navigateToNewDay();

    expect(vm.showOverstaffSettings).toBe(false);
    expect(vm.generateChart).toHaveBeenCalled();
  });

  it('should be able to use use overstaff settings', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)
    vm.showOverstaffSettings = false;

    vm.selectedSkill = skill;
    vm.toggleOverstaffSettings()

    expect(vm.showOverstaffSettings).toBe(true);
  });

  it('should be able to use see overstaff settings', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)
    vm.showOverstaffSettings = false;

    vm.selectedSkill = skill;
    vm.toggleOverstaffSettings()

    expect(vm.showOverstaffSettings).toBe(true);
  });

  it('should be able to use use overstaff settings', function () {
    var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id:'123'  }

    var vm = $controller('StaffingController', {
      $scope: scope
    });
    var a = vm.dynamicIcon(skill)

    vm.selectedSkill = skill;
    vm.selectedSkillChange(skill);
    vm.overtimeForm = {
      Compensation:"9019d62f-0086-44b1-a977-9bb900b8c361",
      MinMinutesToAdd: new Date(),
      MaxMinutesToAdd: new Date()
    }

    vm.suggestOvertime(vm.overtimeForm);
    // expect query to have been called without error
  });

});

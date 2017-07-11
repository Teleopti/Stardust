'use strict';
describe('StaffingController', function () {
    var $httpBackend,
        $controller,
        fakeBackend;

    beforeEach(function () {
        module('wfm.staffing');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_ ) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;

    }));


    it('should toggle overtime settings', function () {

        var vm = $controller('StaffingController');
        vm.toggleOverstaffSettings()

        expect(vm.showOverstaffSettings).toBe(true);
        vm.toggleOverstaffSettings()
        expect(vm.showOverstaffSettings).toBe(false);
    });

    it('should return mdi-alert if do display data is false', function () {
        var skill = { DoDisplayData: false, SkillType: 'SkillTypeChat' }

        var vm = $controller('StaffingController');
        var a = vm.dynamicIcon(skill)

        expect(a).toBe('mdi mdi-alert');
    });

    it('should return multisite icon if multisite', function () {
        var skill = { DoDisplayData: true, IsMultisiteSkill: true }

        var vm = $controller('StaffingController');
        var a = vm.dynamicIcon(skill)

        expect(a).toBe('mdi mdi-hexagon-multiple');
    });
    it('should return text icon if chat', function () {
        var skill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat' }

        var vm = $controller('StaffingController');
        var a = vm.dynamicIcon(skill)

        expect(a).toBe('mdi mdi-message-text-outline');
    });

});

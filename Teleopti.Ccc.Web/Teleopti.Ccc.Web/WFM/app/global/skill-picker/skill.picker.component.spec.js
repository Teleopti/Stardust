﻿'use strict';

describe('skillPickerComponent', function() {
    var $componentController,
        toggleSvc,
        ctrl,
        mockedSkills,
        mockedSkillAreas,
        mockedItemToReturn,
        preselectedSkill,
        stateService,
        $state,
        preselectedSkillArea;

    beforeEach(function() {
        module('wfm.skillPicker');

        module(function($provide) {
            $provide.service('$state', function() {
                return stateService;
            });
        });
    });

    beforeEach(
        inject(function($injector) {
            $componentController = $injector.get('$componentController');
            toggleSvc = $injector.get('Toggle');
            $state = $injector.get('$state');
            mockedSkills = [
                {
                    Id: '813348e1-7e4b-4252-8346-a39a00b839c7',
                    Name: 'BTS'
                },
                {
                    Id: 'f08d75b3-fdb4-484a-ae4c-9f0800e2f753',
                    Name: 'Channel Sales'
                }
            ];

            mockedSkillAreas = [
                {
                    Name: 'SkillArea1',
                    Id: '40143e07-7a88-4386-8b45-a7a0008fd682',
                    Skills: [
                        {
                            Id: '813348e1-7e4b-4252-8346-a39a00b839c7',
                            Name: 'BTS'
                        }
                    ]
                },
                {
                    Name: 'SkillArea2',
                    Id: 'd7d3dbe8-b8a4-438d-9288-a7a0008fdf6a',
                    Skills: [
                        {
                            Id: 'f08d75b3-fdb4-484a-ae4c-9f0800e2f753',
                            Name: 'Channel Sales'
                        }
                    ]
                }
            ];

            preselectedSkill = {skillIds: ['813348e1-7e4b-4252-8346-a39a00b839c7']};
            preselectedSkillArea = {skillAreaId: '40143e07-7a88-4386-8b45-a7a0008fd682'};

            mockedItemToReturn = function(item) {};
        })
    );

    it('should clear first input when selecting other input', function() {
        ctrl = $componentController('skillPicker', null, {
            skills: mockedSkills,
            skillAreas: mockedSkillAreas,
            itemToReturn: mockedItemToReturn
        });
        spyOn(ctrl, 'itemToReturn');

        ctrl.selectedSkill = ctrl.skills[0];
        ctrl.selectSkill(ctrl.skills[0]);
        ctrl.selectedSkillArea = ctrl.skillAreas[0];
        ctrl.selectSkillArea(ctrl.skillAreas[0]);

        expect(ctrl.selectedSkill).toEqual(null);
        expect(ctrl.selectedSkillArea).toEqual(ctrl.skillAreas[0]);
        expect(ctrl.itemToReturn).toHaveBeenCalledWith(ctrl.skillAreas[0]);
    });

    it('should be able to clear skill input', function() {
        ctrl = $componentController('skillPicker', null, {
            skills: mockedSkills,
            skillAreas: mockedSkillAreas,
            itemToReturn: mockedItemToReturn
        });
        spyOn(ctrl, 'itemToReturn');

        ctrl.selectedSkill = ctrl.skills[0];
        ctrl.selectSkill(ctrl.skills[0]);
        ctrl.selectedSkill = null;
        ctrl.selectSkill(undefined);

        expect(ctrl.selectedSkill).toEqual(null);
        expect(ctrl.itemToReturn).toHaveBeenCalledWith(undefined);
    });

    it('should be able to clear skillArea input', function() {
        ctrl = $componentController('skillPicker', null, {
            skills: mockedSkills,
            skillAreas: mockedSkillAreas,
            itemToReturn: mockedItemToReturn
        });
        spyOn(ctrl, 'itemToReturn');

        ctrl.selectedSkillArea = ctrl.skillAreas[0];
        ctrl.selectSkillArea(ctrl.skillAreas[0]);
        ctrl.selectedSkillArea = null;
        ctrl.selectSkillArea(undefined);

        expect(ctrl.selectedSkillArea).toEqual(null);
        expect(ctrl.itemToReturn).toHaveBeenCalledWith(undefined);
    });

    it('should have preselected skill', function() {
        ctrl = $componentController('skillPicker', null, {
            skills: mockedSkills,
            skillAreas: mockedSkillAreas,
            itemToReturn: mockedItemToReturn,
            preselectedItem: preselectedSkill
        });

        ctrl.$onInit();

        expect(ctrl.selectedSkill).toEqual(mockedSkills[0]);
    });

    it('should have preselected skill area', function() {
        ctrl = $componentController('skillPicker', null, {
            skills: mockedSkills,
            skillAreas: mockedSkillAreas,
            itemToReturn: mockedItemToReturn,
            preselectedItem: preselectedSkillArea
        });

        ctrl.$onInit();

        expect(ctrl.selectedSkillArea).toEqual(mockedSkillAreas[0]);
    });
});

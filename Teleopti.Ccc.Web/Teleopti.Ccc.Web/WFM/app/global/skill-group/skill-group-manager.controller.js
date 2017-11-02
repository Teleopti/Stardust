(function() {
    'use strict';
    angular.module('wfm.skillGroup').controller('SkillGroupManagerController', SkillGroupManagerController);

    SkillGroupManagerController.$inject = [
        '$state',
        'SkillGroupSvc',
        '$filter',
        'NoticeService',
        '$translate',
        'skillIconService',
        '$rootScope'
    ];

    function SkillGroupManagerController(
        $state,
        SkillGroupSvc,
        $filter,
        NoticeService,
        $translate,
        skillIconService,
        $rootScope
    ) {
        var vm = this;

        vm.managerData = {
            currentSkillGroup: null
        };

        var _ = $rootScope._;
        vm.selectedTabIndex = 0;
        vm.skills = [];
        vm.allSkills = [];
        vm.selectedSkills = [];
        vm.selectedGroupSkills = [];
        vm.skillAreaName = '';
        vm.getSkillIcon = skillIconService.get;
        vm.canSave = false;

        vm.selectSkillGroup = function(skillGroup) {
            if (vm.managerData.currentSkillGroup === skillGroup) {
                vm.managerData.currentSkillGroup = null;
            } else {
                vm.managerData.currentSkillGroup = skillGroup;
            }
            if (vm.managerData.currentSkillGroup) {
                vm.skills = _.sortBy(
                    _.differenceBy(vm.allSkills, vm.managerData.currentSkillGroup.Skills, function(skill) {
                        return skill.Id;
                    }),
                    function(item) {
                        return item.Name;
                    }
                );
						}
						vm.canSave = false;
            unselectAllSkills();
        };

        vm.addSkills = function() {
            if (vm.selectedSkills.length <= 0) return;

            vm.managerData.currentSkillGroup.Skills = _.sortBy(
                _.unionBy(vm.selectedSkills, vm.managerData.currentSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
            vm.skills = _.sortBy(
                _.differenceBy(vm.allSkills, vm.managerData.currentSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
						vm.canSave = true;
            unselectAllSkills();
        };

        vm.removeSkills = function() {
            if (vm.selectedGroupSkills.length <= 0) return;

            vm.managerData.currentSkillGroup.Skills = _.sortBy(
                _.difference(vm.managerData.currentSkillGroup.Skills, vm.selectedGroupSkills),
                function(item) {
                    return item.Name;
                }
            );
            vm.skills = _.sortBy(
                _.differenceBy(vm.allSkills, vm.managerData.currentSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
						vm.canSave = true;
            unselectAllSkills();
        };

        var unselectAllSkills = function() {
            vm.selectedSkills = [];
            vm.selectedGroupSkills = [];
        };

        vm.selectSkill = function(skill) {
            if (_.find(vm.selectedSkills, skill)) {
                _.remove(vm.selectedSkills, skill);
            } else {
                vm.selectedSkills = _.unionBy(vm.selectedSkills, [skill], function(skill) {
                    return skill.Id;
                });
            }
        };

        vm.selectGroupSkill = function(skill) {
            if (_.find(vm.selectedGroupSkills, skill)) {
                _.remove(vm.selectedGroupSkills, skill);
            } else {
                vm.selectedGroupSkills = _.unionBy(vm.selectedGroupSkills, [skill], function(skill) {
                    return skill.Id;
                });
            }
        };

        vm.skillIsSelected = function(skill) {
            var index = vm.selectedSkills.indexOf(skill);
            return index !== -1;
        };

        vm.groupSkillIsSelected = function(skill) {
            var index = vm.selectedGroupSkills.indexOf(skill);
            return index !== -1;
        };

        vm.exitConfigMode = function() {
            $state.go($state.params.returnState, {isNewSkillArea: false});
        };

        vm.saveSkillGroup = function(form) {
            if (form.$invalid) {
                return;
            }
            var selectedSkills = $filter('filter')(vm.skills, {isSelected: true});

            var selectedSkillIds = selectedSkills.map(function(skill) {
                return skill.Id;
            });

            if (selectedSkillIds.length <= 0) {
                NoticeService.error($translate.instant('SkillAreaNoSkillSelected'), 5000, false);
                return;
            }

            SkillGroupSvc.createSkillGroup
                .query({
                    Name: vm.skillAreaName,
                    Skills: selectedSkillIds
                })
                .$promise.then(function(result) {
                    notifySkillGroupCreation();
                    $state.go('intraday', {isNewSkillArea: true});
                });
        };

        vm.saveAll = function() {
            SkillGroupSvc.modifySkillGroup(vm.managerData).then(function(result) {
								getAllSkillGroups();
								vm.canSave = false;
            });
        };

        vm.createSkillGroup = function(e) {
            document.getElementById('skillGroupNameBox').focus();
            vm.managerData.currentSkillGroup = null;
            vm.canSave = false;
        };

        var notifySkillGroupCreation = function() {
            NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
        };

        SkillGroupSvc.getSkills().then(function(result) {
            vm.skills = result.data;
            vm.allSkills = vm.skills.slice();
        });

        var getAllSkillGroups = function() {
            SkillGroupSvc.getSkillGroups().then(function(result) {
                vm.skillGroups = result.data.SkillAreas;
            });
        };

        getAllSkillGroups();
    }
})();

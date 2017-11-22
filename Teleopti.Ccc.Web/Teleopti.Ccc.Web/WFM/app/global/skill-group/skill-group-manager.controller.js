//This is the SGM used in PBI #43727 (the new one as of november 2017)
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
        var _ = $rootScope._;
        var originalGroups = [];

        var vm = this;
        vm.selectedTabIndex = 0;
        vm.selectedSkillGroup = null;
        vm.skills = [];
        vm.allSkills = [];
        vm.selectedSkills = [];
        vm.selectedGroupSkills = [];
        vm.skillAreaName = '';
        vm.getSkillIcon = skillIconService.get;
        vm.canSave = false;
        vm.deleteConfirmation = false;

        vm.selectSkillGroup = function(group) {
            if (isNumeric(group)) {
                vm.selectedSkillGroup = vm.skillGroups[group];
            } else {
                vm.selectedSkillGroup = group;
            }
            vm.skills = _.sortBy(
                _.differenceBy(vm.allSkills, vm.selectedSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
            setSaveableState();
            unselectAllSkills();
        };

        vm.addSkills = function() {
            if (vm.selectedSkills.length <= 0) return;

            vm.selectedSkillGroup.Skills = _.sortBy(
                _.unionBy(vm.selectedSkills, vm.selectedSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
            vm.skills = _.sortBy(
                _.differenceBy(vm.allSkills, vm.selectedSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
            setSaveableState();
            unselectAllSkills();
        };

        vm.removeSkills = function() {
            if (vm.selectedGroupSkills.length <= 0) return;

            vm.selectedSkillGroup.Skills = _.sortBy(
                _.difference(vm.selectedSkillGroup.Skills, vm.selectedGroupSkills),
                function(item) {
                    return item.Name;
                }
            );
            vm.skills = _.sortBy(
                _.differenceBy(vm.allSkills, vm.selectedSkillGroup.Skills, function(skill) {
                    return skill.Id;
                }),
                function(item) {
                    return item.Name;
                }
            );
            setSaveableState();
            unselectAllSkills();
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
            SkillGroupSvc.modifySkillGroups(vm.skillGroups).then(function(result) {
                getAllSkillGroups();
                vm.canSave = false;
            });
        };

        vm.createSkillGroup = function(e) {
            document.getElementById('skillGroupNameBox').focus();
            var newGroup = {
                Name: '',
                Id: null,
                Skills: []
            };
            vm.skillGroups.push(newGroup);
            vm.selectedSkillGroup = newGroup;
            vm.skills = vm.allSkills.slice();
            vm.canSave = false;
        };

        vm.deleteSkillGroup = function() {
            SkillGroupSvc.deleteSkillGroup(vm.selectedSkillGroup).then(function() {
                getAllSkillGroups();
                unselectAllSkills();
                vm.selectedSkillGroup = null;
            });
        };

        function unselectAllSkills() {
            vm.selectedSkills = [];
            vm.selectedGroupSkills = [];
        }

        function setSaveableState() {
            if (
                vm.selectedSkillGroup !== null &&
                vm.selectedSkillGroup.Skills &&
                vm.selectedSkillGroup.Skills.length > 0 &&
                vm.selectedSkillGroup.Name &&
                vm.selectedSkillGroup.Name.length > 0
            ) {
                vm.canSave = true;
            } else {
                vm.canSave = false;
            }
        }

        function notifySkillGroupCreation() {
            NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
        }

        function getAllSkillGroups() {
            SkillGroupSvc.getSkillGroups().then(function(result) {
                vm.skillGroups = originalGroups = result.data.SkillAreas;
            });
        }

        function getChangedGroups(originalGroups, newGroups) {
            if (originalGroups && newGroups) {
                return _.differenceWith(originalGroups, newGroups, _.isEqual);
            }
            return [];
        }

        function isNumeric(n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }

        SkillGroupSvc.getSkills().then(function(result) {
            vm.skills = result.data;
            vm.allSkills = vm.skills.slice();
        });

        if ($state.params.selectedGroup && $state.params.selectedGroup.Id) {
            vm.selectSkillGroup($state.params.selectedGroup);
        }

        getAllSkillGroups();
    }
})();

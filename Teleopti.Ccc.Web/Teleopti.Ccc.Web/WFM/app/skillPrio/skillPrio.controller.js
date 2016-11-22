(function() {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .controller('skillPrioController', skillPrioController);

    skillPrioController.$inject = ['$filter', 'Toggle', '$location', 'NoticeService', '$translate', '$q', 'skillPrioAggregator', 'skillPrioService','indexOfPolyfill'];
    function skillPrioController($filter, toggleService, $location, NoticeService, $translate, $q, skillPrioAggregator, skillPrioService, indexOfPolyfill) {
        var vm = this;
        vm.selectActivity = selectActivity;
        vm.activites = skillPrioService.getAdminSkillRoutingActivity.query();
        vm.selectedActivity = "";
        vm.skills = skillPrioService.getAdminSkillRoutingPriority.query();
        vm.activitySkills = [];
        vm.prioritizedSkills = [];
        vm.prioritizeSkill = prioritizeSkill;
        vm.removeFromPrioritized = removeFromPrioritized;
        vm.noPrioritiedSkills = noPrioritiedSkills;
        vm.querySkills = querySkills;
        vm.queryActivities = queryActivities;
        vm.displayAutoComplete = displayAutoComplete;
        vm.save = save;
        vm.toggledOptimization = checkToggles();
        vm.addPrioritizeSkillAbove = addPrioritizeSkillAbove;
        vm.addPrioritizeSkillBelow = addPrioritizeSkillBelow;
        vm.ismodified = false;
        /////////////////////////////////

        function checkToggles() {
            toggleService.togglesLoaded.then(function() {
                vm.toggledOptimization = toggleService.Wfm_SkillPriorityRoutingGUI_39885
                if (!toggleService.Wfm_SkillPriorityRoutingGUI_39885) {
                    $location.path('#/')
                }
            });
        }

        function sortBySkillName(a, b) {
            var nameA = a.SkillName.toUpperCase(); // ignore upper and lowercase
            var nameB = b.SkillName.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }
            return 0;
        }

        function selectActivity(activity) {
            if (!selectActivityPreCheck(activity)) return;
            vm.selectedActivity = activity;
            var allActivitySkills = vm.skills.filter(belongsToActivity);
            vm.prioritizedSkills = unflattendDataFromServer(allActivitySkills.filter(hasPriority));
            vm.activitySkills = allActivitySkills.filter(lacksPriority);
            vm.activitySkills.sort(sortBySkillName);
        }

        function selectActivityPreCheck(activity) {
            var canContinue = true;
            if (!activity) {
                vm.selectedActivity = null;
                canContinue = false;
            }
            return canContinue;
        }

        function unflattendDataFromServer(data) {
            var skills = [];
            data.sort(sortBySkillName);
            data.forEach(function(item) {
                var existing = skills.find(function(skill) {
                    return item.Priority === skill.priority;
                });
                if (existing != null) {
                    existing.skills.push(item);
                } else {
                    skills.push({
                        priority: item.Priority,
                        skills: [item]
                    });
                }
            });
            return skills;
        }

        function belongsToActivity(value) {
            return value.ActivityGuid === vm.selectedActivity.ActivityGuid;
        }

        function hasPriority(value) {
            return value.Priority > 0;
        }
        function lacksPriority(value) {
            return value.Priority == null;
        }

        function skillPreChecks(skill, priority) {
            if (skill.Priority == null) {
                skill.Priority = priority;
            }
            return skill;
        }

        function findParentItem(skill) {
            return vm.prioritizedSkills.find(function(item) {
                return item.priority === skill.Priority;
            });
        }

        function flatMap(arr, lambda) {
            return Array.prototype.concat.apply([], arr.map(lambda));
        }


        function prioritizeSkill(skill, priority) {
            if (!skill) return;

            skillPreChecks(skill, priority);
            var parent = findParentItem(skill);

            if (parent != null) {
                parent.skills.push(skill);
            } else {
                vm.prioritizedSkills.push({
                    priority: skill.Priority,
                    skills: [skill]
                });
            }
            removeFromActivitySkills(skill)
        }

        function addPrioritizeSkillAbove(skill, priority) {
            if (!skill) return;

            var newPriority = priority + 1;

            vm.prioritizedSkills.forEach(function(item) {
                if (item.priority > priority) {
                    item.priority += 1;
                    item.skills.forEach(function(skill) {
                        skill.Priority += 1;
                    });
                }
            });
            prioritizeSkill(skill, newPriority);
        }

        function addPrioritizeSkillBelow(skill, priority) {
            if (!skill) return;

            vm.prioritizedSkills.forEach(function(item) {
                if (item.priority >= priority) {
                    item.priority += 1;

                    item.skills.forEach(function(skill) {
                        skill.Priority += 1;
                    });
                }
            });
            prioritizeSkill(skill, priority);
        }

        function removeSkill(arr, skill) {
            var found = arr.findIndex(function(item) {
                return item.SkillGuid === skill.SkillGuid;
            });
            vm.ismodified = true;
            if (found !== -1) {
                arr.splice(found, 1);
            }
        };

        function sanitizeSkill(skill) {
            skill.Priority = null;
            return skill;
        }

        function removeFromPrioritized(skill) {
            var parent = findParentItem(skill);
            if (parent != null) {
                vm.ismodified = true;
                parent.skills = parent.skills.filter(function(sib) {
                    return sib.SkillGuid != skill.SkillGuid;
                });
                if (parent.skills.length === 0) {
                    vm.prioritizedSkills.splice(vm.prioritizedSkills.indexOf(parent), 1);
                }
            }
            var sanitizedSkill = sanitizeSkill(skill);
            addToActivitySkills(sanitizedSkill);
        }

        function removeFromActivitySkills(skill) {
            removeSkill(vm.activitySkills, skill)
        }

        function addToActivitySkills(skill) {
            vm.activitySkills = vm.activitySkills.concat(skill);
            vm.activitySkills.sort(sortBySkillName);
            return;
        }

        function querySkills(query) {
            var results = $filter('filter')(vm.activitySkills, query);
            return results;
        }
        function queryActivities(query) {
            var results = $filter('filter')(vm.activites, query);
            return results;
        }

        function displayAutoComplete(skill, position) {
            if (!skill || !position) return;
            var currentAutocompletePosition = "showAutoComplete" + position;
            if (skill[currentAutocompletePosition]) {
                var skillAutocompletePosition = skill[currentAutocompletePosition] = !skill[currentAutocompletePosition];
                return skillAutocompletePosition;
            } else {
                skill[currentAutocompletePosition] = true;
                return;
            }
        }

        function flattenPrioritized() {
            return flatMap(vm.prioritizedSkills, function(item) {
                return item.skills;
            });
        }

        function save() {
            var flatSkills = flattenPrioritized();
            var allData = flatSkills.concat(vm.activitySkills);

            var query = skillPrioAggregator.saveSkills().save(allData);
            query.$promise.then(function() {
                vm.ismodified = false;

                NoticeService.success('All changes are saved', 5000, true);
                selectActivity(vm.selectedActivity);
            });
        };

        function noPrioritiedSkills() {
            return vm.prioritizedSkills.length <= 0;
        }
    }

})();

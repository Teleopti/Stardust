(function () {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.controller('skillPrioController', skillPrioController);

	skillPrioController.$inject = ['$scope', '$filter', 'Toggle', 'NoticeService', '$translate', '$q', 'skillPrioAggregator', 'skillPrioService'];
	function skillPrioController($scope, $filter, toggleService, NoticeService, $translate, $q, skillPrioAggregator, skillPrioService) {
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
		vm.queryActivitys = queryActivitys;
		vm.displayAutoComplete = displayAutoComplete;
		vm.save = save;
		vm.toggledOptimization = checkToggles();
		/////////////////////////////////Wfm_SkillPriorityRoutingGUI_39885
		function checkToggles() {
			toggleService.togglesLoaded.then(function () {
				vm.toggledOptimization = toggleService.Wfm_SkillPriorityRoutingGUI_39885
				if (!toggleService.Wfm_SkillPriorityRoutingGUI_39885) {
					document.location = "#/"
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
		}

		function selectActivity(activity) {
			console.log("selectActivity")
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
			var nonFlatData = [];
			data.sort(sortBySkillName);
			data.forEach(function (skill) {
				skill.sibling = []
				if (nonFlatData.some(function (e) {
					return e.Priority == skill.Priority
				})) {
					var parent = nonFlatData.find(function (e) {
						return e.Priority == skill.Priority
					})
					skill.hasParent = true;
					parent.sibling.push(skill);
				} else {
					nonFlatData.push(skill);
				}

			})
			return nonFlatData;
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

		function addSibling(arr, skill) {
			arr.sibling.push(skill);
		}

		function skillPreChecks(skill, priority) {
			if (!skill.Priority) {
				skill.Priority = priority
			} else {
				priority = skill.Priority;
			};
			if (!skill.sibling) {
				skill.sibling = []
			};
			if (isDuplicatePriority(skill, priority)) {
				console.log('has parent');
				skill.hasParent = true;
			} else {
				skill.hasParent = false;
			}
			return skill
		}

		function isDuplicatePriority(skill, priority) {
			if (!priority) return;
			var isDuplicate;
			vm.prioritizedSkills.forEach(function (item) {
				if (skill !== item && item.Priority === priority) {
					isDuplicate = true;
				}
			})
			return isDuplicate;
		}

		function findSkill(element) {
			return this !== element && element.Priority === this.Priority
		}

		function findDuplicateSkill(skill) {
			return vm.prioritizedSkills.find(findSkill, skill);
		}

		function prioritizeSkill(skill, priority) {
			if (!skill) return;
			skillPreChecks(skill, priority);
			if (skill.hasParent) {
				var parentSkill = findDuplicateSkill(skill);
				parentSkill.sibling.push(skill);
			} else {
				var exists = vm.prioritizedSkills.some(function (item) {
					return item.SkillGuid === skill.SkillGuid;
				})
				if (!exists) {
					vm.prioritizedSkills.push(skill);
				}
			}
			removeFromActivitySkills(skill)
		}

		function removeSkill(arr, skill) {
			var found = arr.findIndex(function (item) {
				return item.SkillGuid === skill.SkillGuid;
			});
			console.log(found);
			if (found !== -1)
				arr.splice(found, 1);
		};

		function sanitizeSkill(skill) {
			skill.hasParent = false;
			skill.Priority = null;
			skill.sibling = [];
			skill.showAutoCompleteBottom = false;
			skill.showAutoCompleteMiddle = false;
			skill.showAutoCompleteTop = false;
			return skill;
		}



		function removeFromPrioritized(skill) {
			console.log(skill);
			var skillRepository = [];
			if (skill.hasParent) {
				var parentSkill = findDuplicateSkill(skill);
				parentSkill.sibling = parentSkill.sibling.filter(function (sib) {
					return sib.SkillGuid != skill.SkillGuid;
				});
				//prioritizeSkill(parentSkill, skill.Priority)
				skillRepository.push(parentSkill);
			}
			if (skill.sibling.length > 0) {
				skill.sibling.forEach(function (sib) {
					// prioritizeSkill(sib, skill.Priority);
					skillRepository.push(sib);
				})
			}
			var sanatizedSkill = sanitizeSkill(skill);
			addToActivitySkills(sanatizedSkill)
			removeSkill(vm.prioritizedSkills, sanatizedSkill)
			skillRepository.forEach(function (remainingSkill) {
				prioritizeSkill(remainingSkill);
			})
		}

		function removeFromActivitySkills(skill) {
			console.log("removeFromActivitySkills")
			removeSkill(vm.activitySkills, skill)
		}
		function addToActivitySkills(skill) {
			console.log(skill)
			console.log(vm.activitySkills)
			vm.activitySkills = vm.activitySkills.concat(skill);
			vm.activitySkills.sort(sortBySkillName);
			return;
		}

		function querySkills(query) {
			var results = $filter('filter')(vm.activitySkills, query);
			return results;
		}
		function queryActivitys(query) {
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

		function flattenArr(arr) {
			var flatArr = [];
			arr.forEach(function (skill) {
				if (skill.sibling.length > 0) {
					skill.sibling.forEach(function (subskill) {
						flatArr.push(subskill);
					})
				}
				skill.sibling = [];
				flatArr.push(skill)

			})
			return flatArr
		};

		function save() {
			var flatSkills = flattenArr(vm.prioritizedSkills)
			var allData = flatSkills.concat(vm.activitySkills);

			var query = skillPrioAggregator.saveSkills().save(allData);
			query.$promise.then(function () {
				NoticeService.success('All changes are saved', 5000, true);
			});
		};

		function noPrioritiedSkills() {
			if (vm.prioritizedSkills.length > 0) {
				return false;
			} else {
				return true;
			}
		}

	}
})();
(function () {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.controller('skillPrioController', skillPrioController);

	skillPrioController.$inject = ['Toggle', '$filter', 'NoticeService', '$translate', '$q', 'skillPrioAggregator'];
	function skillPrioController(toggleService, $filter, NoticeService, $translate, $q, skillPrioAggregator) {
		var vm = this;
		vm.selectedActivity = '';
		vm.sortedSkills = [];
		vm.activitys = getActivitys();
		vm.skills = [];
		vm.addSkill = addSkill;
		vm.removeSkill = removeSkill;
		vm.noSortedSkills = noSortedSkills;
		vm.displayAutoComplete = displayAutoComplete;
		vm.querySkills = querySkills;
		vm.save = save;
		vm.getSkills = getSkills;

		////////////////

		function getActivitys() {
				return skillPrioAggregator.getActivitys();
		}

		function getSkills(activity) {
			if (!activity) {
				vm.skills = [];
			}
			vm.selectedActivity = activity;
			vm.skills = skillPrioAggregator.getSkillsForActivity(activity);

		}

		function removeFromUnsorted(skill) {
			vm.skills.splice(vm.skills.indexOf(skill), 1);
		}

		function addToUnsorted(skill) {
			vm.skills.push(skill);
		}

		function removeFromSorted(skill) {
			if (skill.hasParent) {
				vm.sortedSkills.forEach(function (parent) {
					if (parent.Priority === skill.Priority) {
						skill.hasParent = false;
						parent.siblings.splice(parent.siblings.indexOf(skill), 1);
					}
				});
			} else {
				skill.siblings = [];
				skill.hasParent = false;
				vm.sortedSkills.splice(vm.sortedSkills.indexOf(skill), 1);
			}

		}

		function promoteSiblings(arr) {
			arr.forEach(function (sibling) {
				addSkill(sibling.skill, sibling.Priority, sibling.isSibling);
			});
		}

		function removeSkill(skill) {
			var skillRepository = [];
			if (skill.siblings.length > 0) {
				skill.siblings[0].hasParent = false;
				skill.siblings.forEach(function (sibling) {
					var temp = { skill: sibling, Priority: skill.Priority, isSibling: true };
					skillRepository.push(temp);
				});
			}
			removeFromSorted(skill);
			addToUnsorted(skill);
			if (skillRepository.length > 0) {
				promoteSiblings(skillRepository);
			}
		}

		function querySkills(query) {
			var results = $filter('filter')(vm.skills, query);
			return results;
		}
		function queryActivitys(query) {
			var results = $filter('filter')(vm.activitys, query);
			return results;
		}

		function addSkill(skill, prio, isSibling) {
			if (!skill || !prio) return;
			skill.Priority = prio;
			var isDuplicate = false;
			console.log(vm.sortedSkills);
			vm.sortedSkills.forEach(function (sortedSkill) {
				if (skill.Priority === sortedSkill.Priority) {
					skill.hasParent = true;
					sortedSkill.siblings.push(skill);
					isDuplicate = true;
				}
			});
			if (!isDuplicate) {
				vm.sortedSkills.push(skill);
			}
			if (!isSibling) {
				skill.siblings = [];
				removeFromUnsorted(skill);
			}
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

		function save() {
			//skillPrioListService.save
			NoticeService.success('All changes are saved', 5000, true);
		}

		function noSortedSkills() {
			if (vm.sortedSkills.length > 0) {
				return false;
			} else {
				return true;
			}
		}

	}
})();
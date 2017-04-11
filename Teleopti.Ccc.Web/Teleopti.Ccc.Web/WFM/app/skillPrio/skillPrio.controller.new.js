(function () {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.controller('skillPrioControllerNew', skillPrioController);

	skillPrioController.$inject = ['$filter', 'Toggle', '$location', 'NoticeService', '$translate', '$q', 'skillPrioServiceNew'];

	function skillPrioController($filter, toggleService, $location, NoticeService, $translate, $q, skillPrioServiceNew) {
		var vm = this;

		vm.isModified = false;
		vm.selectedActivity = null;
		vm.activites = [];
		vm.cascadeList = [];
		vm.skills = [];
		vm.toggledOptimization = checkToggles;
		vm.selectActivity = selectActivity;
		vm.queryActivities = queryActivities;
		vm.moveBackToUnsort = moveBackToUnsort;
		vm.cancelChanges = cancelChanges;
		vm.save = save;

		resetAllList();
		getActivities();
		getSkills();

		function resetAllList() {
			vm.cascadeList = [];
		}

		function getActivities() {
			var getAct = skillPrioServiceNew.getActivites.query();
			return getAct.$promise.then(function (data) {
				vm.activites = data.sort(sortByName);
				vm.selectedActivity = vm.activites[0];
				return vm.activites;
			});
		}

		function getSkills() {
			var getSki = skillPrioServiceNew.getSkills.query();
			return getSki.$promise.then(function (data) {
				vm.skills = angular.copy(data);
				selectActivity(vm.selectedActivity);
				return vm.skills;
			});
		}

		function checkToggles() {
			toggleService.togglesLoaded.then(function () {
				vm.toggledOptimization = toggleService.Wfm_SkillPriorityRoutingGUI_39885;
				if (!toggleService.Wfm_SkillPriorityRoutingGUI_39885) {
					$location.path('#/')
				}
			});
		}

		vm.optionDrop = {
			moveItem: function (parent_id, source_id, dest_parent_id) {
				if (parent_id == dest_parent_id) {
					if (vm.isModified === true) {
						vm.isModified = true;
					} else {
						vm.isModified = false;
					}
					return;
				} else {
					var item = vm.cascadeList[parent_id].Skills.splice(source_id, 1);
					var moved = vm.cascadeList[dest_parent_id].Skills.push(item[0]);
					if (moved !== null) {
						autoNewRow();
						sortSkill(dest_parent_id);
						deleteEmptyRow(parent_id);
					}
					compareChange();
				}
			}
		}

		function compareChange() {
			var changes = setPriorityForSkill().filter(belongsToActivity);
			var origin = vm.skills.filter(belongsToActivity).sort(sortByName);
			var keepGoing = true;
			angular.forEach(origin, function (skill, i) {
				if (keepGoing) {
					var index = findIndexInData(changes, 'SkillName', skill.SkillName);
					if (index >= 0 && origin[i].Priority !== changes[index].Priority) {
						keepGoing = false;
						vm.isModified = true;
					} else {
						vm.isModified = false;
					}
				}
			});
		}

		function cancelChanges() {
			selectActivity(vm.selectedActivity);
			vm.isModified = false;
		}

		function deleteEmptyRow(parent_id) {
			if (parent_id > 0 && vm.cascadeList[parent_id].Skills.length === 0) {
				vm.cascadeList.splice(parent_id, 1);
				resetLevel(parent_id);
			}
		}

		function resetLevel(parent_id) {
			for (var i = parent_id; i < vm.cascadeList.length; i++) {
				vm.cascadeList[i].Priority = parseInt(i);
			}
		}

		function sortSkill(dest_parent_id) {
			if (dest_parent_id) {
				vm.cascadeList[dest_parent_id].Skills.sort(sortByName);
			}
		}

		function addNewRow() {
			var newRow = {
				Priority: 0,
				Skills: []
			};
			newRow.Priority = vm.cascadeList.length ? vm.cascadeList[vm.cascadeList.length - 1].Priority + 1 : 0;
			vm.cascadeList.push(newRow);
		}

		function autoNewRow() {
			if (vm.cascadeList.length > 1 && vm.cascadeList[vm.cascadeList.length - 1].Skills.length > 0) {
				addNewRow();
			}
		}

		function findIndexInData(data, property, value) {
			var result = -1;
			data.some(function (item, i) {
				if (item[property] === value) {
					result = i;
					return true;
				}
			});
			return result;
		}

		function selectActivity(activity) {
			if (activity !== null) {
				resetAllList();
				vm.selectedActivity = activity;
				var skillsOfSelectedActivity = angular.copy(vm.skills.filter(belongsToActivity).sort(sortByName));
				if (skillsOfSelectedActivity.length !== 0) {
					addNewRow();
					createCascadeLevel(skillsOfSelectedActivity);
				}
			}
		}

		function createCascadeLevel(skills) {
			if (skills.length > 0) {
				vm.cascadeList[0].Skills = skills.filter(lacksPriority);
				var skillsHasPriority = skills.filter(hasPriority);
				angular.forEach(skillsHasPriority, function (skill) {
					var index = findIndexInData(vm.cascadeList, 'Priority', skill.Priority);
					if (index >= 0) {
						vm.cascadeList[index].Skills.push(skill);
					} else {
						addNewRow();
						vm.cascadeList[vm.cascadeList.length - 1].Priority = skill.Priority;
						vm.cascadeList[vm.cascadeList.length - 1].Skills.push(skill);
					}
				});
				vm.cascadeList.sort(sortByPriority);
				addNewRow();
			}
		}

		function sortByName(a, b) {
			var nameA = a.SkillName ? a.SkillName.toUpperCase() : a.ActivityName.toUpperCase();
			var nameB = b.SkillName ? b.SkillName.toUpperCase() : b.ActivityName.toUpperCase();
			if (nameA < nameB) {
				return -1;
			}
			if (nameA > nameB) {
				return 1;
			}
			return 0;
		}

		function sortByPriority(a, b) {
			return a.Priority - b.Priority;
		}

		function belongsToActivity(value) {
			if (vm.selectedActivity) {
				return value.ActivityGuid === vm.selectedActivity.ActivityGuid;
			}
		}

		function hasPriority(value) {
			return value.Priority > 0;
		}

		function lacksPriority(value) {
			return value.Priority == null;
		}

		function queryActivities(query) {
			var results = $filter('filter')(vm.activites, query);
			return results;
		}

		function moveBackToUnsort(skills, skill, parent_id) {
			if (skill) {
				var index = findIndexInData(skills, 'SkillName', skill.SkillName);
				skills.splice(index, 1);
				vm.cascadeList[0].Skills.push(skill);
				vm.cascadeList[0].Skills.sort(sortByName);
				deleteEmptyRow(parent_id);
				compareChange();
			}
		}

		function save() {
			var allData = setPriorityForSkill();
			var query = skillPrioServiceNew.saveSkills.save(allData);
			query.$promise.then(function () {
				vm.isModified = false;
				NoticeService.success('All changes are saved', 5000, true);
			});
		}

		function setPriorityForSkill() {
			var prepareSkills = [];
			if (vm.cascadeList.length > 1) {
				vm.cascadeList[0].Skills.forEach(function (skill) {
					skill.Priority = null;
					prepareSkills.push(skill);
				});
				for (var i = 1; i < vm.cascadeList.length; i++) {
					vm.cascadeList[i].Skills.forEach(function (skill) {
						skill.Priority = vm.cascadeList[i].Priority;
						prepareSkills.push(skill);
					})
				}
			}
			return prepareSkills;
		}
	}
})();

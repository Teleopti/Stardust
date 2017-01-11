(function() {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.controller('skillPrioControllerNew', skillPrioController);

	skillPrioController.$inject = ['$filter', 'Toggle', '$location', 'NoticeService', '$translate', '$q', 'skillPrioServiceNew'];

	function skillPrioController($filter, toggleService, $location, NoticeService, $translate, $q, skillPrioServiceNew) {
		var vm = this;

		vm.ismodified = false;
		vm.selectedActivity = null;
		vm.toggledOptimization = checkToggles;
		vm.selectActivity = selectActivity;
		vm.queryActivities = queryActivities;
		vm.moveBackToUnsort = moveBackToUnsort;
		vm.save = save;

		resetAllList();
		getActivities();
		getSkills();

		function resetAllList() {
			vm.unsortedList = [];
			vm.cascadeList = [];
		}

		function getActivities() {
			var getAct = skillPrioServiceNew.getActivites.query();
			return getAct.$promise.then(function(data) {
				vm.activites = data;
				return vm.activites;
			});
		}

		function getSkills() {
			var getSki = skillPrioServiceNew.getSkills.query();
			return getSki.$promise.then(function(data) {
				vm.skills = data;
				selectActivity(vm.activites[0]); //for default setting
				return vm.skills;
			});
		}

		function checkToggles() {
			toggleService.togglesLoaded.then(function() {
				vm.toggledOptimization = toggleService.Wfm_SkillPriorityRoutingGUI_39885;
				if (!toggleService.Wfm_SkillPriorityRoutingGUI_39885) {
					$location.path('#/')
				}
			});
		}

		vm.unsortedOpt = {
			dragStart: function(event) {
				if (event.source.nodesScope.$id === event.dest.nodesScope.$id) {
					event.dest.nodesScope.nodropEnabled = true;
					event.elements.placeholder.remove();
				}
			},
			dropped: function(event) {
				if (event.pos.moving || vm.ismodified === true) {
					vm.ismodified = true;
					autoDeleteEmptyRow();
					autoSortSkillByEachRow();
					autoNewRow();
				} else {
					vm.ismodified = false;
				}
			},
			dragMove: function(event) {
				var placeholder = document.getElementsByClassName("angular-ui-tree-placeholder");
				for (var i = 0; i < placeholder.length; i++) {
					placeholder[i].innerHTML = "<div class='flip mdi mdi-exit-to-app'></div>";
				}
				if (vm.cascadeList.length === 0) {
					addNewRow();
				}
			}
		};

		vm.cascadeOpt = {
			dropped: function(event) {
				if (event.pos.moving || vm.ismodified === true) {
					vm.ismodified = true;
					autoDeleteEmptyRow();
					autoSortSkillByEachRow();
					autoNewRow();
				} else {
					vm.ismodified = false;
				}
			},
			dragStart: function(event) {
				var placeholder = document.getElementsByClassName("angular-ui-tree-placeholder");
				for (var i = 0; i < placeholder.length; i++) {
					placeholder[i].innerHTML = "<div class='flip mdi mdi-exit-to-app'></div>";
				}
			}
		};

		function addNewRow() {
			var newRow = {
				Priority: 1,
				Skills: []
			};
			newRow.Priority = vm.cascadeList.length ? vm.cascadeList[vm.cascadeList.length - 1].Priority + 1 : 1;
			vm.cascadeList.push(newRow);
		}

		function autoNewRow() {
			if (vm.cascadeList.length > 0 && vm.cascadeList[vm.cascadeList.length - 1].Skills.length > 0) {
				addNewRow();
			}
		}

		function autoDeleteEmptyRow() {
			if (vm.cascadeList.length > 0) {
				var i = vm.cascadeList.length
				while (i--) {
					if (vm.cascadeList[i].Skills.length === 0) {
						vm.cascadeList.splice(i, 1);
						resetCascadeLevel();
						if (vm.cascadeList.length > 0) {
							autoNewRow();
						}
					}
				}
			}
		}

		function resetCascadeLevel() {
			for (var i = 0; i < vm.cascadeList.length; i++) {
				vm.cascadeList[i].Priority = i + 1;
			}
		}

		function autoSortSkillByEachRow() {
			for (var i = 0; i < vm.cascadeList.length - 1; i++) {
				vm.cascadeList[i].Skills.sort(sortBySkillName);
			}
			vm.unsortedList.sort(sortBySkillName);
		}

		function findIndexInData(data, property, value) {
			var result = -1;
			data.some(function(item, i) {
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
				var skillsOfSelectedActivity = vm.skills.filter(belongsToActivity);
				vm.unsortedList = skillsOfSelectedActivity.filter(lacksPriority);
				var storedCascadeSkills = skillsOfSelectedActivity.filter(hasPriority);
				loadCascadeList(storedCascadeSkills);
				autoSortSkillByEachRow();
			}
		}

		function loadCascadeList(skills) {
			if (skills.length > 0) {
				for (var i = 0; i < skills.length; i++) {
					var index = findIndexInData(vm.cascadeList, 'Priority', skills[i].Priority);
					if (index >= 0) {
						vm.cascadeList[index].Skills.push(skills[i]);
					} else {
						addNewRow();
						vm.cascadeList[vm.cascadeList.length - 1].Priority = skills[i].Priority;
						vm.cascadeList[vm.cascadeList.length - 1].Skills.push(skills[i]);
					}
				}
				vm.cascadeList.sort(sortByPriority);
				addNewRow();
			}
		}

		function sortBySkillName(a, b) {
			var nameA = a.SkillName.toUpperCase();
			var nameB = b.SkillName.toUpperCase();
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

		function moveBackToUnsort(skills, skill) {
			if (skill) {
				vm.ismodified = true;
				var index = findIndexInData(skills, 'SkillName', skill.SkillName);
				skills.splice(index, 1);
				vm.unsortedList.push(skill);
				vm.unsortedList.sort(sortBySkillName);
				autoDeleteEmptyRow();
			}
		}

		function save() {
			vm.ismodified = false;
			var allData = setPriorityForSkill();
			var query = skillPrioServiceNew.saveSkills.save(allData);
			query.$promise.then(function() {
				vm.ismodified = false;
				NoticeService.success('All changes are saved', 5000, true);
			});
		}

		function setPriorityForSkill() {
			var prepareSkills = [];
			if (vm.unsortedList.length > 0) {
				vm.unsortedList.forEach(function(skill) {
					skill.Priority = null;
				})
				prepareSkills = angular.copy(vm.unsortedList);
			}
			if (vm.cascadeList.length > 0) {
				for (var i = 0; i < vm.cascadeList.length; i++) {
					vm.cascadeList[i].Skills.forEach(function(skill) {
						skill.Priority = vm.cascadeList[i].Priority;
						prepareSkills.push(skill);
					})
				}
			}
			return prepareSkills;
		}
	}
})();

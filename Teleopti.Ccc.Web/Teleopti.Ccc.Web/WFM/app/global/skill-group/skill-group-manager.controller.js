(function() {
	'use strict';

	angular.module('wfm.skillGroup').directive('focusMe', ['$timeout', '$parse', setTheFocus]);

	function setTheFocus($timeout, $parse) {
		return {
			link: function(scope, element, attrs) {
				var model = $parse(attrs.focusMe);
				scope.$watch(model, function(value) {
					if (value === true) {
						$timeout(function() {
							element[0].focus();
						});
					}
				});
			}
		};
	}

	angular.module('wfm.skillGroup').controller('SkillGroupManagerController', SkillGroupManagerController);

	SkillGroupManagerController.$inject = [
		'$scope',
		'$state',
		'SkillGroupSvc',
		'$filter',
		'NoticeService',
		'$translate',
		'skillIconService',
		'$rootScope',
		'Toggle',
		'$timeout',
		'CurrentUserInfo'
	];

	function SkillGroupManagerController(
		$scope,
		$state,
		SkillGroupSvc,
		$filter,
		NoticeService,
		$translate,
		skillIconService,
		$rootScope,
		Toggle,
		$timeout,
		CurrentUserInfo
	) {
		var _ = $rootScope._;
		var originalGroups = [];
		var vm = this;

		vm.isNew = true;
		vm.selectedTabIndex = 0;
		vm.selectedGroupIndex = -1;
		vm.skills = [];
		vm.allSkills = [];
		vm.selectedSkills = [];
		vm.selectedGroupSkills = [];
		vm.skillGroupName = '';
		vm.getSkillIcon = skillIconService.get;
		vm.canSave = false;
		vm.newGroupName = '';
		vm.deleteConfirmation = false;
		vm.modalShown = false;
		vm.skillNameMaxLength = 50;
		vm.stateName = '';
		vm.editGroupNameBox = false;
		vm.addGroupBox = false;
		vm.toggles = {};

		//----------- scoped functions ----------------------------------------------------

		vm.addSkills = function() {
			if (vm.selectedSkills.length <= 0) return;

			vm.skillGroups[vm.selectedGroupIndex].Skills = vm.sortByName(
				_.unionBy(vm.selectedSkills, vm.skillGroups[vm.selectedGroupIndex].Skills, function(skill) {
					return skill.Id;
				})
			);
			vm.skills = vm.sortByName(
				_.differenceBy(vm.allSkills, vm.skillGroups[vm.selectedGroupIndex].Skills, function(skill) {
					return skill.Id;
				})
			);
			vm.skillGroups[vm.selectedGroupIndex].Saved = false;

			setSaveableState();
			unselectAllSkills();
		};

		vm.copyGroupClicked = function(skillGroup, ev) {
			ev.stopPropagation();
			var clone = _.cloneDeep(skillGroup);
			clone.Name = ($translate.instant('CopyOf') + ' ' + skillGroup.Name).substr(0, vm.skillNameMaxLength);
			clone.Id = 'Copy' + skillGroup.Id + '-' + _.uniqueId();
			clone.Saved = false;
			vm.skillGroups.push(clone);

			vm.skillGroups = vm.sortByName(vm.skillGroups);

			setSaveableState();
		};

		vm.createSkillGroup = function(ev) {
			vm.newGroup = Object.assign({}, { Name: '', Id: 'New' + _.uniqueId(), Skills: [], Saved: false });
			vm.isNew = true;
			vm.addGroupBox = true;
			ev && ev.stopPropagation();
		};

		vm.deleteSkillGroup = function(deleted) {
			if (
				vm.skillGroups[vm.selectedGroupIndex].Id.indexOf('Copy') === 0 ||
				vm.skillGroups[vm.selectedGroupIndex].Id.indexOf('New') === 0
			) {
				_.remove(vm.skillGroups, vm.skillGroups[vm.selectedGroupIndex]);
				unselectAllSkills();
				vm.selectSkillGroup(-1);
				setSaveableState();
			} else {
				SkillGroupSvc.deleteSkillGroup(vm.skillGroups[vm.selectedGroupIndex]).then(function() {
					getAllSkillGroups();
					unselectAllSkills();
					vm.skillGroups[vm.selectedGroupIndex] = null;
					if (typeof deleted === 'function') deleted();
				});
			}
		};

		vm.editNameClicked = function(skillGroup) {
			vm.skillGroups[vm.selectedGroupIndex] = skillGroup;
			vm.newGroupName = skillGroup.Name;
			vm.editGroupNameBox = true;
			vm.isNew = false;
			vm.oldName = vm.skillGroups[vm.selectedGroupIndex].Name;
		};

		vm.exitConfigMode = function() {
			if (vm.stateName.length > 0) {
				$state.go(vm.stateName);
			} else {
				$state.go($state.params.returnState, { isNewSkillArea: false });
			}
		};

		vm.groupSkillIsSelected = function(skill) {
			var index = vm.selectedGroupSkills.indexOf(skill);
			return index !== -1;
		};

		vm.hasChanges = function() {
			return !_.isEqual(originalGroups, vm.skillGroups);
		};

		vm.hasEmptySkillList = function() {
			var hasEmpty = false;
			_.each(vm.skillGroups, function(item) {
				if (item.Skills.length === 0) {
					hasEmpty = true;
				}
			});
			return hasEmpty;
		};

		vm.nameBoxKeyPress = function(ev) {
			if (ev.key === 'Enter') {
				vm.saveNameEdit(ev);
			}
		};

		vm.removeSkills = function() {
			if (vm.selectedGroupSkills.length <= 0) return;

			vm.skillGroups[vm.selectedGroupIndex].Skills = vm.sortByName(
				_.difference(vm.skillGroups[vm.selectedGroupIndex].Skills, vm.selectedGroupSkills)
			);
			vm.skills = vm.sortByName(
				_.differenceBy(vm.allSkills, vm.skillGroups[vm.selectedGroupIndex].Skills, function(skill) {
					return skill.Id;
				})
			);

			vm.skillGroups[vm.selectedGroupIndex].Saved = false;
			setSaveableState();
			unselectAllSkills();
		};

		vm.saveAll = function() {
			SkillGroupSvc.modifySkillGroups(vm.skillGroups).then(function() {
				getAllSkillGroups();
				setSaveableState();
				vm.canSave = false;
			});
		};

		vm.saveNameEdit = function(ev) {
			if (vm.isNew) {
				if (vm.newGroupName && vm.newGroupName.length > 0) {
					vm.newGroup.Name = vm.newGroupName;
					vm.skills = vm.sortByName(vm.allSkills.slice());
					vm.skillGroups.push(vm.newGroup);
					vm.skillGroups = vm.sortByName(vm.skillGroups);
					vm.selectedGroupIndex = vm.skillGroups.indexOf(vm.newGroup);
					vm.newGroup = null;
				}
			} else {
				vm.skillGroups[vm.selectedGroupIndex].Saved = false;
				vm.skillGroups[vm.selectedGroupIndex].Name = vm.newGroupName;
			}
			setSaveableState();
			if (ev) {
				ev.stopPropagation();
			}
			vm.editGroupNameBox = false;
			vm.addGroupBox = false;
			vm.newGroupName = '';
		};

		vm.saveSkillGroup = function(form) {
			if (form.$invalid) {
				return;
			}
			var selectedSkills = vm.sortByName($filter('filter')(vm.skills, { isSelected: true }));

			var selectedSkillIds = selectedSkills.map(function(skill) {
				return skill.Id;
			});

			if (selectedSkillIds.length <= 0) {
				NoticeService.error($translate.instant('SkillAreaNoSkillSelected'), 5000, false);
				return;
			}

			SkillGroupSvc.createSkillGroup
				.query({ Name: vm.skillGroupName, Skills: selectedSkillIds })
				.$promise.then(function() {
					notifySkillGroupCreation();
					$state.go('intraday', { isNewSkillArea: true });
				});
		};

		vm.selectGroupSkill = function(skill) {
			if (_.find(vm.selectedGroupSkills, skill)) {
				_.remove(vm.selectedGroupSkills, skill);
			} else {
				vm.selectedGroupSkills = _.unionBy(vm.selectedGroupSkills, [skill], function(skill) {
					return skill.Id;
				});
			}
			vm.removeSkills();
		};

		vm.selectSkill = function(skill) {
			if (_.find(vm.selectedSkills, skill)) {
				_.remove(vm.selectedSkills, skill);
			} else {
				vm.selectedSkills = _.unionBy(vm.selectedSkills, [skill], function(skill) {
					return skill.Id;
				});
			}
			vm.addSkills();
		};

		vm.selectSkillGroup = function(group) {
			if (!group) return;
			vm.selectedGroupIndex = vm.skillGroups.indexOf(group);
			vm.skills = vm.sortByName(
				_.differenceBy(vm.allSkills, group.Skills, function(skill) {
					return skill.Id;
				})
			);
			unselectAllSkills();
		};

		vm.skillIsSelected = function(skill) {
			var index = vm.selectedSkills.indexOf(skill);
			return index !== -1;
		};

		vm.sortByName = function(arr) {
			if (!arr) return;
			return arr.sort(function(r1, r2) {
				return r1.Name.localeCompare(r2.Name, CurrentUserInfo.CurrentUserInfo().Language);
			});
		};
		//----------- Local functions ----------------------------------------------------

		function getAllSkillGroups(select) {
			SkillGroupSvc.getSkillGroups().then(function(result) {
				vm.skillGroups = result.data.SkillAreas;
				vm.skillGroups = vm.skillGroups.map(function(element) {
					element.Saved = true;
					return element;
				});
				vm.skillGroups = vm.sortByName(vm.skillGroups);
				originalGroups = _.cloneDeep(vm.skillGroups);
				if (select) {
					vm.selectSkillGroup(vm.selectedGroupIndex);
				}
			});
		}

		function setSaveableState() {
			vm.canSave = vm.hasChanges() && !vm.hasEmptySkillList();
		}

		function notifySkillGroupCreation() {
			NoticeService.success($translate.instant('Created') + ' ' + vm.skillGroupName, 5000, false);
		}

		function unselectAllSkills() {
			vm.selectedSkills = [];
			vm.selectedGroupSkills = [];
		}

		//----------- Run at instantiation ----------------------------------------------------

		SkillGroupSvc.getSkills().then(function(result) {
			vm.skills = vm.sortByName(result.data);
			vm.allSkills = vm.skills.slice();
		});

		if ($state.params.selectedGroup && $state.params.selectedGroup.Id) {
			getAllSkillGroups(true);
		} else {
			getAllSkillGroups(false);
		}

		$scope.$on('$stateChangeStart', function(event, next) {
			if (vm.canSave) {
				event.preventDefault();
				$timeout(function() {
					vm.stateName = next.name;
					vm.closeConfirmation = true;
				}, 1);
			}
		});
	}
})();

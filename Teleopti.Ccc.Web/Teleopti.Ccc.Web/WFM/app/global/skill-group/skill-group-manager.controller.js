//This is the SGM used in PBI #43727 (the new one as of november 2017)
(function() {
	'use strict';

	angular.module('wfm.skillGroup').directive('focusMe', ['$timeout', '$parse', setTheFocus]);

	function setTheFocus($timeout, $parse) {
		return {
			//scope: true,   // optionally create a child scope
			link: function(scope, element, attrs) {
				//console.log(scope, element, attrs);
				var model = $parse(attrs.focusMe);
				scope.$watch(model, function(value) {
					if (value === true) {
						$timeout(function() {
							element[0].focus();
						});
					}
				});
				// to address @blesh's comment, set attribute value to 'false'
				// on blur event:
				/*element.bind('blur', function() {
					scope.$apply(model.assign(scope, false));
				});*/
			}
		};
	}

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
		var isNew = true;
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
		vm.newGroupName = '';
		vm.deleteConfirmation = false;
		vm.modalShown = false;

		//----------- scoped functions ----------------------------------------------------

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

		vm.copyGroupClicked = function(skillGroup, ev) {
			ev.stopPropagation();
			var clone = _.cloneDeep(skillGroup);
			clone.Name = $translate.instant('CopyOf') + ' ' + skillGroup.Name;
			clone.Id = skillGroup.Id + '-' + getRandom();
			vm.skillGroups.push(clone);
			vm.skillGroups = _.sortBy(vm.skillGroups, function(item) {
				return item.Name;
			});
		};

		vm.createSkillGroup = function(ev) {
			vm.newGroup = {
				Name: '',
				Id: getRandom(),
				Skills: []
			};
			isNew = true;
			vm.editGroupNameBox = true;
			ev.stopPropagation();
		};

		vm.deleteSkillGroup = function() {
			SkillGroupSvc.deleteSkillGroup(vm.selectedSkillGroup).then(function() {
				getAllSkillGroups();
				unselectAllSkills();
				vm.selectedSkillGroup = null;
			});
		};

		vm.editNameClicked = function(skillGroup) {
			vm.selectedSkillGroup = skillGroup;
			vm.newGroupName = skillGroup.Name;
			vm.editGroupNameBox = true;
			isNew = false;
			vm.oldName = vm.selectedSkillGroup.Name;
		};

		vm.exitConfigMode = function() {
			$state.go($state.params.returnState, { isNewSkillArea: false });
		};
		
		vm.groupSkillIsSelected = function(skill) {
			var index = vm.selectedGroupSkills.indexOf(skill);
			return index !== -1;
		};

		vm.nameBoxKeyPress = function(ev) {
			if (ev.key === 'Enter') {
				vm.saveNameEdit(ev);
			}
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
			//			vm.canSave = true;
			unselectAllSkills();
		};

		vm.saveAll = function() {
			SkillGroupSvc.modifySkillGroups(vm.skillGroups).then(function(result) {
				getAllSkillGroups();
				vm.canSave = false;
			});
		};

		vm.saveNameEdit = function(ev) {
			if (isNew) {
				if (vm.newGroupName && vm.newGroupName.length > 0) {
					vm.newGroup.Name = vm.newGroupName;
					vm.skills = vm.allSkills.slice();
					vm.canSave = false;
					vm.skillGroups.push(vm.newGroup);
					vm.selectedSkillGroup = vm.newGroup;
					vm.newGroup = null;
				}
			} else {
				vm.selectedSkillGroup.Name = vm.newGroupName;
			}
			setSaveableState();
			ev.stopPropagation();
			vm.editGroupNameBox = false;
			vm.newGroupName = '';
		};

		vm.saveSkillGroup = function(form) {
			if (form.$invalid) {
				return;
			}
			var selectedSkills = $filter('filter')(vm.skills, { isSelected: true });

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

		vm.skillIsSelected = function(skill) {
			var index = vm.selectedSkills.indexOf(skill);
			return index !== -1;
		};

		//----------- Local functions ----------------------------------------------------

		function getAllSkillGroups(select) {
			SkillGroupSvc.getSkillGroups().then(function(result) {
				vm.skillGroups = result.data.SkillAreas;
				originalGroups = _.cloneDeep(vm.skillGroups);
				if (select) {
					vm.selectSkillGroup(
						_.find(vm.skillGroups, function(i) {
							return i.Id === $state.params.selectedGroup.Id;
						})
					);
				}
			});
		}

		function getRandom() {
			return Math.ceil(Math.random() * (1000000 - 1) + 1) + '';
		}

		function hasEmptySkillList() {
			var hasEmpty = false;
			_.each(vm.skillGroups, function(item) {
				if (item.Skills.length === 0) {
					hasEmpty = true;
				}
			});
			return hasEmpty;
		}

		function hasChanges() {
			return !_.isEqual(originalGroups, vm.skillGroups);
		}

		function isNumeric(n) {
			return !isNaN(parseFloat(n)) && isFinite(n);
		}

		function setSaveableState() {
			vm.canSave = hasChanges() && !hasEmptySkillList();
		}

		function notifySkillGroupCreation() {
			NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
		}

		function unselectAllSkills() {
			vm.selectedSkills = [];
			vm.selectedGroupSkills = [];
		}

		//----------- Run at instantiation ----------------------------------------------------

		SkillGroupSvc.getSkills().then(function(result) {
			vm.skills = result.data;
			vm.allSkills = vm.skills.slice();
		});

		if ($state.params.selectedGroup && $state.params.selectedGroup.Id) {
			getAllSkillGroups(true);
		} else {
			getAllSkillGroups(false);
		}
	}
})();

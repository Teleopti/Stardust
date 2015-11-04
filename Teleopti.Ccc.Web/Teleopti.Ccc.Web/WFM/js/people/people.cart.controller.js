'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$scope', '$q', '$translate', '$stateParams', 'uiGridConstants', 'People', 'Toggle', '$mdSidenav', '$mdUtil', '$state', '$interval', '$mdComponentRegistry', PeopleCartController
		]);

	function PeopleCartController($scope, $q, $translate, $stateParams, uiGridConstants, peopleSvc, toggleSvc, $mdSidenav, $mdUtil, $state, $interval, $mdComponentRegistry) {
		var vm = this;
		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.processing = false;
		vm.isConfirmCloseNoticeBar = false;
		vm.selectedDate = new Date();
		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		vm.selectedShiftBag = '';
		vm.dataChanged = false;

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};
		vm.paginationOptions = { pageNumber: 1, totalPages: 0 };

		$scope.$watch(function () {
			if (vm.gridApi)
				return vm.gridApi.pagination.getTotalPages();
			return undefined;
		}, function (newVal) {
			if (newVal)
				vm.paginationOptions.totalPages = newVal;
		});

		vm.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
		vm.format = vm.formats[1];

		vm.datePickerStatus = {
			opened: false
		};
		vm.menuState = 'open';
		vm.toggleMenuState = function () {
			if (vm.menuState === 'closed') {
				vm.menuState = 'open';
				if ($mdSidenav('right-skill').isOpen()) {
					$mdSidenav('right-skill').toggle();
				}
				if ($mdSidenav('right-shiftbag').isOpen()) {
					$mdSidenav('right-shiftbag').toggle();
				}
			} else {
				vm.menuState = 'closed';
			}
		}
		vm.isOpen = function () { return false; };

		$scope.$watch("vm.isOpen()", function (newValue, oldValue) {
			vm.menuState = newValue ? 'closed' : 'open';
		}, true);

		vm.commands = [
			{
				label: "AdjustSkill",
				icon: "mdi-package",
				panelName: 'right-skill',
				action: function () {
					vm.toggleMenuState();
					vm.setCurrentCommand("AdjustSkill")();
				},
				active: function () {
					return vm.isAdjustSkillEnabled;
				},
				columns: [
					{ displayName: 'PersonSkill', field: 'Skills()', headerCellFilter: 'translate', minWidth: 100 },
					{ displayName: 'PersonFinderFieldShiftBag', field: 'ShiftBag()', headerCellFilter: 'translate', minWidth: 100 }
				]
			},
			{
				label: "ChangeShiftBag",
				icon: "mdi-package",
				panelName: 'right-shiftbag',
				action: function () {
					vm.toggleMenuState();
					vm.setCurrentCommand("ChangeShiftBag")();
				},
				active: function () {
					return vm.isAdjustSkillEnabled;
				},
				columns: [
					{ displayName: 'PersonFinderFieldShiftBag', field: 'ShiftBag()', headerCellFilter: 'translate', minWidth: 100 },
					{ displayName: 'PersonSkill', field: 'Skills()', headerCellFilter: 'translate', minWidth: 100 }

				]
			}
		];

		vm.back = function () {
			$state.go('people.start', $stateParams);
		}
		vm.clearCart = function () {
			vm.selectedPeopleIds = [];
			$state.go('people.start', { selectedPeopleIds: [], currentKeyword: $stateParams.currentKeyword, paginationOptions: $stateParams.paginationOptions });
		}
		function buildToggler(navID) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navID).toggle().then(function () { });
			}, 200);
			return debounceFn;
		}

		var basicColumnDefs = [
			{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name', minWidth: 100 },
			{
				displayName: 'LastName',
				field: 'LastName',
				headerCellFilter: 'translate',
				sort: {
					direction: uiGridConstants.ASC,
					priority: 0
				},
				minWidth: 100
			}
		];

		vm.gridOptions = {
			enablePaginationControls: false,
			paginationPageSizes: [20, 60, 100],
			paginationPageSize: 20,
			columnDefs: basicColumnDefs,
			data: 'vm.availablePeople'

		};
		vm.gridOptions.onRegisterApi = function (gridApi) {
			vm.gridApi = gridApi;
			var cellTemplate = '<div>' +
				'<md-tooltip>{{"Remove" | translate}}</md-tooltip>' +
				'<i ng-click="grid.appScope.vm.removePerson(row.entity)" style="position: relative;top: 0.5rem;left: 0.5rem;" class="mdi mdi-account-remove">' +
				'</div>';
			vm.gridApi.core.addRowHeaderColumn({ name: 'rowHeaderCol', displayName: '', width: 30, cellTemplate: cellTemplate });
		}

		vm.removePerson = function (person) {
			var personIndex = vm.selectedPeopleIds.indexOf(person.PersonId);
			if (personIndex > -1) {
				vm.selectedPeopleIds.splice(personIndex, 1);
			}
			for (var i = 0; i < vm.availablePeople.length; i++) {
				if (person.PersonId === vm.availablePeople[i].PersonId) {
					vm.availablePeople.splice(i, 1);
					break;
				}
			}
		}

		vm.updateResult = { Success: false };
		vm.updatePeopleWithSkills = function () {
			vm.processing = true;
			peopleSvc.updatePeopleWithSkills.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), People: vm.availablePeople }).$promise.then(
				function (result) {
					vm.updateResult = result;
					vm.dataChanged = false;
					vm.processing = false;
					vm.isConfirmCloseNoticeBar = true;
					$interval(function () {
						vm.isConfirmCloseNoticeBar = false;
					}, 10000, 1);

				}
			);
		};
		vm.updatePeopleWithShiftBag = function () {
			vm.processing = true;
			peopleSvc.updatePeopleWithShiftBag.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), People: vm.availablePeople }).$promise.then(
				function (result) {
					vm.updateResult = result;
					vm.dataChanged = false;
					vm.processing = false;
					vm.isConfirmCloseNoticeBar = true;
					$interval(function () {
						vm.isConfirmCloseNoticeBar = false;
					}, 10000, 1);

				}
			);
		};

		vm.currentCommand = function () {
			if (vm.commandName != undefined) {
				for (var i = 0; i < vm.commands.length; i++) {
					var cmd = vm.commands[i];
					if (cmd.label.toLowerCase() === vm.commandName.toLowerCase()) {
						return cmd;
					}
				};
			}
			return undefined;
		};

		vm.setCurrentCommand = function (cmdName) {
			var currentCmd = vm.currentCommand();
			if (currentCmd != undefined && currentCmd.panelName != undefined && currentCmd.panelName.length > 0) {
				$mdComponentRegistry.when(currentCmd.panelName).then(function (sideNav) {
					if (sideNav.isOpen()) {
						sideNav.toggle();
					}
				});
			}

			vm.commandName = cmdName;
			vm.updateResult = { Success: false };

			var cmd = vm.currentCommand();
			vm.gridOptions.columnDefs = basicColumnDefs.concat(cmd.columns);
			$mdComponentRegistry.when(cmd.panelName).then(function (sideNav) {
				vm.isOpen = angular.bind(sideNav, sideNav.isOpen);
			});
			return buildToggler(cmd.panelName);
		}

		function initialize() {
			vm.setCurrentCommand($stateParams.commandTag);

			var loadSkillPromise = peopleSvc.loadAllSkills.get().$promise;
			loadSkillPromise.then(function (result) {
				vm.availableSkills = result;
			});
			var fetchPeoplePromise = peopleSvc.fetchPeople.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), PersonIdList: vm.selectedPeopleIds }).$promise;
			fetchPeoplePromise.then(function (result) {
				vm.availablePeople = result;
			});
			var loadShiftBagPromise = peopleSvc.loadAllShiftBags.get().$promise;
			loadShiftBagPromise.then(function (result) {
				vm.availableShiftBags = result;
			});
			var promiseForAdjustSkillToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdjustSkill_34138' }).$promise;
			promiseForAdjustSkillToggle.then(function (result) {
				vm.isAdjustSkillEnabled = result.IsEnabled;
			});

			$q.all([promiseForAdjustSkillToggle, loadSkillPromise, fetchPeoplePromise, loadShiftBagPromise]).then(function () {
				updateSkillStatus();
				updatePersonInfo();

				vm.dataInitialized = true;
				vm.currentCommand().action();
			});
		}

		vm.skillSelectedStatusChanged = function (skill) {
			vm.dataChanged = true;
			angular.forEach(vm.availablePeople, function (person) {
				var skillIndex = person.SkillIdList.indexOf(skill.SkillId);
				if (skill.Selected && skillIndex === -1) {
					person.SkillIdList.push(skill.SkillId);
				}
				if (skillIndex > -1 && !skill.Selected) {
					person.SkillIdList.splice(skillIndex, 1);
				}
			});
		}

		vm.selectedShiftBagChanged = function (selectedShiftBagId) {
			vm.dataChanged = true;
			angular.forEach(vm.availablePeople, function (person) {
				person.ShiftBagId = selectedShiftBagId;
			});
		}

		vm.selectedDateChanged = function () {
			peopleSvc.fetchPeople.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), PersonIdList: vm.selectedPeopleIds }).$promise.then(function (result) {
				vm.availablePeople = result;

				updateSkillStatus();
				updatePersonInfo();
			});
		}

		function updateSkillStatus() {
			angular.forEach(vm.availableSkills, function (skill) {
				var hasCount = 0;
				skill.Status = "none";
				skill.Selected = false;
				angular.forEach(vm.availablePeople, function (person) {
					var skillIndex = person.SkillIdList.indexOf(skill.SkillId);
					if (skillIndex > -1) hasCount++;
				});
				if (hasCount === vm.availablePeople.length) {
					skill.Status = "all";
					skill.Selected = true;
				}
				else if (hasCount < vm.availablePeople.length && hasCount > 0) {
					skill.Status = "partial";
				}
			});
		}
		function updatePersonInfo() {
			angular.forEach(vm.availablePeople, function (person) {
				person.Skills = function () {
					var ownSkills = [];
					angular.forEach(vm.availableSkills, function (skill) {
						var skillIndex = person.SkillIdList.indexOf(skill.SkillId);
						if (skillIndex > -1) {
							ownSkills.push(skill.SkillName);
						}
					});
					return ownSkills.length > 0 ? ownSkills.join(", ") : "";
				}
				person.ShiftBag = function () {
					for (var i = 0; i < vm.availableShiftBags.length; i++) {
						if (vm.availableShiftBags[i].ShiftBagId === person.ShiftBagId) {
							return vm.availableShiftBags[i].ShiftBagName;
						}
					}
					return '';
				}
			});
		}

		initialize();
	};
}());
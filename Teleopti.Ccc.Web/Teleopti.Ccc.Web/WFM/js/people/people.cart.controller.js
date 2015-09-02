'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$q', '$translate', '$stateParams', 'uiGridConstants', 'People', 'Toggle', '$mdSidenav', '$mdUtil', '$state','$interval', PeopleCartController
		]);

	function PeopleCartController($q, $translate, $stateParams, uiGridConstants, peopleSvc, toggleSvc, $mdSidenav, $mdUtil, $state, $interval) {
		var vm = this;
		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.processing = false;
		vm.isConfirmCloseNoticeBar = false;
		vm.selectedDate = new Date();
		vm.toggleCalendar = function ($event) {
			vm.status.opened = !vm.status.opened;
		};
		
		vm.closeCalendar = function ($event) {
			vm.status.opened = false;
		};
		vm.selectedShiftBag ='';

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		vm.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
		vm.format = vm.formats[1];

		vm.status = {
			opened: false
		};

		vm.commands = [
			{
				label: "AdjustSkill",
				icon: "mdi-package",
				action: function () {
					vm.toggleSkillPanel("AdjustSkill")();
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
				action: function () {
					vm.toggleShiftBagPanel("ChangeShiftBag")();
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
			$state.go('people', $stateParams);
		}
		vm.clearCart = function() {
			vm.selectedPeopleIds = [];
			$state.go('people', { selectedPeopleIds: [], currentKeyword: $stateParams.currentKeyword, paginationOptions: $stateParams.paginationOptions });
		}
		function buildToggler(navID) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navID).toggle().then(function () { });
			}, 200);
			return debounceFn;
		}

		vm.toggleSkillPanel = function (commandName) {
			vm.commandName = commandName;
			return buildToggler('right-skill');
		}

		vm.toggleShiftBagPanel = function(commandName) {
			vm.commandName = commandName;
			return buildToggler('right-shiftbag');
		}

		vm.gridOptions = {
			paginationPageSizes: [20, 60, 100],
			paginationPageSize: 20,
			columnDefs: [
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
			],

			data: 'vm.availablePeople'

		};

		var loadSkillPromise = peopleSvc.loadAllSkills.get().$promise;
		loadSkillPromise.then(function (result) {
			vm.availableSkills = result;
		});
		var fetchPeoplePromise = peopleSvc.fetchPeople.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), PersonIdList: vm.selectedPeopleIds }).$promise;
		fetchPeoplePromise.then(function (result) {
			vm.availablePeople = result;
			vm.gridOptions.data = result;
		});
		vm.updateResult = { Success: false };
		vm.updatePeople = function () {
			vm.processing = true;
			peopleSvc.updatePeople.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), People: vm.availablePeople }).$promise.then(
				function (result) {
					vm.updateResult = result;
					vm.processing = false;
					vm.isConfirmCloseNoticeBar = true;
					$interval(function () {
						vm.isConfirmCloseNoticeBar = false;
					}, 3000, 1);
					
				}
			);
		};
		var loadShiftBagPromise = peopleSvc.loadAllShiftBags.get().$promise;
		loadShiftBagPromise.then(function (result) {
			vm.availableShiftBags = result;
		});
		var promiseForAdjustSkillToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdjustSkill_34138' }).$promise;
		promiseForAdjustSkillToggle.then(function (result) {
			vm.isAdjustSkillEnabled = result.IsEnabled;
		});

		vm.currentCommand = function() {
			for (var i = 0; i < vm.commands.length; i++) {
				var cmd = vm.commands[i];
				if (cmd.label.toLowerCase() === vm.commandName.toLowerCase()) {
					return cmd;
				}
			};
		};


		function initialize() {
			vm.commandName = $stateParams.commandTag;

			vm.gridOptions.columnDefs = vm.gridOptions.columnDefs.concat(vm.currentCommand().columns);

			$q.all([promiseForAdjustSkillToggle, loadSkillPromise, fetchPeoplePromise, loadShiftBagPromise]).then(function () {
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
					person.ShiftBag = function() {
						for (var i = 0; i < vm.availableShiftBags.length; i++) {
							if (vm.availableShiftBags[i].ShiftBagId === person.ShiftBagId) {
								return vm.availableShiftBags[i].ShiftBagName;
							}
						}
						return '';
					}
				});

				vm.dataInitialized = true;
				vm.currentCommand().action();
			});
		}

		vm.skillSelectedStatusChanged = function (skill) {
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

		vm.selectedShiftBagChanged = function(selectedShiftBagId) {
			angular.forEach(vm.availablePeople, function (person) {
				person.ShiftBagId = selectedShiftBagId;
			});
		}

		initialize();
	};
}());
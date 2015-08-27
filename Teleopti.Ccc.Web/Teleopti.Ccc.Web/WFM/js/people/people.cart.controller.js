﻿'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$q', '$translate', '$stateParams', 'uiGridConstants', 'People', 'Toggle', '$mdSidenav', '$mdUtil', PeopleCartController
		]);

	function PeopleCartController($q, $translate, $stateParams, uiGridConstants, peopleSvc, toggleSvc, $mdSidenav, $mdUtil) {
		var vm = this;
		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.commandName = $stateParams.commandTag;
		vm.processing = false;

		vm.selectedDate = new Date();
		vm.toggleCalendar = function ($event) {
			vm.status.opened = !vm.status.opened;
		};
		vm.closeCalendar = function ($event) {
			vm.status.opened = false;
		};

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		vm.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
		vm.format = vm.formats[1];

		vm.status = {
			opened: false
		};

		vm.buttons = [
			{
				label: 'AdjustSkill',
				icon: 'mdi-package',
				action: function() {
					vm.toggleSkillPanel();
				},
				active: function() {
					return vm.isAdjustSkillEnabled;
				}
			}
		];

		function buildToggler(navID) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navID)
				  .toggle()
				  .then(function () {
				  });
			}, 200);
			return debounceFn;
		}

		vm.toggleSkillPanel = buildToggler('right');

		vm.close = function () {
			$mdSidenav('right').close()
			  .then(function () {
			  });
		};

		vm.columnMap = [
			{
				tag: "adjustSkill",
				columns: [
					{ displayName: 'Skills', field: 'Skills()', headerCellFilter: 'translate'},
					{ displayName: 'ShiftBag', field: 'ShiftBag', headerCellFilter: 'translate', minWidth: 100 }
				]
			}
		];
		vm.constructColumns = function () {
			angular.forEach(vm.columnMap, function (item) {
				if (item.tag === vm.commandName) {
					vm.gridOptions.columnDefs = vm.gridOptions.columnDefs.concat(item.columns);
				}
			});
		};

		vm.gridOptions = {
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
		loadSkillPromise.then(function(result) {
			vm.availableSkills = result;
		});
		var fetchPeoplePromise = peopleSvc.fetchPeople.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), PersonIdList: vm.selectedPeopleIds }).$promise;
		fetchPeoplePromise.then(function (result) {
			vm.availablePeople = result;
			vm.gridOptions.data = result;
		});
		vm.updateResult = { Success: false};
		vm.updateSkillOnPersons = function () {
			vm.processing = true;
			peopleSvc.updateSkillOnPersons.post({ Date: moment(vm.selectedDate).format('YYYY-MM-DD'), People: vm.availablePeople }).$promise.then(
				function(result) {
					vm.updateResult = result;
					if (vm.updateResult.Success) {
						var personOrPeople = vm.updateResult.SuccessCount > 1 ? 'people are' : 'person is';
						vm.updateResult.SuccessInfo = vm.updateResult.SuccessCount + " " + personOrPeople + " " + "updated successfully!"; //TODO: need localization
					} else {
						vm.updateResult.ErrorMsg = "Process failed! Error: " + vm.updateResult.ErrorMsg;//TODO: need localization
					}
					vm.processing = false;
				});
		};

		var promiseForAdjustSkillToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdjustSkill_34138' }).$promise;
		promiseForAdjustSkillToggle.then(function (result) {
			vm.isAdjustSkillEnabled = result.IsEnabled;
		});

		function initialize() {
			$q.all([promiseForAdjustSkillToggle, loadSkillPromise, fetchPeoplePromise]).then(function () {
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
				});
				vm.dataInitialized = true;
				if (vm.commandName === 'adjustSkill') {
					vm.toggleSkillPanel();
				}
			});

		}

		 vm.skillSelectedStatusChanged = function(skill) {
			angular.forEach(vm.availablePeople, function(person) {
				var skillIndex = person.SkillIdList.indexOf(skill.SkillId);
				if (skill.Selected && skillIndex === -1) {
					person.SkillIdList.push(skill.SkillId);
				}
				if (skillIndex > -1 && !skill.Selected) {
					person.SkillIdList.splice(skillIndex, 1);
				}
			});
		}
		vm.constructColumns();
		initialize();
	};
}());
'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$q', '$translate', '$stateParams', 'uiGridConstants', 'People', 'Toggle', '$mdSidenav', '$mdUtil', PeopleImportController
		]);

	function PeopleImportController($q, $translate, $stateParams, uiGridConstants, peopleSvc, toggleSvc, $mdSidenav, $mdUtil) {
		var vm = this;
		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.commandName = $stateParams.commandTag;

		vm.startDate = new Date();
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
					// vm.gotoSkillPanel(); TODO: toggle skill panel
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
					{ displayName: 'Skills', field: 'Skills', headerCellFilter: 'translate', minWidth: 100 },
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
		vm.date = moment().format('YYYY-MM-DD');
		var fetchPeoplePromise = peopleSvc.fetchPeople.post({ Date: vm.date, PersonIdList: vm.selectedPeopleIds }).$promise;
		fetchPeoplePromise.then(function (result) {
			vm.availablePeople = result;
			vm.gridOptions.data = result;
		});
		
		vm.updateSkillOnPersons = function(peopleList) {
			peopleSvc.updateSkillOnPersons.post(peopleList).$promise.then(function (result) {
				vm.updateResult = result.Success;
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
						if (person.Skills == undefined) {
							person.Skills = "";
						}

						for (var i = 0; i < person.SkillIdList.length; i++) {
							if (person.SkillIdList[i] === skill.SkillId) {
								person.Skills = person.Skills + skill.SkillName + ",";
								hasCount++;
								break;
							}
						}
					});
					if (hasCount === vm.availablePeople.length) {
						skill.Status = "all";
						skill.Selected = true;
					}
					else if (hasCount < vm.availablePeople.length && hasCount > 0) {
						skill.Status = "partial";
					}
				});
				vm.dataInitialized = true;
				if (vm.commandName === 'adjustSkill') {
					vm.toggleSkillPanel();
				}
			});

		}
		vm.constructColumns();
		initialize();
	};
}());
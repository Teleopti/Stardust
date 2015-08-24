'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$q', '$translate', '$stateParams', 'uiGridConstants', 'People', PeopleCartController
		]);

	function PeopleCartController($q, $translate, $stateParams, uiGridConstants, peopleSvc) {
		var vm = this;
		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.commandName = $stateParams.commandTag;
		
		vm.columnMap = [
			{
				tag: "adjustSkill",
				columns: [
					{ displayName: 'Skills', field: 'Skills', headerCellFilter: 'translate', minWidth: 100 },
					{ displayName: 'ShiftBag', field: 'ShiftBag', headerCellFilter: 'translate', minWidth: 100 }
				]
			}
		];
		vm.constructColumns = function() {
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
						priority: 0,
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

		function initialize() {
			$q.all([loadSkillPromise, fetchPeoplePromise]).then(function () {
				//update people skill info
				//construct skill list has status
				//render update grid TODO
				angular.forEach(vm.availableSkills, function (skill) {
					var hasCount = 0;
					skill.Status = "none"; 
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
					}
					else if (hasCount < vm.availablePeople.length && hasCount > 0) {
						skill.Status = "partial";
					}
				});
				vm.dataInitialized = true;
				
			});
			
		}
		vm.constructColumns();
		initialize();
	};
}());
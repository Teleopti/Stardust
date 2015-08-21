'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$q','$translate', '$stateParams', 'uiGridConstants', 'People', PeopleImportController
		]);

	function PeopleImportController($q, $translate, $stateParams, uiGridConstants, peopleSvc) {
		var vm = this;

		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.commandName = $stateParams.commandTag;
		
		vm.columnMap = [{ tag: "adjustSkill", columns: ["skill", "shiftBag"] }];
		vm.constructColumns = function() {
			angular.forEach(vm.columnMap, function (item) {
				if (item.tag === vm.commandName) {
					vm.gridOptions.columnDefs.concat(item.columns);
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
			
		};

		vm.loadSkillPromise = peopleSvc.loadAllSkills.get.$promise;
		vm.loadSkillPromise.then(function(result) {
			vm.availableSkills = result;
		});

		vm.fetchPeoplePromise = peopleSvc.fetchPeople(peopleIdList).post.$promise;
		vm.fetchPeoplePromise.then(function (result) {
			vm.availablePeople = result;
			vm.gridOptions.data = result;
			
		});
		
		vm.updateSkillOnPersons = function(peopleList) {
			peopleSvc.updateSkillOnPersons(peopleList).post.$promise.then(function(result) {
				vm.updateResult = result.Success;
			});
		};

		function initialize() {
			$q.all([vm.loadSkillPromise, vm.fetchPeoplePromise]).then(function () {
				//update people skill info
				//construct skill list has status
				//render update grid TODO
				angular.forEach(vm.availableSkills, function (skill) {
					var hasCount = 0;
					skill.Status = "none"; 
					angular.forEach(vm.availablePeople, function(person) {
						person.Skills = "";

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

		initialize();
	};
}());
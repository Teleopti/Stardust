(function() {
	'use strict';

	angular
		.module('wfm.staffing')
		.directive('staffingImportData', ImportDataDirective)
		.controller('ImportDataController',
			[
				'$timeout',
				'Toggle',
				'$translate',
				'staffingService',
				'UtilService',
				'$window',
				'Toggle',
				ImportDataController
			]);

	function ImportDataDirective() {
		return {
			controller: 'ImportDataController',
			controllerAs: 'vm',
			bindToController: true,
			scope: {},
			templateUrl: 'app/staffing/staffing.importData.template.html'
		};
	}

	function ImportDataController($timeout, toggles, $translate, staffingService, UtilService, $window, toggleService) {
		var vm = this;
		vm.getFileTemplate = getFileTemplate;
		vm.checkValid = checkValid;
		vm.invalidFile = {};
		vm.validFile = {};
		vm.errors = [];
		vm.localSkill = {};
		vm.localSkillName = '';

		vm.selected;
		vm.openImportData;
		vm.selectedSkill;
		vm.selectedSkillArea;
		vm.exportPeriod = {
			startDate: {},
			endDate: {}
		};
		vm.skills = [];
		vm.allSkillAreas = [];

		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedAreaChange = selectedAreaChange;
		vm.clearSelectedSkill = clearSelectedSkill;
		vm.clearSelectedSkillGroup = clearSelectedSkillGroup;
		vm.querySearchSkills = querySearchSkills;
		vm.exportFile = exportFile;
		vm.ErrorMessage = '';
		vm.ClearBpoErrorMessage = '';
		vm.ExportPeriodMessage = 'DefaultHejsan';
		vm.ExportBpoPeriodMessage = 'DefaultHejsan';
		vm.ClearBpoPeriodMessage = $translate.instant('ClearBpoPeriodDefaultMessage - TODO');
		vm.isBpoVisualizeEnabled = isBpoVisualizeEnabled;
		vm.isClearBpoEnabled = isClearBpoEnabled;
		vm.GetRangeMessage = getRangeMessage;
		vm.clearBpoPeriod = clearBpoPeriod;
		vm.setSelectedBpo = setSelectedBpo;
		vm.RangeMessage = '';
		vm.bpoPeriod = {
			startDate: {},
			endDate: {}
		};
		vm.bpoClearPeriod = {
			startDate: {},
			endDate: {}
		};
		vm.AllSkills = $translate.instant('ImportBpoImportInformationAllSkills');
		vm.selectedBpo = null;
		vm.activeBpos = [];
		var skills;
		vm.isSuccessfulRemove = false;
		vm.fileName = '';
		getSkills();
		getSkillAreas();
		getMessage();
		resetExportPeriod();
		setBpoPeriodToDefault();
		
		getActiveBpos();

		//function getGanttData() {
		//	vm.data = {};
		//	var parameterData = null;
		//	if (vm.selectedSkill != null) {
		//		vm.localSkill = vm.selectedSkill;
		//		vm.localSkillName = vm.localSkill.Name;
		//		var skillId = vm.selectedSkill.Id;
		//		parameterData = { SkillId: skillId };
		//		var gantSkillDataQuery = staffingService.getGanttDataForOneSkill.get(parameterData);
		//		gantSkillDataQuery.$promise.then(function (response) {
		//			vm.gantData = response;
		//			vm.data = vm.gantData.GanttDataPerBpoList;
		//		});
		//	}
		//	else if (vm.selectedSkillArea != null) {
		//		vm.localSkill = vm.selectedSkillArea;
		//		vm.localSkillName = vm.localSkill.Name;
		//		var skillGroup = vm.selectedSkillArea.Id;

		//		parameterData = { SkillGroupId: skillGroup };
		//		var gantSkillGroupDataQuery = staffingService.getGanttDataForOneSkillGroup.get(parameterData);
		//		gantSkillGroupDataQuery.$promise.then(function (response) {
		//			vm.gantData = response;
		//			vm.data = vm.gantData.GanttDataPerBpoList;
		//		});
		//	}
		//	else {
		//		vm.localSkill = null;
		//		vm.localSkillName = vm.AllSkills;
		//		var gantDataQuery = staffingService.getGanttData.get();
		//		gantDataQuery.$promise.then(function (response) {
		//			vm.gantData = response;
		//			vm.data = vm.gantData.GanttDataPerBpoList;
		//		});
		//	}
		//}

		function resetFileLists() {
			vm.invalidFile = {};
			vm.validFile = {};
		}

		function getFileTemplate() {
			var templateFile =
				'source,skillcombination,startdatetime,enddatetime,agents\r\n' +
					'Generic,Email,2017-08-01 11:00,2017-08-01 11:15,8.75\r\n' +
					'Generic,Channel Sales|Directsales,2017-08-01 10:00,2017-08-01 10:15,12.5\r\n' +
					'Generic,Channel Sales,2017-08-01 10:00,2017-08-01 10:15,8.75';
			UtilService.saveToFs(templateFile, 'template.csv', 'text/csv');
		}

		function readFile(input) {
			var fileReader = new FileReader();
			fileReader.onload = function(event) {
				var filename = input[0].name;
				var content = event.currentTarget.result;

				var query = staffingService.importbpo.save({ FileContent: content, FileName: filename });
				vm.isSuccessful = false;
				vm.filename = '';
				vm.isFailed = false;
				vm.errors = [];

				query.$promise.then(function(response) {
					if (response.Success) {
						vm.fileName = filename;
						vm.isSuccessful = true;
						getActiveBpos();
						//getGanttData();
					} else {
						vm.isFailed = true;
						vm.errors = response.ErrorInformation;
					}
				});
			};
			fileReader.onerror = function(event) {
				//console.log(event);
			};
			var result = fileReader.readAsText(input[0]);
		}

		function checkValid(file) {
			if (!file[0]) return;
			resetFileLists();
			if (file[0].$error && angular.isDefined(file[0].$error)) {
				return (vm.invalidFile = file[0]);
			} else if (!file[0].$error) {
				readFile(file);
				return (vm.validFile = file[0]);
			}
		}

		function getSkills() {
			if ($window.sessionStorage.staffingSelectedDate)
				vm.selectedDate = new Date($window.sessionStorage.staffingSelectedDate);
			var query = staffingService.getSkills.query();
			query.$promise.then(function(result) {
				if ($window.sessionStorage.staffingSelectedSkill) {
					manageSkillSessionStorage();
				} else if (!$window.sessionStorage.staffingSelectedArea) {
					selectSkill(skills[0]);
				}
				skills = result;
				vm.skills = skills;
			});
		}

		function getSkillAreas() {
			var query = staffingService.getSkillAreas.get();
			query.$promise.then(function(response) {
				vm.HasPermissionToModifySkillArea = response.HasPermissionToModifySkillArea;

				if ($window.sessionStorage.staffingSelectedArea) {
					if (response.SkillAreas.find(checkArea)) {
						vm.selectedSkillArea = response.SkillAreas.find(checkArea);
					} else {
						manageAreaSessionStorage();
					}
				}
				vm.allSkillAreas = response.SkillAreas;
				//getGanttData();
			});
		}

		function checkArea(area) {
			return area.Id === angular.fromJson($window.sessionStorage.staffingSelectedArea).Id;
		}

		function manageSkillSessionStorage() {
			if ($window.sessionStorage.staffingSelectedSkill) {
				vm.selectedSkill = null;
				selectedSkillChange(angular.fromJson($window.sessionStorage.staffingSelectedSkill));
			}
		}

		function manageAreaSessionStorage() {
			if ($window.sessionStorage.staffingSelectedArea) {
				vm.selectedSkillArea = null;
				selectedAreaChange(angular.fromJson($window.sessionStorage.staffingSelectedArea));
			}
		}

		function isBpoVisualizeEnabled() {
			return toggleService.Staffing_BPO_Visualization_74958;
		}

		function isClearBpoEnabled() {
			return toggleService.Staffing_BPO_ClearStaffing_75498;
		}

		function getMessage() {
			var query = staffingService.getExportStaffingPeriodMessage.get();
			query.$promise.then(function(response) {
				vm.ExportPeriodMessage = response.ExportPeriodMessage;
			});

			var queryBpo = staffingService.getExportGapPeriodMessage.get();
			queryBpo.$promise.then(function(response) {
				vm.ExportBpoPeriodMessage = response.ExportPeriodMessage;
			});

			//// TODO: Get BPO Period message
			//var queryClearBpo = staffingService.getclearBpoPeriodMessage.get();
			//queryClearBpo.$promise.then(function(response) {
			//	vm.ClearBpoPeriodMessage = response.ExportPeriodMessage;
			//});
		}

		function getRangeMessage() {
			if (vm.selectedBpo === null) {
				vm.RangeMessage = '';
				return;
			}
			var queryRange = staffingService.getrangemessage.get({ bpoGuid : vm.selectedBpo });
			queryRange.$promise.then(function (response) {
				vm.RangeMessage = response.Message;
			});
		}

		function setSelectedBpo(bpo) {
			vm.selectedBpo = bpo.Id;
		}

		function selectedSkillChange(skill) {
			if (skill == null) return;
			selectSkill(skill);
			//getGanttData();
		}

		function clearSelectedSkill() {
			vm.selectedSkill = null;
			//getGanttData();
		}

		function clearSelectedSkillGroup() {
			vm.selectedSkillArea = null;
			//getGanttData();
		}

		function selectedAreaChange(area) {
			vm.selectedSkillArea = area;
			vm.selectedSkill = null;
			//getGanttData();
		}

		function selectSkill(skill) {
			vm.selectedSkill = skill;
			vm.selectedSkillGroup = null;
		}

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return lowercaseName.indexOf(lowercaseQuery) === 0;
			};
		}

		function querySearchSkills(query) {
			var results = query ? skills.filter(createFilterFor(query)) : skills,
				deferred;
			return results;
		}

		function exportFile() {
			if (vm.exportPeriod.startDate === null || vm.exportPeriod.endDate === null) {
				vm.ErrorMessage = $translate.instant('DiscardSuggestionData');
				return;
			}
			if (vm.selectedSkill === null) {
				vm.ErrorMessage = $translate.instant('BpoExportYouMustSelectASkill');
				return;
			}
			var request = staffingService.postFileExport.get({
				skillId: vm.selectedSkill.Id,
				exportStartDateTime: vm.exportPeriod.startDate,
				exportEndDateTime: vm.exportPeriod.endDate
			});
			request.$promise.then(function(response) {
				vm.ErrorMessage = response.ErrorMessage;
				if (vm.ErrorMessage !== '') return;
				var data = angular.toJson(response.Content);
				UtilService.saveToFs(response.Content, vm.selectedSkill.Name + '.csv', 'text/csv');
			});
		}

		function getActiveBpos() {
			if (vm.isClearBpoEnabled() === false)
				return;
			var query = staffingService.getActiveBpos.query();

			query.$promise.then(function(activeBpos) {
				vm.activeBpos = activeBpos;
			});
		}

		function clearBpoPeriod() {
			vm.ClearBpoErrorMessage = '';
			vm.ClearSuccessMessage = '';
			vm.isSuccessfulRemove = false;
			vm.noSuccessfulRemove = false;
			vm.ClearErrorMessage = '';
			if (vm.bpoPeriod.startDate === null || vm.bpoPeriod.endDate === null) {
				vm.ClearBpoErrorMessage = "You must select good dates";
				return;
			}

			if (vm.selectedBpo === null) {
				vm.ClearBpoErrorMessage = "You must select a Bpo";
				return;
			}

			vm.isProcessing = true;
			var query = staffingService.clearBpoPeriod.get({
				bpoGuid: vm.selectedBpo,
				startDate: vm.bpoClearPeriod.startDate,
				endDate: vm.bpoClearPeriod.endDate
			});
			query.$promise.then(function (clearBpoReturnObject) {
				vm.isProcessing = false;
				if (clearBpoReturnObject.ErrorMessage != null && clearBpoReturnObject.ErrorMessage !== "") {
					vm.ClearErrorMessage = clearBpoReturnObject.ErrorMessage;
					vm.noSuccessfulRemove = true;
				} else {
					vm.selectedBpo = null;
					getActiveBpos();
					vm.isSuccessfulRemove = true;
					vm.ClearSuccessMessage = clearBpoReturnObject.SuccessMessage;
				}
				
				console.log(clearBpoReturnObject);
			});
		}

		function resetExportPeriod() {
			vm.exportPeriod = {
				startDate: moment()
					.utc()
					.toDate(),
				endDate: moment()
					.utc()
					.add(7, 'days')
					.toDate()
			};
		}

		function setBpoPeriodToDefault() {
			vm.bpoPeriod = {
				startDate: moment()
					.utc()
					.toDate(),
				endDate: moment()
					.utc()
					.add(7, 'days')
					.toDate()
			};
			vm.bpoClearPeriod = {
				startDate: moment()
					.utc()
					.add(1, 'days')
					.toDate(),
				endDate: moment()
					.utc()
					.add(57, 'days')
					.toDate()
			};
		}

	}
})();

(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('ImportexportController', ImportexportController);

    ImportexportController.inject = [
        '$state',
        'staffingService',
		'UtilService',
		'$translate'
    ];
	function ImportexportController($state, staffingService, UtilService, $translate) {
        var vm = this;
        vm.selected;
        vm.openImportData;
        vm.selectedSkill;
		vm.exportPeriod  = {
			startDate: {},
			endDate: {}
		};
        vm.selectedSkillChange = selectedSkillChange;
        vm.querySearchSkills = querySearchSkills;
		vm.exportFile = exportFile;
		vm.ErrorMessage = "";
		vm.ExportPeriodMessage = "DefaultHejsan";

        var skills;
		getSkills();
		getMessage();
		resetExportPeriod();
        ////////////////

        function getSkills() {
            var query = staffingService.getSkills.query();
			query.$promise.then(function(response) {
				selectSkill(response[0]);
				skills = response;
			});
		}

		function getMessage() {
			var query = staffingService.getExportPeriodMessage.get();
			query.$promise.then(function (response) {
				vm.ExportPeriodMessage = response.ExportPeriodMessage;
			});
		}

        function selectedSkillChange(skill) {
            if (skill == null) return;
            selectSkill(skill);
        }

        function selectSkill(skill) {
            vm.selectedSkill = skill;
        }
        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(item) {
                var lowercaseName = angular.lowercase(item.Name);
                return (lowercaseName.indexOf(lowercaseQuery) === 0);
            };
        };

        function querySearchSkills(query) {
            var results = query ? skills.filter(createFilterFor(query)) : skills,
                deferred;
            return results;
        };

		function exportFile() {
			if ((vm.exportPeriod.startDate === null) || (vm.exportPeriod.endDate === null)) {
				vm.ErrorMessage = $translate.instant('DiscardSuggestionData');
				//vm.ErrorMessage = "You must set both start date and end date for export period";
				return;
			}
			if (vm.selectedSkill === null)
			{
				vm.ErrorMessage = $translate.instant('BpoExportYouMustSelectASkill');
				return;
			}
			var request = staffingService.postFileExport.get({ skillId: vm.selectedSkill.Id, exportStartDateTime: vm.exportPeriod.startDate, exportEndDateTime: vm.exportPeriod.endDate });
			request.$promise.then(function(response) {
					vm.ErrorMessage = response.ErrorMessage;
					if (vm.ErrorMessage !== "")
						return;
					var data = angular.toJson(response.Content);
					UtilService.saveToFs(response.Content, vm.selectedSkill.Name + ".csv", 'text/csv');
				});
		}

		function resetExportPeriod() {
			vm.exportPeriod = {
				startDate:  moment().utc().toDate(),
				endDate: moment().utc().add(7, 'days').toDate()
			};
		}

    }
})();
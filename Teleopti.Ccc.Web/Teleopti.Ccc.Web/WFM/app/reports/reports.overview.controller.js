(function () {
	'use strict';

	angular.module('wfm.reports').controller('ReportsOverviewController', ReportsCtrl);

	ReportsCtrl.$inject = ['ReportsService', '$translate', 'localeLanguageSortingService'];

	function ReportsCtrl(ReportsSvc, $translate, localeLanguageSortingService) {
		var vm = this;
		vm.reports = [];
		vm.customTypes = [];
		init();

		function init() {
			ReportsSvc.getCategorizedReports().then(function (reports) {
				vm.reports = groupReports(reports);		
				vm.reports = sortReportByType(vm.reports);
			});
		}

		function groupReports(reports) {
			var grouped = [],
				types = {},
				i, j, item;
			for (i = 0, j = reports.length; i < j; i++) {
				item = reports[i];
				if (item.Category === null) {
					item.Category = $translate.instant('Custom');
				}
				if (!(item.Category in types)) {
					types[item.Category] = { Type: item.Category, childNodes: [] };
					grouped.push(types[item.Category]);
				}
				types[item.Category].childNodes.push({ Name: item.Name, IsWeb: item.IsWebReport, Url: item.Url });
			}
			grouped.sort(localeLanguageSortingService.localeSort('+Type'));

			return unshiftCustom(grouped);
		}

		function unshiftCustom(groupedReports) {
			var custom = groupedReports.find(function (r) {
				return r.Type === $translate.instant('Custom');
			})
			if (!custom) {
				return groupedReports;
			}
			var index = groupedReports.indexOf(custom);
			vm.customTypes = groupedReports.slice(index, index + 1);
			vm.customTypes = sortReportByType(vm.customTypes);
			groupedReports.splice(index, 1); 

			return groupedReports;
		}

		function sortReportByType(array) {
			if (array.length == 0)
				return;
			array.sort(localeLanguageSortingService.localeSort('+Type'));
			array.forEach(function(element) {
				element.childNodes.sort(localeLanguageSortingService.localeSort('+Name'));
			});
			return array;
		}
	}
})();

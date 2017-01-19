(function() {
	'use strict';
	angular
		.module('wfm.rta')
		.factory('rtaGridService', rtaGridService);

	rtaGridService.$inject = ['$translate', 'Toggle', 'uiGridConstants', 'rtaLocaleLanguageSortingService'];

	function rtaGridService($translate, toggleService, uiGridConstants, rtaLocaleLanguageSortingService) {

		var service = {
			makeAllGrid: makeAllGrid,
			makeInAlarmGrid: makeInAlarmGrid
		};

		return service;
		/////////////////////////


		function makeAllGrid() {
			return makeGridOptions(false);
		};

		function makeInAlarmGrid() {
			return makeGridOptions(true);
		};

		function makeGridOptions(alarmOnly) {
			var rowTemplate = 'app/rta/agents/rta-agents-rowtemplate.html';
			if (toggleService.RTA_SeeAllOutOfAdherencesToday_39146)
				rowTemplate = 'app/rta/agents/rta-agents-rowtemplate-AllOutOfAdherences_39146.html';
			var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
			var alarmCellTemplate = '<div class="ui-grid-cell-contents"><div class="label rta-label" ng-attr-style="font-size: 14px; color: white; background-color: {{grid.appScope.vm.hexToRgb(row.entity.Color)}}">{{COL_FIELD}}</div></div>';
			var headerCellTemplate = 'app/rta/agents/rta-agents-headercelltemplate.html';
			var columnDefs = [];

			var name = {
				displayName: $translate.instant('Name'),
				field: 'Name',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				sort: alarmOnly ? null : {
					direction: 'asc'
				},
				sortingAlgorithm: rtaLocaleLanguageSortingService.sort
			};
			var siteAndTeam = {
				displayName: $translate.instant('SiteTeam'),
				field: 'SiteAndTeamName',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				sortingAlgorithm: rtaLocaleLanguageSortingService.sort
			};

			var state = {
				displayName: $translate.instant('State'),
				field: 'State',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
			};

			var alarm = {
				displayName: $translate.instant('Alarm'),
				field: 'Alarm',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: alarmCellTemplate,
			};

			var timeOutOfAdherence = {
				displayName: $translate.instant('TimeOOA'),
				field: 'TimeOutOfAdherence',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
			};

			var timeInAlarm = {
				displayName: $translate.instant('TimeInAlarm'),
				field: 'TimeInAlarm',
				headerCellTemplate: headerCellTemplate,
				sort: alarmOnly ?  {
					direction: 'desc'
				}
				 : null,
				cellTemplate: coloredCellTemplate,
			};

			var shift = {
				displayName: $translate.instant('Shift'),
				field: 'Shift',
				enableColumnMenu: false,
				headerCellTemplate: 'app/rta/agents/rta-agents-headershiftcelltemplate.html',
				cellClass: 'shift-class',
				cellTemplate: 'app/rta/agents/rta-agents-shiftcelltemplate.html',
				width: "42%",
				headerCellClass: 'white-cell-header',
				enableHiding: false
			};

			columnDefs.push(name);
			columnDefs.push(siteAndTeam);
			columnDefs.push(shift);
			columnDefs.push(alarm);
			columnDefs.push(timeOutOfAdherence);
			columnDefs.push(timeInAlarm);
			columnDefs.push(state);


			return {
				rowTemplate: rowTemplate,
				columnDefs: columnDefs,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableGridMenu: true,
				enableColumnMenus: true,
				enableColumnResizing: true,
				enableSorting: !alarmOnly
			};
		};

	};
})();

(function() {
	'use strict';
	angular.module('wfm.rta')
		.factory('RtaGridServiceRefact', RtaGridServiceRefact);

	RtaGridServiceRefact.$inject = ['Toggle', 'uiGridConstants', 'RtaLocaleLanguageSortingService'];

	function RtaGridServiceRefact(toggleService, uiGridConstants, RtaLocaleLanguageSortingService) {

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

			var service = {
				rowTemplate: rowTemplate,
				columnDefs: columnDefs,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableGridMenu: true,
				enableColumnMenus: true,
				enableColumnResizing: true,
				enableSorting: !alarmOnly
			};

			return service;
			//////////////////////////////////


			//if (toggleService.RTA_AdherenceDetails_34267)
			//	rowTemplate = 'app/rta/agents/rta-agents-rowtemplate-AdherenceDetails_34267.html';
			var rowTemplate = 'app/rta/refact/agentsrefact/rta-agents-rowtemplate.refact.html';
			if (toggleService.RTA_SeeAllOutOfAdherencesToday_39146)
				rowTemplate = 'app/rta/refact/agentsrefact/rta-agents-rowtemplate-AllOutOfAdherences_39146.refact.html';
			var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
			var alarmCellTemplate = '<div class="ui-grid-cell-contents"><div class="label rta-label" ng-attr-style="font-size: 14px; color: white; background-color: {{grid.appScope.vm.hexToRgb(row.entity.Color)}}">{{COL_FIELD}}</div></div>';
			var headerCellTemplate = 'app/rta/agents/rta-agents-headercelltemplate.html';

			var columnDefs = [];
			var name = {
				displayName: 'Name',
				field: 'Name',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				headerCellFilter: 'translate',
				sort: alarmOnly ? null : {
					direction: 'asc'
				},
				sortingAlgorithm: RtaLocaleLanguageSortingService.sort
			};

			var siteAndTeam = {
				displayName: 'Site/Team',
				field: 'SiteAndTeamName',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				headerCellFilter: 'translate',
				sortingAlgorithm: RtaLocaleLanguageSortingService.sort
			};

			var state = {
				displayName: 'State',
				field: 'State',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				headerCellFilter: 'translate'
			};

			var alarm = {
				displayName: 'Alarm',
				field: 'Alarm',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: alarmCellTemplate,
				headerCellFilter: 'translate'
			};

			var timeOutOfAdherence = {
				displayName: 'Time OOA',
				field: 'TimeOutOfAdherence',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				headerCellFilter: 'translate'
			};

			var timeInAlarm = {
				displayName: 'Time in Alarm',
				field: 'TimeInAlarm',
				headerCellTemplate: headerCellTemplate,
				cellTemplate: coloredCellTemplate,
				headerCellFilter: 'translate'
			};

			var shift = {
				displayName: 'Shift',
				field: 'Shift',
				enableColumnMenu: false,
				headerCellTemplate: 'app/rta/agents/rta-agents-headershiftcelltemplate.html',
				cellClass: 'shift-class',
				cellTemplate: 'app/rta/agents/rta-agents-shiftcelltemplate.html',
				headerCellFilter: 'translate',
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
		};
	};
})();

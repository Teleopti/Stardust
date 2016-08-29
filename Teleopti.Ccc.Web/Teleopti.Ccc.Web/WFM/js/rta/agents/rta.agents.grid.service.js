(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle', 'uiGridConstants', 'RtaLocaleLanguageSortingService',
		function(toggleService, uiGridConstants, RtaLocaleLanguageSortingService) {
			
			this.makeAllGrid = function() {
				return makeGridOptions(false);
			};

			this.makeInAlarmGrid = function() {
				return makeGridOptions(true);
			};

			function makeGridOptions(alarmOnly) {
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				
				var alarmCellTemplate = '<div class="ui-grid-cell-contents"><div class="label rta-label" ng-attr-style="font-size: 14px; color: white; background-color: {{grid.appScope.hexToRgb(row.entity.Color)}}">{{COL_FIELD}}</div></div>';
				var headerCellTemplate = 'js/rta/agents/rta-agents-headercelltemplate.html';
			
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

				var shift = {
					displayName: 'Shift',
					field: 'Shift',
					enableColumnMenu: false,
					headerCellTemplate: 'js/rta/agents/rta-agents-headershiftcelltemplate.html',
					cellClass: 'shift-class',
					cellTemplate: 'js/rta/agents/rta-agents-shiftcelltemplate.html',
					headerCellFilter: 'translate',
					width: "42%",
					headerCellClass: 'white-cell-header',
					enableHiding: false
				};

				var columnDefs = [];

				//if (toggleService.RTA_AdherenceDetails_34267)
				//	rowTemplate = 'js/rta/agents/rta-agents-rowtemplate-AdherenceDetails_34267.html';

				var rowTemplate = 'js/rta/agents/rta-agents-rowtemplate.html';
				columnDefs.push(name);
				columnDefs.push(siteAndTeam);
				columnDefs.push(shift);
				columnDefs.push(alarm);
				columnDefs.push(timeOutOfAdherence);
				columnDefs.push(state);


				return {
					rowTemplate: rowTemplate,
					columnDefs: columnDefs,
					enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
					enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
					enableGridMenu: true,
					enableColumnMenus: true,
					enableColumnResizing: true
				};
			};
		}
	]);
})();

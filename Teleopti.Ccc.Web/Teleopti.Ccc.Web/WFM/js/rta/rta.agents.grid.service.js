(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle',
		function(toggleService) {
			this.makeAllGrid = function() {
				var sort = {
					direction: 'asc'
				};
				var sortingAlgorithm = function(a, b) {
					if ((a === null||a === undefined) && (b === null||b === undefined))
							return 0;
					if (a === null||a === undefined)
							return -1;
					if (b === null||b === undefined)
							return 1;
					if (a > b)
						return 1;
					if (a < b)
							return -1;
					return 0;
				};
				if (toggleService.Wfm_RTA_ProperAlarm_34975)
					return makeGridOptions({
						headerCellTemplate: 'js/rta/rta-agents-headercelltemplate-ProperAlarm_34975.html',
						alarmDurationCellTemplate: '<div ng-if="row.entity.TimeInAlarm" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>',
						timeInAlarmField: 'TimeInAlarm',
						sort: sort,
						sortingAlgorithm: sortingAlgorithm
					});
				else
					return makeGridOptions({
						sort: sort,
						sortingAlgorithm: sortingAlgorithm
					});
			};

			this.makeInAlarmGrid = function() {
				if (toggleService.Wfm_RTA_ProperAlarm_34975)
					return makeGridOptions({
						headerCellTemplate: '<div></div>',
						alarmDurationCellTemplate: '<div ng-if="row.entity.TimeInAlarm" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>',
						timeInAlarmField: 'TimeInAlarm'
					});
				else
					return makeGridOptions();
			};

			function makeGridOptions(args) {
				args = args || {};
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var nextActivityCellTemplate = '<div class="ui-grid-cell-contents"><span class="mdi mdi-arrow-right"></span>{{row.entity.NextActivityStartTime}} {{row.entity.NextActivity}}</div>';
				var alarmCellTemplate = coloredCellTemplate;
				var alarmDurationCellTemplate = args.alarmDurationCellTemplate || coloredWithDurationCellTemplate;

				var headerCellTemplate = args.headerCellTemplate || null;
				var rowTemplate = args.rowTemplate || 'js/rta/rta-agents-rowtemplate.html';
				var timeInAlarmField = args.timeInAlarmField || 'TimeInState';

				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/rta-agents-rowtemplate-AdherenceDetails_34267.html';
				var columnDefs = [{
					displayName: 'Name',
					field: 'Name',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'Team',
					field: 'TeamName',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'State',
					field: 'State',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'Activity',
					field: 'Activity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'NextActivity',
					field: 'NextActivity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: nextActivityCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'Alarm',
					field: 'Alarm',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmCellTemplate,
					headerCellFilter: 'translate'
				}, {
					displayName: 'TimeInAlarm',
					field: timeInAlarmField,
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmDurationCellTemplate,
					headerCellFilter: 'translate'
				}];
				columnDefs[0].sort = args.sort || null;
				columnDefs[6].sortingAlgorithm = args.sortingAlgorithm || null;
				return {
					rowTemplate: rowTemplate,
					columnDefs: columnDefs
				};
			};
		}
	]);
})();

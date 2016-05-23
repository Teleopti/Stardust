(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle', 'uiGridConstants',
		function(toggleService, uiGridConstants) {
			this.makeAllGrid = function() {
				var sort = {
					direction: 'asc'
				};
				var sortingAlgorithm = function(a, b) {
					if ((a === null || a === undefined) && (b === null || b === undefined))
						return 0;
					if (a === null || a === undefined)
						return -1;
					if (b === null || b === undefined)
						return 1;
					if (a > b)
						return 1;
					if (a < b)
						return -1;
					return 0;
				};
				return makeGridOptions({
					headerCellTemplate: 'js/rta/rta-agents-headercelltemplate.html',
					sort: sort,
					sortingAlgorithm: sortingAlgorithm,
					alarmOnly: false
				});
			};

			this.makeInAlarmGrid = function() {
				return makeGridOptions({
					headerCellTemplate: '<div></div>',
					alarmOnly: true
				});
			};

			function makeGridOptions(args) {
				args = args || {};
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var nextActivityCellTemplate = '<div class="ui-grid-cell-contents"><span class="mdi mdi-arrow-right"></span>{{row.entity.NextActivityStartTime}} {{row.entity.NextActivity}}</div>';
				var alarmCellTemplate = coloredCellTemplate;
				var alarmDurationCellTemplate = '<div ng-if="row.entity.TimeInAlarm" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var headerCellTemplate = args.headerCellTemplate || null;
				var rowTemplate = args.rowTemplate || 'js/rta/rta-agents-rowtemplate.html';
				var timeInAlarmField = 'TimeInAlarm';

				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/rta-agents-rowtemplate-AdherenceDetails_34267.html';

				var columnDefs = [];
				columnDefs.push({
					displayName: 'Name',
					field: 'Name',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					sort: args.sort || null
				});
				columnDefs.push({
					displayName: 'Team',
					field: 'TeamName',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				});
				columnDefs.push({
					displayName: 'State',
					field: 'State',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				});
				columnDefs.push({
					displayName: 'Activity',
					field: 'Activity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				});
				columnDefs.push({
					displayName: 'NextActivity',
					field: 'NextActivity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: nextActivityCellTemplate,
					headerCellFilter: 'translate'
				});
				columnDefs.push({
					displayName: 'Alarm',
					field: 'Alarm',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmCellTemplate,
					headerCellFilter: 'translate',
					sortingAlgorithm: args.sortingAlgorithm || null
				});
				columnDefs.push({
					displayName: 'TimeInAlarm',
					field: timeInAlarmField,
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmDurationCellTemplate,
					headerCellFilter: 'translate'
				});

				if (toggleService.RTA_AlarmContext_29357) {
					removeByName(columnDefs, "Activity");
					removeByName(columnDefs, "NextActivity");
					var columns = ["15%", "15%", "10%", "10%", "10%"];
					for (var i = 0; i < columns.length; i++) {
						columnDefs[i].width = columns[i];
						if (args.alarmOnly)
							columnDefs[i].headerCellTemplate = 'js/rta/rta-agents-headercelltemplate-RTA_AlarmContext_29357.html';
					}
					columnDefs.push({
						displayName: 'Shift',
						field: 'RandomShift',
						enableColumnMenu: false,
						headerCellTemplate: 'js/rta/rta-agents-headershiftcelltemplate-RTA_AlarmContext_29357.html',
						cellClass: 'shift-class',
						cellTemplate: 'js/rta/rta-agent-shiftcelltemplate-RTA_AlarmContext_29357.html',
						headerCellFilter: 'translate',
						width: "40%"
					});
				}

				return {
					rowTemplate: rowTemplate,
					columnDefs: columnDefs,
					enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
					enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER
				};
			};

			function removeByName(columnDefs, name) {
				for (var i = columnDefs.length - 1; i >= 0; i--) {
					if (columnDefs[i].displayName === name)
						columnDefs.splice(i, 1);
				}
			}
		}
	]);
})();

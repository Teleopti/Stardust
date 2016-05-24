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
				var name = {
					displayName: 'Name',
					field: 'Name',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					sort: args.sort || null
				};
				var team = {
					displayName: 'Team',
					field: 'TeamName',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				};
				var state = {
					displayName: 'State',
					field: 'State',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				};
				var activity = {
					displayName: 'Activity',
					field: 'Activity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				};
				var nextActivity = {
					displayName: 'NextActivity',
					field: 'NextActivity',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: nextActivityCellTemplate,
					headerCellFilter: 'translate'
				};
				var alarm = {
					displayName: 'Alarm',
					field: 'Alarm',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmCellTemplate,
					headerCellFilter: 'translate',
					sortingAlgorithm: args.sortingAlgorithm || null
				};
				var timeInAlarm = {
					displayName: 'TimeInAlarm',
					field: timeInAlarmField,
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmDurationCellTemplate,
					headerCellFilter: 'translate'
				};


				if (toggleService.RTA_AlarmContext_29357) {
					rowTemplate = 'js/rta/rta-agents-rowtemplate-RTA_AlarmContext_29357.html';
					columnDefs.push(name);
					columnDefs.push(team);
					columnDefs.push({
						displayName: 'Shift',
						field: 'RandomShift',
						enableColumnMenu: false,
						headerCellTemplate: 'js/rta/rta-agents-headershiftcelltemplate-RTA_AlarmContext_29357.html',
						cellClass: 'shift-class',
						cellTemplate: 'js/rta/rta-agent-shiftcelltemplate-RTA_AlarmContext_29357.html',
						headerCellFilter: 'translate',
					});
					columnDefs.push(timeInAlarm);
					alarm.cellTemplate = '<div class="label" style="background-color: #FF0000; font-size: 14px; color: white">OutAdherence</div>';
					columnDefs.push(alarm);
					columnDefs.push(state);
					var columns = ["15%", "15%", "40%", "10%", "10%", "10%"];
					for (var i = 0; i < columns.length; i++) {
						columnDefs[i].width = columns[i];
						if (args.alarmOnly && i !== 2)
							columnDefs[i].headerCellTemplate = 'js/rta/rta-agents-headercelltemplate-RTA_AlarmContext_29357.html';
					}
				} else {
					columnDefs.push(name);
					columnDefs.push(team);
					columnDefs.push(state);
					columnDefs.push(activity);
					columnDefs.push(nextActivity);
					columnDefs.push(alarm);
					columnDefs.push(timeInAlarm);
				}


				return {
					rowTemplate: rowTemplate,
					columnDefs: columnDefs,
					enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
					enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER
				};
			};
		}
	]);
})();

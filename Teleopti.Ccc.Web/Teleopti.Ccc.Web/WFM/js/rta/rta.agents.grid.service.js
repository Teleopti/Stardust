(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle', 'uiGridConstants',
		function(toggleService, uiGridConstants) {
			this.makeAllGrid = function() {
				return makeGridOptions(false);
			};

			this.makeInAlarmGrid = function() {
				return makeGridOptions(true);
			};

			var sortingAlgorithm = function(a, b) {
				if (a == null && b == null)
					return 0;
				if (a == null)
					return -1;
				if (b == null)
					return 1;
				if (a > b)
					return 1;
				if (a < b)
					return -1;
				return 0;
			}

			function makeGridOptions(alarmOnly) {
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var nextActivityCellTemplate = '<div class="ui-grid-cell-contents"><span class="mdi mdi-arrow-right"></span>{{row.entity.NextActivityStartTime}} {{row.entity.NextActivity}}</div>';

				var alarmCellTemplate = coloredCellTemplate;
				if (toggleService.RTA_AlarmContext_29357)
					alarmCellTemplate = '<div class="ui-grid-cell-contents"><div class="label rta-label" ng-attr-style="font-size: 14px; color: white; background-color: {{grid.appScope.hexToRgb(row.entity.Color)}}">{{COL_FIELD}}</div></div>';
				var alarmDurationCellTemplate = '<div ng-if="row.entity.TimeInAlarm" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var headerCellTemplate = 'js/rta/rta-agents-headercelltemplate.html';
		
				var timeInRuleTemplate = "";
				if (toggleService.RTA_TotalOutOfAdherenceTime_38702) {
					timeInRuleTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';
				}

				var rowTemplate = 'js/rta/rta-agents-rowtemplate.html';

				var name = {
					displayName: 'Name',
					field: 'Name',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "15%" : null,
					sort: alarmOnly ? null : {
						direction: 'asc'
					}
				};
				var siteAndTeam = {
					displayName: 'Site/Team',
					field: 'SiteAndTeamName',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "18%" : null,
				};
				/*var site = {
					displayName: 'Site',
					field: 'SiteName',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "9%" : null,
				};*/
				var state = {
					displayName: 'State',
					field: 'State',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "8%" : null,
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
					displayName: 'Next activity',
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
					width: toggleService.RTA_AlarmContext_29357 ? "10%" : null
					//,sortingAlgorithm: alarmOnly ? null : sortingAlgorithm
				};
				var timeInAlarm = {
					displayName: 'Time in alarm',
					field: 'TimeInAlarm',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmDurationCellTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "7%" : null,
				};
				var timeInRule = {
					displayName: 'Time in rule',
					field: 'TimeInRule',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: timeInRuleTemplate,
					headerCellFilter: 'translate',
					width: toggleService.RTA_AlarmContext_29357 ? "7%" : null,
				};
				var timeOutOfAdherence = {
					displayName: 'Time OOA',
					field: 'TimeOutOfAdherence',
					enableColumnMenu: false,
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate',
					width: "7%",
				};

				var shiftcelltemplate =  'js/rta/rta-agent-shiftcelltemplate-RTA_AlarmContext_29357.html';

				if (toggleService.RTA_RecentOutOfAdherences_39145) {
					shiftcelltemplate = 'js/rta/rta-agents-shiftcelltemplate-RTA_RecentOutOfAdherences_39145.html';
				}

				var shift = {
					displayName: 'Shift',
					field: 'Shift',
					enableColumnMenu: false,
					headerCellTemplate: 'js/rta/rta-agents-headershiftcelltemplate-RTA_AlarmContext_29357.html',
					cellClass: 'shift-class',
					cellTemplate: shiftcelltemplate,
					headerCellFilter: 'translate',
					width: "42%",
					headerCellClass: 'white-cell-header',
				};

				var columnDefs = [];

				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/rta-agents-rowtemplate-AdherenceDetails_34267.html';

				if (toggleService.RTA_AlarmContext_29357) {
					rowTemplate = 'js/rta/rta-agents-rowtemplate-RTA_AlarmContext_29357.html';
					columnDefs.push(name);
					columnDefs.push(siteAndTeam);
					//columnDefs.push(site);
					columnDefs.push(shift);
					columnDefs.push(alarm);
					if (toggleService.RTA_RecentOutOfAdherences_39145)
						columnDefs.push(timeOutOfAdherence);
					else if (toggleService.RTA_TotalOutOfAdherenceTime_38702)
						columnDefs.push(timeInRule);
					else
						columnDefs.push(timeInAlarm);
					columnDefs.push(state);
				} else {
					columnDefs.push(name);
					columnDefs.push(siteAndTeam);
					//columnDefs.push(site);
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

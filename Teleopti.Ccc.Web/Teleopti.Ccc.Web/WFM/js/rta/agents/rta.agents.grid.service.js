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

				var headerCellTemplate = 'js/rta/agents/rta-agents-headercelltemplate.html';

				var timeInRuleTemplate = "";
				if (toggleService.RTA_TotalOutOfAdherenceTime_38702) {
					timeInRuleTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';
				}

				var rowTemplate = 'js/rta/agents/rta-agents-rowtemplate.html';

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
					//sort: { direction: 'asc' }
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
				var activity = {
					displayName: 'Activity',
					field: 'Activity',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				};
				var nextActivity = {
					displayName: 'Next activity',
					field: 'NextActivity',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: nextActivityCellTemplate,
					headerCellFilter: 'translate'
				};
				var alarm = {
					displayName: 'Alarm',
					field: 'Alarm',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmCellTemplate,
					headerCellFilter: 'translate'
					//,sortingAlgorithm: alarmOnly ? null : sortingAlgorithm
				};
				var timeInAlarm = {
					displayName: 'Time in alarm',
					field: 'TimeInAlarm',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: alarmDurationCellTemplate,
					headerCellFilter: 'translate'
				};
				var timeInRule = {
					displayName: 'Time in rule',
					field: 'TimeInRule',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: timeInRuleTemplate,
					headerCellFilter: 'translate'
				};
				var timeOutOfAdherence = {
					displayName: 'Time OOA',
					field: 'TimeOutOfAdherence',
					headerCellTemplate: headerCellTemplate,
					cellTemplate: coloredCellTemplate,
					headerCellFilter: 'translate'
				};

				var shiftcelltemplate =  'js/rta/agents/rta-agent-shiftcelltemplate-RTA_AlarmContext_29357.html';

				if (toggleService.RTA_RecentOutOfAdherences_39145) {
					shiftcelltemplate = 'js/rta/agents/rta-agents-shiftcelltemplate-RTA_RecentOutOfAdherences_39145.html';
				}

				var shift = {
					displayName: 'Shift',
					field: 'Shift',
					enableColumnMenu: false,
					headerCellTemplate: 'js/rta/agents/rta-agents-headershiftcelltemplate-RTA_AlarmContext_29357.html',
					cellClass: 'shift-class',
					cellTemplate: shiftcelltemplate,
					headerCellFilter: 'translate',
					width: "42%",
					headerCellClass: 'white-cell-header',
					enableHiding: false
				};

				var columnDefs = [];

				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/agents/rta-agents-rowtemplate-AdherenceDetails_34267.html';

				if (toggleService.RTA_AlarmContext_29357) {
					rowTemplate = 'js/rta/agents/rta-agents-rowtemplate-RTA_AlarmContext_29357.html';
					columnDefs.push(name);
					columnDefs.push(siteAndTeam);
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
					enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
					enableGridMenu: true,
					enableColumnMenus: true,
					enableColumnResizing: true
				};
			};
		}
	]);
})();

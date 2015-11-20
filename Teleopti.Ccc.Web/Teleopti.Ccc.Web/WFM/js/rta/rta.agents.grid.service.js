(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle', '$q',
		function(toggleService, $q) {

			var makeGridOptions = function(args) {
				args = args || {};
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var nextActivityCellTemplate = '<div class="ui-grid-cell-contents"><span class="mdi mdi-arrow-right"></span>{{grid.appScope.format(row.entity.NextActivityStartTime)}} {{row.entity.NextActivity}}</div>';
				var alarmCellTemplate = coloredCellTemplate;
				var alarmDurationCellTemplate = args.alarmDurationCellTemplate || coloredWithDurationCellTemplate;

				var headerCellTemplate = args.headerCellTemplate || null;
				var rowTemplate = args.rowTemplate || 'js/rta/rta-agents-rowtemplate.html';

				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/rta-agents-rowtemplate-AdherenceDetails_34267.html';

				return {
					rowTemplate: rowTemplate,
					columnDefs: [{
						name: 'Name',
						field: 'Name',
						sort: {
							direction: 'asc'
						},
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: coloredCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'Team',
						field: 'TeamName',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: coloredCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'State',
						field: 'State',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: coloredCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'Activity',
						field: 'Activity',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: coloredCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'NextActivity',
						field: 'NextActivity',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: nextActivityCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'Alarm',
						field: 'Alarm',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: alarmCellTemplate,
						headerCellFilter:'translate'
					}, {
						name: 'TimeInAlarm',
						field: 'TimeInState',
						enableColumnMenu: false,
						headerCellTemplate: headerCellTemplate,
						cellTemplate: alarmDurationCellTemplate,
						headerCellFilter:'translate'
					}]
				};
			};

			this.makeAllGrid = function() {
				if (toggleService.Wfm_RTA_ProperAlarm_34975)
					return makeGridOptions({
						headerCellTemplate: 'js/rta/rta-agents-headercelltemplate-ProperAlarm_34975.html',
						alarmDurationCellTemplate: '<div ng-if="row.entity.Alarm===\'Out Adherence\'" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>'
					});
				else
					return makeGridOptions();
			};

			this.makeInAlarmGrid = function() {
				if (toggleService.Wfm_RTA_ProperAlarm_34975)
					return makeGridOptions({
						headerCellTemplate: '<div></div>',
						alarmDurationCellTemplate: '<div ng-if="row.entity.Alarm===\'Out Adherence\'" class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>'
					});
				else
					return makeGridOptions();
			};

		}
	]);
})();


(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaGridService',
		function() {
			this.createAgentsGridOptions = function() {
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';
				var rowTemplate = '<div ng-click="checked=!checked" class="agent-state"><div style="background-color: {{grid.appScope.hexToRgb(row.entity.AlarmColor)}} !important;" ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell agent-row" ng-attr-agentid="{{row.entity.PersonId}}" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell></div></div><div ng-show="checked"><a href="{{grid.appScope.changeScheduleUrl(row.entity.TeamId, row.entity.PersonId)}}" class="change-schedule">Change schedule</a></div>';
				return {
					rowTemplate: rowTemplate,
					columnDefs: [{
						name: 'Name',
						field: 'Name',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate,
					}, {
						name: 'TeamName',
						field: 'TeamName',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate,
					}, {
						name: 'State',
						field: 'State',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate,
					}, {
						name: 'Activity',
						field: 'Activity',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate,
					}, {
						name: 'Next Activity',
						field: 'NextActivity',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate,
					}, {
						name: 'Next Activity Start Time',
						field: 'NextActivityStartTime',
						enableColumnMenu: false,
						cellTemplate: coloredWithTimeCellTemplate
					}, {
						name: 'Alarm',
						field: 'Alarm',
						enableColumnMenu: false,
						cellTemplate: coloredCellTemplate
					}, {
						name: 'Time in Alarm',
						field: 'TimeInState',
						enableColumnMenu: false,
						cellTemplate: coloredWithDurationCellTemplate
					}]
				};
			};
		}
	);
})();

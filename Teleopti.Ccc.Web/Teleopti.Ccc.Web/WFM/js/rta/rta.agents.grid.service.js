(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService',
		function() {
			var selectedPersonId;
			this.selectAgent = function(personId) {
				selectedPersonId = this.isSelected(personId) ? '' : personId;
			};
			this.isSelected = function(personId) {
				return selectedPersonId === personId;
			};
			this.createAgentsGridOptions = function() {
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';
				var rowTemplate =
				'<div ng-click="grid.appScope.selectAgent(row.entity.PersonId)" class="agent-state">'
				+ '<div ng-class="{testclass:grid.appScope.isSelected(row.entity.PersonId)}" style="background-color: {{grid.appScope.hexToRgb(row.entity.AlarmColor)}} !important;"'
				+ 'ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell agent-row"'
				+ 'ng-attr-agentid="{{row.entity.PersonId}}" ng-click="grid.appScope.getAdherenceForAgent(row.entity.PersonId)"'
				+ 'ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell=""></div>'
				+ '</div>'
				+ '<div ng-show="grid.appScope.isSelected(row.entity.PersonId)">'
				+ '<div class="agent-menu historical-adherence" layout="row">'
				+ '<a flex href="{{grid.appScope.changeScheduleUrl(row.entity.TeamId,row.entity.PersonId)}}" class="change-schedule"><span class="mdi mdi-pencil"></span>Change schedule</a>'
				+ '<a flex href="{{grid.appScope.agentDetailsUrl(row.entity.PersonId)}}">Adherence: {{grid.appScope.adherencePercent}}% </a>'
				+ '<div flex style="color:#333333;">Latest updated: {{grid.appScope.timeStamp}}</div> '
				+ '</div>';

				 return{
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

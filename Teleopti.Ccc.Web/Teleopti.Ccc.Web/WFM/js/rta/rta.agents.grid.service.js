(function() {
	'use strict';
	angular.module('wfm.rta').service('RtaGridService', ['Toggle', '$q',
		function(toggleService, $q) {
			this.createAgentsGridOptions = function() {
				var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
				var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
				var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

				var rowTemplate = 'js/rta/rta-agents-rowtemplate.html';
				if (toggleService.RTA_AdherenceDetails_34267)
					rowTemplate = 'js/rta/rta-agents-rowtemplate-AdherenceDetails_34267.html';

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
	]);
})();

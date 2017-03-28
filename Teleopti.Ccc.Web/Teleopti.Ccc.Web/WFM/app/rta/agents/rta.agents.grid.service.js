(function () {
	'use strict';
	angular
		.module('wfm.rta')
		.factory('rtaGridService', rtaGridService);

	rtaGridService.$inject = ['$translate', 'Toggle', 'uiGridConstants', 'localeLanguageSortingService'];

	function rtaGridService($translate, toggleService, uiGridConstants, localeLanguageSortingService) {

		var service = {
			makeAllGrid: makeAllGrid,
			makeInAlarmGrid: makeInAlarmGrid
		};

		return service;
		/////////////////////////


		function makeAllGrid() {
			return makeGridOptions(false);
		};

		function makeInAlarmGrid() {
			return makeGridOptions(true);
		};

		function makeGridOptions(alarmOnly) {
			var rowTemplate = 'app/rta/agents/rta-agents-rowtemplate.html';
			if (toggleService.RTA_SeeAllOutOfAdherencesToday_39146)
				rowTemplate = 'app/rta/agents/rta-agents-rowtemplate-AllOutOfAdherences_39146.html';
			var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
			var alarmCellTemplate = '<div class="ui-grid-cell-contents"><div class="label rta-label" ng-attr-style="font-size: 14px; color: white; background-color: {{grid.appScope.vm.hexToRgb(row.entity.Color)}}">{{COL_FIELD}}</div></div>';

			var shiftHeaderTemplate_htmlTemplatesHaveTimingIssues = '<div class="shift-cell-header rta-default-cursor"><div col-index="renderIndex" title="TOOLTIP">'
				+ '<label ng-repeat="row in grid.appScope.vm.timeline" class="label label-info"'
				+ 'ng-attr-style="position: absolute; left: {{row.Offset}}">{{row.Time}}</label></div></div>';

			var cellHeaderTemplate_htmlTemplatesHaveTimingIssues = '<div role="columnheader" ng-class="{ \'sortable\': sortable }" ui-grid-one-bind-aria-labelledby-grid="col.uid + \'-header-text \' + col.uid + \'-sortdir-text\'"'
				+ 'aria-sort="{{col.sort.direction == asc ? \'ascending\' : ( col.sort.direction == desc ? \'descending\' : (!col.sort.direction ? \'none\' : \'other\'))}}">'
				+ '<div role="button" tabindex="0" class="ui-grid-cell-contents ui-grid-header-cell-primary-focus" col-index="renderIndex" title="TOOLTIP" style="width: 84%">'
				+ '<span class="ui-grid-header-cell-label" ui-grid-one-bind-id-grid="col.uid + \'-header-text\'">{{ col.colDef.displayName }}</span>'
				+ '<span ui-grid-one-bind-id-grid="col.uid + \'-sortdir-text\'" ui-grid-visible="col.sort.direction" aria-label="{{getSortDirectionAriaLabel()}}">'
				+ '<i ng-class="{ \'ui-grid-icon-up-dir\': col.sort.direction == asc, \'ui-grid-icon-down-dir\': col.sort.direction == desc, \'ui-grid-icon-blank\': !col.sort.direction }"'
				+ 'title="{{isSortPriorityVisible() ? i18n.headerCell.priority + \' \' + ( col.sort.priority + 1 )  : null}}" aria-hidden="true"></i>'
				+ '<sub ui-grid-visible="isSortPriorityVisible()" class="ui-grid-sort-priority-number">{{col.sort.priority + 1}}</sub></span></div>'
				+ '<div role="button" tabindex="0" ui-grid-one-bind-id-grid="col.uid + \'-menu-button\'" class="ui-grid-column-menu-button" ng-if="grid.options.enableColumnMenus && !col.isRowHeader  && col.colDef.enableColumnMenu !== false"'
				+ 'ng-click="toggleMenu($event)" ng-class=" {\'ui-grid-column-menu-button-last-col\': isLastCol}" ui-grid-one-bind-aria-label="i18n.headerCell.aria.columnMenuButtonLabel" aria-haspopup="true">'
				+ '<i class="ui-grid-icon-angle-down" aria-hidden="true">&nbsp;</i></div><div ui-grid-filter></div></div></div>';

			var columnDefs = [];

			var name = {
				displayName: $translate.instant('Name'),
				field: 'Name',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellTemplate: coloredCellTemplate,
				sort: alarmOnly ? null : {
					direction: 'asc'
				},
				sortingAlgorithm: localeLanguageSortingService.sort
			};
			var siteAndTeam = {
				displayName: $translate.instant('SiteTeam'),
				field: 'SiteAndTeamName',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellTemplate: coloredCellTemplate,
				sortingAlgorithm: localeLanguageSortingService.sort
			};

			var state = {
				displayName: $translate.instant('State'),
				field: 'State',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellTemplate: coloredCellTemplate,
			};

			var rule = {
				displayName: $translate.instant('Rule'),
				field: 'Rule',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellTemplate: alarmCellTemplate,
			};

			var timeOutOfAdherence = {
				displayName: $translate.instant('TimeOOA'),
				field: 'TimeOutOfAdherence',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellTemplate: coloredCellTemplate,
			};

			var timeInAlarm = {
				displayName: $translate.instant('TimeInAlarm'),
				field: 'TimeInAlarm',
				headerCellTemplate: cellHeaderTemplate_htmlTemplatesHaveTimingIssues,
				sort: alarmOnly ? {
					direction: 'desc'
				}
					: null,
				cellTemplate: coloredCellTemplate,
			};

			var shift = {
				displayName: $translate.instant('Shift'),
				field: 'Shift',
				enableColumnMenu: false,
				headerCellTemplate: shiftHeaderTemplate_htmlTemplatesHaveTimingIssues,
				cellClass: 'shift-class',
				cellTemplate: 'app/rta/agents/rta-agents-shiftcelltemplate.html',
				width: "42%",
				headerCellClass: 'white-cell-header',
				enableHiding: false
			};


			columnDefs.push(name);
			columnDefs.push(siteAndTeam);
			columnDefs.push(shift);
			columnDefs.push(rule);
			columnDefs.push(timeOutOfAdherence);
			columnDefs.push(timeInAlarm);
			columnDefs.push(state);
			return {
				rowTemplate: rowTemplate,
				columnDefs: columnDefs,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableGridMenu: true,
				enableColumnMenus: true,
				enableColumnResizing: true,
				enableSorting: !alarmOnly,
				onRegisterApi: function (gridApi) {
					if (!alarmOnly)
						gridApi.core.on.sortChanged(null, function (grid, sortColumns) {
							var sortColumnsFilteredByPriority = sortColumns.filter(function (col) { return angular.isDefined(col.sort) && angular.isDefined(col.sort.priority); })
							updateSortPriority(sortColumnsFilteredByPriority, false);
							var priority = findMinPriorityForRereprioritize(sortColumnsFilteredByPriority);
							if( priority > 0)
								updateSortPriority(sortColumnsFilteredByPriority, true, priority);					
						});
				}
			};
		};
		
		function updateSortPriority(columns, setIfExceedsMinPriority, minPriority) {
			var priorityArray = columns.map(function (col) { return col.sort.priority; });
			minPriority = minPriority || Math.min.apply(Math, priorityArray);

			columns.forEach(function (col) {
				if(setIfExceedsMinPriority){
					if(col.sort.priority > minPriority)
						col.sort.priority = col.sort.priority - 1;
				}
				else
					col.sort.priority = col.sort.priority - minPriority;
			});
		}

		function findMinPriorityForRereprioritize(columns) {
			if (columns.length < 2)
				return 0;
			var i = 0;
			var priorityArray = columns.map(function (col) { return col.sort.priority; }).sort();
			while (i <= priorityArray.length - 0){
				if (priorityArray[i + 1] - priorityArray[i] > 1)
					return i + 1;
				i++;
			}
			return 0;
		}
	};
})();

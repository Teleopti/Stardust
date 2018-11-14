(function() {
	angular.module('wfm.requests').run(['$templateCache', requestsScheduleTableTemplate]);

	function requestsScheduleTableTemplate($templateCache) {
		'use strict';

		$templateCache.put(
			'requests-schedule-table.html',
			'<table class="table" ng-if="vm.schedules.Schedules.length > 0">\r' +
				'\n' +
				'\t<thead>\r' +
				'\n' +
				'\t\t<tr class="person">\r' +
				'\n' +
				'\t\t\t<th class="person-name person-name-column">\r' +
				'\n' +
				'\t\t\t\t<div class="person-name nowrap">{{\'AgentName\' | translate}}</div>\r' +
				'\n' +
				'\t\t\t</th>\r' +
				'\n' +
				'\t\t\t<th id="time-line-container" class="schedule schedule-column">\r' +
				'\n' +
				'\t\t\t\t<time-line ng-if="vm.schedules.Schedules.length > 0" times="vm.schedules.TimeLine.HourPoints" schedule-count="vm.schedules.Schedules.length"></time-line>\r' +
				'\n' +
				'\t\t\t</th>\r' +
				'\n' +
				'\t\t\t<th class="right-border-empty"></th>\r' +
				'\n' +
				'\t\t</tr>\r' +
				'\n' +
				'\t</thead>\r' +
				'\n' +
				'\t<tbody>\r' +
				'\n' +
				'\t\t<tr ng-repeat="personSchedule in vm.schedules.Schedules" >\r' +
				'\n' +
				'\t\t\t<td class="person-name person-name-column">\r' +
				'\n' +
				'\t\t\t\t<div class="person-name nowrap">{{personSchedule.Name}}</div>\r' +
				'\n' +
				'\t\t\t</td>\r' +
				'\n' +
				'\t\t\t<td class="schedule schedule-column">\r' +
				'\n' +
				'\t\t\t\t<div class="relative time-line-for">\r' +
				'\n' +
				'\t\t\t\t\t<span class="dayoff absolute" ng-repeat="dayOff in personSchedule.DayOffs" ng-if="personSchedule.DayOffs.length > 0"\r' +
				'\n' +
				"\t\t\t\t\t\t  ng-style=\"{left: dayOff.StartPosition + '%', width: dayOff.Length + '%'}\">\r" +
				'\n' +
				"\t\t\t\t\t\t<md-tooltip md-z-index='1200' ng-if=\"dayOff.DayOffName !=''\"> {{dayOff.DayOffName}} </md-tooltip>\r" +
				'\n' +
				'\t\t\t\t\t</span>\r' +
				'\n' +
				'\t\t\t\t\t<div class="shift" ng-repeat="shift in personSchedule.Shifts">\r' +
				'\n' +
				'\t\t\t\t\t\t<div class="layer absolute floatleft selectable projection-layer" projection-name="{{projection.Description}}" ng-repeat="projection in shift.Projections"\r' +
				'\n' +
				"\t\t\t\t\t\t\t ng-style=\"{left: projection.StartPosition + '%', width: projection.Length + '%', backgroundColor: projection.Color}\"\r" +
				'\n' +
				'\t\t\t\t\t\t\t ng-class="{overtimeLight: projection.IsOvertime && projection.UseLighterBorder, overtimeDark: projection.IsOvertime && !projection.UseLighterBorder,\r' +
				'\n' +
				'\t\t\t\t\t\t\t personAbsence:projection.ParentPersonAbsences !== null, noneSelected:!projection.IsOvertime,\r' +
				'\n' +
				'\t\t\t\t\t\t\t hasSelected: !projection.IsOvertime, selected:projection.Selected, lighterBorder: projection.UseLighterBorder}">\r' +
				'\n' +
				"\t\t\t\t\t\t\t<md-tooltip md-z-index='1200'>{{projection.Description}}</md-tooltip>\r" +
				'\n' +
				'\t\t\t\t\t\t</div>\r' +
				'\n' +
				'\t\t\t\t\t</div>\r' +
				'\n' +
				'\t\t\t\t</div>\r' +
				'\n' +
				'\t\t\t</td>\r' +
				'\n' +
				'\t\t</tr>\r' +
				'\n' +
				'\t</tbody>\r' +
				'\n' +
				'</table>\r' +
				'\n'
		);
	}
})();

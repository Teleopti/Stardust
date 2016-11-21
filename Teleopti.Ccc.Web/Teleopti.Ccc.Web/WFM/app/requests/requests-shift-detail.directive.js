'use strict';

(function () {

	var requestsShiftDetailDirective = function (teamScheduleSvc, groupScheduleFactory) {
		return {
			restrict: 'A',
			require: ['^requestsTableContainer'],
			scope: {
				personIds: '=',
				date:'='
			},
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {
			var personIds = scope.personIds;
			var scheduleDate = scope.date;
			var requestsTableContainerCtrl = ctrls[0];
			elem.bind('click', function (oEvent) {
				var topLeft = getShiftDetailTopLeft(oEvent);
				updateShiftStatusForSelectedPerson(topLeft.top, topLeft.left, personIds, moment(scheduleDate), requestsTableContainerCtrl);
				oEvent.stopPropagation();
			});
		}

		function getShiftDetailTopLeft(oEvent) {
			var shiftDetailLeft = oEvent.pageX - 5;
			var shiftDetailTop = oEvent.pageY - 5;
			if ((document.body.clientWidth - oEvent.pageX) < 710) shiftDetailLeft = document.body.clientWidth - 710;
			if ((document.body.clientHeight - oEvent.pageY) < 145) shiftDetailTop = document.body.clientHeight - 145;

			return { top: shiftDetailTop, left: shiftDetailLeft };
		}

		function updateShiftStatusForSelectedPerson(top, left, personIds, scheduleDate, requestsTableContainerCtrl) {
			if (personIds.length === 0) {
				return;
			}
			var currentDate = scheduleDate.format('YYYY-MM-DD');
			teamScheduleSvc.getSchedules(currentDate, personIds)
				.then(function (result) {
					var schedules = groupScheduleFactory.Create(result.Schedules, scheduleDate);
					requestsTableContainerCtrl.showShiftDetail(top, left, schedules);
				});
		}
	};

	angular.module('wfm.requests')
		.directive('requestsShiftDetail',['TeamSchedule', 'GroupScheduleFactory', requestsShiftDetailDirective]);
})();
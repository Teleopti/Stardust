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
			});
		}

		function getShiftDetailTopLeft(oEvent) {
			var normalLeftSidenavWidth = 240 + 10;
			var miniLeftSidenavWidth = 40 + 10;
			var shiftDetailTableWidth = 710;
			var shiftDetailTableHeight = 145;
			var aboveHeaderBarHeight = 160;
			var clientWidth = document.body.clientWidth;
			var clientHeight = document.body.clientHeight;

			var shiftDetailLeft = 0;
			var shiftDetailTop = oEvent.pageY - aboveHeaderBarHeight;

			if (document.querySelectorAll('.sidenav.is-hidden').length === 0) {
				shiftDetailLeft = oEvent.pageX - normalLeftSidenavWidth - shiftDetailTableWidth / 2;

				if ((clientWidth - oEvent.pageX) < (shiftDetailTableWidth / 2))
					shiftDetailLeft = shiftDetailLeft - (shiftDetailTableWidth / 2 - (clientWidth - oEvent.pageX));
			} else {
				shiftDetailLeft = oEvent.pageX - miniLeftSidenavWidth - shiftDetailTableWidth / 2;
				if ((clientWidth - oEvent.pageX) < (shiftDetailTableWidth / 2))
					shiftDetailLeft = shiftDetailLeft - (shiftDetailTableWidth / 2 - (clientWidth - oEvent.pageX));
			}

			if ((clientHeight - oEvent.pageY) < shiftDetailTableHeight)
				shiftDetailTop = shiftDetailTop - (shiftDetailTableHeight / 2 - (clientHeight - oEvent.pageY));

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
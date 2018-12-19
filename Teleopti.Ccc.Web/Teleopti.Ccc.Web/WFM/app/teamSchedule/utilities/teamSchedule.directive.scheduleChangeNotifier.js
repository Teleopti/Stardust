(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('scheduleChangeNotifier', scheduleChangeNotifierDirective);

	scheduleChangeNotifierDirective.$inject = ['signalRSVC', 'ScheduleManagement'];

	function scheduleChangeNotifierDirective(signalRSVC, scheduleMgmtSvc) {
		return {
			restrict: 'E',
			link: function ($scope, $element, $attr) {

				var cb = $scope.$eval($attr.onNotification);

				signalRSVC.subscribeBatchMessage(
					{ DomainType: 'IScheduleChangedInDefaultScenario' },
					scheduleChangedEventHandler,
					300);

				function isMessageNeedToBeHandled() {
					var personIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) { return schedule.PersonId; });

					var scheduleDate = $scope.$eval($attr.scheduleDate);
					var lastCommandTrackId = $scope.$eval($attr.lastCommandTrackId);

					var scheduleDateMoment = moment(scheduleDate);
					var viewRangeStart = scheduleDateMoment.clone().add(-1, 'day').startOf('day');
					var viewRangeEnd = scheduleDateMoment.clone().add(1, 'day').startOf('day');

					return function (message) {
						if (message.TrackId === lastCommandTrackId) {
							return false;
						}

						var isMessageInsidePeopleList = personIds.indexOf(message.DomainReferenceId) > -1;
						var startDate = moment(message.StartDate.substring(1, message.StartDate.length));
						var endDate = moment(message.EndDate.substring(1, message.EndDate.length));
						var isScheduleDateInMessageRange = startDate.isBetween(viewRangeStart, viewRangeEnd, 'day', '[]')
							|| endDate.isBetween(viewRangeStart, viewRangeEnd, 'day', '[]');

						return isMessageInsidePeopleList && isScheduleDateInMessageRange;
					}
				}

				function scheduleChangedEventHandler(messages) {
					if (!scheduleMgmtSvc.groupScheduleVm.Schedules
						|| !scheduleMgmtSvc.groupScheduleVm.Schedules.length)
						return;

					var messagesNeedToBeHandled = messages.filter(isMessageNeedToBeHandled());

					var personIds = messagesNeedToBeHandled
						.map(function (message) {
							return message.DomainReferenceId;
						});

					var uniquePersonIds = [];
					personIds.forEach(function (personId) {
						if (uniquePersonIds.indexOf(personId) === -1) uniquePersonIds.push(personId);
					});

					if (uniquePersonIds.length !== 0) {
						cb(uniquePersonIds, messagesNeedToBeHandled);
					}
				}
			}
		};
	}

})();
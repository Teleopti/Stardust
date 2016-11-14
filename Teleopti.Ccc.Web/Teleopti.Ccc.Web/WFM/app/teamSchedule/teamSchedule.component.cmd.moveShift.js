(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('moveShift',
	{
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		templateUrl: 'app/teamSchedule/html/moveShift.tpl.html',
		controller: ['$scope', '$locale', 'MoveActivityValidator', 'PersonSelection', MoveShiftCtrl]
	});

	function MoveShiftCtrl($scope,$locale, validator, personSelectionSvc) {
		var ctrl = this;
		ctrl.label = 'MoveShift';
		ctrl.checkingCommand = false;
		ctrl.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
		ctrl.meridians = ctrl.showMeridian ? $locale.DATETIME_FORMATS.AMPMS : [];
		ctrl.invalidAgents = [];
		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			ctrl.moveToTime = getDefaultMoveToTime();
			$scope.$watch(function () {
				return getMoveToStartTimeStr();
			}, function(n, o) {
				updateInvalidAgents();
			});
		};

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};

		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};

		ctrl.moveShift = function () {};

		function getDefaultMoveToTime() {
			return moment(ctrl.containerCtrl.getDate() + " 08:00").toDate();
		}

		function getMoveToStartTimeStr() {
			var dateStr = ctrl.containerCtrl.getDate();
			var timeStr = moment(ctrl.moveToTime).format('HH:mm');
			return dateStr + ' ' + timeStr;
		}

		function updateInvalidAgents() {
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			validator.validateMoveToTimeForShift(moment(getMoveToStartTimeStr()), currentTimezone);
			ctrl.invalidAgents = validator.getInvalidPeople();
		}
	}
})();
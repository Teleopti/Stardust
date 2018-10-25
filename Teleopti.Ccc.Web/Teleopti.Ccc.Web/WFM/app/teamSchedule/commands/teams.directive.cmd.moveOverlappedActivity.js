(function () {
	'use strict';

	angular.module('wfm.teamSchedule').component('moveOverlappedActivity', {
		require: {
			CmdContainerCtrl: '^teamscheduleCommandContainer'
		},
		controller: MoveOverlappedActivityCtrl
	});

	MoveOverlappedActivityCtrl.$inject = ['$scope', '$translate', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];

	function MoveOverlappedActivityCtrl($scope, $translate, PersonSelection, ActivityService, notification) {
		this.$onInit = function () {
			var CmdContainerCtrl = this.CmdContainerCtrl;
			var cmdName = 'MoveInvalidOverlappedActivity';
			var date = CmdContainerCtrl.getDate();
			var trackId = CmdContainerCtrl.getTrackId();

			var personInfo = PersonSelection.getCheckedPersonInfoList();
			var personIds = personInfo.map(function (p) {
				return p.PersonId;
			});
			var cmdInput = {
				PersonIds: personIds,
				Date: date,
				TrackedCommandInfo: { TrackId: trackId }
			};

			$scope.$emit('teamSchedule.show.loading');
			ActivityService.moveInvalidOverlappedActivity(cmdInput).then(function (response) {
				$scope.$emit('teamSchedule.hide.loading');
				if (CmdContainerCtrl.getActionCb(cmdName)) {
					CmdContainerCtrl.getActionCb(cmdName)(trackId, personIds);
				}
				notification.reportActionResult({
					'success': $translate.instant('MoveInvalidOverlappedActivitySuccess'),
					'warning': $translate.instant('MoveInvalidOverlappedActivityWarning')
				}, personInfo.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					};
				}), response.data);
			});
		};
	}

})();

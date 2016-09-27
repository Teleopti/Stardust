(function () {
	'use strict';

	angular.module('wfm.teamSchedule').component('moveOverlappedActivity', {
		require: {
			CmdContainerCtrl: '^teamscheduleCommandContainer'
		},
		controller: MoveOverlappedActivityCtrl
	});

	MoveOverlappedActivityCtrl.$inject = ['PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];

	function MoveOverlappedActivityCtrl(PersonSelection, ActivityService, notification) {
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

			ActivityService.moveInvalidOverlappedActivity(cmdInput).then(function (response) {
				if (CmdContainerCtrl.getActionCb(cmdName)) {
					CmdContainerCtrl.getActionCb(cmdName)(trackId, personIds);
				}

				notification.reportActionResult({
					'success': 'MoveInvalidOverlappedActivitySuccess',
					'warning': 'MoveInvalidOverlappedActivityWarning'
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

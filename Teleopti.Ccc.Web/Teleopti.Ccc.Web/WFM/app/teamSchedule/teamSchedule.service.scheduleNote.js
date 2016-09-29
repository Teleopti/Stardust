(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ScheduleNoteManagementService", ScheduleNoteService);

	ScheduleNoteService.$inject = ['$http'];

	function ScheduleNoteService($http) {
		var self = this;
		var noteDict = {};
		var editScheduleNoteUrl = '../api/TeamScheduleCommand/EditScheduleNote';

		self.resetScheduleNotes = resetScheduleNotes;
		self.getInternalNoteForPerson = getInternalNoteForPerson;
		self.setInternalNoteForPerson = setInternalNoteForPerson;

		function resetScheduleNotes(scheduleData, selectedDateMoment) {
			noteDict = {};
			scheduleData.forEach(function (schedule) {
				if (selectedDateMoment.isSame(schedule.Date, 'day')) {
					noteDict[schedule.PersonId] = schedule.InternalNotes;
				}
			});
		}

		function getInternalNoteForPerson(personId) {
			return noteDict[personId];
		}

		function setInternalNoteForPerson(personId, note, date) {
			var inputData = {
				SelectedDate: moment(date).format('YYYY-MM-DD'),
				PersonId: personId,
				InternalNote: note
			}
			$http.post(editScheduleNoteUrl, inputData).then(function (response) {
				if (!response.data || response.data.length === 0) {
					noteDict[personId] = note;
				} else {
					return response.data;
				}
				return null;
			});
			
		}

	}
})();
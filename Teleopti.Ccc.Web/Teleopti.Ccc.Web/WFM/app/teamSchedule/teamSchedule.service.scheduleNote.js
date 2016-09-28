(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ScheduleNoteManagementService", ScheduleNoteService);

	ScheduleNoteService.$inject = [];

	function ScheduleNoteService() {
		var self = this;
		var noteDict = {};

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

		function setInternalNoteForPerson(personId, note) {
			noteDict[personId] = note;
		}

	}
})();
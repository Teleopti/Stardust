(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ScheduleNoteManagementService", ScheduleNoteService);

	ScheduleNoteService.$inject = ['$q','$http'];

	function ScheduleNoteService($q, $http) {
		var self = this;
		var noteDict = {};
		var editScheduleNoteUrl = '../api/TeamScheduleCommand/EditScheduleNote';

		self.resetScheduleNotes = resetScheduleNotes;
		self.getNoteForPerson = getNoteForPerson;
		self.setNoteForPerson = setNoteForPerson;
		self.submitNoteForPerson = submitNoteForPerson;

		function resetScheduleNotes(scheduleData, selectedDateMoment) {
			noteDict = {};
			scheduleData.forEach(function (schedule) {
				if (selectedDateMoment.isSame(schedule.Date, 'day')) {
					noteDict[schedule.PersonId] = {
						internalNotes: schedule.InternalNotes,
						publicNotes: schedule.PublicNotes
					};
				}
			});
		}

		function getNoteForPerson(personId) {
			return noteDict[personId];
		}

		function setNoteForPerson(personId, note) {
			noteDict[personId] = note;
		}

		function submitNoteForPerson(personId, note, date) {
			var deferred = $q.defer();
			var inputData = {
				SelectedDate: moment(date).format('YYYY-MM-DD'),
				PersonId: personId,
				InternalNote: note.internalNotes,
				PublicNote: note.publicNotes
			}
			$http.post(editScheduleNoteUrl, inputData).then(function (response) {
				deferred.resolve(response.data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}

	}
})();
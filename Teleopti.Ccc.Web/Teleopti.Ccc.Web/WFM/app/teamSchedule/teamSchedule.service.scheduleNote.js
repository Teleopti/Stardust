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
		self.setInternalNoteForPerson = setInternalNoteForPerson;
		self.submitInternalNoteForPerson = submitInternalNoteForPerson;

		function resetScheduleNotes(scheduleData, selectedDateMoment) {
			noteDict = {};
			scheduleData.forEach(function (schedule) {
				if (selectedDateMoment.isSame(schedule.Date, 'day')) {
					noteDict[schedule.PersonId] = { internalNotes: schedule.InternalNotes };
				}
			});
		}

		function getNoteForPerson(personId) {
			return noteDict[personId];
		}

		function setInternalNoteForPerson(personId, note) {
			noteDict[personId] = note;
		}

		function submitInternalNoteForPerson(personId, note, date) {
			var deferred = $q.defer();
			var inputData = {
				SelectedDate: moment(date).format('YYYY-MM-DD'),
				PersonId: personId,
				InternalNote: note
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
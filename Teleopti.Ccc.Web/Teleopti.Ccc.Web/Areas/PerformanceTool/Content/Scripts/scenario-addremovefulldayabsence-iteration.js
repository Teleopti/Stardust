
define([
    ], function(
    ) {

        return function(data) {

            var self = this;

            this.AbsenceId = data.AbsenceId;
            this.PersonId = data.PersonId;
            this.Date = data.Date;

            this.Start = function () {
                self.AddCommandCompletedPromise = $.Deferred();
                self.RemoveCommandCompletedPromise = $.Deferred();
                self.RemoveCommandSent = false;
                self.SendAddCommand();
            };
            this.SendAddCommand = function() {
                $.ajax({
                    url: 'Anywhere/PersonScheduleCommand/AddFullDayAbsence',
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json',
                    cache: false,
                    data: JSON.stringify({
                        StartDate: self.Date,
                        EndDate: self.Date,
                        AbsenceId: self.AbsenceId,
                        PersonId: self.PersonId
                    }),
                    error: function() {
                        data.PersonScheduleDayReadModelUpdateFailed();
                        data.PersonScheduleDayReadModelUpdateFailed();
                        self.RemoveCommandCompletedPromise.resolve();
                        self.RemoveCommandSent = true;
                    },
                    complete: function() {
                        self.AddCommandCompletedPromise.resolve();
                    }
                });
            };

            this.SendRemoveCommand = function(personAbsenceId) {
                $.ajax({
                    url: 'Anywhere/PersonScheduleCommand/RemovePersonAbsence',
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json',
                    cache: false,
                    data: JSON.stringify({ PersonAbsenceId: personAbsenceId }),
                    error: function() {
                        data.PersonScheduleDayReadModelUpdateFailed();
                    },
                    complete: function() {
                        self.RemoveCommandCompletedPromise.resolve();
                    }
                });
            };

            this.NotifyPersonAbsenceChanged = function(personAbsenceId) {
                if (!self.RemoveCommandSent) {
                    self.RemoveCommandSent = true;
                    self.SendRemoveCommand(personAbsenceId);
                }
            };

            this.NotifyPersonScheduleDayReadModelChanged = function() {
                data.PersonScheduleDayReadModelUpdated();
            };

            this.AllCommandsCompletedPromise = $.when(this.AddCommandCompletedPromise, this.RemoveCommandCompletedPromise);

        };

    });

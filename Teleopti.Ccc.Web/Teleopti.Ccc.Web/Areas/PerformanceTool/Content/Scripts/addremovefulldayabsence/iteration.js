
define([
], function (
    ) {

        return function(data) {

            var self = this;

            this.AbsenceId = data.AbsenceId;
            this.PersonId = data.PersonId;
            this.Date = data.Date;

            this.Start = function () {
                self.AddCommandCompletedPromise = $.Deferred();
                self.RemoveCommandCompletedPromise = $.Deferred();
                self.AllCommandsCompletedPromise = $.when(self.AddCommandCompletedPromise, self.RemoveCommandCompletedPromise);
                self.RemoveCommandSent = false;
                self.ReadModelUpdatedCount = 0;
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
                        data.ReadModelUpdateFailed();
                        data.ReadModelUpdateFailed();
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
                        data.ReadModelUpdateFailed();
                    },
                    complete: function() {
                        self.RemoveCommandCompletedPromise.resolve();
                    }
                });
            };
            
            var applicableAddNotification = function (notification) {
                if (self.RemoveCommandSent)
                    return false;
                if (self.PersonId != notification.DomainReferenceId)
                    return false;
                return true;
            };

            this.NotifyPersonAbsenceChanged = function (notification) {
                if (applicableAddNotification(notification)) {
                    var personAbsenceId = notification.DomainId;
                    self.RemoveCommandSent = true;
                    self.SendRemoveCommand(personAbsenceId);
                    return true;
                }
                return false;
            };

            this.NotifyReadModelChanged = function (notification) {
                if (self.ReadModelUpdatedCount < 2 && data.IsApplicableNotification(notification)) {
                    self.ReadModelUpdatedCount++;
                    data.ReadModelUpdated();
                    return true;
                }
                return false;
            };
        };

    });

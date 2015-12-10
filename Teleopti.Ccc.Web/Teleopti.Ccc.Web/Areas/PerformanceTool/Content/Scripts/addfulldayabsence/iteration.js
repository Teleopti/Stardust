
define([
], function (
    ) {

        return function(data) {

            var self = this;

            this.AbsenceId = data.AbsenceId;
            this.PersonId = data.PersonId;
            this.Date = data.Date;

            this.Start = function () {
                self.AllCommandsCompletedPromise = $.Deferred();
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
                    },
                    complete: function() {
                    	self.AllCommandsCompletedPromise.resolve();
                    }
                });
            };

            this.NotifyReadModelChanged = function (notification) {
            	if (self.ReadModelUpdatedCount < 1 && data.IsApplicableNotification(notification)) {
                    self.ReadModelUpdatedCount++;
                    data.ReadModelUpdated();
                    return true;
                }
                return false;
            };
        };

    });

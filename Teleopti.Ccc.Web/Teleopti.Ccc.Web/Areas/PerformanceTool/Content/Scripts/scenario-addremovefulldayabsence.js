
define([
        'knockout',
        'progressitem-count',
        'result',
        'messagebroker',
        'helpers'
    ], function(
        ko,
        ProgressItemCountViewModel,
        ResultViewModel,
        messagebroker,
        helpers
    ) {

        return function() {

            var self = this;

            var progressItemPersonScheduleDayReadModel = new ProgressItemCountViewModel("PersonScheduleDayReadModel");

            this.Name = "Add and remove full day absence";
            this.ProgressItems = [
                progressItemPersonScheduleDayReadModel
            ];

            this.LoadDefaultConfiguration = function (callback) {

                var absenceId;
                var personId;
                
                var promise1 = $.ajax({
                    url: 'Configuration/GetAAbsenceId',
                    success: function (data, textStatus, jqXHR) {
                        absenceId = data;
                    }
                });

                var promise2 = $.ajax({
                    url: 'Configuration/GetAPersonId',
                    success: function (data, textStatus, jqXHR) {
                        personId = data;
                    }
                });

                $.when(promise1, promise2).done(function () {
                    var date = moment().format('YYYY-MM-DD');
                    callback({
                        AbsenceId: absenceId,
                        PersonIds: [
                            personId
                        ],
                        DateRange: {
                            From: date,
                            To: date
                        }
                    });
                });
            };

            var iterations = [];
            
            this.IterationsExpected = ko.observable();

            this.ConfigurationChanged = function(configuration) {
                var startDate = moment(configuration.DateRange.From);
                var endDate = moment(configuration.DateRange.To);
                var numberOfDays = endDate.diff(startDate, 'days') +1;
                var personIds = configuration.PersonIds;

                iterations = [];
                
                for (var i = 0; i < personIds.length; i++) {
                    var personId = personIds[i];
                    var date = startDate.clone().subtract('days', 1);
                    
                    for (var j = 0; j < numberOfDays; j++) {
                        date.add('days', 1);

                        iterations.push({
                            absenceId: configuration.AbsenceId,
                            personId: personId,
                            date: date.format()
                        });

                        if (iterations.length > 2000) {
                            self.IterationsExpected(undefined);
                            progressItemPersonScheduleDayReadModel.Target(undefined);
                            throw "Too many combinations";
                        }

                    }
                }
                
                self.IterationsExpected(iterations.length);
                progressItemPersonScheduleDayReadModel.Target(iterations.length * 2);
            };
            
            var startPromise = messagebroker.start();
            
            var scheduleChangeSubscription = null;

            var result;

            var messageReceived = function () {
                if (!result)
                    return;
                
                progressItemPersonScheduleDayReadModel.Increment();
                
                var calculatedInterationsDone = progressItemPersonScheduleDayReadModel.Count() / 2;
                if (calculatedInterationsDone > result.IterationsDone()) {
                    result.IterationsDone(calculatedInterationsDone);
                    if (result.IterationsDone() >= self.IterationsExpected()) {
                        result.RunDone(true);
                        result = null;
                    }
                }
            };

            this.Run = function () {
                
                progressItemPersonScheduleDayReadModel.Reset();
                result = new ResultViewModel();

                for (var i = 0; i < iterations.length; i++) {
                    var iteration = iterations[i];
                    
                    startPromise.done(function() {
                        messagebroker.subscribe({
                            domainReferenceType: 'Person',
                            domainReferenceId: iteration.personId,
                            domainType: 'IPersonScheduleDayReadModel',
                            callback: function(notification) {
                                var momentDate = moment(iteration.date);
                                var startDate = helpers.Date.ToMoment(notification.StartDate);
                                var endDate = helpers.Date.ToMoment(notification.EndDate);
                                if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0) {
                                    messageReceived();
                                }
                            }
                        });
                    });

                    var sendData = JSON.stringify({
                        StartDate: iteration.date,
                        EndDate: iteration.date,
                        AbsenceId: iteration.absenceId,
                        PersonId: iteration.personId
                    });

                    $.ajax({
                        url: 'Anywhere/PersonScheduleCommand/AddFullDayAbsence',
                        type: 'POST',
                        dataType: 'json',
                        contentType: 'application/json',
                        cache: false,
                        data: sendData,
                        success: function (data, textStatus, jqXHR) {
                           
                        }
                    }
                    );
                }

               result.CommandsDone(true);
                
                //setTimeout(function() {
                //    result.CommandsDone(true);
                //}, 1200);

                //var fakeMessage = function() {
                //    messageReceived();
                //    if (!result)
                //        return;
                //    setTimeout(fakeMessage, 1300);
                //};
                //setTimeout(fakeMessage, 1300);

                return result;
            };


        };

    });
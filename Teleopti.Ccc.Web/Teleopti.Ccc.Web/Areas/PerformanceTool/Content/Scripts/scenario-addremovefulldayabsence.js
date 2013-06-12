
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
                            date: date.clone()
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
            var personScheduleDayReadModelSubscription;
            var personAbsenceSubscription;
            var result;

            var personScheduleDayUpdated = function () {
                progressItemPersonScheduleDayReadModel.Success();
                calculateRunDone();
            };

            var personScheduleDayFailed = function () {
                //progressItemPersonScheduleDayReadModel.Failure();
                progressItemPersonScheduleDayReadModel.Success();
                calculateRunDone();
            };

            var calculateRunDone = function () {
                var calculatedInterationsDone = progressItemPersonScheduleDayReadModel.Count() / 2;
                if (calculatedInterationsDone > result.IterationsDone()) {
                    result.IterationsDone(calculatedInterationsDone);
                    if (result.IterationsDone() >= self.IterationsExpected()) {
                        result.RunDone(true);
                        result = null;
                        messagebroker.unsubscribe(personScheduleDayReadModelSubscription);
                        personScheduleDayReadModelSubscription = null;
                        messagebroker.unsubscribe(personAbsenceSubscription);
                        personAbsenceSubscription = null;
                    }
                }
            };
            
            this.Run = function () {
                
                progressItemPersonScheduleDayReadModel.Reset();
                result = new ResultViewModel();

                $.each(iterations, function (i, e) {
                    e.addCommandSentPromise = $.Deferred();
                    e.removeCommandSentPromise = $.Deferred();
                    e.commandsSentPromise = $.when(e.addCommandSentPromise, e.removeCommandSentPromise);
                });

                startPromise.done(function () {
                    
                    personScheduleDayReadModelSubscription = messagebroker.subscribe({
                        domainType: 'IPersonScheduleDayReadModel',
                        callback: function (notification) {
                            
                            console.log(notification);
                            
                            var startDate = helpers.Date.ToMoment(notification.StartDate);
                            var endDate = helpers.Date.ToMoment(notification.EndDate);
                            var matchedIterations = $.grep(iterations, function(item) {
                                return item.date.diff(startDate) == 0 && item.date.diff(endDate) == 0 && notification.DomainReferenceId == item.personId;
                            });

                            if (matchedIterations.length > 0) {
                                personScheduleDayUpdated();
                            }
                        }
                    });
                    
                    personAbsenceSubscription = messagebroker.subscribe({
                        domainType: 'IPersonAbsence',
                        callback: function (notification) {
                            
                            console.log(notification);

                            var startDate = helpers.Date.ToMoment(notification.StartDate);
                            var endDate = helpers.Date.ToMoment(notification.EndDate);
                            var personAbsenceId = notification.DomainId;

                            var matchedIterations = $.grep(iterations, function (item) {
                                return notification.DomainReferenceId == item.personId;
                                //return item.date.diff(startDate) == 0 && item.date.diff(endDate) == 0 && notification.DomainReferenceId == item.personId;
                            });

                            if (matchedIterations.length > 0) {

                                $.ajax({
                                    url: 'Anywhere/PersonScheduleCommand/RemovePersonAbsence',
                                    type: 'POST',
                                    dataType: 'json',
                                    contentType: 'application/json',
                                    cache: false,
                                    data: JSON.stringify(
                                        {
                                            PersonAbsenceId: personAbsenceId
                                        }),
                                    success: function(data, textStatus, jqXHR) {

                                    },
                                    error: function() {
                                        personScheduleDayFailed();
                                    },
                                    complete: function () {
                                        matchedIterations[0].removeCommandSentPromise.resolve();
                                    }
                                });

                            }


                        }
                    });

                    $.each(iterations, function (i, e) {
                        
                        $.ajax({
                            url: 'Anywhere/PersonScheduleCommand/AddFullDayAbsence',
                            type: 'POST',
                            dataType: 'json',
                            contentType: 'application/json',
                            cache: false,
                            data: JSON.stringify({
                                StartDate: e.date,
                                EndDate: e.date,
                                AbsenceId: e.absenceId,
                                PersonId: e.personId
                            }),
                            success: function (data, textStatus, jqXHR) {

                            },
                            error: function () {
                                personScheduleDayFailed();
                                personScheduleDayFailed();
                            },
                            complete: function () {
                                e.addCommandSentPromise.resolve();
                            }
                        });
                    });

                    var commandsSentPromises = $.map(iterations, function (e) {
                        return e.commandsSentPromise;
                    });
                    $.when(commandsSentPromises).then(function () {
                        result.CommandsDone(true);
                    });

                });

                return result;
            };


        };

    });
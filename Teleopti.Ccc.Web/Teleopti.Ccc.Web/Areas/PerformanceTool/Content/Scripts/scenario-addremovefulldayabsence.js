
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

            var personScheduleDayUpdateFailed = function () {
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

            var iterationForNotification = function (notification) {
                
                var startDate = helpers.Date.ToMoment(notification.StartDate);
                var endDate = helpers.Date.ToMoment(notification.EndDate);
                var personId = notification.DomainReferenceId;
                
                if (startDate.diff(endDate, 'days') != 0)
                    return null;
                
                var matchedIterations = $.grep(iterations, function (item) {
                    return item.date.diff(startDate) == 0 &&
                        item.personId == personId;
                });
                
                if (matchedIterations.length == 1)
                    return matchedIterations[0];
                if (matchedIterations.length > 1)
                    throw "What?! Found more than one iteration for this notification! gah!";
                
                return null;
            };

            var sendAddCommand = function(iteration) {
                $.ajax({
                    url: 'Anywhere/PersonScheduleCommand/AddFullDayAbsence',
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json',
                    cache: false,
                    data: JSON.stringify({
                        StartDate: iteration.date,
                        EndDate: iteration.date,
                        AbsenceId: iteration.absenceId,
                        PersonId: iteration.personId
                    }),
                    error: function() {
                        iteration.notifyAddCommandFailure();
                    },
                    complete: function() {
                        iteration.notifyAddCommandComplete();
                    }
                });
            };

            var sendDeleteCommand = function(iteration, personAbsenceId) {
                $.ajax({
                    url: 'Anywhere/PersonScheduleCommand/RemovePersonAbsence',
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json',
                    cache: false,
                    data: JSON.stringify({ PersonAbsenceId: personAbsenceId }),
                    error: function () {
                        iteration.notifyDeleteCommandFailure();
                    },
                    complete: function () {
                        iteration.notifyDeleteCommandComplete();
                    }
                });
            };
            
            this.Run = function () {
                
                progressItemPersonScheduleDayReadModel.Reset();
                result = new ResultViewModel();

                $.each(iterations, function (i, e) {
                    e.addCommandSentPromise = $.Deferred();
                    e.removeCommandSentPromise = $.Deferred();
                    e.commandsSentPromise = $.when(e.addCommandSentPromise, e.removeCommandSentPromise);
                    e.personAbsenceDeleteCommandSent = false;

                    e.notifyPersonScheduleDayReadModelChanged = function() {
                        personScheduleDayUpdated();
                    };

                    e.notifyAddCommandFailure = function() {
                        personScheduleDayUpdateFailed();
                        personScheduleDayUpdateFailed();
                        e.removeCommandSentPromise.resolve();
                        e.personAbsenceDeleteCommandSent = true;
                    };

                    e.notifyAddCommandComplete = function() {
                        e.addCommandSentPromise.resolve();
                    };
                    
                    e.notifyPersonAbsenceChanged = function (personAbsenceId) {
                        if (!e.personAbsenceDeleteCommandSent) {
                            e.personAbsenceDeleteCommandSent = true;
                            sendDeleteCommand(e, personAbsenceId);
                        }
                    };

                    e.notifyDeleteCommandFailure = function() {
                        personScheduleDayUpdateFailed();
                    };

                    e.notifyDeleteCommandComplete = function() {
                        e.removeCommandSentPromise.resolve();
                    };

                });

                startPromise.done(function () {
                    
                    personScheduleDayReadModelSubscription = messagebroker.subscribe({
                        domainType: 'IPersonScheduleDayReadModel',
                        callback: function (notification) {
                            var iteration = iterationForNotification(notification);
                            if (iteration) {
                                iteration.notifyPersonScheduleDayReadModelChanged();
                            }
                        }
                    });
                    
                    personAbsenceSubscription = messagebroker.subscribe({
                        domainType: 'IPersonAbsence',
                        callback: function (notification) {
                            var personAbsenceId = notification.DomainId;
                            var iteration = iterationForNotification(notification);
                            if (iteration)
                                iteration.notifyPersonAbsenceChanged(personAbsenceId);
                        }
                    });

                    $.when(personScheduleDayReadModelSubscription.promise, personAbsenceSubscription.promise)
                        .done(
                            function() {
                                $.each(iterations, function(i, e) {
                                    sendAddCommand(e);
                                });
                            });

                    var commandsSentPromises = $.map(iterations, function (e) {
                        return e.commandsSentPromise;
                    });
                    $.when.apply($, commandsSentPromises).then(function () {
                        result.CommandsDone(true);
                    });

                });

                return result;
            };


        };

    });
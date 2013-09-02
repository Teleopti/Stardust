
define([
        'knockout',
        'progressitem-count',
        'scenario-resourcescalculationaddremovefulldayabsence-iteration',
        'result',
        'messagebroker'
    ], function(
        ko,
        ProgressItemCountViewModel,
        Iteration,
        ResultViewModel,
        messagebroker
    ) {
        

        return function() {

            var self = this;

            var progressItemScheduledResourcesReadModel = new ProgressItemCountViewModel("ScheduledResourcesReadModel");

            this.Name = "Resource calculation - add and remove full day absence";
            
            this.ProgressItems = [
                progressItemScheduledResourcesReadModel
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

            this.ConfigurationChanged = function (configuration) {
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

                        iterations.push(new Iteration({
                            AbsenceId: configuration.AbsenceId,
                            PersonId: personId,
                            Date: date.clone(),
                            ScheduledResourcesReadModelUpdated: function () {
                                progressItemScheduledResourcesReadModel.Success();
                                calculateRunDone();
                            },
                            ScheduledResourcesReadModelUpdateFailed: function () {
                                progressItemScheduledResourcesReadModel.Failure();
                                calculateRunDone();
                            }
                        }));

                        if (iterations.length > 2000) {
                            self.IterationsExpected(undefined);
                            progressItemScheduledResourcesReadModel.Target(undefined);
                            throw "Too many combinations";
                        }

                    }
                }
                
                self.IterationsExpected(iterations.length);
                progressItemScheduledResourcesReadModel.Target(iterations.length * 2);
            };

            var startPromise = messagebroker.start();
            var scheduledResourcesReadModelSubscription;
            var persistableScheduleDataSubscription;
            var result;

            var calculateRunDone = function () {
                var calculatedInterationsDone = progressItemScheduledResourcesReadModel.Count() / 2;
                if (calculatedInterationsDone > result.IterationsDone()) {
                    result.IterationsDone(calculatedInterationsDone);
                    if (result.IterationsDone() >= self.IterationsExpected()) {
                        result.RunDone(true);
                        result = null;
                        messagebroker.unsubscribe(scheduledResourcesReadModelSubscription);
                        scheduledResourcesReadModelSubscription = null;
                    }
                }
            };
            
            this.Run = function () {
                
                progressItemScheduledResourcesReadModel.Reset();
                result = new ResultViewModel();

                startPromise.done(function () {
                    
                    scheduledResourcesReadModelSubscription = messagebroker.subscribe({
                        domainType: 'IScheduledResourcesReadModel',
                        callback: function (notification) {
                            $.each(iterations, function (i, e) {
                                if (e.NotifyScheduledResourcesReadModelChanged(notification))
                                    return false;
                            });
                        }
                    });
                    
                    persistableScheduleDataSubscription = messagebroker.subscribe({
                        domainType: 'IPersistableScheduleData',
                        callback: function (notification) {
                            
                            if (notification.DomainUpdateType == 0) //Only insert
                            {
                                $.each(iterations, function(i, e) {
                                    if (e.NotifyPersonAbsenceChanged(notification)) {
                                        if (e == iterations[iterations.length - 1]) {
                                            messagebroker.unsubscribe(persistableScheduleDataSubscription);
                                            persistableScheduleDataSubscription = null;
                                        }
                                        return false;
                                    }
                                });
                            }
                        }
                    });

                    $.when(
                        scheduledResourcesReadModelSubscription.promise,
                        persistableScheduleDataSubscription.promise
                    ).done(function() {

                        $.each(iterations, function(i, e) {
                            e.Start();
                        });

                        var commandsSentPromises = $.map(iterations, function (e) {
                            return e.AllCommandsCompletedPromise;
                        });
                        $.when.apply($, commandsSentPromises).then(function () {
                            result.CommandsDone(true);
                        });

                    });

                });

                return result;
            };
        };
    });
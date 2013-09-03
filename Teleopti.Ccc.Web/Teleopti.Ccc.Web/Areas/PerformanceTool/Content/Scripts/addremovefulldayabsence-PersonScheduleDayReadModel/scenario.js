
define([
        'knockout',
        'progressitem-count',
        'addremovefulldayabsence-PersonScheduleDayReadModel/iteration',
        'result',
        'messagebroker',
        'scenario-shared/configuration'
    ], function(
        ko,
        ProgressItemCountViewModel,
        Iteration,
        ResultViewModel,
        messagebroker,
        ConfigurationViewModel
    ) {
        

        return function() {

            var self = this;

            var progressItemPersonScheduleDayReadModel = new ProgressItemCountViewModel("PersonScheduleDayReadModel");

            this.Name = "Add and remove full day absence -> PersonScheduleDayReadModel";
            
            this.ProgressItems = [
                progressItemPersonScheduleDayReadModel
            ];

            this.Configuration = new ConfigurationViewModel();

            var iterations = [];
            
            this.IterationsExpected = ko.observable();

            var startPromise = messagebroker.start();
            var personScheduleDayReadModelSubscription;
            var personAbsenceSubscription;
            var result;

            this.ConfigurationChanged = function (configuration) {
                var startDate = moment(configuration.DateRange.From);
                var endDate = moment(configuration.DateRange.To);
                var numberOfDays = endDate.diff(startDate, 'days') + 1;
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
                            PersonScheduleDayReadModelUpdated: function () {
                                progressItemPersonScheduleDayReadModel.Success();
                                calculateRunDone();
                            },
                            PersonScheduleDayReadModelUpdateFailed: function () {
                                progressItemPersonScheduleDayReadModel.Failure();
                                calculateRunDone();
                            }
                        }));

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

            var calculateRunDone = function () {
                var calculatedInterationsDone = progressItemPersonScheduleDayReadModel.Count() / 2;
                if (calculatedInterationsDone > result.IterationsDone()) {
                    result.IterationsDone(calculatedInterationsDone);
                    if (result.IterationsDone() >= self.IterationsExpected()) {
                        result.RunDone(true);
                        result = null;
                        messagebroker.unsubscribe(personScheduleDayReadModelSubscription);
                        personScheduleDayReadModelSubscription = null;
                    }
                }
            };
            
            this.Run = function () {
                
                progressItemPersonScheduleDayReadModel.Reset();
                result = new ResultViewModel();

                startPromise.done(function () {
                    
                    personScheduleDayReadModelSubscription = messagebroker.subscribe({
                        domainType: 'IPersonScheduleDayReadModel',
                        callback: function (notification) {
                            $.each(iterations, function (i, e) {
                                if (e.NotifyPersonScheduleDayReadModelChanged(notification))
                                    return false;
                            });
                        }
                    });
                    
                    personAbsenceSubscription = messagebroker.subscribe({
                        domainType: 'IPersistableScheduleData',
                        callback: function (notification) {
                            
                            if (notification.DomainUpdateType == 0) //Only insert
                            {
                                $.each(iterations, function(i, e) {
                                    if (e.NotifyPersonAbsenceChanged(notification)) {
                                        if (e == iterations[iterations.length - 1]) {
                                            messagebroker.unsubscribe(personAbsenceSubscription);
                                            personAbsenceSubscription = null;
                                        }
                                        return false;
                                    }
                                });
                            }
                        }
                    });

                    $.when(
                        personScheduleDayReadModelSubscription.promise,
                        personAbsenceSubscription.promise
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
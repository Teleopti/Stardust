
define([
        'knockout',
        'progressitem-count',
        'result'
    ], function(
        ko,
        ProgressItemCountViewModel,
        ResultViewModel
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

            this.IterationsExpected = ko.observable();

            this.ConfigurationChanged = function(configuration) {
                
                var numberOfDays = moment(configuration.DateRange.To).diff(moment(configuration.DateRange.From), 'days') + 1;

                var numberOfIterations = configuration.PersonIds.length;
                
                if (numberOfDays > 1) {
                    numberOfIterations = numberOfIterations * numberOfDays;
                }

                self.IterationsExpected(numberOfIterations);
                progressItemPersonScheduleDayReadModel.Target(numberOfIterations * 2);
            };




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
                
                setTimeout(function() {
                    result.CommandsDone(true);
                }, 1200);

                var fakeMessage = function() {
                    messageReceived();
                    if (!result)
                        return;
                    setTimeout(fakeMessage, 1300);
                };
                setTimeout(fakeMessage, 1300);

                return result;
            };


        };

    });
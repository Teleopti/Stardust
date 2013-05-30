
define([
        'knockout',
        'progressitem-count'
    ], function(
        ko,
        ProgressItemCountViewModel
    ) {

        return function() {

            var self = this;

            var progressItemPersonScheduleDayReadModel = new ProgressItemCountViewModel("PersonScheduleDayReadModel");

            this.Name = "Add and remove full day absence";
            this.ProgressItems = [
                progressItemPersonScheduleDayReadModel
            ];

            this.LoadDefaultConfiguration = function() {
                return {
                    "AbsenceId": "E171AA82-DED8-4776-9F10-B694D5E376BD",
                    "PersonIds": [
                        "05C03AD1-9473-4797-BFC7-B86C6ED7323B",
                        "2E76E514-202A-477A-BE40-B99AF3609714"
                    ],
                    "DateRange": {
                        "From": "2013-05-29",
                        "To": "2013-05-31"
                    }
                };
            };

            this.IterationsExpected = ko.observable();
            this.IterationsDone = ko.observable(0);
            this.CommandsDone = ko.observable(false);
            this.RunDone = ko.observable(false);

            this.ConfigurationChanged = function(configuration) {
                
                var numberOfDays = moment(configuration.DateRange.To).diff(moment(configuration.DateRange.From), 'days') + 1;

                var numberOfIterations = configuration.PersonIds.length;
                
                if (numberOfDays > 1) {
                    numberOfIterations = numberOfIterations * numberOfDays;
                }

                self.IterationsExpected(numberOfIterations);
                progressItemPersonScheduleDayReadModel.Target(numberOfIterations * 2);
            };

            var messageReceived = function() {
                progressItemPersonScheduleDayReadModel.Increment();
                
                var calculatedInterationsDone = progressItemPersonScheduleDayReadModel.Count() / 2;
                if (calculatedInterationsDone > self.IterationsDone()) {
                    self.IterationsDone(calculatedInterationsDone);
                    if (self.IterationsDone() >= self.IterationsExpected()) {
                        self.RunDone(true);
                    }
                }
            };
            
            this.Run = function(callbacks) {
                setTimeout(function() {
                    self.CommandsDone(true);
                }, 2500);
                
                setTimeout(messageReceived, 1020);
                setTimeout(messageReceived, 2770);
                setTimeout(messageReceived, 3064);
                setTimeout(messageReceived, 4500);
                setTimeout(messageReceived, 5100);
                setTimeout(messageReceived, 6342);
                setTimeout(messageReceived, 7423);
                setTimeout(messageReceived, 8442);
                setTimeout(messageReceived, 9974);
                setTimeout(messageReceived, 10346);
                setTimeout(messageReceived, 12000);
                setTimeout(messageReceived, 14000);
            };


        };

    });
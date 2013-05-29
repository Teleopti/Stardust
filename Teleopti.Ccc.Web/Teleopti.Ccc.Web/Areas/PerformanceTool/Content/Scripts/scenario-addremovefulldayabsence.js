
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
                        "To": "2013-05-29"
                    }
                };
            };

            this.Count = ko.observable();

            this.ConfigurationChanged = function(configuration) {
                self.Count(2);
                progressItemPersonScheduleDayReadModel.Target(2);
            };

        };

    });
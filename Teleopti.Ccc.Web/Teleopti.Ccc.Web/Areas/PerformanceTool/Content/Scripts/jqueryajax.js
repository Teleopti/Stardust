
define([
        'knockout'
], function(
        ko
    ) {
        return function() {
            var self = this;

            this.Count = ko.observable(0);
            this.Active = ko.observable(false);
            this.Inactive = ko.computed(function() {
                return !self.Active();
            });

            $(document).ajaxStart(function() {
                self.Active(true);
            });
            $(document).ajaxStop(function () {
                self.Active(false);
            });

            $(document).ajaxSend(function () {
                self.Count(self.Count() + 1);
            });
            $(document).ajaxComplete(function () {
                self.Count(self.Count() - 1);
            });
        };
    });

define([
        'knockout'
], function (
    ko
	) {

	return function(text) {
	    var self = this;
	    
	    this.Text = text;
	    
	    this.Target = ko.observable(0);
	    this.Successes = ko.observable(null);
	    this.Failures = ko.observable(null);

	    this.Reset = function() {
	        self.Successes(null);
	        self.Failures(null);
	    };
	    
	    this.Success = function() {
	        self.Successes((self.Successes() | 0) + 1);
	    };
	    
	    this.Failure = function () {
	        self.Failures((self.Failures() | 0) + 1);
	    };

	    this.Count = ko.computed(function() {
	        return (self.Successes() | 0) + (self.Failures() | 0);
	    });
	};

});


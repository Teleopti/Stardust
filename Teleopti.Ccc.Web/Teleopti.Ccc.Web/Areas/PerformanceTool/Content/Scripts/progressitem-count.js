
define([
        'knockout'
], function (
    ko
	) {

	return function(text) {
	    var self = this;
	    
	    this.Text = text;
	    
	    this.Target = ko.observable(0);
	    this.Count = ko.observable(0);

	    this.Reset = function() {
	        self.Count(0);
	    };
	    
	    this.Increment = function() {
	        self.Count(self.Count() + 1);
	    };
	    
	    this.Value = ko.computed(function() {
	        return self.Count() + " / " + self.Target();
	    });

	};

});


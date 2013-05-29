
define([
        'knockout'
], function (
    ko
	) {

	return function(text) {
	    var self = this;
	    
	    this.Text = text;
	    
	    this.Target = ko.observable(0);
	    var count = ko.observable(0);
	    
	    this.Increment = function() {
	        count(count() + 1);
	    };
	    
	    this.Value = ko.computed(function() {
	        return count() + " / " + self.Target();
	    });

	};

});


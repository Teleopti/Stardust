define([
],
    function () {
    	return function () {
    		var that = {};
    		that.fill = function (data) {
    			that.PersonId = data.PersonId;
    			that.Name = data.Name;
    		};

			
    		return that;
    	};
    }
);
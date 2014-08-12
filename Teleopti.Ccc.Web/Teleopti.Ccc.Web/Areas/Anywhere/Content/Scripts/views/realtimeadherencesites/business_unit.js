define([
        'knockout',
        'navigation'
],
    function (
        ko,
        navigation
		) {
    	return function () {

    		var that = {};

    		that.fill = function (data) {
    			that.Id = data.Id;
    			that.Name = data.Name;
    		};

    		return that;
    	};
    }
);
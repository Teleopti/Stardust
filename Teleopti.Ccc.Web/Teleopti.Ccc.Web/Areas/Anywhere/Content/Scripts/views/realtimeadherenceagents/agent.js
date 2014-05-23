define([
],
    function () {
    	return function () {
    		var that = {};
    		that.fill = function (data) {
    			that.PersonId = data.PersonId;
    			that.Name = data.Name;
    			that.SiteId = data.SiteId;
    			that.SiteName = data.SiteName;
    			that.TeamId = data.TeamId;
    			that.TeamName = data.TeamName;
    			that.TimeZoneOffset = data.TimeZoneOffsetMinutes;
		    };

			
    		return that;
    	};
    }
);
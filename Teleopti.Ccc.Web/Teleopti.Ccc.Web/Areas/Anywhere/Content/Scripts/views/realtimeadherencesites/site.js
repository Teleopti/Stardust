﻿define([
        'knockout',
        'navigation'
],
    function (
        ko,
        navigation
		) {
    	return function () {

    		var that = {};
    		that.OutOfAdherence = ko.observable();

    		that.fill = function (data) {
			    that.BusinessUnitId = data.BusinessUnitId;
    			that.Id = data.Id;
    			that.Name = data.Name;
    			that.NumberOfAgents = data.NumberOfAgents;
    		};

    		that.openSite = function () {
			    navigation.GotoRealTimeAdherenceTeams(that.BusinessUnitId, that.Id);
    		};

    		that.hasBeenUpdated = ko.observable(false);

    		return that;
    	};
    }
);
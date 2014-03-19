define([
        'knockout',
        'window'
    ],
    function(
        ko,
        window) {
        return function() {

            var that = {};
            that.OutOfAdherence = ko.observable();
		
            that.fill = function(data) {
                that.Id = data.Id;
                that.Name = data.Name;
                that.NumberOfAgents = data.NumberOfAgents;
            };

            that.openSite = function () {
                window.setLocationHash("realtimeadherenceteams/" + that.Id);
            };
            return that;
        };
    }
);
define([
		'knockout'
],
	function (ko) {
		return function () {

			var that = {};
			that.OutOfAdherence = ko.observable();
		
			that.fill = function (data) {
				console.log(data);
				that.Id = data.Id;
				that.Name = data.Name;
				that.NumberOfAgents = data.NumberOfAgents;
			};

			that.openSite = function() {
				window.location.hash = "#realtimeadherenceteams/" + that.Id;
			};
			return that;
		};
	}
);
define([
		'knockout'
], function (
	ko
	) {

	return function() {

		var self = this;

		self.sendTestMessage = function() {
			alert('sending testmessage');
		};

		self.testMessage = ko.observable('Send message');
	};

});


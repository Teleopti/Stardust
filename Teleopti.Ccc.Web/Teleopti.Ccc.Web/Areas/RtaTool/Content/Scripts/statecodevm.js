define([
	'knockout'
], function (
	ko
) {

	return function (data, send) {

		var self = this;
		self.code = ko.observable(data.code);
		self.loggedon = ko.observable(data.loggedon);
		self.sendState = function () {
			send();
		};

	};
});


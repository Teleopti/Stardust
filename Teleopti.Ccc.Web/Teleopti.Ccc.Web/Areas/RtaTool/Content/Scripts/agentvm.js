define([
	'knockout',
	'moment',
	'rta',
	'statecodevm'
], function (
	ko,
	moment,
	rta,
	statecodevm
) {

	return function (data) {
		var self = this;

		self.name = ko.observable(data.name);
		self.usercode = ko.observable(data.usercode);

		self.statecodes = ko.computed(function() {
			var codes = [];
			ko.utils.arrayForEach(rta.StateCodes(), function(c) {
				c.authenticationKey = data.authenticationKey;
				c.error = data.error;
				c.usercode = data.usercode;
				codes.push(new statecodevm(c));
			});
			return codes;
		});

	};
});


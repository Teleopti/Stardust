
define([
], function (
	) {

	return function (data) {

		var self = this;

		this.Number = data.Number;

		this.Start = function () {

			// do your async server calls here
			// now we fake it

			self.AllCommandsCompletedPromise = $.Deferred();

			setTimeout(function () {
				self.AllCommandsCompletedPromise.resolve();
			}, 50 * data.Number);

			setTimeout(function () {
				data.Success();
			}, 100 * data.Number);
		};

	};

});

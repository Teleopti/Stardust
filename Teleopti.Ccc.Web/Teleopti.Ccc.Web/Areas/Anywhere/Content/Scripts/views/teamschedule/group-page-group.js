define([
		'knockout'
	], function (
		ko
	) {

		return function (data) {

			this.Name = data.Name;
			this.Id = data.Id;
		};
	});

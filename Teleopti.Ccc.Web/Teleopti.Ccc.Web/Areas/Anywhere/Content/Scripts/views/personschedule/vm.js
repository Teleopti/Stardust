define([
		'knockout',
		'helpers'
	], function (
		ko,
		helpers
		) {

		return function () {

			var self = this;

			//this.TimeLine = data.timeLine;

			self.Id = ko.observable("");
			self.Date = ko.observable();
			self.Name = ko.observable("");
			self.Site = ko.observable("");
			self.Team = ko.observable("");

			this.SetData = function (data) {
				self.Id(data.Id);
				self.Name(data.Name);
				self.Site(data.Site);
				self.Team(data.Team);
			};

		};
	});

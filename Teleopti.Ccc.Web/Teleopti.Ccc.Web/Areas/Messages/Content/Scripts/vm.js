define([
		'knockout',
		'resources',
		'ajax'
], function (
	ko,
	resources,
	ajax
	) {

	return function () {
		var self = this;

		self.MyTimeVisible = ko.observable();
		self.AnywhereVisible = ko.observable();
		self.UserName = ko.observable();

		self.ErrorMessage = ko.observable("");

		self.Receivers = ko.observableArray();
		self.Subject = ko.observable("");
		self.Message = ko.observable("");
		self.Resources = resources;

		self.DisplaySendMessage = ko.observable(true);

		self.IsMessageEmpty = ko.computed(function() {
			return self.Subject().length == 0 && self.Message().length == 0;
		});

		self.StatusText = ko.observable();
		self.StatusClass = ko.observable();

		self.SendMessage = function () {
			var ids = ko.utils.arrayMap(self.Receivers(), function (item) {
				return item.Id;
			});

			ajax.ajax({
				url: 'Messages/Application/SendMessage',
				contentType: "application/json; charset=utf-8",
				dataType: "json",
				type: 'POST',
				data: JSON.stringify({ 'receivers': ids, 'subject': self.Subject(), 'body': self.Message() }),
				success: function (data) {
					self.StatusText(self.Resources.Sent);
					self.StatusClass("label-success");
				},
				error: function (data) {
					self.ErrorMessage(data.statusText);
				}
			});

			self.StatusText(self.Resources.Pending);
			self.StatusClass("label-warning");
			self.DisplaySendMessage(false);
		};

		self.CloseSendMessage = function() {
			window.close();
		};
	};
});


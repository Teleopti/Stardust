
Teleopti.SSO.Authentication.JQueryAjaxViewModel = function () {
	var self = this;

	this.Count = ko.observable(0);
	this.Active = ko.observable(false);
	this.Inactive = ko.computed(function () {
		return !self.Active();
	});

    var doc = $(document);
	doc.ajaxStart(function () {
		self.Active(true);
	});

	doc.ajaxStop(function () {
		self.Active(false);
	});

	doc.ajaxSend(function () {
		self.Count(self.Count() + 1);
	});
	
	doc.ajaxComplete(function () {
		self.Count(self.Count() - 1);
	});
};
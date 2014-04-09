
Teleopti.SSO.Authentication.JQueryAjaxViewModel = function () {
	var self = this;

	this.Count = ko.observable(0);
	this.Active = ko.observable(false);
	this.Inactive = ko.computed(function () {
		return !self.Active();
	});

	$('body').ajaxStart(function () {
		self.Active(true);
	});
	$('body').ajaxStop(function () {
		self.Active(false);
	});

	$('body').ajaxSend(function () {
		self.Count(self.Count() + 1);
	});
	$('body').ajaxComplete(function () {
		self.Count(self.Count() - 1);
	});


};
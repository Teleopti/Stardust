/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.StudentAvailability.DayViewModel = function (ajax) {
	var self = this;

	this.Date = "";

	this.IsLoading = ko.observable(false);
	this.AjaxError = ko.observable('');
	this.AvailableTimeSpan = ko.observable('');


	var ajaxForDate = function (options) {

		var type = options.type || 'GET',
		    data = options.data || {},
		    statusCode400 = options.statusCode400,
		    statusCode404 = options.statusCode404,
		    url = options.url || "StudentAvailability/StudentAvailability",
		    success = options.success || function () {
		    },
		    complete = options.complete || null;

		return ajax.Ajax({
			url: url,
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			type: type,
			beforeSend: function (jqXHR) {
				self.AjaxError('');
				self.IsLoading(true);
			},
			complete: function (jqXHR, textStatus) {
				self.IsLoading(false);
				if (complete)
					complete(jqXHR, textStatus);
			},
			success: success,
			data: data,
			statusCode404: statusCode404,
			statusCode400: statusCode400,
			error: function (jqXHR, textStatus, errorThrown) {
				var error = {
					ShortMessage: "Error!"
				};
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {
				}
				self.AjaxError(error.ShortMessage);
			}
		});
	};

	this.SetStudentAvailability = function (value, editFormViewModel) {
		var deferred = $.Deferred();
		ajaxForDate({
			type: 'POST',
			data: JSON.stringify(value),
			date: self.Date,
			success: function(data) {
				if (typeof data.Errors != 'undefined') {
					editFormViewModel.ValidationError(data.Errors.join('</br>') || '');
					return;
				}
				self.ReadStudentAvailability(data);
			},
			complete: function() {
				deferred.resolve();
				//				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.DeleteStudentAvailability = function () {
		var deferred = $.Deferred();
		ajaxForDate({
			type: 'DELETE',
			data: JSON.stringify({ Date: self.Date }),
			date: self.Date,
			statusCode404: function () { },
			success: this.ReadStudentAvailability,
			complete: function () {
				deferred.resolve();
				//				self.LoadFeedback();
			}
		});
		return deferred.promise();
	};

	this.ReadElement = function (element) {
		var item = $(element);
		self.Date = item.attr('data-mytime-date');
		//		self.EditableIsInOpenPeriod(item.attr('data-mytime-editable') == "True");
	};

	this.ReadStudentAvailability = function (data) {
		if (!data) return;

		self.AvailableTimeSpan(data.AvailableTimeSpan);
	};
};


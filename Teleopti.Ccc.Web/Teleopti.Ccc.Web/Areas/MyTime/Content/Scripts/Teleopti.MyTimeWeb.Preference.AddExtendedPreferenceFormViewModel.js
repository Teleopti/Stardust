/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function (ajax) {
	var self = this;

	this.AvailableTemplates = ko.observableArray();
	this.SelectedTemplate = ko.observable();
	this.IsTemplateDetailsVisible = ko.observable(true);

	this.PreferenceId = ko.observable();
	this.EarliestStartTimeInternal = ko.observable();
	this.LatestStartTimeInternal = ko.observable();
	this.EarliestEndTimeInternal = ko.observable();
	this.EarliestEndTimeNextDay = ko.observable();
	this.LatestEndTimeInternal = ko.observable();
	this.LatestEndTimeNextDay = ko.observable();
	this.MinimumWorkTime = ko.observable();
	this.MaximumWorkTime = ko.observable();
	this.ActivityEarliestStartTime = ko.observable();
	this.ActivityLatestStartTime = ko.observable();
	this.ActivityEarliestEndTime = ko.observable();
	this.ActivityLatestEndTime = ko.observable();
	this.ActivityPreferenceId = ko.observable();
	this.ActivityMinimumTime = ko.observable();
	this.ActivityMaximumTime = ko.observable();
    
	this.EarliestStartTimeIsEnabled = ko.observable(false);
	this.LatestStartTimeIsEnabled = ko.observable(false);
    
	this.EarliestEndTimeIsEnabled = ko.observable(false);
	this.LatestEndTimeIsEnabled = ko.observable(false);

	this.IsSaveAsNewTemplate = ko.observable();
	this.NewTemplateName = ko.observable('');

	this.IsTimeInputEnabled = ko.observable();

    this.EarliestStartTime = ko.computed(function() {
        if (!self.EarliestStartTimeIsEnabled()) return undefined;
        return self.EarliestStartTimeInternal();
    });
    this.LatestStartTime = ko.computed(function () {
        if (!self.LatestStartTimeIsEnabled()) return undefined;
        return self.LatestStartTimeInternal();
    });
    
    this.EarliestEndTime = ko.computed(function () {
        if (!self.EarliestEndTimeIsEnabled()) return undefined;
        return self.EarliestEndTimeInternal();
    });
    this.LatestEndTime = ko.computed(function () {
        if (!self.LatestEndTimeIsEnabled()) return undefined;
        return self.LatestEndTimeInternal();
    });

	this.EnableActivityTimeEditing = ko.computed(function () {
		var result = self.ActivityPreferenceId();
		return result != undefined && result != '';
	});

	_initPreferenceString();

	function _initPreferenceString() {
		self.PreferenceString = ko.computed(function () {
			var str = self.PreferenceId() + self.EarliestStartTime() + self.LatestStartTime() + self.EarliestEndTime() +
				self.EarliestEndTimeNextDay() + self.LatestEndTime() + self.LatestEndTimeNextDay() + self.MinimumWorkTime() +
				self.MaximumWorkTime() + self.ActivityEarliestStartTime() + self.ActivityLatestStartTime() + self.ActivityEarliestEndTime() +
				self.ActivityLatestEndTime() + self.ActivityPreferenceId() + self.ActivityMinimumTime() + self.ActivityMaximumTime();
			return str;
		});

		self.PreferenceString.subscribe(function (newValue) {
			if (self.SelectedTemplate()) {
				self.SelectedTemplate(undefined);
				$("#Preference-template")
					.selectbox({ value: '' });
			}
		});
	}

	function _disposePreferenceString() {
		self.PreferenceString.dispose();
	}

	this.HasTemplates = ko.computed(function () {
	    return self.AvailableTemplates().length > 0;
	});

	this.IsDetailsVisible = ko.computed(function() {
	    return !self.HasTemplates() || self.IsTemplateDetailsVisible();
	});

    this.ToggleSign = ko.computed(function() {
        return self.IsTemplateDetailsVisible() ? '-' : '+';
    });

	this.SelectedTemplate.subscribe(function (newValue) {
		if (newValue) {
			_disposePreferenceString();
			if (!newValue.PreferenceId)
				self.PreferenceId('');
			else
				self.PreferenceId(newValue.PreferenceId);
			self.EarliestStartTimeInternal(newValue.EarliestStartTime);
			self.LatestStartTimeInternal(newValue.LatestStartTime);
			self.EarliestEndTimeInternal(newValue.EarliestEndTime);
			self.LatestEndTimeInternal(newValue.LatestEndTime);
			self.EarliestEndTimeNextDay(newValue.EarliestEndTimeNextDay);
			self.LatestEndTimeNextDay(newValue.LatestEndTimeNextDay);
			self.MinimumWorkTime(newValue.MinimumWorkTime);
			self.MaximumWorkTime(newValue.MaximumWorkTime);
			if (!newValue.ActivityPreferenceId)
				self.ActivityPreferenceId('');
			else
				self.ActivityPreferenceId(newValue.ActivityPreferenceId);
			self.ActivityEarliestStartTime(newValue.ActivityEarliestStartTime);
			self.ActivityLatestStartTime(newValue.ActivityLatestStartTime);
			self.ActivityEarliestEndTime(newValue.ActivityEarliestEndTime);
			self.ActivityLatestEndTime(newValue.ActivityLatestEndTime);
			self.ActivityMinimumTime(newValue.ActivityMinimumTime);
			self.ActivityMaximumTime(newValue.ActivityMaximumTime);

			_initPreferenceString();
		}
	});

	this.IsTimeInputEnabled.subscribe(function (newValue) {
		if (!newValue) {
			self.ActivityPreferenceId('');
			self.EarliestStartTimeInternal(undefined);
			self.LatestStartTimeInternal(undefined);
			self.EarliestEndTimeInternal(undefined);
			self.LatestEndTimeInternal(undefined);
			self.EarliestEndTimeNextDay(false);
			self.LatestEndTimeNextDay(false);
			self.MinimumWorkTime(undefined);
			self.MaximumWorkTime(undefined);
		}
	});

	this.EnableActivityTimeEditing.subscribe(function (newValue) {
		if (!newValue) {
			self.ActivityEarliestStartTime(undefined);
			self.ActivityLatestStartTime(undefined);
			self.ActivityEarliestEndTime(undefined);
			self.ActivityLatestEndTime(undefined);
			self.ActivityMinimumTime(undefined);
			self.ActivityMaximumTime(undefined);
		}
	});

	this.ValidationError = ko.observable();
};
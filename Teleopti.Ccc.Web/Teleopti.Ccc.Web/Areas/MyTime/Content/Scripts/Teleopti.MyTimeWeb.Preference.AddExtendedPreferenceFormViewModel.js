/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


if (typeof(Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Preference) === 'undefined') {
	Teleopti.MyTimeWeb.Preference = {};
}


Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function (ajax, showMeridian, saveAsNewTemplateMethod, deletePreferenceTemplateMethod, savePreferenceMethod, isShiftCategorySelectedAsStandardPreferenceMethod) {
	var self = this;

	this.AvailableTemplates = ko.observableArray();
	this.SelectedTemplateIdInternal = ko.observable();
	this.IsTemplateDetailsVisible = ko.observable(true);

	this.PreferenceId = ko.observable();
	this.EarliestStartTime = ko.observable();
	this.LatestStartTime = ko.observable();
	this.EarliestEndTime = ko.observable();
	this.EarliestEndTimeNextDay = ko.observable();
	this.LatestEndTime = ko.observable();
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
	this.IsSaveAsNewTemplate = ko.observable();
	this.NewTemplateName = ko.observable('');
	this.AddPreferenceFormVisible = ko.observable(false);
	this.ValidationError = ko.observable();
	this.IsPreferenceInputVisible = ko.observable(true);
	this.ShowMeridian = ko.observable(showMeridian);
	this.IsTimeInputVisible = ko.observable(true);
	this.NextDayMin = ko.observable(false);
	this.NextDayMax = ko.observable(false);

    this.IsPreferenceInputVisibleToggle = function() {
        self.IsPreferenceInputVisible(!self.IsPreferenceInputVisible());
    };

    this.IsPreferenceInputVisibleToggleCss = ko.computed(function() {
        return !self.IsPreferenceInputVisible() ? 'glyphicon glyphicon-circle-arrow-down' : 'glyphicon glyphicon-circle-arrow-up';
    });

	this.SelectedTemplateId = ko.computed({
	    read: self.SelectedTemplateIdInternal,
	    write: function(value) {
	        self.SelectedTemplateIdInternal(value);
	        self.UpdateModelFromTemplate();
	    }
	});

    this.ShowError = ko.computed(function() {
        return self.ValidationError() !== undefined && self.ValidationError() !== '';
    });
    this.DeleteTemplateEnabled = ko.computed(function() {
        return (self.SelectedTemplateId() !== undefined && self.SelectedTemplateId().length > 0);
    });
    
    this.DeleteTemplate = function() {
        deletePreferenceTemplateMethod(self.SelectedTemplateId());
    };

    this.SaveAsNewTemplate = function() {
        saveAsNewTemplateMethod(ko.toJS(self));
    };

    this.SavePreferences = function() {
        savePreferenceMethod(ko.toJS(self));
    };

	this.ToggleAddPreferenceFormVisible = function () {
	    self.AddPreferenceFormVisible(!self.AddPreferenceFormVisible());
	};

	this.EnableActivityTimeEditing = ko.computed(function () {
		var result = self.ActivityPreferenceId();
		return result != undefined && result != '';
	});
    
	this.EarliestEndTimeNextDayClass = ko.computed(function () {
	    return self.EarliestEndTimeNextDay() ? undefined : 'icon-white';
	});
	this.LatestEndTimeNextDayClass = ko.computed(function () {
	    return self.LatestEndTimeNextDay() ? undefined : 'icon-white';
	});
    
	this.EarliestEndTimeNextDayToggleEnabled = ko.computed(function () {
	    var isEnabled = self.EarliestEndTime() !== undefined && self.EarliestEndTime() !== '';
	    if (!isEnabled)
	        self.EarliestEndTimeNextDay(false);
	    return isEnabled;
	});
	this.LatestEndTimeNextDayToggleEnabled = ko.computed(function () {
	    var isEnabled = self.LatestEndTime() !== undefined && self.LatestEndTime() !== '';
	    if (!isEnabled)
	        self.LatestEndTimeNextDay(false);
	    return isEnabled;
	});
    
	this.LatestEndTimeNextDayToggle = ko.computed(function () {
	    self.LatestEndTimeNextDay(self.NextDayMax());
	    return self.NextDayMax();
	});
	this.EarliestEndTimeNextDayToggle = ko.computed(function () {
	    self.EarliestEndTimeNextDay(self.NextDayMin());
	    return self.NextDayMin();
	});
    
	this.IsSaveAsNewTemplateToggle = function () {
	    self.IsSaveAsNewTemplate(!self.IsSaveAsNewTemplate());
	};
	this.IsSaveAsNewTemplateClass = ko.computed(function () {
		return self.IsSaveAsNewTemplate() ? 'glyphicon glyphicon-minus-sign' : 'glyphicon glyphicon-plus-sign';
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
			if (self.SelectedTemplateId()) {
				self.SelectedTemplateId('');
			}
			self.IsTimeInputVisible(isShiftCategorySelectedAsStandardPreferenceMethod());
			if (!self.IsTimeInputVisible()) {
			    self.Reset();
			}
		});
	}

	function _disposePreferenceString() {
		self.PreferenceString.dispose();
	}

	this.HasTemplates = ko.computed(function () {
	    return self.AvailableTemplates().length > 1;
	});
    
	this.IsDetailsVisible = ko.computed(function() {
	    return !self.HasTemplates() || self.IsTemplateDetailsVisible();
	});

    this.ToggleSign = ko.computed(function() {
        return self.IsTemplateDetailsVisible() ? '-' : '+';
    });

    this.UpdateModelFromTemplate = function () {
        var newValue = self.SelectedTemplate();
        if (newValue === undefined || newValue.Value.length === 0)
            return;
	    _disposePreferenceString();
		if (!newValue.PreferenceId)
			self.PreferenceId('');
		else
			self.PreferenceId(newValue.PreferenceId);
		self.EarliestStartTime(newValue.EarliestStartTime);
		self.LatestStartTime(newValue.LatestStartTime);
		self.EarliestEndTime(newValue.EarliestEndTime);
		self.LatestEndTime(newValue.LatestEndTime);
		self.EarliestEndTimeNextDay(newValue.EarliestEndTimeNextDay);
		self.LatestEndTimeNextDay(newValue.LatestEndTimeNextDay);
		self.MinimumWorkTime(newValue.MinimumWorkTime === undefined ? '' : newValue.MinimumWorkTime);
		self.MaximumWorkTime(newValue.MaximumWorkTime === undefined ? '' : newValue.MaximumWorkTime);
		if (!newValue.ActivityPreferenceId)
			self.ActivityPreferenceId('');
		else
			self.ActivityPreferenceId(newValue.ActivityPreferenceId);
		self.ActivityEarliestStartTime(newValue.ActivityEarliestStartTime);
		self.ActivityLatestStartTime(newValue.ActivityLatestStartTime);
		self.ActivityEarliestEndTime(newValue.ActivityEarliestEndTime);
		self.ActivityLatestEndTime(newValue.ActivityLatestEndTime);
		self.ActivityMinimumTime(newValue.ActivityMinimumTime === undefined ? '' : newValue.ActivityMinimumTime);
		self.ActivityMaximumTime(newValue.ActivityMaximumTime == undefined ? '' : newValue.ActivityMaximumTime);

		self.IsTimeInputVisible(isShiftCategorySelectedAsStandardPreferenceMethod());
		_initPreferenceString();
	};
    
	this.SelectedTemplate = ko.computed({
	    read: function() {
	        var selectedItemArray = $.grep(self.AvailableTemplates(), function (item, index) {
	            return self.SelectedTemplateId() === item.Value;
	        });

	        if (selectedItemArray.length > 0)
	            return selectedItemArray[0];
	        
	        return undefined;
	    },
	    write: function (value) {
	        if (value !== undefined && value.Value !== undefined) {
	            self.SelectedTemplateId(value.Value);
	        }
	    }
	});

	this.Reset = function () {
	    if (self.IsTimeInputVisible()) {
	        self.PreferenceId('');
	    }
	    self.EarliestStartTime(undefined);
	    self.LatestStartTime(undefined);
	    self.EarliestEndTime(undefined);
	    self.EarliestEndTimeNextDay(undefined);
	    self.LatestEndTime(undefined);
	    self.LatestEndTimeNextDay(undefined);
	    self.MinimumWorkTime('');
	    self.MaximumWorkTime('');
	    self.ActivityPreferenceId('');
	    self.ValidationError(undefined);
	    self.SelectedTemplateId(undefined);
	    self.NextDayMin(false);
	    self.NextDayMax(false);
	};

	this.EnableActivityTimeEditing.subscribe(function (newValue) {
		if (!newValue) {
			self.ActivityEarliestStartTime(undefined);
			self.ActivityLatestStartTime(undefined);
			self.ActivityEarliestEndTime(undefined);
			self.ActivityLatestEndTime(undefined);
			self.ActivityMinimumTime('');
			self.ActivityMaximumTime('');
		}
	});

	this.FormatPreferencesOption = function (option) {
	    var optionElement = $(option.element);
	    var activityColor = optionElement.data('color');

	    if (option.text.length === 0) return option.text;
	    
	    return '<span class="pull-left" style="padding-left: 16px;margin-right: 5px;border-radius: 4px;background-color: ' + activityColor + '">&nbsp;</span>' + option.text;
	};
};
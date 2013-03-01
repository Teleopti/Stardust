/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

     
//$(function() {
//    $( "#accordion" ).accordion({
//        collapsible: true
//    });
//});

Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function (ajax) {
    var self = this;

    this.LoadAvailableTemplates = function () {
        var deferred = $.Deferred();
        ajax.Ajax({
            url: "Preference/GetPreferenceTemplates",
            dataType: "json",
            type: 'GET',
            success: function (data, textStatus, jqXHR) {
                data = data || [];

                //				self.AvailableTemplates.removeAll();
                //				self.AvailableTemplates.push("");
                //				$.each(data, function (index, element) {
                //					self.AvailableTemplates.push(element);
                //				});

                //				self.AvailableTemplates.valueHasMutated();
                self.AvailableTemplates(data);

                $("#Preference-template").selectbox({ refreshMenu: data });

                //				self.AvailableTemplates.push(new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel(0));
                //				self.AvailableTemplates.push(ko.utils.arrayMap(data,
                //					function(i) { return new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel(i); }
                //				));
                deferred.resolve();
            }
        });
        return deferred.promise();
    };

    this.AvailableTemplates = ko.observableArray();
    this.SelectedTemplateId = ko.observable();
    this.SelectedTemplate = ko.observable();
    this.UrlTimeStamp = ko.observable();

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
    this.IsShowDetails = ko.observable(true);

    this.toggleDetails = function () {
        var currentValue = this.IsShowDetails();
        self.IsShowDetails(!currentValue);
    };

    this.IsTimeInputEnabled = ko.observable();

    this.EnableActivityTimeEditing = ko.computed(function () {
        var result = self.ActivityPreferenceId();
        alert("activity");
        return result != undefined && result != '';
    });

    this.SelectedTemplate = ko.computed(function () {
        var templateId = self.SelectedTemplateId();
        alert("template");
        return templateId != undefined && templateId != '';
    });

    this.SelectedTemplate.subscribe(function (newValue) {
        if (newValue) {
            self.PreferenceId(newValue.PreferenceId);
            self.EarliestStartTime(newValue.EarliestStartTime);
            self.LatestStartTime(newValue.LatestStartTime);
            self.EarliestEndTime(newValue.EarliestEndTime);
            self.LatestEndTime(newValue.LatestEndTime);
        }
    });

    this.IsTimeInputEnabled.subscribe(function (newValue) {
        if (!newValue) {
            self.ActivityPreferenceId('');
            self.EarliestStartTime(undefined);
            self.LatestStartTime(undefined);
            self.EarliestEndTime(undefined);
            self.LatestEndTime(undefined);
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

    this.reset = function () {
        self.PreferenceId('');
        self.EarliestStartTime(undefined);
        self.LatestStartTime(undefined);
        self.EarliestEndTime(undefined);
        self.EarliestEndTimeNextDay(undefined);
        self.LatestEndTime(undefined);
        self.LatestEndTimeNextDay(undefined);
        self.MinimumWorkTime(undefined);
        self.MaximumWorkTime(undefined);
        self.ActivityPreferenceId('');
        self.ValidationError(undefined);
    };
};


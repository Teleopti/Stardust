Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function(
	ajax,
	showMeridian,
	saveAsNewTemplateMethod,
	deletePreferenceTemplateMethod,
	savePreferenceMethod,
	isShiftCategorySelectedAsStandardPreferenceMethod
) {
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
		return !self.IsPreferenceInputVisible()
			? 'glyphicon glyphicon-circle-arrow-down'
			: 'glyphicon glyphicon-circle-arrow-up';
	});

	this.IsHostAMobile = ko.observable(Teleopti.MyTimeWeb.Common.IsHostAMobile());

	this.ToggleOpen = function(data, event) {
		//for mobile compatibility
		if (!self.IsHostAMobile()) return;
		$(event.currentTarget).toggleClass('open');
	};

	this.SelectedTemplateId = ko.computed({
		read: self.SelectedTemplateIdInternal,
		write: function(value) {
			self.ValidationError('');
			if (value == undefined || value.length == 0) {
				self.SelectedTemplateIdInternal(value);
				self.UpdateModelFromTemplate();
			} else {
				var newTemplate = self.GetTemplateById(value);
				var periodData = $('#Preference-body').data('mytime-periodselection');

				var parameters = {
					date: moment(periodData.Date).format('YYYY-MM-DD'),
					preferenceId: newTemplate.PreferenceId
				};

				ajax.Ajax({
					url: 'Preference/ValidatePreference',
					contentType: 'application/json; charset=utf-8',
					dataType: 'json',
					type: 'GET',
					data: parameters,
					statusCode400: function(jqXHR, textStatus, errorThrown) {
						var errorMessage = $.parseJSON(jqXHR.responseText);
						var message = errorMessage.Errors.join('</br>');
						self.ValidationError(message);
					},
					success: function(data, textStatus, jqXHR) {
						var result = $.parseJSON(jqXHR.responseText);
						if (result.isValid) {
							self.SelectedTemplateIdInternal(value);
							self.UpdateModelFromTemplate();
						} else {
							self.Reset();
							self.ValidationError(result.message);
						}
					}
				});
			}
		}
	});

	this.ShowError = ko.computed(function() {
		return self.ValidationError() !== undefined && self.ValidationError() !== '';
	});
	this.DeleteTemplateEnabled = ko.computed(function() {
		return self.SelectedTemplateId() !== undefined && self.SelectedTemplateId().length > 0;
	});

	this.DeleteTemplate = function() {
		deletePreferenceTemplateMethod(self.SelectedTemplateId());
	};

	this.SaveAsNewTemplate = function() {
		saveAsNewTemplateMethod(ko.toJS(self));
	};

	this.SavePreferences = function() {
		savePreferenceMethod(ko.toJS(self), function() {
			if (!self.ShowError() && self.IsHostAMobile()) self.AddPreferenceFormVisible(false);
		});
	};

	this.ToggleAddPreferenceFormVisible = function() {
		self.AddPreferenceFormVisible(!self.AddPreferenceFormVisible());
	};

	this.EnableActivityTimeEditing = ko.computed(function() {
		var result = self.ActivityPreferenceId();
		return result != undefined && result != '';
	});

	this.EarliestEndTimeNextDayClass = ko.computed(function() {
		return self.EarliestEndTimeNextDay() ? undefined : 'icon-white';
	});
	this.LatestEndTimeNextDayClass = ko.computed(function() {
		return self.LatestEndTimeNextDay() ? undefined : 'icon-white';
	});

	this.EarliestEndTimeNextDayToggleEnabled = ko.computed(function() {
		var isEnabled = self.EarliestEndTime() !== undefined && self.EarliestEndTime() !== '';
		if (!isEnabled) self.EarliestEndTimeNextDay(false);
		return isEnabled;
	});
	this.LatestEndTimeNextDayToggleEnabled = ko.computed(function() {
		var isEnabled = self.LatestEndTime() !== undefined && self.LatestEndTime() !== '';
		if (!isEnabled) self.LatestEndTimeNextDay(false);
		return isEnabled;
	});

	this.LatestEndTimeNextDayToggle = ko.computed(function() {
		self.LatestEndTimeNextDay(self.NextDayMax());
		return self.NextDayMax();
	});
	this.EarliestEndTimeNextDayToggle = ko.computed(function() {
		self.EarliestEndTimeNextDay(self.NextDayMin());
		return self.NextDayMin();
	});

	this.IsSaveAsNewTemplateToggle = function() {
		self.IsSaveAsNewTemplate(!self.IsSaveAsNewTemplate());
	};
	this.IsSaveAsNewTemplateClass = ko.computed(function() {
		return self.IsSaveAsNewTemplate() ? 'glyphicon glyphicon-minus-sign' : 'glyphicon glyphicon-plus-sign';
	});

	_initPreferenceString();

	function _initPreferenceString() {
		self.PreferenceString = ko.computed(function() {
			var str =
				self.PreferenceId() +
				self.EarliestStartTime() +
				self.LatestStartTime() +
				self.EarliestEndTime() +
				self.EarliestEndTimeNextDay() +
				self.LatestEndTime() +
				self.LatestEndTimeNextDay() +
				self.MinimumWorkTime() +
				self.MaximumWorkTime() +
				self.ActivityEarliestStartTime() +
				self.ActivityLatestStartTime() +
				self.ActivityEarliestEndTime() +
				self.ActivityLatestEndTime() +
				self.ActivityPreferenceId() +
				self.ActivityMinimumTime() +
				self.ActivityMaximumTime();
			return str;
		});

		self.PreferenceString.subscribe(function(newValue) {
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

	this.HasTemplates = ko.computed(function() {
		return self.AvailableTemplates().length > 1;
	});

	this.IsDetailsVisible = ko.computed(function() {
		return !self.HasTemplates() || self.IsTemplateDetailsVisible();
	});

	this.ToggleSign = ko.computed(function() {
		return self.IsTemplateDetailsVisible() ? '-' : '+';
	});

	this.UpdateModelFromTemplate = function() {
		var newValue = self.SelectedTemplate();
		if (newValue === undefined || newValue.Value.length === 0) return;
		_disposePreferenceString();
		if (!newValue.PreferenceId) self.PreferenceId('');
		else self.PreferenceId(newValue.PreferenceId);
		self.EarliestStartTime(newValue.EarliestStartTime);
		self.LatestStartTime(newValue.LatestStartTime);
		self.EarliestEndTime(newValue.EarliestEndTime);
		self.LatestEndTime(newValue.LatestEndTime);
		self.EarliestEndTimeNextDay(newValue.EarliestEndTimeNextDay);
		self.LatestEndTimeNextDay(newValue.LatestEndTimeNextDay);
		self.MinimumWorkTime(newValue.MinimumWorkTime === undefined ? '' : newValue.MinimumWorkTime);
		self.MaximumWorkTime(newValue.MaximumWorkTime === undefined ? '' : newValue.MaximumWorkTime);
		if (!newValue.ActivityPreferenceId) self.ActivityPreferenceId('');
		else self.ActivityPreferenceId(newValue.ActivityPreferenceId);
		self.ActivityEarliestStartTime(newValue.ActivityEarliestStartTime);
		self.ActivityLatestStartTime(newValue.ActivityLatestStartTime);
		self.ActivityEarliestEndTime(newValue.ActivityEarliestEndTime);
		self.ActivityLatestEndTime(newValue.ActivityLatestEndTime);
		self.ActivityMinimumTime(newValue.ActivityMinimumTime === undefined ? '' : newValue.ActivityMinimumTime);
		self.ActivityMaximumTime(newValue.ActivityMaximumTime == undefined ? '' : newValue.ActivityMaximumTime);

		self.IsTimeInputVisible(isShiftCategorySelectedAsStandardPreferenceMethod());
		_initPreferenceString();
	};

	this.GetTemplateById = function(id) {
		var selectedItemArray = $.grep(self.AvailableTemplates(), function(item, index) {
			return id === item.Value;
		});

		return selectedItemArray.length > 0 ? selectedItemArray[0] : undefined;
	};

	this.SelectedTemplate = ko.computed({
		read: function() {
			return self.GetTemplateById(self.SelectedTemplateId());
		},
		write: function(value) {
			if (value !== undefined && value.Value !== undefined) {
				self.SelectedTemplateId(value.Value);
			}
		}
	});

	this.Reset = function() {
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

	this.EnableActivityTimeEditing.subscribe(function(newValue) {
		if (!newValue) {
			self.ActivityEarliestStartTime(undefined);
			self.ActivityLatestStartTime(undefined);
			self.ActivityEarliestEndTime(undefined);
			self.ActivityLatestEndTime(undefined);
			self.ActivityMinimumTime('');
			self.ActivityMaximumTime('');
		}
	});

	this.FormatPreferencesOption = function(option) {
		var optionElement = $(option.element);
		var activityColor = optionElement.data('color');

		if (option.text.length === 0) return option.text;

		return (
			'<span class="pull-left" style="padding-left: 16px;margin-right: 5px;background-color: ' +
			activityColor +
			'">&nbsp;</span>' +
			option.text
		);
	};
};

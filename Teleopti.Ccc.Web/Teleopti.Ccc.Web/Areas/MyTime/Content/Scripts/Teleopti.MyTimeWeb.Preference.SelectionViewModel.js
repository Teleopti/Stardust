Teleopti.MyTimeWeb.Preference.SelectionViewModel = function(
	maxMustHave,
	setMustHaveMethod,
	setPreferenceMethod,
	deletePreferenceMethod,
	currentMustHave
) {
	var self = this;
	this.MaxMustHaves = ko.observable(0);
	self.minDate = ko.observable(moment());
	self.maxDate = ko.observable(moment());

	self.displayDate = ko.observable();
	self.nextPeriodDate = ko.observable(moment());
	self.previousPeriodDate = ko.observable(moment());
	self.currentMustHaves = ko.observable(currentMustHave);
	self.selectedDate = ko.observable(moment().startOf('day'));
	self.IsHostAMobile = ko.observable(Teleopti.MyTimeWeb.Common.IsHostAMobile());

	self.setCurrentDate = function(date) {
		self.selectedDate(date);
	};

	self.nextPeriod = function() {
		self.selectedDate(self.nextPeriodDate());
	};

	self.previousPeriod = function() {
		self.selectedDate(self.previousPeriodDate());
	};

	self.updateMustHave = function(newMustHave, originalMustHave) {
		if (originalMustHave) {
			if (!newMustHave) self.currentMustHaves(self.currentMustHaves() - 1);
		} else {
			if (newMustHave) self.currentMustHaves(self.currentMustHaves() + 1);
		}
	};

	self.addMustHave = function() {
		setMustHaveMethod(true, self.updateMustHave);
	};

	self.removeMustHave = function() {
		setMustHaveMethod(false, self.updateMustHave);
	};

	self.mustHaveEnabled = function() {
		return self.maxMustHave() > 0;
	};

	self.maxMustHave = ko.observable(maxMustHave);

	self.selectedPreference = ko.observable();
	self.availablePreferences = ko.observableArray();

	self.selectAndapplyPreference = function(item) {
		self.selectedPreference(item);
		setPreferenceMethod(self.selectedPreference().Value);
	};

	self.applyPreference = function() {
		setPreferenceMethod(self.selectedPreference().Value);
	};

	self.addMustHaveEnabled = ko.computed(function() {
		return self.currentMustHaves() < maxMustHave;
	});

	self.deletePreference = function() {
		deletePreferenceMethod(self.updateMustHave);
	};

	self.selectedPreferenceText = ko.computed(function() {
		if (self.selectedPreference()) {
			return self.selectedPreference().Text;
		}
		return '';
	});

	self.enableDateSelection = function() {
		self.subscription = self.selectedDate.subscribe(function(d) {
			Teleopti.MyTimeWeb.Portal.NavigateTo(
				'Preference/Index' + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD'))
			);
		});
	};
};

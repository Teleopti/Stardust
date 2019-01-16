Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel = function() {
	var self = this;
	this.MaxMustHaves = ko.observable(0);
	this.DayViewModels = ko.observableArray();

	this.CurrentMustHaves = ko.computed(function() {
		var total = 0;
		$.each(self.DayViewModels(), function(index, day) {
			if (day.MustHave()) total += 1;
		});
		return total;
	});

	this.MustHaveText = ko.computed(function() {
		return self.CurrentMustHaves() + '(' + self.MaxMustHaves() + ')';
	});

	this.MustHaveClass = ko.computed(function() {
		if (self.CurrentMustHaves() == self.MaxMustHaves()) return 'grey-out';

		return undefined;
	});

	this.SetData = function(inDayViewModels, inMaxMustHave) {
		self.DayViewModels([]);
		self.DayViewModels.push.apply(
			self.DayViewModels,
			$.map(inDayViewModels, function(value, key) {
				return value;
			})
		);

		self.MaxMustHaves(inMaxMustHave);
	};
};

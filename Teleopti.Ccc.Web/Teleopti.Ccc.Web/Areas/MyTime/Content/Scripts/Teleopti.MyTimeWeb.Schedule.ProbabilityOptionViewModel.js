Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel = function (selectedOptionValue, parent) {
	var probabilityType = Teleopti.MyTimeWeb.Common.Constants.probabilityType;

	this.Template = "probability-options-template";
	this.noneOption = ko.observable(probabilityType.none);
	this.absenceOption = ko.observable(probabilityType.absence);
	this.overtimeOption = ko.observable(probabilityType.overtime);

	this.checkedProbability = ko.observable(selectedOptionValue);

	this.onOptionSelected = function (opValue) {
		this.checkedProbability(opValue);
		parent.onProbabilityOptionSelectCallback(parseInt(this.checkedProbability()));
	};
	this.absenceProbabilityEnabled = ko.computed(function() {
		return parent.absenceProbabilityEnabled();
	});
}
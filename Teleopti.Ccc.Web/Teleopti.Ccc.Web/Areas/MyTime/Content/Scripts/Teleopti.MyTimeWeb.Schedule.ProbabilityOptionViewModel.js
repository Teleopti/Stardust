Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel = function (selectedOptionValue, weekViewModelParent) {
	var probabilityType = Teleopti.MyTimeWeb.Schedule.Constants.probabilityType;

	this.Template = "probability-options-template";
	this.noneOption = ko.observable(probabilityType.none);
	this.absenceOption = ko.observable(probabilityType.absence);
	this.overtimeOption = ko.observable(probabilityType.overtime);

	this.checkedProbability = ko.observable(selectedOptionValue);

	this.onOptionSelected = function (opValue) {
		this.checkedProbability(opValue);
		weekViewModelParent.OnProbabilityOptionSelectCallback(parseInt(this.checkedProbability()));
	};
	this.absenceProbabilityEnabled = ko.computed(function() {
		return weekViewModelParent.absenceProbabilityEnabled();
	});
}
Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel = function (selectedOptionValue, weekViewModelParent) {
	this.Template = "probability-options-template";
	this.noneOption = ko.observable(Teleopti.MyTimeWeb.Schedule.Constants.noneProbabilityType);
	this.absenceOption = ko.observable(Teleopti.MyTimeWeb.Schedule.Constants.absenceProbabilityType);
	this.overtimeOption = ko.observable(Teleopti.MyTimeWeb.Schedule.Constants.overtimeProbabilityType);

	this.checkedProbability = ko.observable(selectedOptionValue);

	this.onOptionSelected = function (opValue) {
		this.checkedProbability(opValue);
		weekViewModelParent.OnProbabilityOptionSelectCallback(parseInt(this.checkedProbability()));
	}
}
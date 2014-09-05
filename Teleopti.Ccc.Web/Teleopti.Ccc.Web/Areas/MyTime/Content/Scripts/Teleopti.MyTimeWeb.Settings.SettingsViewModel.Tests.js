
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Settings.SettingsViewModel");
	
	test("should load cultrues", function () {
		var cultures = [{ id: "1", text: "Culture A" },
			{ id: "2", text: "Culture B" },
			{ id: "3", text: "Culture C" }];
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Settings/Cultures") {
					options.success(
						{
							Cultures: cultures,
							ChoosenUiCulture: { id: 2 },
							ChoosenCulture: {id: 1}
						}
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Settings.SettingsViewModel(ajax);

		viewModel.loadCultures();

		equal(viewModel.cultures().length, 3);
		equal(viewModel.cultures()[0].id, 1);
		equal(viewModel.cultures()[0].text, "Culture A");
		equal(viewModel.selectedUiCulture(), 2);
		equal(viewModel.selectedCulture(), 1);
		equal(viewModel.culturesLoaded(), true);
		equal(viewModel.avoidReload, false);
	});
	
});

define([
	], function(
	) {
		var selectedPersonId;

		return {
			SelectedPersonId: function() {
				return selectedPersonId;
			},
			SetSelectedPersonId: function (personId) {
				selectedPersonId = personId;
			},
			Clear: function() {
				selectedPersonId = undefined;
			}
		};
	});

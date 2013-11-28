define([
	], function(
	) {
		var selectedPersonId;
		var selectedLayer;
		var selectedGroupId;

		return {
			SelectedPersonId: function() {
				return selectedPersonId;
			},
			SetSelectedPersonId: function (personId) {
				selectedPersonId = personId;
			},
			SelectedGroupId: function () {
				return selectedGroupId;
			},
			SetSelectedGroupId: function (groupId) {
				selectedGroupId = groupId;
			},
			SelectedLayer:function() {
				return selectedLayer;
			},
			SetSelectedLayer: function (layer) {
				selectedLayer = layer;
			},
			Clear: function() {
				selectedPersonId = undefined;
				selectedLayer = undefined;
			}
		};
	});

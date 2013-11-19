define([
	], function(
	) {
		var selectedPersonId;
		var selectedLayer;

		return {
			SelectedPersonId: function() {
				return selectedPersonId;
			},
			SetSelectedPersonId: function (personId) {
				selectedPersonId = personId;
			},
			SelectedLayer:function() {
				return selectedLayer;
			},
			SetSelectedLayer: function (layer) {
				selectedLayer = layer;
			}
		};
	});

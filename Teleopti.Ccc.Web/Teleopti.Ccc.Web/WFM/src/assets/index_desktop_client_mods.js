function iAmCalledFromFatClient() {
	removeElements();
	expandMaterialContainer();
	changeToClassicTheme();
};

var removeElements = function() {
	var elementsToRemove = [
		document.getElementById('nav'),
		document.getElementById('notice')
	];

	elementsToRemove.forEach(function(elmt) {
		if (elmt) elmt.style.display = 'none';
	});
};

var expandMaterialContainer = function() {
	var materialcontainer = document.getElementById('materialcontainer');
	if (materialcontainer) {
		materialcontainer.style["margin-top"] = '0';
		materialcontainer.style.height = '100vh';
	}
};

var changeToClassicTheme = function() {
	if (angular.element(document.getElementById('darkTheme')).scope() == null || angular.element(document.getElementById('overlay')).scope() == null) {
		return;
	} else {
		angular.element(document.getElementById('darkTheme')).scope().toggleTheme('classic');
		angular.element(document.getElementById('overlay')).scope().toggleOverlay(false);
	}
};

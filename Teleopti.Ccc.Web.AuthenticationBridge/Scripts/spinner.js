$(document).ready(function () {
	var spinnerWrapper = document.getElementById('spinnerwrapper');
	window.onbeforeunload = function() {
		spinnerWrapper.style.visibility = "visible";
	}
});
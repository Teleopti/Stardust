function loadToggle(toggle, callback) {
	var xmlhttp = new XMLHttpRequest();
	xmlhttp.onreadystatechange = function () {
		if (xmlhttp.readyState == 4) {
			if (xmlhttp.status == 200) {
				if (xmlhttp.responseText.length > 0) {
					var result = JSON.parse(xmlhttp.responseText);
					callback(result.IsEnabled);
				} else {
					callback(false);
				}
			}
		}
	};
	xmlhttp.open("GET", "Web/ToggleHandler/IsEnabled?toggle=" + toggle, true);
	xmlhttp.setRequestHeader("Accept", "application/json");
	xmlhttp.send();

	callback(true);
};

function setCookieFromQueryString() {
	var date = new Date();
	date.setTime(date.getTime() + (30 * 60 * 1000));
	var expires = "; expires=" + date.toUTCString();

	var query = window.location.search.substring(1);
	var vars = query.split('&');
	for (var i = 0; i < vars.length; i++) {
		var pair = vars[i].split('=');
		document.cookie = decodeURIComponent(pair[0]) + "=" + decodeURIComponent(pair[1]) + expires + "; path=/";
	}
}

window.onload = function () {
	setCookieFromQueryString();

	window.location.href = '/TeleoptiWFM/Web/Wfm/#/forecast';
};
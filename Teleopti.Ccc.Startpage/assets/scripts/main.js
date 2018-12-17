function setCookieFromQueryString() {
	var date = new Date();
	date.setTime(date.getTime() + (30 * 60 * 1000));
	var expires = "; expires=" + date.toUTCString();

	var query = window.location.search.substring(1);
	var vars = query.split('&');

	if (vars.length === 1 && vars[0] === '') {
		return;
	}
	for (var i = 0; i < vars.length; i++) {
		var pair = vars[i].split('=');
		document.cookie = decodeURIComponent(pair[0]) + "=" + decodeURIComponent(pair[1]) + expires + "; path=/";
	}
}

window.onload = function () {

	document.getElementById("launcher").addEventListener("click", function (e) {
		e.preventDefault();
		window.location.href = "/TeleoptiWFM/Client/Teleopti.Ccc.SmartClientPortal.Shell.application?" + window.location.search.substring(1);
		return false;
	});

	var xmlhttp = new XMLHttpRequest();
	xmlhttp.onreadystatechange = function () {
		if (xmlhttp.readyState == 4) {
			if (xmlhttp.status == 200) {
				if (xmlhttp.responseText.length > 0) {
					var result = JSON.parse(xmlhttp.responseText);

					if (result.IsEnabled === true) {
						document.getElementById("webwfm").style.visibility = "visible";
						document.getElementById("webwfm").style.display = "block";
					} else {
						console.log(result.IsEnabled, 'false');
						document.getElementById("webwfm").style.display = "none";
						document.getElementById("webwfm").style.visibility = "hidden";
					}

				} else {
					console.log('fail', result);
				}
			}
		}
	};
	xmlhttp.open("GET", "Web/ToggleHandler/IsEnabled?toggle=Wfm_MinimumScaffolding_32659", true);
	xmlhttp.setRequestHeader("Accept", "application/json");
	xmlhttp.send();

	setCookieFromQueryString();
};
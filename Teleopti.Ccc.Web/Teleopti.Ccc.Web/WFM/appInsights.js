
var xmlhttp = new XMLHttpRequest();
var url = "../Start/Config/SharedSettings";

xmlhttp.onreadystatechange = function () {
	if (this.readyState == 4 && this.status == 200) {
		var sharedSettings = JSON.parse(this.responseText);
		var instrumentationKey = sharedSettings.InstrumentationKey;
		var iKey =
			{
				instrumentationKey: instrumentationKey
			};
		var appInsights = window.appInsights ||
			function (a) {
				function b(a) {
					c[a] = function () {
						var b = arguments;
						c.queue.push(function () { c[a].apply(c, b) })
					}
				}

				var c = { config: a }, d = document, e = window;
				setTimeout(
					function () {
						var b = d.createElement("script");
						b.src = a.url || "ai.js";
						d.getElementsByTagName("script")[0].parentNode.appendChild(b);
					});
				try {
					c.cookie = d.cookie
				} catch (a) {
				}
				c.queue = [];
				for (var f = ["Event", "Exception", "Metric", "PageView", "Trace", "Dependency"];
					f.length;
				) b("track" + f.pop());
				if (b("setAuthenticatedUserContext"), b("clearAuthenticatedUserContext"), b("startTrackEvent"),
					b("stopTrackEvent"),
					b("startTrackPage"), b("stopTrackPage"), b("flush"), !a.disableExceptionTracking) {
					f = "onerror", b("_" + f);
					var g = e[f];
					e[f] = function (a, b, d, e, h) {
						var i = g && g(a, b, d, e, h);
						return !0 !== i && c["_" + f](a, b, d, e, h), i
					}
				}

				return c
			}(iKey);
		window.appInsights = appInsights, appInsights.queue && 0 === appInsights.queue.length && appInsights.trackPageView();
	}
};

xmlhttp.open("GET", url, true);
xmlhttp.send();
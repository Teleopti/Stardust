var appInsights = window.appInsights || function (config) {
    function r(config) {
        t[config] = function () {
            var i = arguments;
            t.queue.push(function () {
                t[config].apply(t, i)
            })
        }
    }
    var t = {
        config: config
    },
        u = document,
        e = window,
        o = "script",
        s = u.createElement(o),
        i, f;
    for (s.src = config.url || "//az416426.vo.msecnd.net/scripts/a/ai.0.js", u.getElementsByTagName(o)[0].parentNode.appendChild(s), t.cookie = u.cookie, t.queue = [], i = ["Event", "Exception", "Metric", "PageView", "Trace"]; i.length;) r("track" + i.pop());
    return r("setAuthenticatedUserContext"), r("clearAuthenticatedUserContext"), config.disableExceptionTracking || (i = "onerror", r("_" + i), f = e[i], e[i] = function (config, r, u, e, o) {
        var s = f && f(config, r, u, e, o);
        return s !== !0 && t["_" + i](config, r, u, e, o), s
    }), t
}({
    instrumentationKey: "c932b48c-1076-419d-b691-3e88e41becb3"
});
window.appInsights = appInsights;
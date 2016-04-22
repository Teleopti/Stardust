(function () {
    'use strict';
    angular.module('appInsightsService', []).service('appInsights', [function () {

        var config = appInsights.config;
                config.enableDebug = true;
                config.verboseLogging = true;
                appInsights.config = config;

                function trackPageView(pageName, url) {
                appInsights.trackPageView(pageName, url);
            }

            function trackEvent(event) {
                appInsights.trackEvent(event);
            }

            return {
                trackPageView: trackPageView,
                trackEvent: trackEvent
            }

        }
    ]);
})();

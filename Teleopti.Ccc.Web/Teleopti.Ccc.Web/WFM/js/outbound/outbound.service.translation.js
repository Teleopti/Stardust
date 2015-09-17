(function() {

    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundTranslationService', ['$q', '$translate', outboundTranslationService]);

    function outboundTranslationService($q, $translate) {
              
        this.translate = translate;
        this.applyTranslation = applyTranslation;
        this.translateWeekdays = translateWeekdays;
	    this.translatePromise = $translate;   

        function loadDictionary(key, dictionary) {
            var keys = angular.isArray(key) ? key : [key];
            var translations = key.map(function(x) { return $translate(x); });
               
            return $q.all(translations).then(function(results) {
                for (var i = 0; i < translations.length; i++) {
                    dictionary[keys[i]] = results[i];
                }              
            });
        }

        function translate(key, cb, target) {
            var dictionary = {};
            loadDictionary(key, dictionary).then(function () {
                target.dictionary = dictionary;
                cb.apply(target);
            });                       
        }

        function applyTranslation(key, f, target) {
            return function() {
                var _arguments = arguments;
                translate(key, function() {
                    f.apply(target, _arguments);
                }, target);
            };
        }

        function translateWeekdays(cb, target) {

            var localeData = moment.localeData($translate.use()),
                weekdayNames = localeData._weekdaysShort,
                weekdays = [];

            var startDow = (moment.localeData()._week) ? moment.localeData()._week.dow : 0;

            for (var i = 0; i < 7; i++) {
                var curDow = (startDow + i) % 7;
                weekdays.push({ weekday: curDow, weekdayName: weekdayNames[curDow] });
            }
           
            target.weekdays = weekdays;
            cb.apply(target);
        }

    }

})();
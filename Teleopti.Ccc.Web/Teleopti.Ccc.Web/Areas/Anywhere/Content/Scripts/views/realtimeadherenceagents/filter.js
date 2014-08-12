define([
		'knockout'
],
    function (ko) {
    	return function () {
    		var that = {};

			that.match = function (items, filter) {
    			var matchedItems = 0;
    			var filterWords = getFilterWordsWithoutQuotes(filter);
    			if (!filterWords) {
    				return false;
    			}

				for (var i = 0; i < items.length; i++) {
					var item = items[i];
					if (!item) {
						continue;
					}
					for (var wordIter = 0; wordIter < filterWords.length; wordIter++) {
						var filterWord = filterWords[wordIter];

						if (shouldNegate(filterWord) && stringContains(item, filterWord.slice(1))) {
							return false;
						}
						if (stringContains(item, filterWord)) {
							matchedItems++;
						}
					}

				}
				var unNegatedFilterWords = ko.utils.arrayFilter(filterWords, function (word) { return !shouldNegate(word); }).length;
				if (matchedItems === unNegatedFilterWords) {
					return true;
				}
				return false;
			};

			var getFilterWordsWithoutQuotes = function (rawInput) {
				var retWords = [];
				var filterWords = rawInput.match(/([!]?\w+)|(?:["'](.*?)["'])/g);
				if (filterWords) {
					for (var i = 0; i < filterWords.length; i++) {
						retWords.push(filterWords[i].match(/[!A-Za-z0-9\s]+/g)[0]);
					}
				}
				return retWords;
			}
			var shouldNegate = function (word) { return word.indexOf("!") === 0; }


    		var stringContains = function (item, filter) {
    			return item.toUpperCase().indexOf(filter.toUpperCase()) > -1;
    		}
			
    		return that;
    	};
    }
);
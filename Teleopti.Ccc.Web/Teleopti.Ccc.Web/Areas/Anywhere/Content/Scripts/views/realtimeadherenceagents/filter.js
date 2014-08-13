define([
		'knockout'
],
	function(ko) {
		return function() {
			var that = {};

			that.match = function(items, filter) {
				var matchedItems = 0;
				var filterWords = mapOutFilterWords(filter);
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

						if (shouldNegate(filterWord)) {
							var filterWordWithoutExlamationMark = filterWord.slice(1);
							if (shouldMatchExact(filterWordWithoutExlamationMark) &&
								stringContains(item, removeQuotes(filterWordWithoutExlamationMark))) {
								return false;
							}

							if (stringContains(item, filterWord.slice(1))) {
								return false;
							}
						}
						if (shouldMatchExact(filterWord) && item.toUpperCase() === removeQuotes(filterWord).toUpperCase()) {
							matchedItems++;
						}
						if (stringContains(item, filterWord)) {
							matchedItems++;
						}
					}

				}
				var unNegatedFilterWords = ko.utils.arrayFilter(filterWords, function(word) { return !shouldNegate(word); }).length;
				if (matchedItems === unNegatedFilterWords) {
					return true;
				}
				return false;
			};

			var mapOutFilterWords = function(rawInput) { return rawInput.match(/([!]*\w+)|(?:[!"']{1,2}(.*?)["'])/g); }
			var shouldNegate = function(word) { return word.indexOf("!") === 0; }
			var shouldMatchExact = function (word) { return word.indexOf("'") === 0 || word.indexOf('"') === 0; }
			var removeQuotes = function(word) { return word.slice(1, word.length - 1); }

			var stringContains = function(item, filter) {
				return item.toUpperCase().indexOf(filter.toUpperCase()) > -1;
			}

			return that;
		};
	}
);
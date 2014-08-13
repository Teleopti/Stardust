define([
		'knockout'
],
	function (ko) {
		return function () {
			var that = {};

			that.match = function(items, filter) {
				var andRelationalMatches = 0;
				var orRelationalMatches = 0;
				var filterWords = mapOutFilterWords(filter);
				if (!filterWords) {
					return false;
				}

				var andRelationalWords = [],
					orRelationalWords = [];
				for (var j = 0; j < filterWords.length; j++) {
					if (filterWords[j].toUpperCase() === "OR") {
						orRelationalWords.push([filterWords[j - 1], filterWords[j + 1]]);
						filterWords.splice(j - 1, 3);
						j++;
					}
				}
				andRelationalWords = filterWords;


				for (var i = 0; i < items.length; i++) {
					var item = items[i];
					if (!item) {
						continue;
					}

					for (var orWordIterator = 0; orWordIterator < orRelationalWords.length; orWordIterator++) {
						var wordPair = orRelationalWords[orWordIterator];
						var firstWordMatch = matchItem(wordPair[0], item);
						if (firstWordMatch === - 1) {
							return false;
						}
						var secondWordMatch = matchItem(wordPair[1], item);
						if (secondWordMatch === -1) {
							return false;
						}

						orRelationalMatches += firstWordMatch;
						orRelationalMatches += secondWordMatch;
					}
					
					for (var andWordIterator = 0; andWordIterator < andRelationalWords.length; andWordIterator++) {
						var filterWord = andRelationalWords[andWordIterator];

						var wordMatch = matchItem(filterWord, item);
						if (wordMatch === -1) {
							return false;
						}
						
						andRelationalMatches += wordMatch;
					}

				}
				var unNegatedAndRelationalWords = ko.utils.arrayFilter(andRelationalWords, function (word) { return !shouldNegate(word); }).length;
				var unNegatedOrRelationalWords = ko.utils.arrayFilter(orRelationalWords, function (word) { return !shouldNegate(word); }).length;
				if (andRelationalMatches === unNegatedAndRelationalWords
					&& orRelationalMatches >= unNegatedOrRelationalWords) {
					return true;
				}
				return false;
			};

			var mapOutFilterWords = function (rawInput) { return rawInput.match(/([!]*\w+)|(?:[!"']{1,2}(.*?)["'])/g); }

			var matchItem = function (word, item) {
				if (shouldNegate(word)) {
					if (matchesNegated(item, word)) {
						return -1;
					}
				}
				if (shouldMatchExact(word) && item.toUpperCase() === removeQuotes(word).toUpperCase()) {
					return 1;
				}
				if (stringContains(item, word)) {
					return 1;
				}
				return 0;
			}

			var shouldNegate = function (word) { return word.indexOf("!") === 0; }

			var matchesNegated = function (item, filterWord) {
				var filterWordWithoutExlamationMark = filterWord.slice(1);

				if (shouldMatchExact(filterWordWithoutExlamationMark) &&
					stringContains(item, removeQuotes(filterWordWithoutExlamationMark))) {
					return true;
				}

				if (stringContains(item, filterWord.slice(1))) {
					return true;
				}
				return false;
			}

			var shouldMatchExact = function (word) { return word.indexOf("'") === 0 || word.indexOf('"') === 0; }
			var removeQuotes = function (word) { return word.slice(1, word.length - 1); }

			var stringContains = function (item, filter) {
				return item.toUpperCase().indexOf(filter.toUpperCase()) > -1;
			}

			return that;
		};
	}
);
define([
		'knockout'
],
	function(ko) {
		return function() {
			var that = {};

			that.match = function(items, filter) {
				var andRelationalMatches = 0;
				var orRelationalMatches = 0;
				var filterWords = mapOutFilterWords(filter);
				if (!filterWords) {
					return false;
				}

				var orRelationalWords = mapRelationsBetweenWords(filterWords);
				var andRelationalWords = filterWords;

				for (var i = 0; i < items.length; i++) {
					var item = items[i];
					if (!item) {
						continue;
					}

					for (var orWordIterator = 0; orWordIterator < orRelationalWords.length; orWordIterator++) {
						var wordPair = orRelationalWords[orWordIterator];
						if (!wordPair[0] || !wordPair[1])
							continue;

						var firstWordMatch = matchItem(wordPair[0], item, orNegateMatching);
						if (firstWordMatch === -1) {
							return false;
						}
						var secondWordMatch = matchItem(wordPair[1], item, orNegateMatching);
						if (secondWordMatch === -1) {
							return false;
						}

						orRelationalMatches += firstWordMatch;
						orRelationalMatches += secondWordMatch;
					}

					for (var andWordIterator = 0; andWordIterator < andRelationalWords.length; andWordIterator++) {
						var filterWord = andRelationalWords[andWordIterator];
						if (filterWord.toUpperCase() === "OR") {
							continue;
						}
						var wordMatch = matchItem(filterWord, item, andNegateMatching);
						if (wordMatch === -1) {
							return false;
						}

						andRelationalMatches += wordMatch;
					}

				}

				return calculateResult(andRelationalWords, andRelationalMatches, orRelationalWords, orRelationalMatches);
			};

			var mapOutFilterWords = function(rawInput) { return rawInput.match(/([!]*\w+)|(?:[!"']{1,2}(.*?)["'])/g); }

			var mapRelationsBetweenWords = function(filterWords) {
				var orRelationalWords = [];
				if (filterWords.length >= 3) {
					for (var j = 0; j < filterWords.length; j++) {
						if (filterWords[j].toUpperCase() === "OR") {
							orRelationalWords.push([filterWords[j - 1], filterWords[j + 1]]);
							filterWords.splice(j - 1, 3);
							j++;
						}
					}
				}
				return orRelationalWords;
			}

			var orNegateMatching = function(word, item) {
				if (matchesNegated(item, word)) {
					return -1;
				}
				return 1;
			}

			var andNegateMatching = function(word, item) {
				if (matchesNegated(item, word)) {
					return -1;
				}
				return 0;
			}

			var matchesNegated = function(item, filterWord) {
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

			var matchItem = function(word, item, negateMatching) {
				if (shouldNegate(word)) {
					return negateMatching(word, item);
				}
				if (shouldMatchExact(word) && item.toUpperCase() === removeQuotes(word).toUpperCase()) {
					return 1;
				}
				if (stringContains(item, word)) {
					return 1;
				}
				return 0;
			}

			var shouldNegate = function(word) { return word.indexOf("!") === 0; }
			var shouldMatchExact = function(word) { return word.indexOf("'") === 0 || word.indexOf('"') === 0; }
			var removeQuotes = function(word) { return word.slice(1, word.length - 1); }
			var stringContains = function(item, filter) { return item.toUpperCase().indexOf(filter.toUpperCase()) > -1; }

			var calculateResult = function(andWords, andMatches, orWords, orMatches) {
				var unNegatedAndRelationalWords = ko.utils.arrayFilter(andWords, function(word) { return !shouldNegate(word); }).length;
				var unNegatedOrRelationalWords = ko.utils.arrayFilter(orWords, function(word) { return !shouldNegate(word); }).length;
				return (andMatches === unNegatedAndRelationalWords
					&& orMatches >= unNegatedOrRelationalWords);
			}
			return that;
		};
	}
);
define([
		'knockout',
		'xregexp'
],
	function(ko, xregexp) {
		return function() {
			var that = {};

			that.match = function (items, filter) {
				var andRelationalMatches = 0;
				var orRelationalMatches = 0;
				var filterWords = mapOutFilterWords(filter);
				if (!filterWords || filterWords.length === 0) {
					return false;
				}

				var mapping =  mapRelationsBetweenWords(filterWords);
				var orRelationalCombinations = mapping.orRelationalWords;
				var andRelationalWords = mapping.andRelationalWords;
				
				for (var i = 0; i < items.length; i++) {
					var item = items[i];
					if (!item) {
						continue;
					}

					for (var orCombinationIter = 0; orCombinationIter < orRelationalCombinations.length; orCombinationIter++) {
						var orCombination = orRelationalCombinations[orCombinationIter];
						var combinationMatch = matchOrCombination(orCombination, item);
						if (combinationMatch === -1) {
							return false;
						}
						orRelationalMatches += combinationMatch;
					}

					for (var andWordIterator = 0; andWordIterator < andRelationalWords.length; andWordIterator++) {
						var currentAndWord = andRelationalWords[andWordIterator];
						if (currentAndWord.toUpperCase() === "OR") {
							continue;
						}
						var wordMatch = matchItem(currentAndWord, item, andNegateMatching);
						if (wordMatch === -1) {
							return false;
						}

						andRelationalMatches += wordMatch;
					}

				}

				return calculateResult(andRelationalWords, andRelationalMatches, orRelationalCombinations, orRelationalMatches);
			};

			var mapOutFilterWords = function (rawInput) {
				var reg = xregexp('([!]*(\\p{L}|\\p{N}|\\p{S}|(?![\'\!])\\p{P})+)|(?:[!"\']{1,2}(.*?)["\'])', 'g');
				return xregexp.match(rawInput, reg);
			}

			var mapRelationsBetweenWords = function (filterWords) {
				var returnObject = {};
				var orRelationalWords = [];
				var orKeywordWasFoundOnIndex = [];
				if (couldContainOrRelation(filterWords)) {
					for (var j = 0; j < filterWords.length; j++) {
						if (filterWords[j].toUpperCase() === "OR") {
							orRelationalWords.push([filterWords[j - 1], filterWords[j + 1]]);
							if (j > 1)
								orKeywordWasFoundOnIndex.push(j-1);
							j += 2;

							while (filterWords[j] && filterWords[j].toUpperCase() === "OR") {
								orRelationalWords[orRelationalWords.length - 1].push(filterWords[j + 1]);
								orKeywordWasFoundOnIndex.push(j);
								j += 2;
							}
						}
					}
					if (orRelationalWords.length > 0) {
						for (var k = orKeywordWasFoundOnIndex.length; k >= 0; k--) {
							filterWords.splice(k, 3);
						}
					}
				}
				returnObject.orRelationalWords = orRelationalWords;
				returnObject.andRelationalWords = filterWords;
				return returnObject;
			}
			var couldContainOrRelation = function (words) { return words.length >= 3; }

			var matchOrCombination = function (orCombination, item) {
				for (var wordInCombinationIter = 0; wordInCombinationIter < orCombination.length; wordInCombinationIter++) {
					var currentOrWord = orCombination[wordInCombinationIter];
					if (!currentOrWord) {
						continue;
					}
					var combinationMatch = matchItem(currentOrWord, item, orNegateMatching);
					if (combinationMatch === -1) {
						return -1;
					}
					if (combinationMatch === 1) {
						return 1;
					}
				}
				return 0;
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
				var unNegatedOrRelationalWords = ko.utils.arrayFilter(orWords, function (word) { return !shouldNegate(word); }).length;
				return (andMatches === unNegatedAndRelationalWords
					&& orMatches >= unNegatedOrRelationalWords);
			}
			return that;
		};
	}
);
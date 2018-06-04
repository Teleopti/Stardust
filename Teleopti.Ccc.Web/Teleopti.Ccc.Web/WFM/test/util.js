function parseTestPattern(argv) {
	var found = false;
	var pattern = argv
		.map(function(v) {
			if (found) {
				return v;
			}
			if (v === '--') {
				found = true;
			}
		})
		.filter(function(a) {
			return a;
		})
		.join(' ');
	return pattern ? ['--grep', pattern] : [];
}

module.exports = { parseTestPattern };

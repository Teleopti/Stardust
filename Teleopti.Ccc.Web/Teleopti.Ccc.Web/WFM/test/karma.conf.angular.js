const { applyBaseConfig } = require('./karma.shared');

module.exports = function(config) {
	applyBaseConfig(config);
	config.set({
		files: [
			{ pattern: 'node_modules/moment/min/moment-with-locales.js', watched: false }]})
};

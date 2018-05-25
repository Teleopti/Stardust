const { applyBaseConfig } = require('./karma.shared');

module.exports = function(config) {
	applyBaseConfig(config);
	config.set({
		reporters: ['mocha'],
		mochaReporter: {
			symbols: {
				success: '✔'
			}
		},
		colors: true
	});
};

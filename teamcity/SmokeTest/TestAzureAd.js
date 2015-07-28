var webdriverio = require('webdriverio');
var request = require("request");
var client = webdriverio
	.remote({
		desiredCapabilities: {
			browserName: 'chrome'
		}
	});

var log = function (msg) {
	if (client)
		client.call(function () { console.log(msg) });
};

var closeAndThrow = function (msg) {
	log(msg);
	log('shutdown client.');
	client.end();
	throw new Error(msg);
};

client.init();
var webUrl = process.env.UrlToTest + '/Web/MyTime';
log('navigate to url ' + webUrl);
client.url(webUrl)
	.waitForExist('#Username-input', 60000, false, function (err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to navigate to sign in page. ' + err);
		}
	});
log('try to sign in');
client.setValue('#Username-input', 'demo')
	.setValue('#Password-input', 'demo')
	.click('#Signin-button')
	.waitForExist('.user-name', 120000, false, function(err, res, response) {
		if (err || !res) {
			log('failed to sign in first time. res: ' + res);
			//second try
			client.url(webUrl)
				.waitForExist('.user-name', 120000, false, function(err, res, response) {
					if (err || !res) {
						log('failed to sign in second time. res: ' + res);
						closeAndThrow('failed to sign in. ' + err);
					} else {
						log('sign in succeeded second time:' + res);
					}
				});
		} else {
			log('sign in succeeded first time:' + res);
		}
	});

log('try to sign out');
client.click('.user-name-link')
	.click('#signout')
	.waitForExist('.azuread', 60000, false, function(err, res, response) {
		client.click('.azuread')
			.waitForExist('#cred_userid_inputtext', 60000, false, function(err, res, response) {
				client.setValue('#cred_userid_inputtext', 'demo@teleopti.com')
					.setValue('#cred_password_inputtext', 'teleoptidemo')
					.waitForExist('#passwordInput', 120000, false, function(err, res, response) {
						client.setValue('#passwordInput', 'teleoptidemo')
							.click('#submitButton')
							.waitForExist('.user-name', 120000, false, function(err, res, response) {
								if (err || !res) {
									log('failed to sign in first time. res: ' + res);
									//second try
									client.url(webUrl)
										.waitForExist('.user-name', 120000, false, function(err, res, response) {
											if (err || !res) {
												log('failed to sign in second time. res: ' + res);
												closeAndThrow('failed to sign in. ' + err);
											} else {
												log('sign in succeeded second time:' + res);
											}
										});
								} else {
									log('sign in succeeded first time:' + res);
								}
							});
					});
			});
	});

log('shutdown client.');
client.end();



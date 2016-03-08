var webdriverio = require('webdriverio');
var request = require("request");
var client = webdriverio
	.remote({
		desiredCapabilities: {
			browserName: 'chrome'
		}
	});
	
var log = function(msg){
	if(client)
		client.call(function(){console.log(msg)});
};

var closeAndThrow = function(msg){
	log(msg);
	log('shutdown client.');
	client.end();
	throw new Error(msg);
};	

client.init();
var webUrl = process.env.UrlToTest + '/Web';
log('navigate to url ' + webUrl + '/Anywhere');
client.url(webUrl + '/Anywhere')
	.waitForExist('#Username-input', 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to navigate to sign in page. ' + err);
		}
	});
log('try to sign in');
client.setValue('#Username-input', 'demo')
	.setValue('#Password-input', 'demo')
	.click('#Signin-button')
	.waitForExist('.user-name', 300000, false, function(err, res, response) {
		if (err || !res) {
			log('failed to sign in first time.');
			//second try
			client.url(webUrl + '/Anywhere')
				.waitForExist('.user-name', 120000, false, function(err, res, response) {
					if (err || !res) {
						log('failed to sign in first time after refresh.');
						
						log('try to sign in second time:');
						log('navigate to url ' + webUrl + '/Anywhere');
						client.url(webUrl + '/Anywhere')
							.waitForExist('#Username-input', 60000, false, function(err, res, response) {
								if (err || !res) {
									closeAndThrow('failed to navigate to sign in page. ' + err);
								}
							});
							
						log('try to sign in');
						client.setValue('#Username-input', 'demo')
							.setValue('#Password-input', 'demo')
							.click('#Signin-button')
							.waitForExist('.user-name', 300000, false, function(err, res, response) {
								if (err || !res) {
									closeAndThrow('failed to sign in second time.' + err);
								}else{
									log('succeeded to sign in second time.');
								}
							});

					}else{
						log('succeeded to sign in first time after refresh.');
					}
				});
		}else{
			log('succeeded to sign in first time.');
		}
	});

log('navigate to health check');
client.url(webUrl + '/HealthCheck')
	.waitForExist(".services li span", 300000, false, function(err, res, response) {
		if (err || !res) {
			log('service bus isnot up and running after trying 5 minutes. Try to refresh, and have another try.');
			client.refresh()
				.waitForExist(".services li span", 300000, false, function(err, res, response) {
					if (err || !res) {
						closeAndThrow('service bus isnot up and running after trying 5 minutes. Please visit ' + webUrl + '/HealthCheck , and have a look. ' + err);
					}
				});
		}
	})
.waitForExist(".stardust", 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('Stardust is not up and running. Please visit ' + webUrl + '/HealthCheck , and have a look. ' + err);
		}
		log('Stardust is up and running');
	});	
log('check service bus and broker');
client.click('#Start-Check');
// have no idea why first time of checking is not working, have to refresh and check it again
client.pause(5000);
client.refresh()
	.waitForExist(".services li span", 300000, false, function(err, res, response) {
		if (err || !res) {
			log('service bus isnot up and running after trying 5 minutes. Try to refresh, and have another try.');
			client.refresh()
				.waitForExist(".services li span", 300000, false, function(err, res, response) {
					if (err || !res) {
						closeAndThrow('service bus isnot up and running after trying 5 minutes. Please visit ' + webUrl + '/HealthCheck , and have a look. ' + err);
					}
				});
		}
	});
client.pause(5000);
client.click('#Start-Check')
	.waitForExist('#Bus-Results', 300000, false, function(err, res, response) {
		if (err || !res) {
			log('service bus or message broker donot work well after trying 5 minutes. Try to refresh, and have another try.');
			//second try
			client.refresh()
				.waitForExist(".services li span", 300000, false, function(err, res, response) {
					if (err || !res) {
						closeAndThrow('service bus isnot up and running after trying 5 minutes. Please visit ' + webUrl + '/HealthCheck , and have a look. ' + err);
					}
				});
			client.pause(5000);
			
			client.click('#Start-Check')
				.waitForExist('#Bus-Results', 300000, false, function(err, res, response) {
					if (err || !res) {
						closeAndThrow('service bus or message broker donot work well after trying 5 minutes. Please visit ' + webUrl + '/HealthCheck , and have a look. ' + err);
					}else{
						log('service bus and broker work well');
					}
				});
		}else{
			log('service bus and broker work well');
		}
	});
	
log('shutdown client.');
client.end();



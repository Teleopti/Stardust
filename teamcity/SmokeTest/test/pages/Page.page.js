function Page () {
    this.title = 'My Page';
	this.webUrl = process.env.UrlToTest + '/Web';
}

Page.prototype.open = function (path) {
	console.log('Navigating to: ' + this.webUrl + '/' + path);
    browser.url(this.webUrl + '/' + path)
};

Page.prototype.isCurrentPage = function() {
	console.log('Comparing "' + this.title + '" with the current page: "' + browser.getTitle().trim() + '"');
	return  this.title === browser.getTitle().trim();
};

module.exports = new Page()
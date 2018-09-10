const theme = localStorage.getItem('theme') || 'classic';

const linkHrefs = [
	{
		id: 'themeStyleguide',
		href: 'dist/styleguide_' + theme + '.css'
	},
	{
		id: 'themeAnt',
		href: 'dist/ant_' + theme + '.css'
	}
];

if (theme === 'classic') {
	document.documentElement.classList.add('angular-theme-classic');
} else if (theme === 'dark') {
	document.documentElement.classList.add('angular-theme-dark');
}

linkHrefs.forEach(function loadStyles(linkHref) {
	const link = document.getElementById(linkHref.id);
	link.href = linkHref.href;
});

function notTranslatedPrefixes() {
	return ['PM_',
	'www.teleopti.com',
	'F01%253APM_',
	'Category%253AP',
	'Special%253ACategories',
	'Category%3AP',
	'F01%3APM_',
	'Special%3ACategories',
	'mailto'	];
}

function notTranslatedPages() {
	return ['f01%253APerformance_Manager.html',
		'Schedule_vs_Forecast_Intraday_Deviation.html',
		'Requested_Days_Count.html',
		'Requests_Count.html',
		'Quality_Questionnaire.html',
		'Percentage_Average_Score.html',
		'Percentage_Evaluations.html',
		'Point_Average_Score.html',
		'Point_Evaluations.html',
		'Pivot_Chart.html',
		'Pivot_Table.html',
		'Readiness.html',
		'Named_Sets.html',
		'Measure.html',
		'Grade_Average_Score.html',
		'Grade_Evaluations.html',
		'Forecast_Accuracy_After_Call_Work.html',
		'Forecast_Accuracy_Talk_Time.html',
		'Dimension.html',
		'Agents_Ready_FTE.html',
		'SDK.html',
		'SDK_Release_Notes.html',
		'Shift_Category_Dimension.html',
		'Scenario_Dimension.html',
		'Day_Off_Dimension.html',
		'Absence_Dimension.html',
		'Activity_Dimension.html',
		'OLAP.html',
		'OLAP_Cube.html',
		'Requested_Time.html',
		'Scheduled_Time_Hourly.html'];
}

function getPageName(link) {
	return link.substr(link.lastIndexOf('/') + 1);
}

function isLinkTranslated(link, forbiddenLinks) {
	link = getPageName(link);
	for (var i = 0, forbiddenLink; forbiddenLink = forbiddenLinks[i]; i++) {
		if (link === forbiddenLink) {
			return false;
		}
	}
	return true;
}

function isPrefixAllowed(link, forbiddenPrefixes) {
	var linkPage = getPageName(link);
	for (var i = 0, prefix; prefix = forbiddenPrefixes[i]; i++) {
		if (linkPage.indexOf(prefix) === 0) {
			return false;
		}
	}
	return true;
}

function endsWith(str, suffix) {
	return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function isSuffixAllowed(link) {
	var linkPage = getPageName(link);
	var forbiddenSuflixes = ['en.html', 'ru.html', 'sv.html', 'zh.html', '.png'];
	for (var i = 0, suffix; suffix = forbiddenSuflixes[i]; i++) {
		if (endsWith(linkPage, suffix)) {
			return false;
		}
	}
	return true;
}

function isCurrentPageTranslated(link) {
	var pageName = getPageName(link)
	return pageName === 'en.html' ||
			pageName === 'ru.html' ||
			pageName === 'sv.html' ||
			pageName === 'zh.html';
}

function isPageToTeleoptiSite(link) {
	return (link.substr(0, 23) === 'http://www.teleopti.com');
}

function shouldReplaceLink(link, pageName) {
	return isPrefixAllowed(link, notTranslatedPrefixes()) &&
		isLinkTranslated(link, notTranslatedPages()) &&
		isSuffixAllowed(link) &&
		isCurrentPageTranslated(pageName);
}

function isLinkInArray(string, array) {
	for (var i = 0, item; item = array[i]; i++) {
		if (item === string) {
			return true;
		}
	}
	return false;
}

function replaceLinks() {
	var url = window.location.pathname;
	var pageName = getPageName(url);
	var links = document.links;
	for (var i = 0, link; link = links[i]; i++) {
		if (link) {
			link = link.href;
			if (isPageToTeleoptiSite(link))
				continue;
			if (shouldReplaceLink(link, pageName)) {
				var trimmedLink;
				if (endsWith(link, '1.html'))
					trimmedLink = link.substr(0, link.length - 7);
				else if (endsWith(link, 'en') ||
					endsWith(link, 'ru') ||
					endsWith(link, 'sv') ||
					endsWith(link, 'zh') ||
					endsWith(link, 'pt'))
					trimmedLink = link.substr(0, link.length - 3);
				else
					trimmedLink = link.substr(0, link.length - 5);

				document.links[i].href = trimmedLink + '\\' + pageName;
			}
		}
		document.links[i].href = document.links[i].href.replace(/\+/g, '_');
		document.links[i].href = document.links[i].href.replace('wiki.teleopti.com/TeleoptiCCC/', 'localhost/TeleoptiWFM/LocalWIki/');
		document.links[i].href = document.links[i].href.replace('%3A', '%253A');
	}
	fixNavigationLinkTexts(pageName);
}

function fixNavigationLinkTexts(pageName) {
	/*
	0 English
	1 Russian
	2 Swedish
	3 Chinese
	*/
	var language = 0;
	if (pageName == 'en.html') {
		language = 0;
	} else if (pageName == 'ru.html') {
		language = 1;
	} else if (pageName == 'sv.html') {
		language = 2;
	} else if (pageName == 'zh.html') {
		language = 3;
	}

	var linkTranslation = getLinkTranslationText();
	var navigationPanel = document.getElementById('mw-panel');
	var links = navigationPanel.getElementsByTagName('a');
	for (var i = 0, link; link = links[i]; i++) {
		if (i >= 56 && i < 58)
			continue;
		if (i === 58) {
			link = undefined;
			continue;
		}
		if (linkTranslation[i])
			link.innerHTML = linkTranslation[i][language];
	}

	var titleTranslation = getTitleTranslationText();
	var titles = navigationPanel.getElementsByTagName('h5')
	for (var i = 1, title; title = titles[i]; i++) {
		title.innerHTML = titleTranslation[i][language];
	}
}

function getTitleTranslationText() {
	return [[], ['Modules', 'Модули', 'Moduler', '模块'],
	['Agent tools', 'Инструменты оператора', 'Agentverktyg', '坐席工具'],
	['Terms', 'Термины', 'Termer', '词条'],
	['Functionality', 'Функциональность', 'Funktionalitet', '功能'],
	['Processes', 'Процессы', 'Processer', '进度'],
	['Troubleshooting', 'Поиск и устранение неисправностей', 'Felsökning', '故障检查'],
	['General settings', 'Общие настройки', 'Allmänna inställningar', '一般设置'],
	['Interface and layout', 'Интерфейс и размещение', 'Gränssnitt och utformning', '界面和布局'],
	['Toolbox', 'Инструменты', 'Verktygslåda', '工具箱'],
	['Contact', 'Контакт', 'Kontakt', '联系'], ['', '', '', '']];
}

function getLinkTranslationText() {
	return [['', '', '', ''],
		// Modules
['People', 'Сотрудники', 'Personer', '人员'],
['Forecasts', 'Прогнозы', 'Prognoser', '预测'],
['Shifts', 'Смены', 'Skift', '班次'],
['Schedules', 'Расписание работы', 'Scheman', '排班'],
['Intraday', 'Мониторинг', 'Inom dag', '当天管理模块'],
['Reports', 'Отчеты', 'Rapporter', '报告'],
['Budgets', 'Бюджеты', 'Budget', '预算'],
['Performance Manager', 'Performance Manager', 'Performance Manager', '绩效管理工具'],
['Payroll Integration', 'Интеграция с зарплатной системой', 'Löneintegration', '工资表集成'],
['ASM', 'ASM', 'ASM', '坐席代表排班消息工具栏'],
['Holiday Planner', 'Планировщик отсутствий', 'Holiday Planner', '假期计划'],
['Real Time Adherence', 'Соблюдение графика в режиме реального времени', 'Real Time Adherence', '遵时管理'],
['Shift Trader', 'Обмен сменами', 'Shift Trader', '调班'],
['System overview', 'Обзор системы', 'Systemöversikt', '系统总览'],
['Module interdependencies', 'Взаимозависимости модулей', 'Beroenden mellan moduler', '模块中的相互依赖性'],
	// AgentTools
['MyTime', 'MyTime', 'MyTime', 'MyTime'],
['MyTimeWeb', 'MyTimeWeb', 'MyTimeWeb', 'MyTime网'],
	// Terms
['People terms', 'Термины модуля сотрудники', 'Termer i Personer', '人员 词条'],
['Forecasts terms', 'Термины модуля прогнозы', 'Termer i Prognoser', '预算 词条'],
['Shifts terms', 'Термины модуля смены', 'Termer i Skift', '班次 词条'],
['Schedules terms', 'Термины модуля расписание работы', 'Termer i Scheman', '排班 词条'],
['Intraday terms', 'Термины модуля мониторинг', 'Termer i Inom dag', '当天管理模块 词条'],
['Reports terms', 'Термины модуля отчеты', 'Termer i Rapporter', '报告 词条'],
['Budgets terms', 'Термины модуля бюджеты', 'Termer i Budget', '预算 词条'],
['Performance Manager terms', 'Термины модуля Performance Manager', 'Termer i Performance Manager', '绩效管理器 词条'],
['Payroll terms', 'Термины модуля интеграции с зарплатной системой', 'Termer i Löneintegration', '工资单 词条'],
['Options terms', 'Термины опций', 'Termer för inställningar', '选项 词条'],
['Permissions terms', 'Термины прав доступа', 'Termer i Rättigheter', '允许 词条'],
['General terms', 'Общие термины', 'Allmänna termer', '一般 词条'],
['Technical terms', 'Технические термины', 'Tekniska termer', '技术词条'],
['MyTime terms', 'Термины MyTime', 'Termer i MyTime', 'MyTime 词条'],
	// Functionality
['People functions', 'Функции модуля сотрудники', 'Funktioner i Personer', '人员功能'],
['Forecasts functions', 'Функции прогнозов', 'Funktioner i Prognoser', '预测功能'],
['Shifts functions', 'Функции модуля смены', 'Funktioner i Skift', '版此功能'],
['Schedules functions', 'Функции расписания работы', 'Funktioner i Scheman', '排班功能'],
['Meeting functions', 'Функции мероприятий', 'Funktioner i Möteshanteraren', '会议功能'],
['Intraday functions', 'Функции мониторинга', 'Funktioner i Inom dag', '当天管理模块功能'],
['Budgets functions', 'Функции бюджетов', 'Funktioner i Budget', '预算功能'],
['Performance Manager functions', 'Функции Performance Manager', 'Funktioner i Performance Manage', '绩效管理功能'],
['Payroll Integration functions', 'Функции интеграции с зарплатной системой', 'Funktioner i Löneintegration', '工资单整合功能'],
['Options functions', 'Функции опций', 'Funktioner i Inställningar', '选项功能'],
['Permissions functions', 'Функции прав доступа', 'Funktioner i Rättigheter', '允许（权限）功能'],
['MyTime functions', 'Функции MyTime', 'Funktioner i MyTime', 'MyTime功能'],
	// Processes
['Forecasting', 'Прогнозирование', 'Prognostisera', '预测'],
['Creating Shifts','Creating Shifts','Skapa skift','Creating Shifts'],
['Budgeting', 'Составление бюджетов', 'Budgetering', '预算'],
	// Troubleshooting
['Scheduling', 'Планирование расписания', 'Schemaläggning', '排班'],
	// General settings
['Options', 'Опции', 'Inställningar', '选项'],
['Permissions', 'Права доступа', 'Rättigheter', '允许'],
['Licenses', 'Лицензии', 'Licenser', '授权'],
	// Interface and layout
['Main portal', 'Главный портал', 'Portal', '主界面'],
['Ribbon bar', 'Панель иструментов', 'Verktygsfält', '提示栏'],
['Graphs', 'Графики', 'Grafer', '图形'],
['Tables', 'Таблицы', 'Tabeller', '表格'],
['Keyboard shortcuts', 'Сочетания клавиш', 'Kortkommandon', '键盘快捷键'],
['Wiki help', 'Помощь Wiki', 'Wikihjälp', 'Wiki 帮助'],
['Upload file', 'Загрузить файл', 'Ladda upp fil', '上传文件'],
	// Contact
['Feedback', 'Обратная связь', 'Feedback', '反馈']];
}

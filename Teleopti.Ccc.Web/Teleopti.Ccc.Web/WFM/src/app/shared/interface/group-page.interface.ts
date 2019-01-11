export interface GroupPage {
	BusinessHierarchy: GroupPageSite[];
	GroupPages: GroupPageSite[];
}

export interface GroupPageSite {
	Id: string;
	Name: string;
	Children?: GroupPageTeam[];
}

export interface GroupPageSiteItem extends GroupPageSite {
	SelectedCalendarId: string;
}

export interface GroupPageTeam {
	Id: string;
	Name: string;
}

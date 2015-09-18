
/* storing the encoding safe fixed legacy key in the database for legacy lookup */
update tenant.tenant 
set RtaKey = '!#?atAbgT%'
where 
RtaKey like '!#%' and
RtaKey like '%atAbgT[%]'

merge Historical.dbo.HR_Availability_TemporaryAvailability x
using(
select * from (
SELECT 
[FileDate]
,a.[NationalStoreNumber]
,a.[GEID]
,a.[TemporaryAvailabilityType]
,a.[TemporaryAvailabilityStartDate]
,[TemporaryAvailabilityEndDate]
,[TemporaryAvailabilityStartTime]
,[TemporaryAvailabilityEndTime]
,a.[TemporaryAvailabilityDayOfWeek]
,[LoadDateTime]
,a.[Provider]
,ROW_NUMBER() OVER (PARTITION BY a.NationalStoreNumber,a.GEID,a.[TemporaryAvailabilityType],a.[TemporaryAvailabilityStartDate],a.[TemporaryAvailabilityDayOfWeek],a.Provider order by FileDate desc) as RowNum
FROM Historical.dbo.STAGING_HR_Availability_TemporaryAvailability a
inner join(
select
max(filedate) as maxfiledate,
NationalStoreNumber,
GEID,
TemporaryAvailabilityType,
TemporaryAvailabilityStartDate,
TemporaryAvailabilityDayOfWeek,
Provider
from Historical.dbo.STAGING_HR_Availability_TemporaryAvailability
group by NationalStoreNumber,GEID,TemporaryAvailabilityType,TemporaryAvailabilityStartDate,TemporaryAvailabilityDayOfWeek,Provider
) b on a.NationalStoreNumber = b.NationalStoreNumber and a.GEID = b.GEID and a.TemporaryAvailabilityType = b.TemporaryAvailabilityType and a.TemporaryAvailabilityStartDate = b.TemporaryAvailabilityStartDate
	and a.TemporaryAvailabilityDayOfWeek = b.TemporaryAvailabilityDayOfWeek and a.Provider = b.Provider
) g where RowNum = 1
) y on x.NationalStoreNumber = y.NationalStoreNumber and x.GEID= y.GEID and x.TemporaryAvailabilityType = y.TemporaryAvailabilityType and x.TemporaryAvailabilityStartDate = y.TemporaryAvailabilityStartDate
	and x.TemporaryAvailabilityDayOfWeek = y.TemporaryAvailabilityDayOfWeek and x.Provider = y.Provider
when matched then update set
 x.[FileDate] = y.FileDate
,x.[NationalStoreNumber] = y.NationalStoreNumber
,x.[GEID] = y.GEID
,x.[TemporaryAvailabilityType] = y.TemporaryAvailabilityType
,x.[TemporaryAvailabilityStartDate] = y.TemporaryAvailabilityStartDate
,x.[TemporaryAvailabilityEndDate] = y.TemporaryAvailabilityEndDate
,x.[TemporaryAvailabilityStartTime] = y.TemporaryAvailabilityStartTime
,x.[TemporaryAvailabilityEndTime] = y.TemporaryAvailabilityEndTime
,x.[TemporaryAvailabilityDayOfWeek] = y.TemporaryAvailabilityDayOfWeek
,x.[Provider] = y.Provider
,UpdateDate = GETDATE()
when not matched by target
then insert
(
[FileDate]
,[NationalStoreNumber]
,[GEID]
,[TemporaryAvailabilityType]
,[TemporaryAvailabilityStartDate]
,[TemporaryAvailabilityEndDate]
,[TemporaryAvailabilityStartTime]
,[TemporaryAvailabilityEndTime]
,[TemporaryAvailabilityDayOfWeek]
,[Provider]
,CreateDate
,UpdateDate
) values (
 y.[FileDate]
,y.[NationalStoreNumber]
,y.[GEID]
,y.[TemporaryAvailabilityType]
,y.[TemporaryAvailabilityStartDate]
,y.[TemporaryAvailabilityEndDate]
,y.[TemporaryAvailabilityStartTime]
,y.[TemporaryAvailabilityEndTime]
,y.[TemporaryAvailabilityDayOfWeek]
,y.[Provider]
,GETDATE()
,GETDATE()
);
-- Script to incrementally add tables to purge list
declare @tablename varchar(50),@startDays int,@incrementDays int;
set @tablename = ''; --Enter table name here
set @startDays = 300; --Set the starting number of days out to purge
set @incrementDays = 1; --Set the incrementing amount of days out to purge

declare @tablecount int;
set @tablecount = (select count(*) from [Ctrl_PurgeControl]
where TableName = @tablename)

declare @daystokeep int
set @daystokeep = (select AmountOfDaysToKeep from [Ctrl_PurgeControl]
where TableName = @tablename)

if (@tablecount = 0)
  insert into [Ctrl_PurgeControl]
  values (@tablename, @startDays)
else
	if (@daystokeep > 60)
	  update [Ctrl_PurgeControl]
	  set AmountOfDaysToKeep = @daystokeep - @incrementDays
	  where TableName = @tablename
	else
		select @tablename + ' is already at 60 days' as [Result]

declare @amountOfDaysToKeep varchar(50),
		@sql nvarchar(500),
		@amountOfRecordsDeleted int


-- delete from table
set @amountOfDaysToKeep = cast((select AmountOfDaysToKeep from [Ctrl_PurgeControl] where TableName = @tableName) as varchar(50))
set @sql = 'delete from '+ @tableName+' where DATEDIFF(day,cast(substring(FileDate,1,8)+'' ''+SUBSTRING(filedate,9,2)+'':''+SUBSTRING(filedate,11,2)+'':''+SUBSTRING(filedate,13,2)+''.''+SUBSTRING(filedate,15,3) as datetime),GETDATE()) > '+@amountOfDaysToKeep
--select @sql
exec sp_executesql @sql
set @amountOfRecordsDeleted = @@ROWCOUNT

--Record how many records were deleted
if @amountOfRecordsDeleted > 1
insert into [Ctrl_PurgeHistory] (TableName, PurgeDate, AmountOfRecordsDeleted) values (@tableName, cast(GETDATE() as datetime2), @amountOfRecordsDeleted)
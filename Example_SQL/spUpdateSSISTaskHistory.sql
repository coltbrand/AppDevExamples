SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Colton Zimmerman
-- Create date: 11/18/2021
-- Description:	Updates rpt_SSISTaskHistory for use in audit reporting
-- =============================================
CREATE PROCEDURE sp_UpdateSSISTaskHistory
	@TaskName varchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	Declare @OpenCount int
	SET @OpenCount = (select count(*) from rpt_SSISTaskHistory where TaskName = @TaskName and DATEDIFF(hour,StartTime,GETDATE()) < 24 and EndTime is null)
	if(@OpenCount = 1) update rpt_SSISTaskHistory set EndTime = GETDATE() where TaskName = @TaskName and EndTime is null;
	if(@OpenCount = 0) insert into rpt_SSISTaskHistory values (@TaskName, GETDATE(), null)
END
GO

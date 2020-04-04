
IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_VW_FE_PE_ORIN_R' AND type = 'V')
   DROP VIEW VID_VW_FE_PE_ORIN_R
GO--

CREATE VIEW [dbo].[VID_VW_FE_PE_ORIN_R]
AS

	 SELECT ISNULL(O0.U_BPP_MDTD,
				CASE O0.DocSubType
				 WHEN '--' THEN '01'
				 WHEN 'IX' THEN '01'
				 WHEN 'IB' THEN '03'
				 WHEN 'DN' THEN '08'
				END)					[documentType]
		   ,O0.U_BPP_MDSD +'-'+ O0.U_BPP_MDCD	[referencedFolio]
		   ,O0.TaxDate			[date]
		   ,ISNULL((SELECT Name 'Name' FROM [@FM_NOTES] WHERE Code = T0.U_BPP_MDTN), CAST(O0.DocNum AS VARCHAR(20)))			[description]
		   ,T0.DocEntry
		   ,T0.ObjType
		   ,REPLACE(O0.U_BPP_MDSD,'-','') [SerieO]
	  FROM ORIN T0
	  JOIN RIN1 T1 ON T1.DocEntry = T0.DocEntry
      JOIN OINV O0 ON O0.DocEntry = T1.BaseEntry
	              AND O0.ObjType = T1.BaseType
	  JOIN NNM1 N0 ON N0.Series = T0.Series
	              AND N0.ObjectCode = T0.ObjType
     WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
	   AND ISNULL(O0.U_BPP_MDSD,'') <> ''
	   AND ISNULL(O0.U_BPP_MDCD,'') <> ''
	   AND O0.DocSubType IN ('--','IB','IX','DN')
	 GROUP BY O0.U_BPP_MDTD, O0.DocNum, O0.TaxDate, O0.DocSubType, O0.U_BPP_MDSD, O0.U_BPP_MDCD, T0.DocEntry, T0.ObjType, T0.U_BPP_MDTN

	UNION ALL
	SELECT  T0.U_BPP_MDTO	[documentType] --opciones 01 - 03 - 08 (tipo documento)
		   ,T0.U_BPP_MDSO + '-' + T0.U_BPP_MDCO	[referencedFolio]
		   ,T0.TaxDate			[date]
		   ,ISNULL((SELECT Name 'Name' FROM [@FM_NOTES] WHERE Code = T0.U_BPP_MDTN), CAST(T0.DocNum AS VARCHAR(20)))			[description]
		   ,T0.DocEntry
		   ,T0.ObjType
		   ,REPLACE(T0.U_BPP_MDSO,'-','') [SerieO]
	  FROM ORIN T0
	  JOIN NNM1 N0 ON N0.Series = T0.Series
	              AND N0.ObjectCode = T0.ObjType
	 WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
	   AND T0.DocEntry NOT IN (SELECT DocEntry FROM RIN1 WHERE DocEntry = T0.DocEntry AND BaseType = '13')
	
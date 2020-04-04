IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera09R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera09R
GO--
CREATE PROCEDURE VID_SP_FE_IN_ED_PE_Genera09R @DocEntry Integer, @TipoDoc VarChar(10), @ObjType VarChar(10)
AS
BEGIN

	SELECT --CAST(ROW_NUMBER() OVER(ORDER BY documentType) AS INT) [position]
		   T0.NroDocumento 		'NroDocumento'
		  ,T0.TipoDocumento 	'TipoDocumento'
	  FROM VID_VW_FE_PE_GUIA_R T0
	 WHERE T0.DocEntry = @DocEntry
	   AND T0.ObjType = @ObjType
END
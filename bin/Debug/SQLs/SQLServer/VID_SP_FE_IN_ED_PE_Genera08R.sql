IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera08R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera08R
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_IN_ED_PE_Genera08R]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
AS
BEGIN
	SELECT --CAST(ROW_NUMBER() OVER(ORDER BY documentType) AS INT) [position]
		   T0.[referencedFolio] 'nroReferencia'
		  ,T0.[documentType] 'Tipo'
		  --,T0.[date]
		  ,T0.[description] 'Descripcion'
	  FROM VID_VW_FE_PE_OINV_R T0
	 WHERE T0.DocEntry = @DocEntry
	   AND T0.ObjType = @ObjType
END



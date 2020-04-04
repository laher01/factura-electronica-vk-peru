IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera07R' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera07R
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_IN_ED_PE_Genera07R]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
AS
BEGIN

		SELECT --CAST(ROW_NUMBER() OVER(ORDER BY documentType) AS INT) [position]
			   CASE WHEN LEFT(T0.SerieO,1) NOT IN ('F','B') AND LEN(T0.SerieO) = 4
		          THEN T0.referencedFolio
		        WHEN T0.documentType  = '03' 
		          THEN CASE WHEN LEFT(T0.referencedFolio,1) = 'B' THEN T0.referencedFolio
		                    ELSE 'B' + T0.referencedFolio END 
	            ELSE CASE WHEN LEFT(T0.referencedFolio,1) = 'F' THEN T0.referencedFolio
	                      ELSE 'F' + T0.referencedFolio END 
	       END 'nroReferencia'
			  ,T0.[documentType] 'Tipo'
			  ,T0.[description] 'Descripcion'
		  FROM VID_VW_FE_PE_ORIN_R T0
	     WHERE T0.DocEntry = @DocEntry
		   AND T0.ObjType = @ObjType
END



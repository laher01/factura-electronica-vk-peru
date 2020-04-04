--DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera09R
CREATE PROCEDURE VID_SP_FE_IN_ED_PE_Genera09R
(
     IN DocEntry	Integer
    ,IN TipoDoc		VarChar(10)
    ,IN ObjType		VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN

	SELECT --CAST(ROW_NUMBER() OVER(ORDER BY documentType) AS INT) [position]
		   T0."NroDocumento" 		"NroDocumento"
		  ,T0."TipoDocumento" 		"TipoDocumento"
	  FROM "VID_VW_FE_PE_GUIA_R" T0
	 WHERE T0."DocEntry" = :DocEntry
	   AND T0."ObjType" = :ObjType;
END;
--DROP VIEW VID_VW_FE_PE_GUIA_R
CREATE VIEW VID_VW_FE_PE_GUIA_R
AS
	--Referencia a Guias de Remision - Opcion "Copiar a" o "Copiar de"
	SELECT	DISTINCT CASE WHEN LEFT(T5."U_BPP_MDSD",1) = 'F' THEN T5."U_BPP_MDSD" || '-' || T5."U_BPP_MDCD" ELSE 'F' || T5."U_BPP_MDSD" || '-' || T5."U_BPP_MDCD" END  "NroDocumento",
			T5."U_BPP_MDTD" "TipoDocumento" ,
			T0."DocEntry" ,
			T0."ObjType" 
	FROM "ODLN" T0 
	JOIN "DLN1" T1 ON T1."DocEntry" = T0."DocEntry"
	JOIN "OINV" T5 ON T5."DocEntry" = T1."BaseEntry"
	            AND T5."ObjType" = T1."BaseType"
	JOIN "NNM1" T2 ON T2."Series" = T0."Series" 
	WHERE 1 = 1

	 
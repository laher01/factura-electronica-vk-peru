--DROP VIEW VID_VW_FE_PE_OINV_R
CREATE VIEW VID_VW_FE_PE_OINV_R
AS
	--Referencia a Guias de Remision - Opcion "Copiar a" o "Copiar de"
	SELECT  '09'				"documentType"
		   ,G0."U_BPP_MDSD" || '-' || G0."U_BPP_MDCD"	"referencedFolio"
		   ,G0."TaxDate"			"date"
		   ,CAST(G0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "OINV" T0
	  JOIN "INV1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "ODLN" G0 ON G0."DocEntry" = T1."BaseEntry"
	              AND G0."ObjType" = T1."BaseType"
      JOIN "NNM1" T2 ON T2."Series" = G0."Series"
     WHERE IFNULL(G0."U_BPP_MDSD",'') <> '' 
	   AND IFNULL(G0."U_BPP_MDCD",'') <> ''
	   AND IFNULL(T1."BaseType",-1) = 15
	 GROUP BY T0."FolioNum", G0."U_BPP_MDCD", G0."U_BPP_MDSD", G0."TaxDate", G0."DocNum", T0."DocEntry", T0."ObjType"

	 UNION
	 --Referencia a Guia - Opcion "Copiar a" o "Copiar de" (Orden de venta por medio de la guia)
	SELECT '91'				"documentType"
		   ,IFNULL(O0."NumAtCard",'--')	"referencedFolio"
		   ,O0."TaxDate"			"date"
		   ,CAST(O0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "OINV" T0
	  JOIN "INV1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "DLN1" G1 ON G1."DocEntry" = T1."BaseEntry"
                  AND G1."ObjType" = T1."BaseType"
                  AND G1."LineNum" = T1."BaseLine"
      JOIN "ODLN" G0 ON G0."DocEntry" = G1."DocEntry"
	  JOIN "RDR1" O1 ON O1."DocEntry" = G1."BaseEntry"
                  AND O1."ObjType" = G1."BaseType"
                  AND O1."LineNum" = G1."BaseLine"
	  JOIN "ORDR" O0 ON O0."DocEntry" = O1."DocEntry"
	  JOIN "NNM1" ON G0."Series" = NNM1."Series"
     WHERE IFNULL(T0."FolioNum",0) <> 0
	   AND IFNULL(G1."BaseType",-1) = 17
	   AND IFNULL(O0."NumAtCard",'--') <> ''
	 GROUP BY T0."FolioNum", O0."DocNum", O0."TaxDate", O0."NumAtCard", T0."DocEntry", T0."ObjType"

	 UNION
	--Referencia a Orden de Venta - Opcion "Copiar a" o "Copiar de"
	SELECT  '91'				"documentType"
		   ,IFNULL(O0."NumAtCard",'--')	"referencedFolio"
		   ,O0."TaxDate"			"date"
		   ,CAST(O0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "OINV" T0
	  JOIN "INV1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "ORDR" O0 ON O0."DocEntry" = T1."BaseEntry"
     WHERE IFNULL(T0."FolioNum",0) <> 0
	   AND IFNULL(T1."BaseType",-1) = 17
	 GROUP BY T0."FolioNum", O0."DocNum", O0."TaxDate", O0."NumAtCard", T0."DocEntry", T0."ObjType"

	 --FACTURAS DE ANTICIPO
	 UNION ALL
	 --Referencia a Guias de Remision - Opcion "Copiar a" o "Copiar de"
	SELECT  '09'				"documentType"
		   ,G0."U_BPP_MDSD" || '-' || G0."U_BPP_MDCD"	"referencedFolio"
		   ,G0."TaxDate"			"date"
		   ,CAST(G0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "ODPI" T0
	  JOIN "DPI1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "ODLN" G0 ON G0."DocEntry" = T1."BaseEntry"
	              AND G0."ObjType" = T1."BaseType"
      JOIN "NNM1" T2 ON T2."Series" = G0."Series"
     WHERE IFNULL(G0."U_BPP_MDSD",'') <> '' 
	   AND IFNULL(G0."U_BPP_MDCD",'') <> ''
	   AND IFNULL(T1."BaseType",-1) = 15
	 GROUP BY T0."FolioNum", G0."U_BPP_MDCD", G0."U_BPP_MDSD", G0."TaxDate", G0."DocNum", T0."DocEntry", T0."ObjType"

	 UNION
	 --Referencia a Guia - Opcion "Copiar a" o "Copiar de" (Orden de venta por medio de la guia)
	SELECT '91'				"documentType"
		   ,IFNULL(O0."NumAtCard",'--')	"referencedFolio"
		   ,O0."TaxDate"			"date"
		   ,CAST(O0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "ODPI" T0
	  JOIN "DPI1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "DLN1" G1 ON G1."DocEntry" = T1."BaseEntry"
                  AND G1."ObjType" = T1."BaseType"
                  AND G1."LineNum" = T1."BaseLine"
      JOIN "ODLN" G0 ON G0."DocEntry" = G1."DocEntry"
	  JOIN "RDR1" O1 ON O1."DocEntry" = G1."BaseEntry"
                  AND O1."ObjType" = G1."BaseType"
                  AND O1."LineNum" = G1."BaseLine"
	  JOIN "ORDR" O0 ON O0."DocEntry" = O1."DocEntry"
	  JOIN "NNM1" ON G0."Series" = NNM1."Series"
     WHERE IFNULL(T0."FolioNum",0) <> 0
	   AND IFNULL(G1."BaseType",-1) = 17
	 GROUP BY T0."FolioNum", O0."DocNum", O0."TaxDate", O0."NumAtCard", T0."DocEntry", T0."ObjType"

	 UNION
	--Referencia a Orden de Venta - Opcion "Copiar a" o "Copiar de"
	SELECT  '91'				"documentType"
		   ,IFNULL(O0."NumAtCard",'--')	"referencedFolio"
		   ,O0."TaxDate"			"date"
		   ,CAST(O0."DocNum" AS VARCHAR(50))			"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "ODPI" T0
	  JOIN "DPI1" T1 ON T1."DocEntry" = T0."DocEntry"
	  JOIN "ORDR" O0 ON O0."DocEntry" = T1."BaseEntry" 
     WHERE IFNULL(T0."FolioNum",0) <> 0
	   AND IFNULL(T1."BaseType",-1) = 17
	 GROUP BY T0."FolioNum", O0."DocNum", O0."TaxDate", O0."NumAtCard", T0."DocEntry", T0."ObjType"
	 
	 UNION
	 --para nota de debito
	 SELECT  T0."U_BPP_MDTO"	"documentType" --opciones 01 - 03 - 08 (tipo documento)
		   ,T0."U_BPP_MDSO" || '-' || T0."U_BPP_MDCO"	"referencedFolio"
		   ,T0."TaxDate"			"date"
		   ,IFNULL((SELECT "Name" "Name" FROM "@FM_NOTES" WHERE "Code" = T0."U_BPP_MDTN"), CAST(T0."DocNum" AS VARCHAR(20)))	"description"
		   ,T0."DocEntry"
		   ,T0."ObjType"
	  FROM "OINV" T0
	  JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	              AND N0."ObjectCode" = T0."ObjType"
	 WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'
	   AND T0."DocSubType" = 'DN'
	   AND T0."U_BPP_MDSO" IS NOT NULL
	   AND T0."U_BPP_MDCO" IS NOT NULL;
	   
	   
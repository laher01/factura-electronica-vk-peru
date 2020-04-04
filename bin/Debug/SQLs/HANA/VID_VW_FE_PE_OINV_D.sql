--DROP VIEW VID_VW_FE_PE_OINV_D
CREATE VIEW VID_VW_FE_PE_OINV_D
AS
	SELECT  T0."DocType"							"DocType"
	       ,IFNULL(T1."ItemCode", 'Servicio')		"code"
		   ,IFNULL(LEFT(T1."Dscription",200),'Servicio')	"name"
		   ,/*CASE WHEN T0.DocType = 'S' THEN 'NIU'
		         ELSE ISNULL((SELECT U_UMISO
		                        FROM [@FM_UMISO] 
					           WHERE U_UMBASE = I0.SalUnitMsr),'')	
			END*/'NIU'									"unit"
		   ,T1."Price"							"price"
		   ,IFNULL((SELECT TO_VARCHAR("Name") 
		              FROM "@FM_IVA" 
					 WHERE "Code" = T1."TaxCode"),'10')	"exemptType"
		   ,CASE WHEN T0."DocType" = 'S' THEN 1
		         ELSE T1."Quantity"
			END									"quantity"
		   ,CASE WHEN T0."DocType" <> 'S' THEN T1."ItemCode"
		         ELSE T1."Dscription"
		    END									"description"
		   ,T1."Dscription"						"longDescription"
		   ,T1."VisOrder"						"LineaOrden"
		   ,1									"LineaOrden2"
		   ,T0."DocEntry"
		   ,T0."ObjType"
		   ,IFNULL((SELECT SUM(CASE WHEN A0."TaxSumFrgn" <> 0.0 THEN A0."TaxSumFrgn" ELSE A0."TaxSum" END)
		              FROM "INV4" A0
					  JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					 WHERE A0."DocEntry" = T0."DocEntry"
					   AND "LineNum" = T1."LineNum"
					   AND TO_VARCHAR(A1."Name") = '10'), 0.0)	"Gravadas"
		   ,0.0									"Gratuitas"
		   ,IFNULL((SELECT SUM(CASE WHEN A0."BaseSumFrg" <> 0.0 THEN A0."BaseSumFrg" ELSE A0."BaseSum" END)
		              FROM "INV4" A0
					  JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					 WHERE A0."DocEntry" = T0."DocEntry"
					   AND "LineNum" = T1."LineNum"
					   AND TO_VARCHAR("Name") IN ('20', '40')), 0.0)	"Inafectas"
		   ,0.0									"Exoneradas"
		   ,T1."TaxCode"
	  FROM "OINV" T0
	  JOIN "INV1" T1 ON T1."DocEntry" = T0."DocEntry"
	  LEFT JOIN "OITM" I0 ON I0."ItemCode" = T1."ItemCode"
	  JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	  JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	              AND N0."ObjectCode" = T0."ObjType"
	 WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'
	   
	UNION
	--lineas de Texto
	SELECT T0."DocType"										"DocType"
	       ,'TEXTO'											"code"
		   ,LEFT(CAST(T10."LineText" AS VARCHAR(300)), 200)	"name"
		   ,'NIU'											"unit"
		   ,0.0												"price"
		   ,IFNULL((SELECT TO_VARCHAR("Name") 
		              FROM "@FM_IVA"
					 WHERE "Code" = 'IGV_EXE'),'10')		"exemptType"
		   ,1												"quantity"
		   ,'Texto'											"description"
		   ,LEFT(CAST(T10."LineText" AS VARCHAR(300)), 250)	"longDescription"
		   ,T10."AftLineNum"						"LineaOrden"
		   ,2										"LineaOrden2"
		   ,T0."DocEntry"
		   ,T0."ObjType"
		   ,0.0									"Gravadas"
		   ,0.0									"Gratuitas"
		   ,0.0									"Inafectas"
		   ,0.0									"Exoneradas"
		   ,''									"TaxCode"
	  FROM "OINV" T0
	  JOIN "INV10" T10 ON T10."DocEntry" = T0."DocEntry"
	  JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	  JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	              AND N0."ObjectCode" = T0."ObjType"
	 WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'

	 --FACTURAS DE ANTICIPO
	 UNION
	 SELECT  T0."DocType"									"DocType"
	       ,IFNULL(T1."ItemCode",'Servicio')				"code"
		   ,IFNULL(LEFT(T1."Dscription",200),'Servicio')	"name"
		   ,/*CASE WHEN T0.DocType = 'S' THEN 'NIU'
		         ELSE ISNULL((SELECT U_UMISO
		                        FROM [@FM_UMISO] 
					           WHERE U_UMBASE = I0.SalUnitMsr),'')	
			END*/'NIU'										"unit"
		   ,T1."Price"										"price"
		   ,IFNULL((SELECT TO_VARCHAR("Name") 
		              FROM "@FM_IVA" 
					 WHERE "Code" = T1."TaxCode"),'10')	"exemptType"
		   ,CASE WHEN T0."DocType" = 'S' THEN 1
		         ELSE T1."Quantity"
			END									"quantity"
		   ,CASE WHEN T0."DocType" <> 'S' THEN T1."ItemCode"
		         ELSE T1."Dscription"
		    END									"description"
		   ,T1."Dscription"						"longDescription"
		   ,T1."VisOrder"						"LineaOrden"
		   ,1									"LineaOrden2"
		   ,T0."DocEntry"
		   ,T0."ObjType"
		   ,IFNULL((SELECT SUM(CASE WHEN A0."TaxSumFrgn" <> 0.0 THEN A0."TaxSumFrgn" ELSE A0."TaxSum" END)
		              FROM "DPI4" A0
					  JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					 WHERE A0."DocEntry" = T0."DocEntry"
					   AND "LineNum" = T1."LineNum"
					   AND TO_VARCHAR("Name") = '10'), 0.0)	"Gravadas"
		   ,0.0									"Gratuitas"
		   ,IFNULL((SELECT SUM(CASE WHEN A0."BaseSumFrg" <> 0 THEN A0."BaseSumFrg" ELSE A0."BaseSum" END)
		              FROM "DPI4" A0
					  JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					 WHERE A0."DocEntry" = T0."DocEntry"
					   AND "LineNum" = T1."LineNum"
					   AND TO_VARCHAR("Name") IN ('20', '40')), 0.0)	"Inafectas"
		   ,0.0									"Exoneradas"
		   ,T1."TaxCode"						"TaxCode"
	  FROM "ODPI" T0
	  JOIN "DPI1" T1 ON T1."DocEntry" = T0."DocEntry"
	  LEFT JOIN "OITM" I0 ON I0."ItemCode" = T1."ItemCode"
	  JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	  JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	              AND N0."ObjectCode" = T0."ObjType"
	 WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'
	   
	UNION
	--lineas de Texto
	SELECT T0."DocType"										"DocType"
	       ,'TEXTO'											"code"
		   ,LEFT(CAST(T10."LineText" AS VARCHAR(300)), 200)	"name"
		   ,'NIU'											"unit"
		   ,0.0												"price"
		   ,IFNULL((SELECT TO_VARCHAR("Name") 
		              FROM "@FM_IVA" 
					 WHERE "Code" = 'IGV_EXE'),'10')		"exemptType"
		   ,1												"quantity"
		   ,'Texto'											"description"
		   ,LEFT(CAST(T10."LineText" AS VARCHAR(300)), 250)	"longDescription"
		   ,T10."AftLineNum"					"LineaOrden"
		   ,2									"LineaOrden2"
		   ,T0."DocEntry"
		   ,T0."ObjType"
		   ,0.0									"Gravadas"
		   ,0.0									"Gratuitas"
		   ,0.0									"Inafectas"
		   ,0.0									"Exoneradas"
		   ,''									"TaxCode"
	  FROM "ODPI" T0
	  JOIN "DPI10" T10 ON T10."DocEntry" = T0."DocEntry"
	  JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	  JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	              AND N0."ObjectCode" = T0."ObjType"
	 WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E';
	   
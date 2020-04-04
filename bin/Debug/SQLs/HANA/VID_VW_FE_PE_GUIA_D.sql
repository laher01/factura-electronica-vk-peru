--DROP VIEW VID_VW_FE_PE_GUIA_D
CREATE VIEW VID_VW_FE_PE_GUIA_D
AS
	SELECT  T0."DocType" "DocType" ,
			IFNULL(T1."ItemCode",'Servicio') "ItemCode" ,
			IFNULL(LEFT(T1."Dscription",200),'Servicio') "Descripcion" ,
			'NIU' "UnidadMedida" ,
			T1."Price" "price" ,
			IFNULL((SELECT TO_VARCHAR("Name") 
					  FROM "@FM_IVA" 
					 WHERE "Code" = T1."TaxCode"),'10') "exemptType" ,
			CASE WHEN T0."DocType" = 'S' THEN 1 
				ELSE T1."Quantity" 
			END "Cantidad" ,
			IFNULL(T0."U_BPP_MDCD",'1') "Correlativo" ,
			TO_VARCHAR(T1."VisOrder" + 1) "LineaReferencia" ,
			T1."VisOrder" "LineaOrden" ,
			1 "LineaOrden2" ,
			T0."DocEntry" ,
			T0."ObjType" ,
			IFNULL((SELECT SUM(CASE WHEN A0."TaxSumFrgn" <> 0.0 THEN A0."TaxSumFrgn" 
									ELSE A0."TaxSum" 
								END) 
					FROM "DLN4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode" 
					WHERE A0."DocEntry" = T0."DocEntry" 
					AND "LineNum" = T1."LineNum" 
					AND TO_VARCHAR(A1."Name") = '10'),0.0) "Gravadas" ,
			0.0 "Gratuitas" ,
			IFNULL((SELECT SUM(CASE WHEN A0."BaseSumFrg" <> 0.0 THEN A0."BaseSumFrg" 
									ELSE A0."BaseSum" 
								END) 
					FROM "DLN4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode" 
					WHERE A0."DocEntry" = T0."DocEntry" 
					AND "LineNum" = T1."LineNum" 
					AND TO_VARCHAR("Name") IN ('20','40')),0.0) "Inafectas" ,
			0.0 "Exoneradas" ,
			T1."TaxCode" 
	FROM "ODLN" T0 
	JOIN "DLN1" T1 ON T1."DocEntry" = T0."DocEntry" 
	LEFT JOIN "OITM" I0 ON I0."ItemCode" = T1."ItemCode" 
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign" 
	JOIN "NNM1" N0 ON N0."Series" = T0."Series" 
	AND N0."ObjectCode" = T0."ObjType" 
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'	   
--Devolucion de compra
	UNION
	SELECT  T0."DocType" "DocType",
			IFNULL(T1."ItemCode",'Servicio') "ItemCode",
			IFNULL(LEFT(T1."Dscription",200),'Servicio') "Descripcion",
			'NIU' "UnidadMedida",
			T1."Price" "price",
			IFNULL((SELECT CAST("Name" AS VARCHAR(30)) 
					  FROM "@FM_IVA" 
					 WHERE "Code" = T1."TaxCode"),'10') "exemptType",
			CASE WHEN T0."DocType" = 'S' THEN 1 
				ELSE T1."Quantity"
			END "Cantidad",
			IFNULL(T0."U_BPP_MDCD",'1')  "Correlativo", 
			CAST(T1."VisOrder" + 1 AS VARCHAR(30))  "LineaReferencia",
			T1."VisOrder" "LineaOrden",
			1 "LineaOrden2",
			T0."DocEntry",
			T0."ObjType",
			IFNULL((SELECT SUM(CASE WHEN A0."TaxSumFrgn" <> 0.0 THEN A0."TaxSumFrgn"
									ELSE A0."TaxSum"
								END) 
					FROM "RPD4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					WHERE A0."DocEntry" = T0."DocEntry"
					AND "LineNum" = T1."LineNum" 
					AND CAST(A1."Name" AS VARCHAR(30)) = '10'),0.0) "Gravadas",
			0.0 "Gratuitas",
			IFNULL((SELECT SUM(CASE WHEN A0."BaseSumFrg" <> 0.0 THEN A0."BaseSumFrg"
									ELSE A0."BaseSum"
								END) 
					FROM "RPD4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					WHERE A0."DocEntry" = T0."DocEntry"
					AND "LineNum" = T1."LineNum"
					AND CAST("Name" AS VARCHAR(30)) IN ('20','40')),0.0) "Inafectas",
			0.0 "Exoneradas",
			T1."TaxCode"
	FROM "ORPD" T0 
	JOIN "RPD1" T1 ON T1."DocEntry" = T0."DocEntry"
	LEFT JOIN "OITM" I0 ON I0."ItemCode" = T1."ItemCode"
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	AND N0."ObjectCode" = T0."ObjType" 
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E'

	--Transferencia Stock
	UNION
	SELECT  T0."DocType" "DocType",
			IFNULL(T1."ItemCode",'Servicio') "ItemCode",
			IFNULL(LEFT(T1."Dscription",200),'Servicio') "Descripcion",
			'NIU' "UnidadMedida",
			T1."Price" "price",
			IFNULL((SELECT CAST("Name" AS VARCHAR(30)) 
					  FROM "@FM_IVA"
					 WHERE "Code" = T1."TaxCode"),'10') "exemptType",
			CASE WHEN T0."DocType" = 'S' THEN 1 
				ELSE T1."Quantity"
			END "Cantidad",
			IFNULL(T0."U_BPP_MDCD",'1')  "Correlativo", 
			CAST(T1."VisOrder" + 1 AS VARCHAR(30))  "LineaReferencia",
			T1."VisOrder" "LineaOrden",
			1 "LineaOrden2",
			T0."DocEntry",
			T0."ObjType",
			IFNULL((SELECT SUM(CASE WHEN A0."TaxSumFrgn" <> 0.0 THEN A0."TaxSumFrgn"
									ELSE A0."TaxSum"
								END) 
					FROM "WTR4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					WHERE A0."DocEntry" = T0."DocEntry"
					AND "LineNum" = T1."LineNum"
					AND CAST(A1."Name" AS VARCHAR(30)) = '10'),0.0) "Gravadas",
			0.0 "Gratuitas",
			IFNULL((SELECT SUM(CASE WHEN A0."BaseSumFrg" <> 0.0 THEN A0."BaseSumFrg"
									ELSE A0."BaseSum"
								END) 
					FROM "WTR4" A0 JOIN "@FM_IVA" A1 ON A1."Code" = A0."StaCode"
					WHERE A0."DocEntry" = T0."DocEntry"
					AND "LineNum" = T1."LineNum"
					AND CAST("Name" AS VARCHAR(30)) IN ('20','40')),0.0) "Inafectas",
			0.0 "Exoneradas",
			T1."TaxCode"
	FROM "OWTR" T0 
	JOIN "WTR1" T1 ON T1."DocEntry" = T0."DocEntry"
	LEFT JOIN "OITM" I0 ON I0."ItemCode" = T1."ItemCode"
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign"
	JOIN "NNM1" N0 ON N0."Series" = T0."Series"
	AND N0."ObjectCode" = T0."ObjType"
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E';

	
--DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera20D;
CREATE PROCEDURE VID_SP_FE_IN_ED_PE_Genera20D
(
     IN DocEntry	Integer
    ,IN TipoDoc		VarChar(10)
    ,IN ObjType		VarChar(10)
)
LANGUAGE SqlScript
AS
BEGIN
			  SELECT 
			          IFNULL((CASE T2."InvType"
					              WHEN '19' THEN (SELECT CASE WHEN LEFT(A."U_BPP_MDSD", 1) = 'F' THEN A."U_BPP_MDSD" || '-' || A."U_BPP_MDCD" ELSE 'F' || A."U_BPP_MDSD" || '-' || A."U_BPP_MDCD" END
													FROM "ORPC" A
												   WHERE A."DocEntry" = T2."DocEntry"
												     AND A."ObjType" = T2."InvType")
								  ELSE (SELECT CASE WHEN LEFT(A."U_BPP_MDSD", 1) = 'F' THEN A."U_BPP_MDSD" || '-' || A."U_BPP_MDCD" ELSE 'F' || A."U_BPP_MDSD" || '-' || A."U_BPP_MDCD" END
								          FROM "OPCH" A
										 WHERE A."DocEntry" = T2."DocEntry"
										   AND A."ObjType" = T2."InvType")
							   END),'')	"NroDocumento"
					  ,IFNULL((CASE T2."InvType"
					              WHEN '19' THEN (SELECT "U_BPP_MDTD"
													FROM "ORPC" A
												   WHERE A."DocEntry" = T2."DocEntry"
												     AND A."ObjType" = T2."InvType")
								  ELSE (SELECT "U_BPP_MDTD"
								          FROM "OPCH" A
										 WHERE A."DocEntry" = T2."DocEntry"
										   AND A."ObjType" = T2."InvType")
							   END),'')	"TipoDocumento"
					  ,TO_VARCHAR(J0."RefDate",'yyyy-MM-dd')	"FechaEmision"
					  ,CASE WHEN T2."AppliedFC" > 0 THEN T2."AppliedFC" + T2."vatAppldFC"
					        ELSE T2."SumApplied" + T2."vatApplied" END	"ImporteTotal"
					  ,IFNULL(M0."DocCurrCod",'PEN')	"MonedaDocumentoRelacionado"
					  --,CAST(ROW_NUMBER() OVER(ORDER BY T2."InvoiceId") AS INT)	"NumeroPago"
					  ,1 "NumeroPago"
					  ,CASE WHEN T2."AppliedFC" > 0 THEN T2."AppliedFC" ELSE T2."SumApplied" END	"ImporteTotalNeto"
					  ,CASE WHEN T2."AppliedFC" > 0 THEN (T2."AppliedFC" + T2."vatAppldFC") - T6."WTSumFC" ELSE (T2."SumApplied" + T2."vatApplied") - T6."WTSum" END	"ImporteSinRetencion"
					  ,TO_VARCHAR(T0."DocDate",'yyyy-MM-dd')	"FechaPago"
					  ,CASE WHEN T6."WTSumFC" > 0 THEN T6."WTSumFC" ELSE T6."WTSum" END		"ImporteRetencion"
					  ,TO_VARCHAR(T0."DocDate",'yyyy-MM-dd')	"FechaRetencion"
					  ,CASE WHEN IFNULL(M0."DocCurrCod",'PEN') <> 'PEN' THEN T0."DocRate" ELSE 1.0 END	"TipoCambio"
					  ,TO_VARCHAR(T0."DocDate",'yyyy-MM-dd')	"FechaTipoCambio"
				 FROM "OVPM" T0
				 JOIN "VPM2" T2 ON T2."DocNum" = T0."DocEntry"
				 JOIN "VPM6" T6 ON T6."DocNum" = T0."DocEntry"
				             AND T6."InvoiceId" = T2."InvoiceId"
			     JOIN OJDT J0 ON J0."TransId" = T2."DocTransId"
				 ,"OCRN" M0
				 ,"OADM" A0
				WHERE T0."DocEntry" = :DocEntry
				  AND T0."ObjType" = :ObjType
				  AND M0."CurrCode" = IFNULL(J0."TransCurr", A0."MainCurncy")
				ORDER BY  T2."InvoiceId";
END;
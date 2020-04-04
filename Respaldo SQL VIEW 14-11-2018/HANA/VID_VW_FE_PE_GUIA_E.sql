--DROP VIEW VID_VW_FE_PE_GUIA_E
ALTER VIEW VID_VW_FE_PE_GUIA_E
AS
	--Entrega
	SELECT	IFNULL(M0."DocCurrCod",'PEN') "currency" ,
			T0."TaxDate" "TaxDate" ,
			T0."U_BPP_MDSD" "series" ,
			T0."U_BPP_MDSD" || '-' || T0."U_BPP_MDCD" "externalFolio" ,
			V0."SlpName" "sellerCode" ,
			T0."LicTradNum" "LicTradNum" ,
			T0."CardName" "CardName" ,
			IFNULL(T12."StreetS",'') || ', '|| IFNULL(T12."BlockS",'') || ', ' || IFNULL(T12."CityS",'') "address" ,
			IFNULL(T12."StreetNoS",'') "LlegadaUbigeo",
			IFNULL(C2."U_BPP_CODI",'') "municipality" ,
			IFNULL(C1."FirstName",'') "contact" ,
			IFNULL(C0."Phone1",'') "phone" ,
			IFNULL(C0."E_Mail",'') "email" ,
			T0."DocDueDate" "expirationDate" ,
			T0."DocDueDate" "datePayment" ,
			CASE WHEN T0."DocTotalFC" > 0 THEN T0."DocTotalFC" 
				ELSE T0."DocTotal" 
			END "amountPayment" ,
			T5."PymntGroup" "descriptionPayment" ,
			'' "estimateNumber" ,
			'' "account" ,
			'' "reference" ,
			'' "project" ,
			'' "certificateNumber" ,
			'' "contactP" ,
			'' "gloss" ,
			IFNULL(C0."U_BPP_BPTD",'') "identityDocumentType" ,
			T0."DocType" ,
			T0."DocEntry" ,
			T0."ObjType" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."DiscSumFC" 
				ELSE T0."DiscSum" 
			END "DiscSum" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."VatSumFC" 
				ELSE T0."VatSum" 
			END "VatSum" ,
			M0."CurrName" "CurrName" ,
			IFNULL(T0."U_BPP_MDCT",'') "CodigoTransp" ,
			IFNULL(T0."U_BPP_MDRT",'') "RUCTransp" ,
			IFNULL(T0."U_BPP_MDNT",'') "NombreTransp" ,
			IFNULL(T0."U_BPP_MDFC",'') "LicenciaConductor" ,
			IFNULL(VEH."U_BPP_VEPL",'') "PlacaVehiculo" ,
			IFNULL(T0."U_BPP_MDVN",'') "MarcaVehiculo" ,
			IFNULL(CON."Name",'') "NombreConductor" ,
			'0' "Transbordo" ,
			0.0 "PesoBrutoTotal" ,
			0.0 "NroPallets" ,
			IFNULL(T0."U_BPP_MDMT",'01') "ModalidadTraslado" ,
			T0."TaxDate" "FechaInicioTraslado" ,
			T0."U_BPP_MDMT" "CodigoMotivoTraslado" ,
			IFNULL((SELECT "U_BPP_MTDT" 
					FROM "@BPP_MOTTRA" 
					WHERE "Code" = T0."U_BPP_MDMT"),'') "DescripcionMotivo" ,
			'' "NumeroContenedor" ,
			'' "CodigoPuerto" ,
			IFNULL(T0."Comments",'') "Comentario" 
	FROM "ODLN" T0 
	JOIN "OCTG" T5 ON T5."GroupNum" = T0."GroupNum" 
	JOIN "DLN12" T12 ON T12."DocEntry" = T0."DocEntry" 
	JOIN "OCRD" C0 ON C0."CardCode" = T0."CardCode" 
	JOIN "OSLP" V0 ON V0."SlpCode" = T0."SlpCode" 
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign" 
	LEFT JOIN "OCRN" M0 ON M0."CurrCode" = T0."DocCur" 
	LEFT JOIN "OCPR" C1 ON C1."CardCode" = T0."CardCode" 
						AND C1."CntctCode" = T0."CntctCode" 
	LEFT JOIN "CRD1" C2 ON C2."CardCode" = T0."CardCode" 
						AND C2."Address" = T0."PayToCode" 
						AND C2."AdresType" = 'S' 
	JOIN "NNM1" N0 ON N0."Series" = T0."Series" 
	LEFT JOIN "@BPP_CONDUC" CON ON CON."Code" = T0."U_BPP_MDFN" 
	LEFT JOIN "@BPP_VEHICU" VEH ON VEH."Code" = T0."U_BPP_MDVC" ,
	 "OADM" A0 
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E' 
	AND T0."CANCELED" = 'N'
	
	--Devolucion Compra
	UNION
	SELECT	IFNULL(M0."DocCurrCod",'PEN') "currency" ,
			T0."TaxDate" "TaxDate" ,
			T0."U_BPP_MDSD" "series" ,
			T0."U_BPP_MDSD" || '-' || T0."U_BPP_MDCD" "externalFolio" ,
			V0."SlpName" "sellerCode" ,
			T0."LicTradNum" "LicTradNum" ,
			T0."CardName" "CardName" ,
			IFNULL(T12."StreetS",'') || ', '|| IFNULL(T12."BlockS",'') || ', ' || IFNULL(T12."CityS",'') "address" ,
			IFNULL(T12."StreetNoS",'') "LlegadaUbigeo",
			IFNULL(C2."U_BPP_CODI",'') "municipality" ,
			IFNULL(C1."FirstName",'') "contact" ,
			IFNULL(C0."Phone1",'') "phone" ,
			IFNULL(C0."E_Mail",'') "email" ,
			T0."DocDueDate" "expirationDate" ,
			T0."DocDueDate" "datePayment" ,
			CASE WHEN T0."DocTotalFC" > 0 THEN T0."DocTotalFC" 
				ELSE T0."DocTotal" 
			END "amountPayment" ,
			T5."PymntGroup" "descriptionPayment" ,
			'' "estimateNumber" ,
			'' "account" ,
			'' "reference" ,
			'' "project" ,
			'' "certificateNumber" ,
			'' "contactP" ,
			'' "gloss" ,
			IFNULL(C0."U_BPP_BPTD",'') "identityDocumentType" ,
			T0."DocType" ,
			T0."DocEntry" ,
			T0."ObjType" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."DiscSumFC" 
				ELSE T0."DiscSum" 
			END "DiscSum" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."VatSumFC" 
				ELSE T0."VatSum" 
			END "VatSum" ,
			M0."CurrName" "CurrName" ,
			IFNULL(T0."U_BPP_MDCT",'') "CodigoTransp" ,
			IFNULL(T0."U_BPP_MDRT",'') "RUCTransp" ,
			IFNULL(T0."U_BPP_MDNT",'') "NombreTransp" ,
			IFNULL(T0."U_BPP_MDFC",'') "LicenciaConductor" ,
			IFNULL(VEH."U_BPP_VEPL",'') "PlacaVehiculo" ,
			IFNULL(T0."U_BPP_MDVN",'') "MarcaVehiculo" ,
			IFNULL(CON."Name",'') "NombreConductor" ,
			'0' "Transbordo" ,
			0.0 "PesoBrutoTotal" ,
			0.0 "NroPallets" ,
			IFNULL(T0."U_BPP_MDMT",'01') "ModalidadTraslado" ,
			T0."TaxDate" "FechaInicioTraslado" ,
			T0."U_BPP_MDMT" "CodigoMotivoTraslado" ,
			IFNULL((SELECT "U_BPP_MTDT" 
					FROM "@BPP_MOTTRA" 
					WHERE "Code" = T0."U_BPP_MDMT"),'') "DescripcionMotivo" ,
			'' "NumeroContenedor" ,
			'' "CodigoPuerto" ,
			IFNULL(T0."Comments",'') "Comentario" 
	FROM "ORPD" T0 
	JOIN "OCTG" T5 ON T5."GroupNum" = T0."GroupNum" 
	JOIN "RPD12" T12 ON T12."DocEntry" = T0."DocEntry" 
	JOIN "OCRD" C0 ON C0."CardCode" = T0."CardCode" 
	JOIN "OSLP" V0 ON V0."SlpCode" = T0."SlpCode" 
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign" 
	LEFT JOIN "OCRN" M0 ON M0."CurrCode" = T0."DocCur" 
	LEFT JOIN "OCPR" C1 ON C1."CardCode" = T0."CardCode" 
						AND C1."CntctCode" = T0."CntctCode" 
	LEFT JOIN "CRD1" C2 ON C2."CardCode" = T0."CardCode" 
						AND C2."Address" = T0."PayToCode" 
						AND C2."AdresType" = 'S' 
	JOIN "NNM1" N0 ON N0."Series" = T0."Series" 
	LEFT JOIN "@BPP_CONDUC" CON ON CON."Code" = T0."U_BPP_MDFN" 
	LEFT JOIN "@BPP_VEHICU" VEH ON VEH."Code" = T0."U_BPP_MDVC" ,
	 "OADM" A0 
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E' 
	AND T0."CANCELED" = 'N'
	
	--Transferencia Stock
	UNION
	SELECT	IFNULL(M0."DocCurrCod",'PEN') "currency" ,
			T0."TaxDate" "TaxDate" ,
			T0."U_BPP_MDSD" "series" ,
			T0."U_BPP_MDSD" || '-' || T0."U_BPP_MDCD" "externalFolio" ,
			V0."SlpName" "sellerCode" ,
			T0."LicTradNum" "LicTradNum" ,
			T0."CardName" "CardName" ,
			IFNULL(C0."MailAddres",'') || ', '|| IFNULL(C0."MailBlock",'') || ', ' || IFNULL(C0."MailCity",'') "address" ,
			IFNULL(C0."MailStrNo",'') "LlegadaUbigeo",
			IFNULL(C2."U_BPP_CODI",'') "municipality" ,
			IFNULL(C1."FirstName",'') "contact" ,
			IFNULL(C0."Phone1",'') "phone" ,
			IFNULL(C0."E_Mail",'') "email" ,
			T0."DocDueDate" "expirationDate" ,
			T0."DocDueDate" "datePayment" ,
			CASE WHEN T0."DocTotalFC" > 0 THEN T0."DocTotalFC" 
				ELSE T0."DocTotal" 
			END "amountPayment" ,
			T5."PymntGroup" "descriptionPayment" ,
			'' "estimateNumber" ,
			'' "account" ,
			'' "reference" ,
			'' "project" ,
			'' "certificateNumber" ,
			'' "contactP" ,
			'' "gloss" ,
			IFNULL(C0."U_BPP_BPTD",'') "identityDocumentType" ,
			T0."DocType" ,
			T0."DocEntry" ,
			T0."ObjType" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."DiscSumFC" 
				ELSE T0."DiscSum" 
			END "DiscSum" ,
			CASE WHEN T0."DocCur" <> A0."MainCurncy" THEN T0."VatSumFC" 
				ELSE T0."VatSum" 
			END "VatSum" ,
			M0."CurrName" "CurrName" ,
			IFNULL(T0."U_BPP_MDCT",'') "CodigoTransp" ,
			IFNULL(T0."U_BPP_MDRT",'') "RUCTransp" ,
			IFNULL(T0."U_BPP_MDNT",'') "NombreTransp" ,
			IFNULL(T0."U_BPP_MDFC",'') "LicenciaConductor" ,
			IFNULL(VEH."U_BPP_VEPL",'') "PlacaVehiculo" ,
			IFNULL(T0."U_BPP_MDVN",'') "MarcaVehiculo" ,
			IFNULL(CON."Name",'') "NombreConductor" ,
			'0' "Transbordo" ,
			0.0 "PesoBrutoTotal" ,
			0.0 "NroPallets" ,
			IFNULL(T0."U_BPP_MDMT",'01') "ModalidadTraslado" ,
			T0."TaxDate" "FechaInicioTraslado" ,
			T0."U_BPP_MDMT" "CodigoMotivoTraslado" ,
			IFNULL((SELECT "U_BPP_MTDT" 
					FROM "@BPP_MOTTRA" 
					WHERE "Code" = T0."U_BPP_MDMT"),'') "DescripcionMotivo" ,
			'' "NumeroContenedor" ,
			'' "CodigoPuerto" ,
			IFNULL(T0."Comments",'') "Comentario" 
	FROM "OWTR" T0 
	JOIN "OCTG" T5 ON T5."GroupNum" = T0."GroupNum" 
	JOIN "OCRD" C0 ON C0."CardCode" = T0."CardCode" 
	JOIN "OSLP" V0 ON V0."SlpCode" = T0."SlpCode" 
	JOIN "OUSR" U0 ON U0."INTERNAL_K" = T0."UserSign" 
	LEFT JOIN "OCRN" M0 ON M0."CurrCode" = T0."DocCur" 
	LEFT JOIN "OCPR" C1 ON C1."CardCode" = T0."CardCode" 
						AND C1."CntctCode" = T0."CntctCode" 
	LEFT JOIN "CRD1" C2 ON C2."CardCode" = C0."CardCode"
	                 AND C2."Address" = C0."ShipToDef"
	JOIN "NNM1" N0 ON N0."Series" = T0."Series" 
	LEFT JOIN "@BPP_CONDUC" CON ON CON."Code" = T0."U_BPP_MDFN" 
	LEFT JOIN "@BPP_VEHICU" VEH ON VEH."Code" = T0."U_BPP_MDVC" ,
	 "OADM" A0 
	WHERE UPPER(LEFT(N0."BeginStr",1)) = 'E' 
	AND T0."CANCELED" = 'N';
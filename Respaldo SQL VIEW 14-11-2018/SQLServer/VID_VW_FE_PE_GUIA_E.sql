IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_VW_FE_PE_GUIA_E' AND type = 'V')
   DROP VIEW VID_VW_FE_PE_GUIA_E
GO--

CREATE VIEW [dbo].[VID_VW_FE_PE_GUIA_E]
AS
	--Entrega
	SELECT	ISNULL(M0.DocCurrCod,'PEN') 'currency',
			T0.TaxDate 'TaxDate',
			T0.U_BPP_MDSD 'series',
			T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD 'externalFolio',
			V0.SlpName 'sellerCode',
			T0.LicTradNum 'LicTradNum',
			T0.CardName 'CardName',
			ISNULL(T12.StreetS,'') + ', '+ ISNULL(T12.BlockS,'') + ', ' + ISNULL(T12.CityS,'') 'address',
			ISNULL(T12.StreetNoS,'') 'LlegadaUbigeo',
			ISNULL(C2.U_BPP_CODI,'') 'municipality',
			ISNULL(C1.FirstName,'') 'contact',
			ISNULL(C0.Phone1,'') 'phone',
			ISNULL(C0.E_Mail,'') 'email',
			T0.DocDueDate 'expirationDate',
			T0.DocDueDate 'datePayment',
			CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC 
				ELSE T0.DocTotal 
			END 'amountPayment',
			T5.PymntGroup 'descriptionPayment',
			'' 'estimateNumber',
			'' 'account',
			'' 'reference',
			'' 'project',
			'' 'certificateNumber',
			'' 'contactP',
			'' 'gloss',
			ISNULL(C0.U_BPP_BPTD,'') 'identityDocumentType',
			T0.DocType,
			T0.DocEntry,
			T0.ObjType,
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.DiscSumFC 
				ELSE T0.DiscSum 
			END 'DiscSum',
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.VatSumFC 
				ELSE T0.VatSum 
			END 'VatSum',
			M0.CurrName 'CurrName',
			ISNULL(T0.U_BPP_MDCT,'') 'CodigoTransp',
			ISNULL(T0.U_BPP_MDRT,'') 'RUCTransp',
			ISNULL(T0.U_BPP_MDNT,'') 'NombreTransp',
			ISNULL(T0.U_BPP_MDFC,'') 'LicenciaConductor',
			ISNULL(VEH.U_BPP_VEPL,'') 'PlacaVehiculo',
			ISNULL(T0.U_BPP_MDVN,'') 'MarcaVehiculo',
			ISNULL(CON.Name,'') 'NombreConductor',
			'0' 'Transbordo',
			0.0 'PesoBrutoTotal',
			0.0 'NroPallets',
			ISNULL(T0.U_BPP_MDMT,'01') 'ModalidadTraslado',
			T0.TaxDate 'FechaInicioTraslado',
			T0.U_BPP_MDMT 'CodigoMotivoTraslado',
			ISNULL((SELECT U_BPP_MTDT 
					FROM [@BPP_MOTTRA]
					WHERE Code = T0.U_BPP_MDMT),'') 'DescripcionMotivo',
			'' 'NumeroContenedor',
			'' 'CodigoPuerto',
			ISNULL(T0.Comments,'') 'Comentario'
	FROM ODLN T0 
	JOIN OCTG T5 ON T5.GroupNum = T0.GroupNum 
	JOIN DLN12 T12 ON T12.DocEntry = T0.DocEntry 
	JOIN OCRD C0 ON C0.CardCode = T0.CardCode 
	JOIN OSLP V0 ON V0.SlpCode = T0.SlpCode 
	JOIN OUSR U0 ON U0.INTERNAL_K = T0.UserSign 
	LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCur 
	LEFT JOIN OCPR C1 ON C1.CardCode = T0.CardCode 
						AND C1.CntctCode = T0.CntctCode 
	LEFT JOIN CRD1 C2 ON C2.CardCode = T0.CardCode 
						AND C2.Address = T0.PayToCode 
						AND C2.AdresType = 'S' 
	JOIN NNM1 N0 ON N0.Series = T0.Series 
	LEFT JOIN [@BPP_CONDUC] CON ON CON.Code = T0.U_BPP_MDFN 
	LEFT JOIN [@BPP_VEHICU] VEH ON VEH.Code = T0.U_BPP_MDVC ,
	 OADM A0 
	WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E' 
	AND T0.CANCELED = 'N'

	--Devolucion Compra
	UNION
	SELECT	ISNULL(M0.DocCurrCod,'PEN') 'currency',
			T0.TaxDate 'TaxDate',
			T0.U_BPP_MDSD 'series',
			T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD 'externalFolio',
			V0.SlpName 'sellerCode',
			T0.LicTradNum 'LicTradNum',
			T0.CardName 'CardName',
			ISNULL(T12.StreetS,'') + ', '+ ISNULL(T12.BlockS,'') + ', ' + ISNULL(T12.CityS,'') 'address',
			ISNULL(T12.StreetNoS,'') 'LlegadaUbigeo',
			ISNULL(C2.U_BPP_CODI,'') 'municipality',
			ISNULL(C1.FirstName,'') 'contact',
			ISNULL(C0.Phone1,'') 'phone',
			ISNULL(C0.E_Mail,'') 'email',
			T0.DocDueDate 'expirationDate',
			T0.DocDueDate 'datePayment',
			CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC 
				ELSE T0.DocTotal 
			END 'amountPayment',
			T5.PymntGroup 'descriptionPayment',
			'' 'estimateNumber',
			'' 'account',
			'' 'reference',
			'' 'project',
			'' 'certificateNumber',
			'' 'contactP',
			'' 'gloss',
			ISNULL(C0.U_BPP_BPTD,'') 'identityDocumentType',
			T0.DocType,
			T0.DocEntry,
			T0.ObjType,
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.DiscSumFC 
				ELSE T0.DiscSum 
			END 'DiscSum',
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.VatSumFC 
				ELSE T0.VatSum 
			END 'VatSum',
			M0.CurrName 'CurrName',
			ISNULL(T0.U_BPP_MDCT,'') 'CodigoTransp',
			ISNULL(T0.U_BPP_MDRT,'') 'RUCTransp',
			ISNULL(T0.U_BPP_MDNT,'') 'NombreTransp',
			ISNULL(T0.U_BPP_MDFC,'') 'LicenciaConductor',
			ISNULL(VEH.U_BPP_VEPL,'') 'PlacaVehiculo',
			ISNULL(T0.U_BPP_MDVN,'') 'MarcaVehiculo',
			ISNULL(CON.Name,'') 'NombreConductor',
			'0' 'Transbordo',
			0.0 'PesoBrutoTotal',
			0.0 'NroPallets',
			ISNULL(T0.U_BPP_MDMT,'01') 'ModalidadTraslado',
			T0.TaxDate 'FechaInicioTraslado',
			T0.U_BPP_MDMT 'CodigoMotivoTraslado',
			ISNULL((SELECT U_BPP_MTDT 
					FROM [@BPP_MOTTRA]
					WHERE Code = T0.U_BPP_MDMT),'') 'DescripcionMotivo',
			'' 'NumeroContenedor',
			'' 'CodigoPuerto',
			ISNULL(T0.Comments,'') 'Comentario'
	FROM ORPD T0 
	JOIN OCTG T5 ON T5.GroupNum = T0.GroupNum 
	JOIN RPD12 T12 ON T12.DocEntry = T0.DocEntry 
	JOIN OCRD C0 ON C0.CardCode = T0.CardCode 
	JOIN OSLP V0 ON V0.SlpCode = T0.SlpCode 
	JOIN OUSR U0 ON U0.INTERNAL_K = T0.UserSign 
	LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCur 
	LEFT JOIN OCPR C1 ON C1.CardCode = T0.CardCode 
						AND C1.CntctCode = T0.CntctCode 
	LEFT JOIN CRD1 C2 ON C2.CardCode = T0.CardCode 
						AND C2.Address = T0.PayToCode 
						AND C2.AdresType = 'S' 
	JOIN NNM1 N0 ON N0.Series = T0.Series 
	LEFT JOIN [@BPP_CONDUC] CON ON CON.Code = T0.U_BPP_MDFN 
	LEFT JOIN [@BPP_VEHICU] VEH ON VEH.Code = T0.U_BPP_MDVC ,
	 OADM A0 
	WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E' 
	AND T0.CANCELED = 'N'

	--Transferencia Stock
	UNION 
	SELECT	ISNULL(M0.DocCurrCod,'PEN') 'currency',
			T0.TaxDate 'TaxDate',
			T0.U_BPP_MDSD 'series',
			T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD 'externalFolio',
			V0.SlpName 'sellerCode',
			T0.LicTradNum 'LicTradNum',
			T0.CardName 'CardName',
			ISNULL(C0.MailAddres,'') + ', '+ ISNULL( C0.MailBlock,'') + ', ' + ISNULL(C0.MailCity,'') 'address',
			ISNULL(C0.MailStrNo,'') 'LlegadaUbigeo',
			ISNULL(C2.U_BPP_CODI,'') 'municipality',
			ISNULL(C1.FirstName,'') 'contact',
			ISNULL(C0.Phone1,'') 'phone',
			ISNULL(C0.E_Mail,'') 'email',
			T0.DocDueDate 'expirationDate',
			T0.DocDueDate 'datePayment',
			CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC 
				ELSE T0.DocTotal 
			END 'amountPayment',
			T5.PymntGroup 'descriptionPayment',
			'' 'estimateNumber',
			'' 'account',
			'' 'reference',
			'' 'project',
			'' 'certificateNumber',
			'' 'contactP',
			'' 'gloss',
			ISNULL(C0.U_BPP_BPTD,'') 'identityDocumentType',
			T0.DocType,
			T0.DocEntry,
			T0.ObjType,
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.DiscSumFC 
				ELSE T0.DiscSum 
			END 'DiscSum',
			CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.VatSumFC 
				ELSE T0.VatSum 
			END 'VatSum',
			M0.CurrName 'CurrName',
			ISNULL(T0.U_BPP_MDCT,'') 'CodigoTransp',
			ISNULL(T0.U_BPP_MDRT,'') 'RUCTransp',
			ISNULL(T0.U_BPP_MDNT,'') 'NombreTransp',
			ISNULL(T0.U_BPP_MDFC,'') 'LicenciaConductor',
			ISNULL(VEH.U_BPP_VEPL,'') 'PlacaVehiculo',
			ISNULL(T0.U_BPP_MDVN,'') 'MarcaVehiculo',
			ISNULL(CON.Name,'') 'NombreConductor',
			'0' 'Transbordo',
			0.0 'PesoBrutoTotal',
			0.0 'NroPallets',
			ISNULL(T0.U_BPP_MDMT,'01') 'ModalidadTraslado',
			T0.TaxDate 'FechaInicioTraslado',
			T0.U_BPP_MDMT 'CodigoMotivoTraslado',
			ISNULL((SELECT U_BPP_MTDT 
					FROM [@BPP_MOTTRA]
					WHERE Code = T0.U_BPP_MDMT),'') 'DescripcionMotivo',
			'' 'NumeroContenedor',
			'' 'CodigoPuerto',
			ISNULL(T0.Comments,'') 'Comentario'
	FROM OWTR T0 
	JOIN OCTG T5 ON T5.GroupNum = T0.GroupNum 
	JOIN OCRD C0 ON C0.CardCode = T0.CardCode 
	JOIN OSLP V0 ON V0.SlpCode = T0.SlpCode 
	JOIN OUSR U0 ON U0.INTERNAL_K = T0.UserSign 
	LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCur 
	LEFT JOIN OCPR C1 ON C1.CardCode = T0.CardCode 
						AND C1.CntctCode = T0.CntctCode  
	LEFT JOIN CRD1 C2 ON C2.CardCode = C0.CardCode
	                 AND C2.Address = C0.ShipToDef
	JOIN NNM1 N0 ON N0.Series = T0.Series 
	LEFT JOIN [@BPP_CONDUC] CON ON CON.Code = T0.U_BPP_MDFN 
	LEFT JOIN [@BPP_VEHICU] VEH ON VEH.Code = T0.U_BPP_MDVC ,
	 OADM A0 
	WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E' 
	AND T0.CANCELED = 'N'
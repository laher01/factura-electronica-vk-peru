IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_VW_FE_PE_OINV_E' AND type = 'V')
   DROP VIEW VID_VW_FE_PE_OINV_E
GO--

CREATE VIEW VID_VW_FE_PE_OINV_E

AS

	SELECT ISNULL(M0.DocCurrCod,'PEN')				[currency]
	      ,T0.TaxDate								[date]
		  ,T0.U_BPP_MDSD							[series]
		  ,T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD		[externalFolio]
		  ,V0.SlpName								[sellerCode]
		  ,T0.LicTradNum							[code]
		  ,T0.CardName								[name]
		  ,ISNULL(T12.StreetB,'') + ', '+ISNULL(T12.BlockB,'') + ', ' + ISNULL(T12.CityB,'')	[address]
		  ,ISNULL(C2.U_BPP_CODI,'')					[municipality]
		  ,ISNULL(C1.FirstName,'')					[contact]
		  ,ISNULL(C0.Phone1,'')						[phone]
		  ,ISNULL(C0.E_Mail,'')						[email]
		  ,T0.DocDueDate							[expirationDate]
		  ,T0.DocDueDate							[datePayment]
		  ,CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC
			    ELSE T0.DocTotal
		   END										[amountPayment]
		  ,T5.PymntGroup							[descriptionPayment]
		  ,''										[estimateNumber]
		  ,''										[account]
		  ,''										[reference]
		  ,''										[project]
		  ,''										[certificateNumber]
		  ,''										[contactP]
		  ,''										[gloss]
		  ,ISNULL(C0.U_BPP_BPTD,'')					[identityDocumentType]
		  ,T0.DocType
		  ,T0.DocEntry
		  ,T0.ObjType
		  ,CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.DiscSumFC
		        ELSE T0.DiscSum
		   END										[DiscSum]
		  ,CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.VatSumFC
		        ELSE T0.VatSum
		   END										[VatSum]
		  ,M0.CurrName								[CurrName]
	  FROM OINV T0
	  JOIN OCTG T5 ON T5.GroupNum = T0.GroupNum
	  JOIN INV12 T12 ON T12.DocEntry = T0.DocEntry
	  JOIN OCRD C0 ON C0.CardCode = T0.CardCode
	  JOIN OSLP V0 ON V0.SlpCode = T0.SlpCode
	  JOIN OUSR U0 ON U0.INTERNAL_K = T0.UserSign
	  LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCur
	  LEFT JOIN OCPR C1 ON C1.CardCode = T0.CardCode
	                   AND C1.CntctCode = T0.CntctCode
	  LEFT JOIN CRD1 C2 ON C2.CardCode = T0.CardCode
	                   AND C2.Address = T0.PayToCode
					   AND C2.AdresType = 'B'
	  JOIN NNM1 N0 ON N0.Series = T0.Series
	              --AND N0.ObjectCode = T0.ObjType
	  , OADM A0
	 WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
	   AND T0.Canceled = 'N'

	 --FACTURA DE ANTICIPO
	 UNION ALL
	 SELECT ISNULL(M0.DocCurrCod,'PEN')				[currency]
	      ,T0.TaxDate								[date]
		  ,T0.U_BPP_MDSD							[series]
		  ,'F' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD		[externalFolio]
		  ,V0.SlpName								[sellerCode]
		  ,T0.LicTradNum							[code]
		  ,T0.CardName								[name]
		  ,ISNULL(T12.StreetB,'') + ', '+ISNULL(T12.BlockB,'') + ', ' + ISNULL(T12.CityB,'')	[address]
		  ,ISNULL(C2.U_BPP_CODI,'')					[municipality]
		  ,ISNULL(C1.FirstName,'')					[contact]
		  ,ISNULL(C0.Phone1,'')						[phone]
		  ,ISNULL(C0.E_Mail,'')						[email]
		  ,T0.DocDueDate							[expirationDate]
		  ,T0.DocDueDate							[datePayment]
		  ,CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC
			    ELSE T0.DocTotal
		   END										[amountPayment]
		  ,T5.PymntGroup							[descriptionPayment]
		  ,''								[estimateNumber]
		  ,''								[account]
		  ,''								[reference]
		  ,''							[project]
		  ,''								[certificateNumber]
		  ,''							[contact]
		  ,''								[gloss]
		  ,ISNULL(C0.U_BPP_BPTD,'')					[identityDocumentType]
		  ,T0.DocType
		  ,T0.DocEntry
		  ,T0.ObjType
		  ,CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.DiscSumFC
		        ELSE T0.DiscSum
		   END										[DiscSum]
		  ,CASE WHEN T0.DocCur <> A0.MainCurncy THEN T0.VatSumFC
		        ELSE T0.VatSum
		   END										[VatSum]
		  ,M0.CurrName								[CurrName]
	  FROM ODPI T0
	  JOIN OCTG T5 ON T5.GroupNum = T0.GroupNum
	  JOIN DPI12 T12 ON T12.DocEntry = T0.DocEntry
	  JOIN OCRD C0 ON C0.CardCode = T0.CardCode
	  JOIN OSLP V0 ON V0.SlpCode = T0.SlpCode
	  JOIN OUSR U0 ON U0.INTERNAL_K = T0.UserSign
	  LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCur
	  LEFT JOIN OCPR C1 ON C1.CardCode = T0.CardCode
	                   AND C1.CntctCode = T0.CntctCode
	  LEFT JOIN CRD1 C2 ON C2.CardCode = T0.CardCode
	                   AND C2.Address = T0.PayToCode
					   AND C2.AdresType = 'B'
	  JOIN NNM1 N0 ON N0.Series = T0.Series
	              --AND N0.ObjectCode = T0.ObjType
	  , OADM A0
	 WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
	   AND T0.Canceled = 'N'


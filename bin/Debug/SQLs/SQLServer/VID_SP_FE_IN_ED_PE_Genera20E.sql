IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera20E' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera20E
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_IN_ED_PE_Genera20E]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
AS
BEGIN

		SELECT CASE WHEN LEFT(T0.U_BPP_PTSC, 1) = 'R' THEN T0.U_BPP_PTSC + '-' + T0.U_BPP_PTCC ELSE 'R' + T0.U_BPP_PTSC + '-' + T0.U_BPP_PTCC END [IdDocumento] --FF11-004 
		      ,REPLACE(CONVERT(CHAR(10),T0.DocDate,102),'.','-')  [FechaEmision] --2016-08-27
		      --,@TipoDoc					[TipoDocumento] 
		      ,A0.TaxIdNum				[EmisorNroDocumento]
			  ,'6'						[EmisorTipoDocumento]
			  ,A0.CompnyName			[EmisorNombreLegal]
			  ,A0.AliasName				[EmisorNombreComercial]
			  ,'150136'					[EmisorUbigeo]
			  ,A1.Street + ' ' + ISNULL(A1.StreetNo,'')	[EmisorDireccion]
			  ,'-'						[EmisorUrbanizacion]
			  ,'-'						[EmisorDepartamento]
			  ,A1.City					[EmisorProvincia]
			  ,A1.County				[EmisorDistrito]

			  ,C0.LicTradNum			[ReceptorNroDocumento]
			  ,ISNULL(C0.U_BPP_BPTD,'')	[ReceptorTipoDocumento]
			  ,T0.CardName				[ReceptorNombreLegal]
			  ,T0.CardName				[ReceptorNombreComercial]
			  
			  ,ISNULL(M0.DocCurrCod,'PEN')		[Moneda] --PEN
			  ,ISNULL(T0.Comments,'')	[Observaciones]
			  ,'01'		[RegimenRetencion]
			  ,'3'		[TasaRetencion]
			  ,ISNULL((SELECT SUM(WTSUM)
			             FROM VPM6
						WHERE DocNum = T0.DocEntry),0.0)[ImporteTotalRetenido]
			  ,CASE WHEN T0.CashSumFC + T0.TrsfrSumFC + T0.CheckSumFC + T0.CredSumFC > 0 THEN T0.CashSumFC + T0.TrsfrSumFC + T0.CheckSumFC + T0.CredSumFC
			        ELSE T0.CashSum + T0.TrsfrSum + T0.CheckSum + T0.CreditSum 
			   END [ImporteTotalPagado]
			  ,ISNULL(C0.E_Mail,'')		[CamposExtrasCorreoReceptor]
			  
		  FROM OVPM T0
		  JOIN OCRD C0 ON C0.CardCode = T0.CardCode
		  LEFT JOIN OCRN M0 ON M0.CurrCode = T0.DocCurr
		      ,OADM A0, ADM1 A1
		 WHERE T0.DocEntry = @DocEntry
		   AND T0.ObjType = @ObjType
END
USE [GENERAL_PRUEBAS]
GO
/****** Object:  StoredProcedure [dbo].[VID_SP_FE_IN_ED_PE_Genera03E]    Script Date: 21/11/2018 20:32:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[VID_SP_FE_IN_ED_PE_Genera03E]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
--WITH ENCRYPTION
AS
BEGIN

		SELECT @TipoDoc					[TipoDocumento] 
		      ,A0.TaxIdNum				[EmisorNroDocumento]
			  ,'6'						[EmisorTipoDocumento]
			  ,A0.CompnyName			[EmisorNombreLegal]
			  ,A0.AliasName				[EmisorNombreComercial]
			  ,A1.Street + ' ' + ISNULL(A1.StreetNo,'')	[EmisorDireccion]
			  ,'-'						[EmisorUrbanizacion]
			  ,''						[EmisorDepartamento]
			  ,A1.City					[EmisorProvincia]
			  ,A1.County				[EmisorDistrito]

			  ,T0.Code					[ReceptorNroDocumento]
			  ,T0.identityDocumentType	[ReceptorTipoDocumento]
			  ,T0.name					[ReceptorNombreLegal]

			  ,CASE WHEN LEFT(T0.externalFolio,1) = 'B' THEN T0.externalFolio ELSE 'B' + T0.externalFolio END	[IdDocumento] --FF11-004
			  ,REPLACE(CONVERT(CHAR(10),T0.date,102),'.','-')  [FechaEmision]--2016-08-27
			  ,T0.currency				[Moneda] --PEN
			  --,ISNULL((SELECT SUM(A0.Gravadas)
			  --           FROM VID_VW_FE_PE_OINV_D A0
					--	WHERE A0.DocEntry = T0.DocEntry
					--	  AND A0.ObjType = T0.ObjType), 0.0)	[Gravadas]
			  ,T0.Gravadas										[Gravadas]
			  ,ISNULL((SELECT SUM(A0.Gratuitas)
			             FROM VID_VW_FE_PE_OINV_D A0
						WHERE A0.DocEntry = T0.DocEntry
						  AND A0.ObjType = T0.ObjType), 0.0)	[Gratuitas]
		      ,ISNULL((SELECT SUM(A0.Inafectas)
			             FROM VID_VW_FE_PE_OINV_D A0
						WHERE A0.DocEntry = T0.DocEntry
						  AND A0.ObjType = T0.ObjType), 0.0)	[Inafectas]
			  ,ISNULL((SELECT SUM(A0.Exoneradas)
			             FROM VID_VW_FE_PE_OINV_D A0
						WHERE A0.DocEntry = T0.DocEntry
						  AND A0.ObjType = T0.ObjType), 0.0)	[Exoneradas]
			  ,T0.DiscSum				[DescuentoGlobal]
			  ,ISNULL(T0.email,'')		[CamposExtrasCorreoReceptor]
			  ,T0.amountPayment			[TotalVenta]
		      ,T0.VatSum				[TotalIgv]
			  ,0.0						[TotalIsc]
			  ,0.0						[TotalOtrosTributos]
			  ,[dbo].[VID_FN_FE_CantidadConLetra] (T0.amountPayment, T0.CurrName)	[MontoEnLetras]
			  ,'01'						[TipoOperacion]
			  ,ISNULL((SELECT ROUND(B1.Rate / 100, 2)
		                 FROM [@FM_IVA] B0
						 JOIN OSTA B1 ON B1.Code = B0.Code
					    WHERE CONVERT(VARCHAR, B0.NAME) = '10'), 0.0)	[CalculoIgv]
			  ,0.0						[CalculoIsc]
			  ,0.0						[CalculoDetraccion]
			  ,0.0						[MontoPercepcion]
			  ,0.0						[MontoDetraccion]
			  ,0.0						[MontoAnticipo]
			  ,''						[DatoAdicionales]
			  ,'Relacionados'			[Relacionados] 
		  FROM VID_VW_FE_PE_OINV_E T0
		      ,OADM A0, ADM1 A1
		 WHERE T0.DocEntry = @DocEntry
		   AND T0.ObjType = @ObjType


END
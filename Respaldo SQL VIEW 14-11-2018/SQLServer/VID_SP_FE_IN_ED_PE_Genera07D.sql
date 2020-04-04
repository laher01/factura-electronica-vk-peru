IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera07D' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera07D
GO--

CREATE PROCEDURE [dbo].[VID_SP_FE_IN_ED_PE_Genera07D]
 @DocEntry Int ,@TipoDoc  Varchar(10), @ObjType VarChar(10)
AS
BEGIN

		SELECT CAST(ROW_NUMBER() OVER(ORDER BY T0.LineaOrden, T0.LineaOrden2) AS INT)	[Id]
			          ,T0.[quantity]	[Cantidad]
					  ,T0.[unit]		[UnidadMedida]
					  ,T0.[price] * T0.[quantity]	[Suma]
					  ,T0.[price] * T0.[quantity]	[TotalVenta]
					  ,T0.[price]		[PrecioUnitario]
					  ,'01'				[TipoPrecio]
					  ,ISNULL((SELECT Rate FROM OSTC WHERE Code = T0.TaxCode),0.0)		[Impuesto]
					  ,T0.[exemptType]	[TipoImpuesto]
					  ,0.0				[ImpuestoSelectivo]
					  ,0.0				[OtroImpuesto]
					  ,ISNULL(T0.[description],'') [Descripcion]
					  ,T0.[code]		[CodigoItem]
					  ,T0.[price]		[PrecioReferencial]
				 FROM VID_VW_FE_PE_ORIN_D T0
				WHERE T0.DocEntry = @DocEntry
				  AND T0.ObjType = @ObjType
				ORDER BY  T0.[LineaOrden], T0.[LineaOrden2]
END

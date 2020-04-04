IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera09E' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera09E
GO--

CREATE PROCEDURE VID_SP_FE_IN_ED_PE_Genera09E @DocEntry Integer, @TipoDoc VarChar(10), @ObjType VarChar(10)
AS
BEGIN

	SELECT CASE WHEN LEFT(T0.externalFolio,1) = 'T' THEN T0.externalFolio ELSE 'T' + T0.externalFolio END	'IdDocumento' --FF11-004
	      ,REPLACE(CONVERT(CHAR(10), T0.TaxDate, 111),'/','-')  'FechaEmision'--2016-08-27'
	      ,@TipoDoc					'TipoDocumento'
		  ,ISNULL(T0.Comentario,'')	'Glosa'
		  ,T0.CodigoMotivoTraslado	'CodigoMotivoTraslado'
		  ,T0.DescripcionMotivo		'DescripcionMotivo'
		  ,T0.Transbordo			'Transbordo'
		  ,T0.PesoBrutoTotal		'PesoBrutoTotal'
		  ,T0.NroPallets			'NroPallets'
		  ,T0.ModalidadTraslado		'ModalidadTraslado'
		  ,REPLACE(CONVERT(CHAR(10), T0.FechaInicioTraslado, 111),'/','-')	'FechaInicioTraslado'
		  ,T0.NombreTransp			'RazonSocialTransportista'
		  ,T0.RUCTransp				'RucTransportista'
		  ,T0.PlacaVehiculo			'NroPlacaVehiculo'
		  ,T0.LicenciaConductor		'NroDocumentoConductor'
		  ,ISNULL(A1.Street,'')		'DireccionPartidaCompleta'
		  ,ISNULL(A1.StreetNo,'')	'DireccionPartidaUbigeo'
		  ,ISNULL(T0.address,'')	'DireccionLlegadaCompleta'
		  ,ISNULL(T0.LlegadaUbigeo,'')	'DireccionLlegadaUbigeo'
		  ,''		'NumeroContenedor'
		  ,''		'CodigoPuerto'
		  
	      ,A0.TaxIdNum				'RemitenteNroDocumento'
		  ,'6'						'RemitenteTipoDocumento'
		  ,ISNULL(A1.Street,'')		'RemitenteDireccion'
		  ,'-'						'RemitenteUrbanizacion'
		  ,'-'						'RemitenteDepartamento'
		  ,A1.City					'RemitenteProvincia'
		  ,A1.County				'RemitenteDistrito'
		  ,A0.AliasName				'RemitenteNombreComercial'
		  ,A0.CompnyName			'RemitenteNombreLegal'
		  ,ISNULL(A1.StreetNo,'')	'RemitenteUbigeo'

		  ,T0.LicTradNum			'DestinatarioNroDocumento'
		  ,T0.identityDocumentType 	'DestinatarioTipoDocumento'
		  ,T0.CardName				'DestinatarioNombreLegal'
		  ,''			'DestinatarioNombreComercial'
		  ,T0.email 				'CamposExtrasCorreoReceptor'
	  FROM VID_VW_FE_PE_GUIA_E T0
	      ,OADM A0, ADM1 A1
	 WHERE T0.DocEntry = @DocEntry
	   AND T0.ObjType = @ObjType
END
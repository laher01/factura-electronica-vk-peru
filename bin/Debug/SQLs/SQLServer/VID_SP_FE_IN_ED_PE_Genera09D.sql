IF EXISTS(SELECT name FROM sysobjects
      WHERE name = 'VID_SP_FE_IN_ED_PE_Genera09D' AND type = 'P')
   DROP PROCEDURE VID_SP_FE_IN_ED_PE_Genera09D
                  
GO--
CREATE PROCEDURE VID_SP_FE_IN_ED_PE_Genera09D @DocEntry Integer, @TipoDoc VarChar(10), @ObjType VarChar(10)
AS
BEGIN
	
	SELECT CAST(ROW_NUMBER() OVER(ORDER BY T0.LineaOrden, T0.LineaOrden2) AS INT)	'Id'
		    ,T0.Cantidad	'Cantidad'
			,T0.ItemCode	'CodigoItem'
			,T0.Correlativo	'Correlativo'   --CASE WHEN LEFT(T0.Correlativo,1) = 'F' THEN T0.Correlativo ELSE 'F' + T0.Correlativo END
			,ISNULL(T0.Descripcion,'')	'Descripcion'
			,T0.LineaReferencia			'LineaReferencia'
			,T0.UnidadMedida			'UnidadMedida'
	    FROM VID_VW_FE_PE_GUIA_D T0
	   WHERE T0.DocEntry = @DocEntry
		 AND T0.ObjType = @ObjType
	   ORDER BY T0.LineaOrden, T0.LineaOrden2
END



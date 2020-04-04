--DROP FUNCTION VID_FN_FE_CantidadConLetra;
CREATE FUNCTION VID_FN_FE_CantidadConLetra
(
	IN Numero Decimal(18,2),
	IN CurrName VarChar(50)
)
RETURNS MontoLetras VarChar(180)
LANGUAGE SQLSCRIPT AS
BEGIN
    DECLARE lnEntero INT;
    DECLARE lcRetorno VARCHAR(512) := '';
    DECLARE lnTerna INT := 1;
	DECLARE lcMiles VARCHAR(512);
	DECLARE lcCadena VARCHAR(512);
	DECLARE lnUnidades INT;
	DECLARE lnDecenas INT;
	DECLARE lnCentenas INT;
	DECLARE lnFraccion INT;
	DECLARE sFraccion VARCHAR(15);
    
	SELECT CAST(:Numero AS INT) INTO lnEntero FROM DUMMY;
    SELECT (:Numero - :lnEntero) * 100 INTO lnFraccion FROM DUMMY;

	WHILE :lnEntero > 0 DO
		-- Recorro terna por terna
		lcCadena := '';
		lnUnidades := MOD(:lnEntero, 10);
		lnEntero := CAST(:lnEntero/10 AS INT);
		lnDecenas := MOD(:lnEntero, 10);
		lnEntero := CAST(:lnEntero/10 AS INT);
		lnCentenas := MOD(:lnEntero, 10);
		lnEntero = CAST(:lnEntero/10 AS INT);
        -- Analizo las unidades
        SELECT 
            CASE /* UNIDADES */
              WHEN :lnUnidades = 1 THEN 'UN ' || :lcCadena
              WHEN :lnUnidades = 2 THEN 'DOS ' || :lcCadena
              WHEN :lnUnidades = 3 THEN 'TRES ' || :lcCadena
              WHEN :lnUnidades = 4 THEN 'CUATRO ' || :lcCadena
              WHEN :lnUnidades = 5 THEN 'CINCO ' || :lcCadena
              WHEN :lnUnidades = 6 THEN 'SEIS ' || :lcCadena
              WHEN :lnUnidades = 7 THEN 'SIETE ' || :lcCadena
              WHEN :lnUnidades = 8 THEN 'OCHO ' || :lcCadena
              WHEN :lnUnidades = 9 THEN 'NUEVE ' || :lcCadena
              ELSE :lcCadena
            END INTO lcCadena /* UNIDADES */
		  FROM DUMMY;
		  
            -- Analizo las decenas
		SELECT
            CASE /* DECENAS */
				WHEN :lnDecenas = 1 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'DIEZ '
						WHEN 1 THEN 'ONCE '
						WHEN 2 THEN 'DOCE '
						WHEN 3 THEN 'TRECE '
						WHEN 4 THEN 'CATORCE '
						WHEN 5 THEN 'QUINCE '
						WHEN 6 THEN 'DIEZ Y SEIS '
						WHEN 7 THEN 'DIEZ Y SIETE '
						WHEN 8 THEN 'DIEZ Y OCHO '
						WHEN 9 THEN 'DIEZ Y NUEVE '
					END
				WHEN :lnDecenas = 2 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'VEINTE '
						ELSE 'VEINTI' || :lcCadena
					END
				WHEN :lnDecenas = 3 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'TREINTA '
						ELSE 'TREINTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 4 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'CUARENTA'
						ELSE 'CUARENTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 5 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'CINCUENTA '
						ELSE 'CINCUENTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 6 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'SESENTA '
						ELSE 'SESENTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 7 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'SETENTA '
						ELSE 'SETENTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 8 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'OCHENTA '
						ELSE  'OCHENTA Y ' || :lcCadena
					END
				WHEN :lnDecenas = 9 THEN
					CASE :lnUnidades
						WHEN 0 THEN 'NOVENTA '
						ELSE 'NOVENTA Y ' || :lcCadena
					END
				ELSE :lcCadena
			END INTO lcCadena /* DECENAS */
		  FROM DUMMY;
			  
		-- Analizo las centenas
        SELECT
            CASE /* CENTENAS */
              WHEN :lnCentenas = 1 THEN 'CIENTO ' || :lcCadena
              WHEN :lnCentenas = 2 THEN 'DOSCIENTOS ' || :lcCadena
              WHEN :lnCentenas = 3 THEN 'TRESCIENTOS ' || :lcCadena
              WHEN :lnCentenas = 4 THEN 'CUATROCIENTOS ' || :lcCadena
              WHEN :lnCentenas = 5 THEN 'QUINIENTOS ' || :lcCadena
              WHEN :lnCentenas = 6 THEN 'SEISCIENTOS ' || :lcCadena
              WHEN :lnCentenas = 7 THEN 'SETECIENTOS ' || :lcCadena
              WHEN :lnCentenas = 8 THEN 'OCHOCIENTOS ' || :lcCadena
              WHEN :lnCentenas = 9 THEN 'NOVECIENTOS ' || :lcCadena
              ELSE :lcCadena
            END INTO lcCadena
		  FROM DUMMY;/* CENTENAS */
		  
        -- Analizo la terna
        SELECT 
            CASE /* TERNA */
              WHEN :lnTerna = 1 THEN :lcCadena
              WHEN :lnTerna = 2 THEN :lcCadena || 'MIL '
              WHEN :lnTerna = 3 THEN :lcCadena || 'MILLONES '
              WHEN :lnTerna = 4 THEN :lcCadena || 'MIL '
              ELSE ''
            END INTO lcCadena
		  FROM DUMMY;/* TERNA */
		  
		-- Armo el retorno terna a terna
        SELECT :lcCadena || :lcRetorno INTO lcRetorno FROM DUMMY;
        SELECT :lnTerna + 1 INTO lnTerna FROM DUMMY;
	END WHILE;
	
	IF :lnTerna = 1 THEN
       SELECT 'CERO' INTO lcRetorno FROM DUMMY;
	END IF;
	
	sFraccion := '00' || LTRIM(CAST(:lnFraccion AS varchar));
	SELECT 'SON ' || RTRIM(:lcRetorno) || ' ' || UPPER(:CurrName) || ' CON ' || SUBSTRING(:sFraccion, LENGTH(:sFraccion)-1,2) || '/100' INTO MontoLetras FROM DUMMY;
	
END;

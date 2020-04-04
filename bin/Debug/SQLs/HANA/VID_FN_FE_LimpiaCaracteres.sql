--DROP FUNCTION VID_FN_FE_LimpiaCaracteres;
CREATE FUNCTION VID_FN_FE_LimpiaCaracteres
(
	IN texto VarChar(180)
)
RETURNS TextoFinal VarChar(180)
LANGUAGE SQLSCRIPT AS
BEGIN
	texto := REPLACE(texto, 'º', '');
	texto := REPLACE(texto, '\\', '');
	texto := REPLACE(texto, '"', '');
	texto := REPLACE(texto, '!', '');
	texto := REPLACE(texto, '·', '');
	texto := REPLACE(texto, '|', '');
	texto := REPLACE(texto, '#', '');
	texto := REPLACE(texto, '$', '');
	texto := REPLACE(texto, '=', '');
	texto := REPLACE(texto, '?', '');
	texto := REPLACE(texto, '¡', '');
	texto := REPLACE(texto, '<', '');
	texto := REPLACE(texto, '>', '');
	texto := REPLACE(texto, ']', '');
	texto := REPLACE(texto, '[', '');
	texto := REPLACE(texto, '{', '');
	texto := REPLACE(texto, '}', '');
	texto := REPLACE(texto, '^', '');
	texto := REPLACE(texto, ':', '');
	texto := REPLACE(texto, ';', '');
	texto := REPLACE(texto, '&', '');
	texto := REPLACE(texto, 'Ñ', 'N');
	texto := REPLACE(texto, 'ñ', 'n');

	SELECT :texto INTO TextoFinal FROM DUMMY;
END;
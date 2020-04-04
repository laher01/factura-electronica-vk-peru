IF EXISTS(SELECT name,* FROM sysobjects
      WHERE name = 'VID_FN_FE_LimpiaCaracteres' AND type = 'FN')
   DROP FUNCTION VID_FN_FE_LimpiaCaracteres
GO--

CREATE FUNCTION VID_FN_FE_LimpiaCaracteres (@texto VARCHAR(MAX))
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @TextoFinal VARCHAR(MAX), @wcount INT, @index INT, @len INT, @char VARCHAR(1)
	DECLARE @ListofIDs TABLE(IDs VARCHAR(100));
	INSERT INTO @ListofIDs
	VALUES ('°'),('\'),('"'),('!'),('|'),('·'),('#'),('$'),('='),('?'),('¿'),('¡'),('~'),('{'),('}'),('['),(']'),('%'),('&'),('-'),(':'),(';'),('`'),('^');	
	SET @TextoFinal = ''
	SET @wcount= 0 
	SET @index = 1 
	SET @len= LEN(@texto)
	--SELECT IDs FROM @ListofIDs;
	WHILE @index<= @len 
	BEGIN 
		set @char = SUBSTRING(@texto, @index, 1) 
		IF NOT EXISTS(SELECT IDs FROM @ListofIDs WHERE IDs = @char)
			SET @TextoFinal = @TextoFinal + @char
		SET @index= @index+ 1 
	END

	SET @TextoFinal = REPLACE(REPLACE(@TextoFinal,'Ñ','N'),'ñ','n')

	RETURN @TextoFinal
END



USE [Casino]
GO
/****** Object:  StoredProcedure [dbo].[SP_RouletteWheelEvents]   Script Date: 07/08/2021 07:12:10 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[SP_RouletteWheelEvents]
	
	@OPTION						INT				= 0,
	@IDROULETTEWHEEL			NUMERIC(18,0)	= 0,
	@ESTADOROULETTE				SMALLINT		= 0,
	@USERNAME					NVARCHAR(50)	= '', 
	@TYPEBET					SMALLINT		= 0, 
	@SINGLENUMBER				SMALLINT		= NULL, 
	@SINGLECOLOR				NVARCHAR(10)	= NULL,
	@AMOUNT						MONEY			= 0,
	@IDROUNDBET					NUMERIC(18, 0)	= NULL,
	@WINNINGNUMBER				SMALLINT		= NULL

 AS

---------------------------------------------------------------------------------------
-- SP, encargado de los procesos respectivos para el funcionamiento
-- de la ruleta: Crear, consultar, agregar apuesta. 
-- en la(s) tabla(s) : dbo.Bets,dbo.RouletteWheel,dbo.RoundBet,dbo.WinnerBet
---------------------------------------------------------------------------------------
-- Fecha : 07/08/2021                                               --
-- Autor : Harold Beltrán
--------------------------------------------------------------------------------------- 

BEGIN -- del SP

SET NOCOUNT ON

SET QUOTED_IDENTIFIER OFF

---------------------
--
DECLARE @Error_Code		varchar( 10 )   = '0' 
DECLARE @Error_Desc		varchar( 100 )  = ''

DECLARE @WINNINGCOLOR NVARCHAR(10)

--
BEGIN TRY
-------------------------------
-- Devuelve Datos
-------------------------------
--
	--OPTION = 100: Crear nueva ruleta
	--Parámetros: @ESTADOROULETTE
	IF @OPTION = 100
	BEGIN
		--Inicio TrInsRoulette: Insertar una nueva ruleta
		BEGIN TRAN TrInsRoulette
			INSERT INTO dbo.RouletteWheel 
				(Estado)
			VALUES 
				(@ESTADOROULETTE)
		COMMIT TRAN 		
		--Fin TrInsRoulette

		SELECT 
			'1' AS 'Result',
			MAX(IdRouletteWheel) AS ID
		FROM dbo.RouletteWheel
	END

	--Option 110: Cambiar estado de ruleta
	--Parámetros: @IDROULETTEWHEEL, @ESTADOROULETTE
	IF @OPTION = 110
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM dbo.RouletteWheel WHERE IdRouletteWheel = @IDROULETTEWHEEL)
		BEGIN
			SELECT '2' AS 'Result', 'La ruleta indicada no existe.' AS 'Message'
			RETURN
		END

		--Inicio TrUpdRoulette: Cambiar Estado de la tabla dbo.RouletteWheel
		BEGIN TRAN TrUpdRoulette
			UPDATE dbo.RouletteWheel
			SET Estado = @ESTADOROULETTE
			WHERE IdRouletteWheel = @IDROULETTEWHEEL
		COMMIT TRAN TrUpdRoulette
		--Fin TrUpdRoulette

		IF @ESTADOROULETTE = 1
		BEGIN
			--Abrir ronda de apuestas
			EXEC SP_RouletteWheelEvents @OPTION = 140, @IDROULETTEWHEEL = @IDROULETTEWHEEL
			SELECT '1' AS 'Result', 'La ruleta está ahora disponible.' AS 'Message'
			RETURN
		END
		ELSE	
		BEGIN
			--Cerrar ronda de apuestas
			EXEC SP_RouletteWheelEvents @OPTION = 150, @IDROULETTEWHEEL = @IDROULETTEWHEEL
			RETURN
		END
	END
	
	--Option 120: Validar si existe la ruleta y su estado actual
	--Parámetros: @IDROULETTEWHEEL
	IF @OPTION = 120
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM dbo.RouletteWheel WHERE IdRouletteWheel = @IDROULETTEWHEEL)
		BEGIN
			SELECT '2' AS 'Result', 'La ruleta indicada no existe.' AS 'Message'
			RETURN
		END

		IF (SELECT Estado FROM dbo.RouletteWheel WHERE IdRouletteWheel = @IDROULETTEWHEEL) <> 1
		BEGIN
			SELECT '2' AS 'Result', 'La ruleta indicada no está disponible.' AS 'Message'
			RETURN
		END

		SELECT '1' AS 'Result', 'La ruleta indicada está disponible.' AS 'Message'
	END


	--Option 130:Agregar nueva apuesta a la ruleta
	--Parámetros: @IDROULETTEWHEEL, @USERNAME, @TYPEBET, @AMOUNT, @SINGLENUMBER, @SINGLECOLOR
	IF @OPTION = 130
	BEGIN
		--Validar si hay ronda de apuestas abierta
		EXEC SP_RouletteWheelEvents @OPTION = 140, @IDROULETTEWHEEL = @IDROULETTEWHEEL		

		--Inicio TrInsBets: Agregar apuesta a la ronda de apuestas de la ruleta.
		BEGIN TRAN TrInsBets
			SELECT @IDROUNDBET = MAX (IdRoundBet) 
			FROM dbo.RoundBet 
			WHERE IdRouletteWheel = @IDROULETTEWHEEL
			AND RoundBetOpen = 1

			INSERT INTO dbo.Bets
				(IdRoundBet
				,Username
				,TypeBet
				,SingleNumber
				,SingleColor
				,Amount)
			VALUES
				(@IDROUNDBET
				,@USERNAME
				,@TYPEBET
				,@SINGLENUMBER
				,@SINGLECOLOR
				,@AMOUNT)
		COMMIT TRAN TrInsBets
		--Fin TrInsBets

		SELECT '1' AS 'Result', 'Apuesta realizada exitosamente' AS 'Message'
	END

	--Option 140: Valida si hay una ronda de apuestas para la ruta, si no la crea.
	--Parámetros: @IDROULETTEWHEEL
	IF @OPTION = 140
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM dbo.RoundBet WHERE IdRouletteWheel = @IDROULETTEWHEEL
						AND RoundBetOpen = 1)
		BEGIN
			--Inicio TrInsRoundBet: Abrir ronda de apuestas
			BEGIN TRAN TrInsRoundBet
				INSERT INTO dbo.RoundBet
					(IdRouletteWheel, RoundBetOpen)
				VALUES 
					(@IDROULETTEWHEEL, 1)
			COMMIT TRAN TrInsRoundBet
			--Fin TrInsRoundBet
		END
	END

	--Option 150: Valida si hay una ronda de apuestas abierta para la ruleta y la cierra.
	--Parámetros: @IDROULETTEWHEEL
	IF @OPTION = 150
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM dbo.RoundBet WHERE IdRouletteWheel = @IDROULETTEWHEEL
						AND RoundBetOpen = 1)
		BEGIN			
			SELECT '2' AS 'Result', 'La ruleta indicada estaba cerrada con anterioridad.' AS 'Message'
			RETURN
		END
		SELECT @IDROUNDBET = MAX(IdRoundBet) 
		FROM dbo.RoundBet 
		WHERE IdRouletteWheel = @IDROULETTEWHEEL
		AND RoundBetOpen = 1

		--Inicio TrInsRoundBet: Abrir ronda de apuestas
		BEGIN TRAN TrInsRoundBet
			UPDATE dbo.RoundBet
			SET RoundBetOpen = 0, DateClosed = GETDATE()
			WHERE IdRouletteWheel = @IDROULETTEWHEEL
			AND IdRoundBet = @IDROUNDBET
		COMMIT TRAN TrInsRoundBet
		--Fin TrInsRoundBet

		SELECT '1' AS 'Result', @IDROUNDBET AS 'IdRoundBet'
	END

	--Option 160: Registra el número ganador en la tabla dbo.WinningBets.
	--Parámetros: @IDROUNDBET, @WINNINGNUMBER
	IF @OPTION = 160
	BEGIN
		IF EXISTS (SELECT 1 FROM dbo.WinningBets WHERE IdRoundBet = @IDROUNDBET)
		BEGIN
			SELECT '2' AS 'Result', 'Esta ronda ya fue cerrada.' AS 'Mensaje'
			RETURN
		END

		SELECT @WINNINGCOLOR = 
		(CASE
			WHEN @WINNINGNUMBER % 2 = 0 THEN 'ROJO'
			WHEN @WINNINGNUMBER % 2 = 1 THEN 'NEGRO'
		END)

		--Inicio TrInsWinningBet:  Registra el número ganador de la ronda de apuestas
		BEGIN TRAN TrInsWinningBet
			INSERT INTO dbo.WinningBets
				(IdRoundBet, WinningNumber, WinningColor)
			VALUES
				(@IDROUNDBET, @WINNINGNUMBER, @WINNINGCOLOR)
		COMMIT TRAN TrInsWinningBet 
		--Fin TrInsWinningBet

		SELECT '1' AS 'Result', 'Número ganador guardado exitosamente.' AS 'Message'
	END

	--Option 170: Lista de los ganadores de la ronda.
	--Parámetros: @IDROUNDBET
	IF @OPTION = 170
	BEGIN
		SELECT @SINGLENUMBER = WinningNumber
		FROM dbo.WinningBets
		WHERE IdRoundBet = @IDROUNDBET

		SET @SINGLECOLOR = (SELECT CASE 
								WHEN @SINGLENUMBER % 2 = 0 THEN 'ROJO'
								WHEN @SINGLENUMBER % 2 = 1 THEN 'NEGRO'
							END)
		
		IF NOT EXISTS (SELECT 1 FROM Bets 
						WHERE IdRoundBet = @IDROUNDBET
						AND (SingleNumber = @SINGLENUMBER OR SingleColor = @SINGLECOLOR))
		BEGIN
			SELECT '2' AS 'Result', 'No hubo ganadores en esta ronda.' AS 'Message'
			RETURN
		END
		
		SELECT 
			'1' AS 'Result',
			Username,
			B.TypeBet,
			IIF(B.TypeBet = 1, CAST(B.SingleNumber AS NVARCHAR(10)), B.SingleColor) AS 'SingleWinner',
			Amount * P.BetPrize AS 'PayOut'
		FROM Bets B
		INNER JOIN Prizes P ON P.TypeBet = B.TypeBet
		WHERE B.IdRoundBet = @IDROUNDBET
		AND (B.SingleNumber = @SINGLENUMBER OR B.SingleColor = @SINGLECOLOR)			
	END 

END TRY
---------------------
BEGIN CATCH
--
	IF @@TRANCOUNT > 0
		ROLLBACK TRAN

    SELECT
	  CONCAT('-',ERROR_NUMBER()) AS 'Result'
    , ERROR_NUMBER() AS NUMBER_OF_ERROR
    , ERROR_SEVERITY() AS SEVERITY_OF_ERROR
    , ERROR_STATE() AS STATE_OF_ERROR
    , ERROR_PROCEDURE() AS PROCEDURE_OF_ERROR
    , ERROR_LINE() AS LINE_OF_ERROR
    , ERROR_MESSAGE() AS 'Message'

--
END CATCH
---------------------
SET NOCOUNT OFF
SET QUOTED_IDENTIFIER ON
END -- del SP
GO

--EXEC SP_RouletteWheelEvents @OPTION = 110, @IDROULETTEWHEEL = 1, @ESTADOROULETTE = 0
--EXEC SP_RouletteWheelEvents @OPTION = 130, 
--							@IDROULETTEWHEEL = 3, 
--							@USERNAME = 'JugadorProfesional', 
--							@TYPEBET = 2, 
--							@SINGLENUMBER = NULL, 
--							@SINGLECOLOR = 'ROJO',
--							@AMOUNT = 1000

--EXEC SP_RouletteWheelEvents @OPTION = 160, @IDROUNDBET = 5, @WINNINGNUMBER = 27
--	
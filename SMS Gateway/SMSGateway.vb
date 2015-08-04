Imports System
Imports System.IO
Imports System.Configuration
Imports System.Globalization
Imports System.Threading
Imports System.Text.RegularExpressions

Module SMSGateway
    ' Conexion a la base de datos
    Public conn As New Conexion

    ' Puerto Serie
    Private WithEvents puertoSerie As New System.IO.Ports.SerialPort
    Private readBuffer As String = String.Empty
    Private _timeout As Integer = 20
    'Delegate Sub SetTextCallBack(ByVal [text] As String)

    ' Expresion regular
    Private regex As Regex = New Regex("^(\s\d+,.*,[0-9]+,,\d{2}/\d{2}/\d{2},\d{2}:\d{2}:\d{2}-\d{2}\s).*")
    Private match As Match

    ' Variable de error
    Private _error As String = "Desconocido"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property timeout() As Integer
        Get
            Return _timeout
        End Get
        Set(ByVal value As Integer)
            _timeout = value
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Sub Main()
        Try
            ' Establece el archivo de Log
            Log.archivo = "Log.txt"

            ' Mensaje de inicio
            Log.escribir("Aplicación Iniciada")

            ' Crea la conexion a la base de datos
            Log.consola("Creando conexión a la base de datos...")
            conn.servidor = My.Settings.ServidorBD
            conn.usuario = My.Settings.UsuarioBD
            conn.password = My.Settings.PasswordBD
            conn.baseDatos = My.Settings.NombreBD
            conn.connect()
            conn.open()


            ' Verifica el estado de la Conexión a la base de datos
            If conn.comprobarConexion IsNot Nothing Then
                Log.consola("Conectado correctamente a la base de datos!")
            Else
                Log.consola("Error: no se pudo conectar a la base de datos. Reintentando en 30s")

                ' Cierre seguro
                cierreSeguro()

                ' Espera 30seg 
                Thread.Sleep(30000)

                ' Vuelve a empezar la ejecución
                Main()
            End If

            ' Configura el puerto serie
            Log.consola("Configurando y abriendo el puerto serie...")

            ' Valida el estado del puerto serie
            If puertoSerie.IsOpen = False Then
                With puertoSerie
                    .ParityReplace = &H3B                    ' replace ";" when parity error occurs
                    .PortName = My.Settings.Puerto
                    .BaudRate = My.Settings.BaudRate
                    .Parity = IO.Ports.Parity.None
                    .DataBits = 8
                    .StopBits = IO.Ports.StopBits.One
                    .Handshake = IO.Ports.Handshake.None
                    .RtsEnable = False
                    .ReceivedBytesThreshold = 1             'threshold: one byte in buffer > event is fired
                    .NewLine = vbCr         ' CR must be the last char in frame. This terminates the SerialPort.readLine
                    .ReadTimeout = 10000
                    .RtsEnable = True
                End With

                ' Abre el puerto
                Try
                    puertoSerie.Open()

                    Log.consola("Puerto serie abierto correctamente!")
                Catch ex As Exception
                    Log.consola("Error: No se ha podido abrir el puerto. Reintentando en 30s")

                    ' Cierre seguro
                    cierreSeguro()

                    ' Espera 30seg 
                    Thread.Sleep(30000)

                    ' Vuelve a empezar la ejecución
                    Main()
                End Try
            End If

            ' Configura el Modem
            If configurarModem() = False Then
                Log.consola("Modem configurado correctamente!")
            Else
                Log.consola("Error: No se ha podido configurar el modem. Reintentando en 30s")

                ' Cierre seguro
                cierreSeguro()

                ' Espera 30seg 
                Thread.Sleep(30000)

                ' Vuelve a empezar la ejecución
                Main()
            End If

            ' Ciclo Infinito
            While (True)

                ' Verifica el estado de la Conexión a la base de datos
                If conn.comprobarConexion IsNot Nothing Then
                    Log.consola("Conexión activa a la base de datos!")
                Else
                    ' Espera 30seg
                    Thread.Sleep(30000)

                    ' Intenta abrir nuevamente la conexion a la BD
                    conn.open()

                    ' Vuelve y valida
                    If conn.comprobarConexion Is Nothing Then
                        _error = "No se ha podido conectar a la base de datos."
                        Exit While
                    End If
                End If

                ' Verifica el estado del PuertoSerie. 
                ' Si está abierto continua. Si está cerrado trata de abrirlo nuevamente
                If verificarPuertoSerie() = False Then
                    ' Trata de abrir nuevamente el puerto
                    puertoSerie.Open()

                    ' Vuelve y valida
                    If verificarPuertoSerie() = False Then
                        _error = "No se ha podido verificar el puerto serie."
                        Exit While
                    End If
                End If

                ' Verifica el estado del modem
                If verificarModem() = False Then

                    ' Espera 30seg por si el modem tuvo un reset programado
                    Thread.Sleep(30000)

                    If verificarModem() = False Then
                        _error = "No se ha podido generar la comunicación con el modem."
                        Exit While
                    End If
                End If

                ' Verifica si hay mensajes entrantes
                smsEntrante()

                ' Envia los mensajes pendientes
                smsSaliente()

                ' Espera para la siguiente ejecución
                Thread.Sleep(My.Settings.IntervaloConsulta * 1000)
            End While

            ' El puerto serie está cerrado, intenta abrirlo
            Log.consola("Fin de ciclo infinito. Error: " & _error)
            Log.escribir("Fin de ciclo infinito. Error: " & _error)
            Email.send("Fin de ciclo infinito. Error: " & _error)

            ' Realiza el cierre seguro
            cierreSeguro()

            ' Informa al usuario del reinicio automático de la APP
            Log.consola("La aplicación ha fallado, será reiniciada.")
            Log.escribir("La aplicación ha fallado, será reiniciada.")
            Email.send("La aplicación ha fallado, será reiniciada.")

            ' Reset
            Main()
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString & ". La aplicación ha fallado, será reiniciada.")
            Log.escribir("ERROR! " & ex.ToString & ". La aplicación ha fallado, será reiniciada.")
            Email.send("ERROR! " & ex.ToString & ". La aplicación ha fallado, será reiniciada.")

            ' Cierre seguro
            cierreSeguro()

            ' Espera 600seg
            Thread.Sleep(600000)

            ' Reset
            Main()
        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Function configurarModem()
        Dim _error As Boolean = False
        Try
            ' Inicializa el Modem en modo texto
            If enviarLeer("AT+CSMP=17,167,0,0", "OK") = False Then
                _error = True
            End If

            ' Selecciona el modo texto
            If enviarLeer("AT+CMGF=1", "OK") = False Then
                _error = True
            End If

            ' Habilita la recepción de mensajes
            If enviarLeer("AT+CNMI=1,1,0,0,0", "OK") = False Then
                _error = True
            End If

            ' Graba la configuracion
            If enviarLeer("AT+CSAS", "OK") = False Then
                _error = True
            End If
        Catch ex As Exception
            _error = True

            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try

        Return _error
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Function verificarPuertoSerie() As Boolean
        Try
            ' Verifica que el puerto esté abierto
            If puertoSerie.IsOpen = True Then
                Return True
            Else
                ' El puerto serie está cerrado
                Log.escribir("El puerto serie no está abierto.")
                Log.consola("El puerto serie no está abierto.")
                Email.send("El puerto serie no está abierto.")
                Return False
            End If
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)

            Return False
        End Try
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Function verificarModem()
        Try
            ' Verifica que el modem responda
            Log.consola("Verificando el estado del modem: AT")
            If enviarLeer("AT", "OK") = True Then
                ' Muestra la respuesta del Modem
                Log.consola("Estado del modem: " & readBuffer.Trim)

                Return True
            Else
                ' El modem está bloqueado
                Log.escribir("No se recibió respuesta del modem.")
                Log.consola("No se recibió respuesta del modem.")

                Return False
            End If
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)

            Return False
        End Try
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub smsSaliente()
        Dim pendientes As ArrayList

        Try
            ' Verifica la tabla de mensajes en cola
            pendientes = Mensaje.loadPendientes(conn)

            If pendientes.Count > 0 Then

                For Each pendiente As Mensaje In pendientes
                    Log.consola("Procesando mensaje: " & pendiente.mnsje)

                    ' Envia el comando de inicio del SMS y espera por el >
                    enviarLeer("AT+CMGS=""" + pendiente.tlfno + """", ">")

                    ' Envia el mensaje y cierra con Control-Z Chr(26) 
                    If enviarLeer(pendiente.mnsje & Chr(26), "CMGS") = True Then
                        ' Marca el mensaje como enviado
                        Mensaje.cambiarEstado(conn, pendiente.id_mnsje, "E")

                        ' Avisa al usuario
                        Log.consola("Mensaje enviado correctamente")
                    Else
                        Log.consola("No se pudo enviar el mensaje.")
                    End If

                Next
            Else
                Log.consola("No hay mensajes pendientes por enviar.")
            End If


        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub puertoSerie_DataReceived(ByVal sender As System.Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles puertoSerie.DataReceived
        Try
            ' Almacena en el Buffer lo que va entrando por el puerto Serie
            readBuffer += puertoSerie.ReadExisting()
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub

    Private Sub puertoSerie_ErrorReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialErrorReceivedEventArgs) Handles puertoSerie.ErrorReceived
        Try
            ' Almacena en el Buffer lo que va entrando por el puerto Serie
            readBuffer += puertoSerie.ReadExisting()
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="text"></param>
    ''' <remarks></remarks>
    Public Sub enviar(ByVal text As String)
        Try
            If text <> "" And puertoSerie.IsOpen Then
                puertoSerie.Write(text & vbCr)
            End If
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub smsEntrante()
        Dim provider As CultureInfo = CultureInfo.InvariantCulture
        Dim pattern As String = "dd/MM/yy"

        Dim arreglo As String()
        Dim arr As New ArrayList
        'Dim temp As String()
        Dim tmp As New ArrayList
        Dim head As String()
        Dim body As String
        Dim telefono As String
        Dim timestamp As String
        Dim id As String
        Dim fecha As Date
        Dim hora As Date
        Dim mnsje As String


        Try
            ' Pide los mensajes que no han sido leidos
            Log.consola("Obteniendo mensajes entrantes...")
            enviarLeer("AT+CMGL=""ALL""", "OK")

            ' Muestra por consola el contenido del Buffer en Hexadecimal
            'Log.consola("Hexa: " & StrToHex(readBuffer))

            ' Valida el contenido del Buffer
            If String.IsNullOrEmpty(readBuffer.Trim) = True Then
                Exit Sub
            End If

            ' Limpia el Buffer
            readBuffer = readBuffer.Replace(Chr(34), "").Trim()

            ' Crea un arreglo a partir del Buffer recibido
            arreglo = regex.Split(readBuffer, "\+CMGL:")

            ' Limpiar el arreglo eliminando nulos o vacios y que no tengan el formato del SMS
            arr = limpiarArreglo(arreglo)

            If arr.Count > 0 Then

                Log.consola("Hay " & arr.Count.ToString & " mensaje(s) nuevos.")

                For j = 0 To arr.Count - 1

                    ' Valida que cumpla el formato del SMS, si NO tiene el formato de SMS salta al siguiente
                    ''Ejemplo
                    ''CMGL: 3,REC UNREAD,3176435455,,11/08/04,10:45:02-20'
                    match = regex.Match(arr(j))
                    If match.Success = False Then
                        Continue For
                    End If

                    Dim temporal_head As String
                    Dim temporal_cuerpo As String

                    temporal_head = match.Groups(1).Value
                    temporal_cuerpo = arr(j).Replace(temporal_head, "")

                    'Obtiene la información del mensaje
                    head = temporal_head.Split(",")
                    body = temporal_cuerpo

                    ' Obtiene el ID del mensaje
                    id = head(0).Replace(Chr(34), "").Replace("CMGL:", "").Trim

                    ' Obtiene el número telefónico
                    telefono = head(2).Replace(Chr(34), "").Trim

                    ' Formatea la hora para crear las conversiones
                    ' Elimina caracteres adicionales a la hora 10:45:02-20
                    timestamp = head(4).Replace(Chr(34), "").Trim() & " " & head(5).Replace(Chr(34), "").Substring(0, 8).Trim()
                    fecha = DateTime.ParseExact(timestamp, "yy/MM/dd HH:mm:ss", Nothing)
                    hora = DateTime.ParseExact(timestamp, "yy/MM/dd HH:mm:ss", Nothing)

                    ' Obtiene el contenido del mensaje SMS
                    mnsje = body.Replace(vbCrLf, "<br>").Replace(vbCr, "<br>").Replace(vbLf, "<br>").Trim

                    Log.consola("Procesando mensaje: " & id.ToString)
                    Log.consola("Telefono: " & telefono.ToString)
                    Log.consola("Fecha: " & fecha.ToString("dd/MM/yyyy"))
                    Log.consola("Hora: " & hora.ToString)
                    Log.consola("Mensaje: " & mnsje.ToString)

                    ' Crea un arreglo con la información
                    Dim data As New ArrayList
                    data.Add(mnsje)
                    data.Add(telefono)
                    data.Add(fecha.ToString("dd/MM/yyyy"))
                    data.Add(hora.ToString("HH:mm:ss"))
                    data.Add("R")

                    ' Graba el mensaje leido a la Base de datos
                    If Mensaje.insertar(conn, data) = True Then
                        ' Borra el mensaje de la SIM
                        If enviarLeer("AT+CMGD=0," & id, "OK") = True Then
                            Log.consola("Mensaje:  " & id.ToString & " borrado correctamente!")
                        End If

                        Log.consola("Continuando con el siguiente mensaje.")
                    End If
                Next
            Else
                Log.consola("No hay mensajes entrantes.")
            End If
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub eliminarSms()
        Try
            ' Pide los mensajes que no han sido leidos
            Log.consola("Eliminando mensajes de la tarjeta SIM")
            enviarLeer("AT+CMGD=0,4", "OK")
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="respuesta"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function enviarLeer(ByVal text As String, ByVal respuesta As String)
        Dim maxLecturas As Integer = 0
        Static start_time As DateTime
        Static stop_time As DateTime
        Dim elapsed_time As Double

        Try
            If text <> "" And puertoSerie.IsOpen Then
                ' Escribe en el puerto serie

                puertoSerie.Write(text & vbCr)

                start_time = Now
                readBuffer = ""
                ' Espera por la respuesta del comando
                Do
                    stop_time = Now
                    elapsed_time = stop_time.Subtract(start_time).TotalSeconds
                Loop Until (InStr(readBuffer, respuesta) Or InStr(readBuffer, "ERROR") Or elapsed_time > timeout)

                ' Evalua que lo recibido sea lo que se espera
                If InStr(readBuffer, respuesta) Then
                    Return True
                End If
            End If
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)

            Return False
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Metodo para limpiar el arreglo de SMS
    ''' </summary>
    ''' <param name="arreglo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function limpiarArreglo(ByVal arreglo As Array) As ArrayList
        Dim arr As New ArrayList

        Try
            For j = 0 To arreglo.Length - 1

                ' Valida que el contenido no sea nulo o vacio
                If String.IsNullOrEmpty(arreglo(j).Trim) = False Then


                    ' Valida que tenga el formato del SMS
                    match = regex.Match(arreglo(j))
                    If match.Success = True Then
                        arr.Add(arreglo(j))
                    End If
                End If
            Next
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try

        Return arr
    End Function


    ''' <summary>
    ''' Metodo para convertir una cadena de ASCII en una cadena de Hexadecimal
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function StrToHex(ByRef Data As String) As String
        Dim sVal As String
        Dim sHex As String = ""

        Try
            While Data.Length > 0
                sVal = Conversion.Hex(Strings.Asc(Data.Substring(0, 1).ToString()))
                Data = Data.Substring(1, Data.Length - 1)
                If sVal.Length = 1 Then
                    sHex &= "0" & sVal & " "
                Else
                    sHex &= sVal & " "
                End If

            End While
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try

        Return sHex
    End Function

    ''' <summary>
    ''' Metodo para hacer todo el proceso de cierre seguro
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub cierreSeguro()
        Try
            ' Cierra el puerto
            If puertoSerie.IsOpen Then
                puertoSerie.Close()
            End If

            ' Cierra la conexion a la BD
            conn.close()
        Catch ex As Exception
            Log.consola("ERROR! " & ex.ToString)
            Log.escribir("ERROR! " & ex.ToString)
            Email.send("ERROR! " & ex.ToString)
        End Try
    End Sub
   
End Module

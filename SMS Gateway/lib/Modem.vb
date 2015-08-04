Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel

Public Class Modem
    Private buffer As String = String.Empty
    Private _timeout As Integer = 20
    Private _PuertoSerie As System.IO.Ports.SerialPort

    Public Property PuertoSerie() As System.IO.Ports.SerialPort
        Get
            Return _PuertoSerie
        End Get
        Set(ByVal value As System.IO.Ports.SerialPort)
            _PuertoSerie = value
        End Set
    End Property

    Public Property timeout() As Integer
        Get
            Return _timeout
        End Get
        Set(ByVal value As Integer)
            _timeout = value
        End Set
    End Property

    Public Sub New(ByVal _puerto As System.IO.Ports.SerialPort)
        Try
            _PuertoSerie = _puerto

        Catch ex As Exception

        End Try
    End Sub

    'Public Function enviarAT(ByVal cadena As String)
    '    Try
    '        Dim retorna As String

    '        ' Manda el comando por el puerto serie
    '        _PuertoSerie.Write(cadena & vbCr)

    '        Me.leer()

    '        If InStr(buffer, "OK" & vbCrLf) Then
    '            buffer = buffer.Trim
    '            If buffer.Length > 2 Then
    '                buffer = buffer.Substring(0, buffer.Length - 2)
    '            End If
    '            retorna = buffer.Trim
    '        Else
    '            retorna = Nothing
    '        End If

    '        Return retorna
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return Nothing
    '    End Try
    'End Function

    '''' <summary>
    '''' 
    '''' </summary>
    '''' <param name="cadena"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Function enviar(ByVal cadena As String)
    '    Try
    '        ' Manda el comando por el puerto serie
    '        _PuertoSerie.Write(cadena & vbCr)

    '        Me.leer()

    '        Return buffer.Trim
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return Nothing
    '    End Try
    'End Function


    '''' <summary>
    '''' 
    '''' </summary>
    '''' <param name="respuesta"></param>
    '''' <remarks></remarks>
    'Private Sub leer(Optional ByVal respuesta As String = "OK")
    '    Dim maxLecturas As Integer = 0
    '    Static start_time As DateTime
    '    Static stop_time As DateTime
    '    Dim elapsed_time As Double

    '    start_time = Now
    '    buffer = ""

    '    Do
    '        stop_time = Now
    '        elapsed_time = stop_time.Subtract(start_time).TotalSeconds

    '        Application.DoEvents()

    '    Loop Until (InStr(buffer, respuesta) Or InStr(buffer, "ERROR") Or elapsed_time > Me.timeout)

    'End Sub

    '''' <summary>
    '''' Metodo para enviar un mensaje por el puerto serie y espera hasta que llegue la respuesta
    '''' </summary>
    '''' <param name="enviar"></param>
    '''' <param name="verificar"></param>
    '''' <remarks></remarks>
    'Public Sub enviarLeer(ByVal enviar As String, ByVal verificar As String)
    '    Try
    '        Me.enviar(enviar)

    '        Me.leer(verificar)

    '    Catch ex As Exception

    '    End Try
    'End Sub

    Public Function crearArregloDeMensajes(ByVal respuestaAt As String) As String()
        Dim mensajes As String()
        Dim temp As String()
        Dim mensajeActual As Integer = -1
        temp = respuestaAt.Split("+CMGL:")

        If temp.Length = 0 Then
            Return Nothing
        End If

        'ReDim mensajes(1)
        ReDim mensajes(temp.Length - 1)
        For Each parte As String In temp
            If String.IsNullOrEmpty(parte) Or parte Is Nothing Then
                ' Sale del For Each
                Continue For
            Else
                If (InStr(parte, "CMGL:") = 0) Then
                    ' Es parte del ultimo mensaje
                    mensajes(mensajeActual) += parte
                Else
                    ' Es un nuevo mensaje e incrementamos el contador
                    mensajeActual += 1
                    mensajes(mensajeActual) += parte
                    ' MsgBox(parte)
                End If
            End If

        Next
        Return mensajes
    End Function

    'Public Function verificarConexion()
    '    Dim envioAT As String

    '    envioAT = Me.enviarAT("AT" + Chr(13))

    '    If envioAT <> "OK" Then
    '        Return Nothing
    '    End If

    '    Return envioAT
    'End Function

    'Public Function haySMS()
    '    Dim envioAT As String

    '    envioAT = Me.enviarAT("AT+CNMI?" + Chr(13))

    '    If InStr(envioAT, "+CNMI:") = 0 Then
    '        Return Nothing
    '    End If

    '    Return envioAT
    'End Function

    'Public Function listaSMS() As String
    '    Dim envioAT As String

    '    envioAT = Me.enviarAT("AT+CMGL=" + Chr(34) + "ALL" + Chr(34) + Chr(13))

    '    ' Valida que dentro de la cadena contenga la palabra +CMGL:
    '    If InStr(envioAT, "+CMGL:") = 0 Then
    '        Return Nothing
    '    End If

    '    Return envioAT
    'End Function

    'Public Function borrarSMS(ByVal id As Integer)
    '    Dim envioAT As String

    '    ' Borra de la tarjeta SIM el mensaje identificado por id
    '    envioAT = Me.enviarAT("AT+CMGD=" + id.ToString + ",0" + Chr(13))

    '    Return envioAT
    'End Function

    'Public Function resetModem()
    '    Dim envioAT As String



    '    ' Resetea el Modem
    '    envioAT = Me.enviarAT("AT$RESET" + Chr(13))

    '    Return envioAT
    'End Function

    'Public Function verificarConexionGSM()
    '    Dim envioAT As String
    '    Dim envioATtemp As String
    '    Dim arreglo() As String
    '    Dim n = "", stat = "", lac = "", ci = ""

    '    ' Network Registration Info
    '    envioAT = Me.enviarAT("AT+CREG?" + Chr(13))

    '    ' Formato de respuesta +CREG: <n>,<stat>[,<lac>,<ci>]
    '    ' <n> = 0 disabled network, <n> = 1 enable network +CREG: <stat>, <n> = 2 enable network +CREG: <stat>[,<lac>,<ci>]
    '    ' <stat> = 0 not registered, <stat> = 1 registered, <stat> = 2 not registered, searching
    '    ' <stat> = 3 registration denied, <stat> = 4 unknown, <stat> = registered, roaming
    '    ' <lac> = two-byte location area code in hexadecimal format e.g. "00C3"
    '    ' <ci> = two-byte cell ID in hexadecimal format

    '    ' Valida que dentro de la cadena contenga la palabra +CREG:
    '    If InStr(envioAT, "+CREG:") = 0 Then
    '        Return Nothing
    '    End If

    '    ' Limpia la cadena recibida
    '    envioATtemp = envioAT.Replace("+CREG:", "")
    '    envioATtemp = envioATtemp.Trim()

    '    arreglo = envioATtemp.Split(",")

    '    If arreglo.Length = 2 Then
    '        ' La respuesta del modem es <n>, <stat>
    '        n = arreglo(0).Trim
    '        stat = arreglo(1).Trim
    '    End If

    '    If arreglo.Length = 4 Then
    '        ' La respuesta del modem es <n>, <stat>
    '        n = arreglo(0).Trim
    '        stat = arreglo(1).Trim
    '        lac = arreglo(2).Trim
    '        ci = arreglo(3).Trim
    '    End If

    '    If stat <> "1" Then
    '        Return Nothing
    '    End If

    '    Return envioAT
    'End Function

End Class

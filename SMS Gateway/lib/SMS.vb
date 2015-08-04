Public Class SMS

    Private _id As Integer
    Private _estado As String
    Private _telefono As String
    Private _mensaje As String
    Private _fecha_sms As String
    Private _hora_sms As String
    Private _data As String

    Public Property id() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property

    Public Property estado() As String
        Get
            Return _estado
        End Get
        Set(ByVal value As String)
            _estado = value
        End Set
    End Property

    Public Property telefono() As String
        Get
            Return _telefono
        End Get
        Set(ByVal value As String)
            _telefono = value
        End Set
    End Property

    Public Property mensaje() As String
        Get
            Return _mensaje
        End Get
        Set(ByVal value As String)
            _mensaje = value
        End Set
    End Property

    Public Property fecha_sms() As String
        Get
            Return _fecha_sms
        End Get
        Set(ByVal value As String)
            _fecha_sms = value
        End Set
    End Property

    Public Property hora_sms() As String
        Get
            Return _hora_sms
        End Get
        Set(ByVal value As String)
            _hora_sms = value
        End Set
    End Property

    Public Property data() As String
        Get
            Return _data
        End Get
        Set(ByVal value As String)
            _data = value
        End Set
    End Property

    Public Sub New(ByVal cadena As String)
        Try
            Dim arreglo As String()
            Dim head As String()
            Dim temp_id As String()
            Dim body As String

            If String.IsNullOrEmpty(cadena) Or cadena Is Nothing Then
                Exit Sub
            End If

            arreglo = cadena.Replace(Chr(34), "").Split(vbCrLf)
            If arreglo.Length < 2 Then
                head = arreglo(0).Split(",")
                body = ""
            Else
                head = arreglo(0).Split(",")
                body = arreglo(1).Trim
            End If

            ' Ejemplo
            ' CMGL: 3,"REC UNREAD","3176435455",,"11/08/04,10:45:02-20"
            ' Numero de guia: 2566390. Codigo: 4

            temp_id = head(0).Split(":")
            Me.id = CInt(temp_id(1).Trim)
            Me.estado = head(1).Trim
            Me.telefono = head(2).Trim
            Me.fecha_sms = Format(CDate("20" + head(4)), "yyyy-MM-dd")
            Me.hora_sms = Format(CDate(head(5).Substring(0, head(5).Length - 3)), "HH:mm:ss")
            Me.data = cadena
            Me.mensaje = body
        Catch ex As Exception

        End Try
    End Sub

    Public Sub enviar(ByVal telefono As String, ByVal id As String, ByVal msg As String)
        Dim mensaje As String

        mensaje = ""


    End Sub

End Class

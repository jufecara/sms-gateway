Imports Npgsql
Imports System.Reflection

Public Class Conexion
    Private conexion As NpgsqlConnection
    Private transaccion As NpgsqlTransaction
    Private stmt As NpgsqlDataReader = Nothing
    Private dbName As String = Nothing
    Private dbHost As String = Nothing
    Private dbUser As String = Nothing
    Private dbPass As String = Nothing
    Private dbPort As String = Nothing
    Private strSql As String = Nothing
    Private strError As String = Nothing
    Private connected As Boolean = False
    Private lastError As String


#Region "Propiedades"
    Public ReadOnly Property Conn() As NpgsqlConnection
        Get
            Return conexion
        End Get
    End Property

    Public WriteOnly Property servidor() As String
        Set(ByVal value As String)
            dbHost = value.ToString()
        End Set
    End Property

    Public WriteOnly Property usuario() As String
        Set(ByVal value As String)
            dbUser = value.ToString
        End Set
    End Property

    Public WriteOnly Property password() As String
        Set(ByVal value As String)
            dbPass = value.ToString
        End Set
    End Property

    Public WriteOnly Property baseDatos() As String
        Set(ByVal value As String)
            dbName = value.ToString
        End Set
    End Property

    Public ReadOnly Property getLastError()
        Get
            Return lastError
        End Get
    End Property
#End Region

    Public Sub New()
        ' Constructor
    End Sub

    Public Sub connect()
        Try
            conexion = New NpgsqlConnection("server=" + dbHost + ";uid=" + dbUser + ";Password=" + dbPass + ";database=" + dbName + ";")
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub open()
        Try
            ' Se asegura que la conexion no este abierta
            If conexion.State = ConnectionState.Closed Then
                conexion.Open()
            End If
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub close()
        Try
            conexion.Close()
            conexion = Nothing
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub beginTransaction()
        Try
            If conexion.State = False Then
                conexion.Open()
            End If

            If (stmt Is Nothing) Then
                transaccion = conexion.BeginTransaction()
            Else
                If (Not stmt.IsClosed) Then
                    stmt.Close()
                End If
                transaccion = conexion.BeginTransaction()
            End If

        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub commit()
        Try
            transaccion.Commit()
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub rollback()
        Try
            transaccion.Rollback()
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub recordError(ByVal descripcion As String)
        Me.lastError = descripcion
    End Sub

    Public Function query(ByVal sql As String)
        Try
            Dim comando As NpgsqlCommand
            'Dim stmt As NpgsqlDataReader

            comando = New NpgsqlCommand(sql, conexion)
            comando.CommandType = CommandType.Text

            If (stmt Is Nothing) Then
                stmt = comando.ExecuteReader()
            Else
                If (Not stmt.IsClosed) Then
                    stmt.Close()
                End If
                stmt = comando.ExecuteReader()
            End If

            Return stmt
        Catch ex As Exception
            'MsgBox(ex.Message)
            recordError(ex.Message)
        End Try

        Return Nothing
    End Function

    Public Function fetchObject(ByVal reader As NpgsqlDataReader, ByVal objectToReturnType As Type) As Object
        Dim props As PropertyInfo() = objectToReturnType.GetProperties()

        Try
            reader.Read()
            Dim newObjectToReturn As Object = Activator.CreateInstance(objectToReturnType)

            For i As Integer = 0 To props.Length - 1
                'objectToReturnType.InvokeMember(props(i).Name, BindingFlags.SetProperty, Nothing, newObjectToReturn, New Object() {reader(props(i).Name)})
                objectToReturnType.InvokeMember(props(i).Name, BindingFlags.SetProperty, Nothing, newObjectToReturn, New Object() {reader.GetValue(reader.GetOrdinal((props(i).Name)))})
            Next

            Return newObjectToReturn
        Catch ex As Exception
            'MsgBox(ex.Message)
            recordError(ex.Message)
        End Try

        Return Nothing
    End Function

    Public Function fetchAll(ByVal reader As NpgsqlDataReader, ByVal objectToReturnType As Type) As ArrayList
        Dim props As PropertyInfo() = objectToReturnType.GetProperties()
        Dim array As New ArrayList()

        Try
            While (reader.Read())
                Dim newObjectToReturn As Object = Activator.CreateInstance(objectToReturnType)

                For i As Integer = 0 To props.Length - 1
                    'objectToReturnType.InvokeMember(props(i).Name, BindingFlags.SetProperty, Nothing, newObjectToReturn, New Object() {reader(props(i).Name)})
                    objectToReturnType.InvokeMember(props(i).Name, BindingFlags.SetProperty, Nothing, newObjectToReturn, New Object() {reader.GetValue(reader.GetOrdinal((props(i).Name)))})
                Next
                array.Add(newObjectToReturn)
            End While

            Return array
        Catch ex As Exception
            'MsgBox(ex.Message)
            recordError(ex.Message)
        End Try

        Return Nothing
    End Function

    Public Function comprobarConexion()
        Dim valor As NpgsqlDataReader = Nothing
        Try
            Dim Sql As String = "SELECT version();"
            valor = Me.query(Sql)
        Catch ex As Exception
            'MsgBox(ex.Message)
            recordError(ex.Message)
            valor = Nothing
        End Try
        Return valor
    End Function

End Class

Imports Npgsql

Public Class Mensaje
    Private _id_mnsje As Integer
    Private _mnsje As String
    Private _tlfno As String
    Private _fcha_crcion As Date
    Private _hra_crcion As Date
    Private _estdo As String
    'Private _fcha_envio As Date
    'Private _hra_envio As Date

    Private Const dbTable As String = "mensaje"
    Private Const dbSchema As String = "sms"

#Region "Propiedades"
    Public Property id_mnsje() As Integer
        Get
            Return _id_mnsje
        End Get
        Set(ByVal value As Integer)
            _id_mnsje = value
        End Set
    End Property
    Public Property mnsje() As String
        Get
            Return _mnsje
        End Get
        Set(ByVal value As String)
            _mnsje = value
        End Set
    End Property
    Public Property tlfno() As String
        Get
            Return _tlfno
        End Get
        Set(ByVal value As String)
            _tlfno = value
        End Set
    End Property
    Public Property fcha_crcion() As Date
        Get
            Return _fcha_crcion
        End Get
        Set(ByVal value As Date)
            _fcha_crcion = value
        End Set
    End Property
    Public Property hra_crcion() As Date
        Get
            Return _hra_crcion
        End Get
        Set(ByVal value As Date)
            _hra_crcion = value
        End Set
    End Property
    Public Property estdo() As String
        Get
            Return _estdo
        End Get
        Set(ByVal value As String)
            _estdo = value
        End Set
    End Property
    'Public Property fcha_envio() As Date
    '    Get
    '        Return _fcha_envio
    '    End Get
    '    Set(ByVal value As Date)
    '        _fcha_envio = value
    '    End Set
    'End Property
    'Public Property hra_envio() As Date
    '    Get
    '        Return _hra_envio
    '    End Get
    '    Set(ByVal value As Date)
    '        _hra_envio = value
    '    End Set
    'End Property
#End Region

    Public Sub New()

    End Sub

    Public Shared Function loadById(ByVal conn As Conexion, ByVal id As String)
        Try
            Dim Sql As String
            Dim stmt As NpgsqlDataReader
            Dim retorna As New Mensaje

            Sql = "SELECT *FROM " + dbSchema.ToString + "." + dbTable.ToString + " WHERE id_mnsje = '" + id.ToString + "'"
            stmt = conn.query(Sql)
            retorna = conn.fetchObject(stmt, retorna.GetType)

            Return retorna
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function loadPendientes(ByVal conn As Conexion)
        Try
            Dim Sql As String
            Dim stmt As NpgsqlDataReader
            Dim retorna As ArrayList
            Dim tipo As New Mensaje

            Sql = "SELECT *FROM " + dbSchema.ToString + "." + dbTable.ToString + " WHERE estdo = 'P'"
            stmt = conn.query(Sql)
            retorna = conn.fetchAll(stmt, tipo.GetType)

            Return retorna
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function insertar(ByVal conn As Conexion, ByVal array As ArrayList)
        Try
            Dim Sql As String
            Dim stmt As NpgsqlDataReader

            Sql = "INSERT INTO " + dbSchema.ToString + "." + dbTable.ToString + " (mnsje, tlfno, fcha_crcion, hra_crcion, estdo) VALUES ('"
            Sql &= array(0) & "','" & array(1) & "','" & array(2) & "','" & array(3) & "','" & array(4) & "')"
            stmt = conn.query(Sql)
            If stmt Is Nothing Then
                Return False
            End If

            Return True
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function cambiarEstado(ByVal conn As Conexion, ByVal id As Integer, ByVal estado As String)
        Try
            Dim Sql As String
            Dim stmt As NpgsqlDataReader

            Sql = "UPDATE " + dbSchema.ToString + "." + dbTable.ToString + " SET estdo = '" & estado.ToString & "', "
            Sql &= "fcha_envio = current_date, hra_envio = current_time "
            Sql &= "WHERE id_mnsje = '" & id.ToString & "'"
            stmt = conn.query(Sql)

            Return True
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


End Class

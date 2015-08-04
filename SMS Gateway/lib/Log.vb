Imports System
Imports System.IO

Public Class Log

    Public Shared archivo As String
    Private Shared sw As StreamWriter

    ''' <summary>
    ''' Metodo para escribir por consola un mensaje
    ''' </summary>
    ''' <param name="mensage"></param>
    ''' <remarks></remarks>
    Public Shared Sub consola(ByVal mensage As String)
        Console.WriteLine("[" & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") & "] " & mensage)
    End Sub

    ''' <summary>
    ''' Metodo para escribir
    ''' </summary>
    ''' <param name="logMessage"></param>
    ''' <remarks></remarks>
    Public Shared Sub escribir(ByVal logMessage As String)
        Try
            If (Not File.Exists(archivo)) Then
                sw = File.CreateText(archivo)
                sw.WriteLine("Inicio del registro de incidencias. Fecha de creación: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            Else
                sw = File.AppendText(archivo)
            End If
            sw.WriteLine("{0}: {1} ", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), logMessage)
            sw.Close()
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Muestra un archivo línea por línea por Consola
    ''' </summary>
    ''' <param name="r"></param>
    ''' <remarks></remarks>
    Public Shared Sub dump(ByVal r As StreamReader)
        Dim line As String
        line = r.ReadLine()
        While Not (line Is Nothing)
            Console.WriteLine(line)
            line = r.ReadLine()
        End While
    End Sub
End Class

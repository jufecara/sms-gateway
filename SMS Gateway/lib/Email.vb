Imports System.Net.Mail

Public Class Email

    Public Shared Sub send(ByVal mensaje As String)
        Try
            Dim SmtpServer As New SmtpClient()
            Dim mail As New MailMessage()

            SmtpServer.Credentials = New Net.NetworkCredential("ingsis2@cenicana.org", "ez63tu24")
            SmtpServer.Port = 25
            SmtpServer.Host = "azucar.cenicana.org"
            mail = New MailMessage()
            mail.From = New MailAddress("ingsis2@cenicana.org")
            mail.To.Add("jfcastrillon@cenicana.org")
            mail.Subject = "SMS Gateway"
            mail.Body = "[" & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") & "]" & mensaje
            SmtpServer.Send(mail)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

End Class

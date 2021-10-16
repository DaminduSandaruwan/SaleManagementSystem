Public Class frmLogin
    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        If txtUName.Text = "Admin" And txtPass.Text = "user" Then
            MsgBox("Login Success", MsgBoxStyle.Information, "Information System")

            Dim frmCon = New frmHome
            frmCon.Show()
            Me.Hide()

        Else
            MsgBox("Login failed, Username or Password Incorrect", MsgBoxStyle.Critical, "Information System")

        End If
    End Sub
End Class
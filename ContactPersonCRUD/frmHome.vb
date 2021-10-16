Public Class frmHome
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim frmCon = New frmRegister
        frmCon.Show()
        Me.Hide()
    End Sub

    Private Sub frmHome_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim frmCon = New frmCustomer
        frmCon.Show()
        Me.Hide()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim frmCon = New frmSales
        frmCon.Show()
        Me.Hide()
    End Sub
End Class
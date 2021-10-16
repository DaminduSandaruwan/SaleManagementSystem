
Imports System.Data.OleDb
Imports System.Drawing
Imports System.IO

Public Class frmSales
    Dim PK As Integer   '// Local variable.
    Dim NewData As Boolean = False  '// NewData = True (Add), False is Edit mode.
    '//
    Dim newFileName As String   '// File name of Image (New)
    Dim orgPicName As String    '// Orginal of Image
    Dim streamPic As Stream     '// Use Steam instead IO.

    ' / --------------------------------------------------------------------------------
    Private Sub frmContactPerson_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Conn = ConnectDataBase()
        lblRecordCount.Text = ""
        Call NewMode()
        Call SetupDGVData()
        Call RetrieveData()
    End Sub

    ' / --------------------------------------------------------------------------------
    Private Sub frmContactPerson_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        If Conn.State = ConnectionState.Open Then Conn.Close()
        Conn.Dispose()
        Me.Dispose()
        Application.Exit()
    End Sub

    ' / --------------------------------------------------------------------------------
    '// Initialize DataGridView @Run Time
    Private Sub SetupDGVData()
        With dgvData
            .RowHeadersVisible = False
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .MultiSelect = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .ReadOnly = True
            .Font = New Font("Tahoma", 9)
            ' Columns Specified
            .Columns.Add("SalesPK", "SalesPK")
            .Columns.Add("InvoiceID", "Invoice ID")
            .Columns.Add("InvoiceDate", "InvoiceDate")
            .Columns.Add("PayementName", "Payement Method")
            .Columns.Add("CustomerName", "Sale Type")
            .Columns.Add("SaleType", "Customer Name")
            .Columns.Add("Brand", "Brand")
            .Columns.Add("Mobile", "Mobile")
            .Columns.Add("LineID", "Selling Price")
            .Columns.Add("Model", "Model")
            .Columns.Add("PictureName", "PictureName")
            .Columns.Add("Note", "Other Specs")
            '// Hidden Columns
            .Columns(0).Visible = False
            .Columns(7).Visible = False
            .Columns(8).Visible = False
            .Columns(9).Visible = False
            .Columns("PictureName").Visible = False
            .Columns("Note").Visible = False
            ' Autosize Column
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .AutoResizeColumns()
            '// Even-Odd Color
            .AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue
            ' Adjust Header Styles
            With .ColumnHeadersDefaultCellStyle
                .BackColor = Color.Navy
                .ForeColor = Color.Black ' Color.White
                .Font = New Font("Tahoma", 9, FontStyle.Bold)
            End With
        End With
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Collect all searches and impressions. Come in the same place
    ' / blnSearch = True, Show that the search results.
    ' / blnSearch is set to False, Show all records.
    Private Sub RetrieveData(Optional ByVal blnSearch As Boolean = False)
        strSQL = " SELECT tblSales.SalesPK, tblSales.InvoiceID, tblSales.InvoiceDate, tblSales.CustomerName, " &
                    " tblSales.Brand, tblSales.Mobile, tblSales.LineID, tblSales.Model, tblSales.PictureName, tblSales.Note, " &
                    " tblPay.PayementName, tblSaleType.SaleType " &
                    " FROM [tblPay] INNER JOIN (tblSaleType INNER JOIN tblSales ON " &
                    " tblSaleType.SaleTypePK = tblSales.SaleTypeFK) ON tblPay.PayementPK = tblSales.PaymentFK "

        '// blnSearch = True for Serach
        If blnSearch Then
            strSQL = strSQL &
                " WHERE " &
                " [InvoiceID] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [InvoiceDate] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [PayementName] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [SaleType] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [CustomerName] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [Brand] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [Mobile] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [LineID] " & " Like '%" & txtSearch.Text & "%'" & " OR " &
                " [Model] " & " Like '%" & txtSearch.Text & "%'" &
                " ORDER BY SalesPK "
        Else
            strSQL = strSQL & " ORDER BY SalesPK "
        End If
        '//
        Try
            Cmd = New OleDbCommand
            If Conn.State = ConnectionState.Closed Then Conn.Open()
            Cmd.Connection = Conn
            Cmd.CommandText = strSQL
            Dim DR As OleDbDataReader = Cmd.ExecuteReader
            Dim i As Long = dgvData.RowCount
            While DR.Read
                With dgvData
                    .Rows.Add(i)
                    .Rows(i).Cells(0).Value = DR.Item("SalesPK").ToString
                    .Rows(i).Cells(1).Value = DR.Item("InvoiceID").ToString
                    .Rows(i).Cells(2).Value = DR.Item("InvoiceDate").ToString
                    .Rows(i).Cells(3).Value = DR.Item("PayementName").ToString
                    .Rows(i).Cells(4).Value = DR.Item("SaleType").ToString
                    .Rows(i).Cells(5).Value = DR.Item("CustomerName").ToString
                    .Rows(i).Cells(6).Value = DR.Item("Brand").ToString
                    .Rows(i).Cells(7).Value = DR.Item("Mobile").ToString
                    .Rows(i).Cells(8).Value = DR.Item("LineID").ToString
                    .Rows(i).Cells(9).Value = DR.Item("Model").ToString
                    .Rows(i).Cells(10).Value = DR.Item("PictureName").ToString
                    newFileName = DR.Item("PictureName").ToString
                    .Rows(i).Cells(11).Value = DR.Item("Note").ToString
                End With
                i += 1
            End While
            lblRecordCount.Text = "[Total : " & dgvData.RowCount & " records]"
            DR.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        '//
        txtSearch.Clear()
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Double click to edit item.
    ' / By pulling data from the table to display. Do not go to the database again.
    Private Sub dgvData_DoubleClick(sender As Object, e As System.EventArgs) Handles dgvData.DoubleClick
        '// If you add / edit information should be reminded before.
        If btnDelete.Text = "Cancel" Then
            Dim Result As Byte = MessageBox.Show("Do you want to abort the on-screen action ?" & vbCrLf &
                                    "Yes, If you want to abort or " & vbCrLf &
                                    "No, you want to continue.", "Comfirm your job",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
            '// No new entries yet.
            If Result = DialogResult.No Then Exit Sub
        End If
        '//
        Dim iRow As Integer
        '// Read the value of the focus row.
        iRow = dgvData.CurrentRow.Index
        '// 
        PK = dgvData.Item(0, iRow).Value  '// Keep Primary Key
        '//
        txtInvoiceID.Text = "" & dgvData.Item(1, iRow).Value
        '// Keep the original value for later comparison.
        txtInvoiceID.Tag = txtInvoiceID.Text
        '// Using Double quote "" for trap error null value
        txtNIC.Text = "" & dgvData.Item(2, iRow).Value
        '// Load data Detail Table into ComboBox
        Call PopulateComboBox(cmbPayementMethod, "tblPay", "PayementName", "PayementPK")
        cmbPayementMethod.Text = "" & dgvData.Item(3, iRow).Value
        Call PopulateComboBox(cmbSaleType, "tblSaleType", "SaleType", "SaleTypePK")
        cmbSaleType.Text = "" & dgvData.Item(4, iRow).Value
        '//
        txtCustomerName.Text = "" & dgvData.Item(5, iRow).Value
        txtBrand.Text = "" & dgvData.Item(6, iRow).Value
        txtMobile.Text = "" & dgvData.Item(7, iRow).Value
        txtSellingPrice.Text = "" & dgvData.Item(8, iRow).Value
        txtModel.Text = "" & dgvData.Item(9, iRow).Value
        txtNote.Text = "" & dgvData.Item(11, iRow).Value
        '// Load Picture
        Call ShowPicture(dgvData.Item(10, iRow).Value)
        '// Change to Edit Mode
        NewData = False
        EditMode()
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Load table detail into ComboBox
    Public Sub PopulateComboBox(cmbCtrl As ComboBox, strTable As String, strFieldName As String, Optional ByVal strFieldPK As String = vbNullString)
        Try
            If Conn.State = ConnectionState.Closed Then Conn.Open()
            strStmt = "SELECT * FROM " & strTable & " ORDER BY " & strFieldName
            Cmd = New OleDb.OleDbCommand(strStmt, Conn)
            DR = Cmd.ExecuteReader
            Dim DT As DataTable = New DataTable
            DT.Load(DR)
            '/ Primary Key (ValueMember)
            cmbCtrl.ValueMember = strFieldPK
            '/ Display the name
            cmbCtrl.DisplayMember = strFieldName
            cmbCtrl.DataSource = DT
            DR.Close()
            Conn.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Add New Mode
    ' / --------------------------------------------------------------------------------
    Private Sub NewMode()
        '// Clear all TextBox.
        For Each c In GroupBox1.Controls
            If TypeOf c Is TextBox Then
                DirectCast(c, TextBox).Clear()
                DirectCast(c, TextBox).Enabled = False
            End If
        Next
        '// Clear all ComboBox
        For Each cbo In GroupBox1.Controls.OfType(Of ComboBox)()
            cbo.Enabled = False
        Next
        '//
        btnAdd.Enabled = True
        btnSave.Enabled = False
        btnDelete.Enabled = True
        btnDelete.Text = "Delete"
        btnRefresh.Enabled = True
        '//
        btnBrowse.Enabled = False
        btnDeleteImg.Enabled = False
        picData.Image = Image.FromFile(strPathImages & "sales.png")
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Edit Data Mode
    Private Sub EditMode()
        '// Clear all TextBox
        For Each c In GroupBox1.Controls
            If TypeOf c Is TextBox Then
                DirectCast(c, TextBox).Enabled = True
            End If
        Next
        '// Clear all ComboBox
        For Each cbo In GroupBox1.Controls.OfType(Of ComboBox)()
            cbo.Enabled = True
        Next
        btnAdd.Enabled = False
        btnSave.Enabled = True
        btnDelete.Enabled = True
        btnDelete.Text = "Cancel"
        btnRefresh.Enabled = False
        '//
        btnBrowse.Enabled = True
        btnDeleteImg.Enabled = True
    End Sub

    ' / --------------------------------------------------------------------------------
    Private Sub btnSave_Click(sender As System.Object, e As System.EventArgs) Handles btnSave.Click
        '// Validate Data
        If txtInvoiceID.Text = "" Or IsNothing(txtInvoiceID.Text) Or txtInvoiceID.Text.Length = 0 Then
            MessageBox.Show("Full name cannot be empty.", "Report Status",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtInvoiceID.Focus()
            Exit Sub
        End If
        '// NewData = True, It's Add New Mode
        If NewData Then
            '// Is there a duplicate of the existing one?
            strSQL =
                " SELECT Count(tblSales.InvoiceID) AS CountInvoiceID FROM tblSales " &
                " WHERE InvoiceID = " & "'" & txtInvoiceID.Text & "'"
            If DuplicateName(strSQL) Then
                MessageBox.Show("Duplicate InvoiceID, please enter new value.", "Report Status",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtInvoiceID.Focus()
                Exit Sub
            End If
            '//
            strSQL = "INSERT INTO tblSales(" &
                " SalesPK, InvoiceID, InvoiceDate, PaymentFK, SaleTypeFK, CustomerName, Brand, Mobile, LineID, Model, " &
                " PictureName, [Note], " &
                " DateAdded, DateModified) " &
                " VALUES(" &
                "'" & SetupNewPK() & "'," &
                "'" & txtInvoiceID.Text & "'," &
                "'" & txtNIC.Text & "'," &
                "'" & PKComboBox(cmbPayementMethod, "tblPay", "PayementName", "PayementPK") & "'," &
                "'" & PKComboBox(cmbSaleType, "tblSaleType", "SaleType", "SaleTypePK") & "'," &
                "'" & txtCustomerName.Text & "'," &
                "'" & txtBrand.Text & "'," &
                "'" & txtMobile.Text & "'," &
                "'" & txtSellingPrice.Text & "'," &
                "'" & txtModel.Text & "'," &
                "'" & GetFileImages() & "'," &
                "'" & txtNote.Text & "'," &
                "'" & Now.ToString("dd/MM/yyyy") & "'," &
                "'" & Now.ToString("dd/MM/yyyy") & "'" &
                ")"
            '// EDIT MODE
        Else
            '// If the new value (Text) with the original value (Tag) is not equal, then the value changed in field "Fullname"
            If txtInvoiceID.Text <> txtInvoiceID.Tag Then
                strSQL =
                " SELECT Count(tblSales.InvoiceID) AS CountInvoiceID FROM tblSales " &
                " WHERE InvoiceID = " & "'" & txtInvoiceID.Text & "'"
                If DuplicateName(strSQL) Then
                    MessageBox.Show("Duplicate Full name, please enter new value.", "Report Status",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtInvoiceID.Focus()
                    Exit Sub
                End If
            End If
            '// START UPDATE
            strSQL = "UPDATE tblSales SET " &
                " [InvoiceID]='" & txtInvoiceID.Text & "', " &
                " [InvoiceDate]='" & txtNIC.Text & "', " &
                " [PaymentFK]=" & PKComboBox(cmbPayementMethod, "tblPay", "PayementName", "PayementPK") & ", " &
                " [SaleTypeFK]=" & PKComboBox(cmbSaleType, "tblSaleType", "SaleType", "SaleTypePK") & ", " &
                " [CustomerName]='" & txtCustomerName.Text & "', " &
                " [Brand]='" & txtBrand.Text & "', " &
                " [Mobile]='" & txtMobile.Text & "', " &
                " [LineID]='" & txtSellingPrice.Text & "', " &
                " [Model]='" & txtModel.Text & "', " &
                " [PictureName]='" & GetFileImages() & "', " &
                " [Note]='" & txtNote.Text & "', " &
                " [DateModified]='" & Now.ToString("dd/MM/yyyy") & "'" &
                " WHERE SalesPK = " & PK & ""
        End If
        '// Insert or Update same as operation
        DoSQL(strSQL)
        '//
        cmbPayementMethod.SelectedIndex = 0
        cmbSaleType.SelectedIndex = 0

        '// Clear rows in DataGridView
        dgvData.Rows.Clear()
        '// Refresh DataGridView
        Call RetrieveData()
        '// Add New Mode
        Call NewMode()
    End Sub

    ' / --------------------------------------------------------------------------------
    '// UPDATE DATA
    Private Sub DoSQL(ByVal Sql As String)
        Cmd = New OleDb.OleDbCommand
        If Conn.State = ConnectionState.Closed Then Conn.Open()
        'MsgBox(Sql)
        Try
            Cmd.Connection = Conn
            Cmd.CommandType = CommandType.Text
            Cmd.CommandText = Sql
            Cmd.ExecuteNonQuery()
            MessageBox.Show("Records Updated Completed.", "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Cmd.Dispose()
        Catch ex As Exception
            MsgBox("Error Update: " & ex.Message)
        End Try
    End Sub

    ' / Get Filename + Extesnsion only, not Path
    Private Function GetFileImages() As String
        '// Get the Filename + Extension only
        Dim iArr() As String
        iArr = Split(newFileName, "\")
        GetFileImages = iArr(UBound(iArr))
        '//
        '// If same original and new
        If orgPicName = newFileName Then Return GetFileImages
        '// Remove original picture
        If orgPicName IsNot Nothing Or orgPicName <> "" Then
            If System.IO.File.Exists(orgPicName) = True Then
                System.IO.File.Delete(orgPicName)
            End If
        End If
        '// ------------- Copy File -------------
        ' Determine whether the source file is real or not.
        If System.IO.File.Exists(newFileName) = True Then
            ' Trap Error in the case source = destination
            If LCase(strPathImages + GetFileImages) <> LCase(newFileName) Then
                ' Copy the Source file (newFileName) to the Destination (DestFile). 
                ' If the same file is found, overwrite (OverWrite = True).
                System.IO.File.Copy(newFileName, strPathImages + GetFileImages, True)
            End If
        End If
    End Function

    ' / --------------------------------------------------------------------------------
    ' / Function to find and create the new Primary Key not to duplicate.
    Function SetupNewPK() As Long
        strSQL =
            " SELECT MAX(tblSales.SalesPK) AS MaxPK FROM tblSales "
        If Conn.State = ConnectionState.Closed Then Conn.Open()
        Cmd = New OleDb.OleDbCommand(strSQL, Conn)
        '/ Check if the information is available. And return it back
        If IsDBNull(Cmd.ExecuteScalar) Then
            '// Start at 1
            SetupNewPK = 1
        Else
            SetupNewPK = Cmd.ExecuteScalar + 1
        End If
    End Function

    ' / --------------------------------------------------------------------------------
    ' / This example uses the Fullname validation.
    Public Function DuplicateName(ByVal Sql As String) As Boolean
        If Conn.State = ConnectionState.Closed Then Conn.Open()
        Cmd = New OleDb.OleDbCommand(Sql, Conn)
        ' Return count records
        DuplicateName = Cmd.ExecuteScalar
    End Function

    ' / --------------------------------------------------------------------------------
    ' / Function insert new data in Detail Table and return Primary Key for Master Table.
    Function PKComboBox(cmbCtrl As ComboBox, strTable As String, strFieldName As String, Optional ByVal strFieldPK As String = vbNullString) As Integer
        '// If ComboBox is blank data then return 1 (Blank Data)
        If cmbCtrl.Text = "" Or cmbCtrl.Text.Length = 0 Or IsDBNull(cmbCtrl.Text) Then
            'PKComboBox = 1
            Return 1
        End If
        strSQL =
            "SELECT * FROM " & strTable & " WHERE " & strFieldName & " = " & "'" & cmbCtrl.Text & "'"
        If Conn.State = ConnectionState.Closed Then Conn.Open()
        Cmd = New OleDb.OleDbCommand(strSQL, Conn)
        '// Get the Primary Key
        Dim cmbPK As Integer = Cmd.ExecuteScalar
        '// If not have in Detail Table
        If cmbPK <= 0 Then
            strStmt =
                " SELECT MAX(" & strFieldPK & ") AS MaxPK FROM " & strTable
            If Conn.State = ConnectionState.Closed Then Conn.Open()
            Cmd = New OleDb.OleDbCommand(strStmt, Conn)
            '// Increment Primary Key with 1, and Return this value.
            PKComboBox = Cmd.ExecuteScalar + 1
            '/ Add New Data in Detail Table
            Try
                Using Comm As New OleDb.OleDbCommand()
                    With Comm
                        .Connection = Conn
                        .CommandType = CommandType.Text
                        .CommandText =
                            " INSERT INTO " & strTable & " (" & strFieldName & ", " & strFieldPK & ") VALUES (@DName, @DPK)"
                        With .Parameters
                            .Add("@DName", OleDbType.VarChar).Value = cmbCtrl.Text
                            '/ ------------------------------------------------------------------
                            .Add("@DPK", OleDbType.Integer).Value = PKComboBox
                            '/ ------------------------------------------------------------------
                        End With
                        ' Insert new record.
                        .ExecuteNonQuery()
                        .Parameters.Clear()
                        '/ ------------------------------------------------------------------
                    End With
                End Using
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        Else
            PKComboBox = cmbPK
        End If
        Cmd.Dispose()
    End Function

    '/ ------------------------------------------------------------------
    '// Load Detail Table into ComboBox
    Private Sub btnAdd_Click(sender As System.Object, e As System.EventArgs) Handles btnAdd.Click
        NewData = True  '// Add New Mode
        EditMode()
        '// Load Detail Table into ComboBox
        Call PopulateComboBox(cmbPayementMethod, "tblPay", "PayementName", "PayementPK")
        Call PopulateComboBox(cmbSaleType, "tblSaleType", "SaleType", "SaleTypePK")
        '//
        txtInvoiceID.Focus()
    End Sub

    '// Load Detail Table into ComboBox
    Private Sub btnDelete_Click(sender As System.Object, e As System.EventArgs) Handles btnDelete.Click
        '// If Edit Data Mode
        If btnDelete.Text = "Cancel" Then
            btnAdd.Enabled = True
            btnSave.Enabled = True
            btnDelete.Enabled = True
            btnDelete.Text = "Delete"
            btnRefresh.Enabled = True
            '//
            btnBrowse.Enabled = False
            btnDeleteImg.Enabled = False
            '//
            cmbPayementMethod.SelectedIndex = -1
            cmbSaleType.SelectedIndex = -1
            picData.Image = Image.FromFile(strPathImages & "sales.png")
            NewMode()
        Else
            If dgvData.RowCount = 0 Then Exit Sub
            '// Receive Primary Key value to confirm the deletion.
            Dim iRow As Long = dgvData.Item(0, dgvData.CurrentRow.Index).Value
            Dim FName As String = dgvData.Item(1, dgvData.CurrentRow.Index).Value
            Dim Result As Byte = MessageBox.Show("Are you sure you want to delete the data?" & vbCrLf & "Full Name: " & FName, "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If Result = DialogResult.Yes Then
                '// iRow is the SalesPK or Primary key that is hidden.
                strStmt = " DELETE FROM tblSales WHERE SalesPK = " & iRow
                '// UPDATE RECORD
                DoSQL(strStmt)
                '// Remove original picture from Column 10 in DataGridView (strPathImages from modDataBase.vb)
                Dim strPicName As String = strPathImages & dgvData.Item(10, dgvData.CurrentRow.Index).Value
                If strPicName IsNot Nothing Then
                    If System.IO.File.Exists(strPicName) = True Then
                        System.IO.File.Delete(strPicName)
                    End If
                End If
                '// Delete Current Row.
                dgvData.Rows.Remove(dgvData.CurrentRow)
                '//
                Call NewMode()
            End If
        End If
    End Sub

    Private Sub btnRefresh_Click(sender As System.Object, e As System.EventArgs) Handles btnRefresh.Click
        dgvData.Rows.Clear()
        Call RetrieveData()
    End Sub

    Private Sub txtSearch_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtSearch.KeyPress
        '// Undesirable characters for the database ex.  ', * or %
        txtSearch.Text = Replace(Trim(txtSearch.Text), "'", "")
        txtSearch.Text = Replace(Trim(txtSearch.Text), "%", "")
        txtSearch.Text = Replace(Trim(txtSearch.Text), "*", "")
        If Trim(txtSearch.Text) = "" Or Len(Trim(txtSearch.Text)) = 0 Then Exit Sub
        ' RetrieveData(True) It means searching for information.
        If e.KeyChar = Chr(13) Then '// Press Enter
            '// No beep.
            e.Handled = True
            '//
            dgvData.Rows.Clear()
            Call RetrieveData(True)
        End If
    End Sub

    Private Sub btnBrowse_Click(sender As System.Object, e As System.EventArgs) Handles btnBrowse.Click
        Dim dlgImage As OpenFileDialog = New OpenFileDialog()

        ' / Open File Dialog
        With dlgImage
            .InitialDirectory = strPathImages
            .Title = "Select images"
            .Filter = "Images types (*.jpg;*.png;*.gif;*.bmp)|*.jpg;*.png;*.gif;*.bmp"
            .FilterIndex = 1
            .RestoreDirectory = True
        End With
        ' Select OK after Browse ...
        If dlgImage.ShowDialog() = DialogResult.OK Then
            '// New Image
            newFileName = dlgImage.FileName
            picData.Image = Image.FromFile(newFileName)
        End If
    End Sub

    Private Sub btnDeleteImg_Click(sender As System.Object, e As System.EventArgs) Handles btnDeleteImg.Click
        If orgPicName = "" Or orgPicName.Length = 0 Then Return
        '//
        picData.Image = Image.FromFile(strPathImages & "sales.png")
        newFileName = ""
    End Sub

    ' / -----------------------------------------------------------------------------
    ' / Use Steam instead IO.
    ' / -----------------------------------------------------------------------------
    Sub ShowPicture(PicName As String)
        Dim imgDB As Image
        ' Get the name of the image file from the database.
        If PicName.ToString <> "" Then
            ' Verify that the image file meets the specified location.
            If System.IO.File.Exists(strPathImages & PicName.ToString) Then
                ' Because when deleting the image file is locked, it can not be removed.
                ' The file is closed after the image is loaded, so you can delete the file if you need to
                streamPic = File.OpenRead(strPathImages & PicName.ToString)
                imgDB = Image.FromStream(streamPic)
                picData.Image = imgDB
                ' Keep the original image file name. If it is recorded, it will be removed.
                orgPicName = strPathImages & PicName.ToString
                newFileName = orgPicName
            Else
                ' No images were retrieved from the database.
                streamPic = File.OpenRead(strPathImages & "sales.png")
                imgDB = Image.FromStream(streamPic)
                picData.Image = imgDB
                ' Keep image filename blank.
                orgPicName = ""
                newFileName = ""
            End If
            ' Is null
        Else
            streamPic = File.OpenRead(strPathImages & "sales.png")
            imgDB = Image.FromStream(streamPic)
            picData.Image = imgDB
            ' Keep image filename blank.
            orgPicName = ""
            newFileName = ""

        End If
        '//
        streamPic.Dispose()
        DR.Close()
        Cmd.Dispose()
        Conn.Close()
    End Sub

    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs) Handles GroupBox2.Enter

    End Sub

    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles GroupBox1.Enter

    End Sub

    Private Sub btnHome_Click(sender As Object, e As EventArgs) Handles btnHome.Click
        Dim frmCon = New frmHome
        frmCon.Show()
        Me.Hide()
    End Sub

    Private Sub dgvData_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvData.CellContentClick

    End Sub
End Class

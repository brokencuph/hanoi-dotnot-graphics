Public Class Form1
    Private graphics As Graphics
    Private Const columnWidth As Integer = 20
    Private column(2) As Stack(Of Rectangle)
    Private baseRect As Rectangle
    Private sStep As Integer
    Private cts As CancellationTokenSource

    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        graphics = PictureBox1.CreateGraphics()
        For i = 0 To 2
            column(i) = New Stack(Of Rectangle)
        Next
    End Sub


    Private Sub InitTower(ByVal n As Integer)
        For i = 0 To 2
            column(i) = New Stack(Of Rectangle)
        Next
        graphics.Clear(PictureBox1.BackColor)
        Dim columnRectangle(2) As Rectangle
        columnRectangle(0) = New Rectangle(PictureBox1.Width \ 4 - columnWidth \ 2,
            PictureBox1.Height \ 4, columnWidth, PictureBox1.Height \ 2)
        columnRectangle(1) = New Rectangle(PictureBox1.Width \ 2 - columnWidth \ 2,
            PictureBox1.Height \ 4, columnWidth, PictureBox1.Height \ 2)
        columnRectangle(2) = New Rectangle((PictureBox1.Width \ 4) * 3 - columnWidth \ 2,
            PictureBox1.Height \ 4, columnWidth, PictureBox1.Height \ 2)
        Dim baseRectangle As New Rectangle(columnRectangle(0).X - 2 * columnWidth,
                                           (PictureBox1.Height \ 4) * 3,
                                           columnRectangle(2).X + 5 * columnWidth - columnRectangle(0).X,
                                           columnWidth)
        graphics.FillRectangle(New SolidBrush(Color.Black), baseRectangle)
        baseRect = baseRectangle

        sStep = IIf(n > 1, ((15 \ 4) * columnWidth) \ (n - 1), 0)

        Dim solidRectangles(n - 1) As Rectangle
        For i = 0 To n - 1
            Dim x As Integer = baseRectangle.X + sStep * i \ 2
            Dim y = baseRectangle.Y - columnWidth * (i + 1)
            Dim width = 5 * columnWidth - sStep * i
            Dim height = columnWidth
            solidRectangles(i) = New Rectangle(x, y, width, height)
            column(0).Push(solidRectangles(i))
        Next
        graphics.FillRectangles(New SolidBrush(Color.Blue), solidRectangles)
        graphics.DrawRectangles(New Pen(Color.Red, 1.0), solidRectangles)
    End Sub

    Private Async Function MoveSolid(ByVal srcIndex As Integer, ByVal dstIndex As Integer) As Task
        cts.Token.ThrowIfCancellationRequested()
        Dim deltaX As Integer = (dstIndex - srcIndex) * PictureBox1.Width \ 4
        Dim y As Integer = baseRect.Y - columnWidth * (column(dstIndex).Count + 1)
        Dim rectangle As Rectangle = column(srcIndex).Pop()
        Dim x As Integer = (PictureBox1.Width \ 4) * (dstIndex + 1) - rectangle.Width \ 2
        graphics.FillRectangle(New SolidBrush(Color.White), rectangle)
        graphics.DrawRectangle(New Pen(Color.White, 1.0), rectangle)
        rectangle.X = x
        rectangle.Y = y
        graphics.FillRectangle(New SolidBrush(Color.Blue), rectangle)
        graphics.DrawRectangle(New Pen(Color.Red, 1.0), rectangle)
        column(dstIndex).Push(rectangle)

        Await Task.Run(Sub() System.Threading.Thread.Sleep(1000))
    End Function

    Private Async Function Hanoi(n As Integer, a As Integer, b As Integer, c As Integer) As Task
        If n = 1 Then
            Await MoveSolid(a, c)
        Else
            Await Hanoi(n - 1, a, c, b)
            Await MoveSolid(a, c)
            Await Hanoi(n - 1, b, a, c)
        End If

    End Function

    Private Sub button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim a As Button = TryCast(sender, Button)
        If (cts IsNot Nothing) Then
            cts.Cancel()
        End If
    End Sub

    Private Async Sub button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim n As Integer
        Try
            n = Integer.Parse(TextBox1.Text)
        Catch ex As Exception
            MessageBox.Show("输入错误")
            Return
        End Try
        Button1.Enabled = False
        Button2.Enabled = True
        cts = New CancellationTokenSource
        InitTower(n)
        Await Task.Run(Sub() System.Threading.Thread.Sleep(1000))
        Try
            Await Hanoi(n, 0, 1, 2)
        Catch ex As OperationCanceledException

        Finally
            Button1.Enabled = True
            Button2.Enabled = False
        End Try
    End Sub
End Class

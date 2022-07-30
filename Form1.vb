Imports System.Drawing.Drawing2D
Imports ImageTools.Tools

Public Class Form1
    Private Const FrameTimeCap = 6 ' how long before a new frame is drawn (in ms), 1ms is practically uncapped.

    Private Shared SW As Integer = My.Computer.Screen.Bounds.Width
    Private Shared SH As Integer = My.Computer.Screen.Bounds.Height
    Private Property SW2 As Integer
        Get
            Return SW / 2
        End Get
        Set(value As Integer)
        End Set
    End Property
    Private Property SH2 As Integer
        Get
            Return SH / 2
        End Get
        Set(value As Integer)
        End Set
    End Property
    Private pixelScale As Single = 1
    Private Property FSW As Integer
        Get
            Return SW * pixelScale
        End Get
        Set(value As Integer)
        End Set
    End Property
    Private Property FSH As Integer
        Get
            Return SH * pixelScale
        End Get
        Set(value As Integer)
        End Set
    End Property

    Private Const numSect = 4
    Private Const numWall = 16

    Private moveForward As Boolean = False
    Private moveBackward As Boolean = False
    Private moveLeft As Boolean = False
    Private moveRight As Boolean = False
    Private moveUp As Boolean = False
    Private moveDown As Boolean = False
    Private lookUp As Boolean = False
    Private lookDown As Boolean = False
    Private lookLeft As Boolean = False
    Private lookRight As Boolean = False
    Private altHeld As Boolean = False
    Private enterHeld As Boolean = False

    Private loadSectors = {
        0, 4, 0, 40, 2, 3,
        4, 8, 0, 40, 4, 5,
        8, 12, 0, 40, 6, 7,
        12, 16, 0, 40, 0, 1
    }

    Private loadWalls = {
    0, 0, 32, 0, 0,
    32, 0, 32, 32, 1,
    32, 32, 0, 32, 0,
    0, 32, 0, 0, 1,
    64, 0, 96, 0, 2,
    96, 0, 96, 32, 3,
    96, 32, 64, 32, 2,
    64, 32, 64, 0, 3,
    64, 64, 96, 64, 4,
    96, 64, 96, 96, 5,
    96, 96, 64, 96, 4,
    64, 96, 64, 64, 5,
    0, 64, 32, 64, 6,
    32, 64, 32, 96, 7,
    32, 96, 0, 96, 6,
    0, 96, 0, 64, 7
    }

    Private Class Wall
        Public x1 As Integer
        Public y1 As Integer
        Public x2 As Integer
        Public y2 As Integer
        Public c As Integer
    End Class
    Private Walls() As Wall = NewWalls()

    Public Class Sectors
        Public ws As Integer
        Public we As Integer
        Public z1 As Integer
        Public z2 As Integer
        Public d As Integer
        Public c1 As Integer
        Public c2 As Integer
        Public Surf(SW) As Integer
        Public surface As Integer
    End Class
    Private Sects() As Sectors = NewSect()

    Public Function NewSect() As Sectors()
        Dim arr() As Sectors = New Sectors(numSect - 1) {
        New Sectors With {.ws = loadSectors(0), .we = loadSectors(1), .z1 = loadSectors(2), .z2 = loadSectors(3), .c1 = loadSectors(4), .c2 = loadSectors(5)},
        New Sectors With {.ws = loadSectors(6), .we = loadSectors(7), .z1 = loadSectors(8), .z2 = loadSectors(9), .c1 = loadSectors(10), .c2 = loadSectors(11)},
        New Sectors With {.ws = loadSectors(12), .we = loadSectors(13), .z1 = loadSectors(14), .z2 = loadSectors(15), .c1 = loadSectors(16), .c2 = loadSectors(17)},
        New Sectors With {.ws = loadSectors(18), .we = loadSectors(19), .z1 = loadSectors(20), .z2 = loadSectors(21), .c1 = loadSectors(22), .c2 = loadSectors(23)}
        }
        Return arr
    End Function
    Private Function NewWalls() As Wall()
        Dim arr() As Wall = New Wall(numWall - 1) {
            New Wall With {.x1 = loadWalls(0), .y1 = loadWalls(1), .x2 = loadWalls(2), .y2 = loadWalls(3), .c = loadWalls(4)},
            New Wall With {.x1 = loadWalls(5), .y1 = loadWalls(6), .x2 = loadWalls(7), .y2 = loadWalls(8), .c = loadWalls(9)},
            New Wall With {.x1 = loadWalls(10), .y1 = loadWalls(11), .x2 = loadWalls(12), .y2 = loadWalls(13), .c = loadWalls(14)},
            New Wall With {.x1 = loadWalls(15), .y1 = loadWalls(16), .x2 = loadWalls(17), .y2 = loadWalls(18), .c = loadWalls(19)},
            New Wall With {.x1 = loadWalls(20), .y1 = loadWalls(21), .x2 = loadWalls(22), .y2 = loadWalls(23), .c = loadWalls(24)},
            New Wall With {.x1 = loadWalls(25), .y1 = loadWalls(26), .x2 = loadWalls(27), .y2 = loadWalls(28), .c = loadWalls(29)},
            New Wall With {.x1 = loadWalls(30), .y1 = loadWalls(31), .x2 = loadWalls(32), .y2 = loadWalls(33), .c = loadWalls(34)},
            New Wall With {.x1 = loadWalls(35), .y1 = loadWalls(36), .x2 = loadWalls(37), .y2 = loadWalls(38), .c = loadWalls(39)},
            New Wall With {.x1 = loadWalls(40), .y1 = loadWalls(41), .x2 = loadWalls(42), .y2 = loadWalls(43), .c = loadWalls(44)},
            New Wall With {.x1 = loadWalls(45), .y1 = loadWalls(46), .x2 = loadWalls(47), .y2 = loadWalls(48), .c = loadWalls(49)},
            New Wall With {.x1 = loadWalls(50), .y1 = loadWalls(51), .x2 = loadWalls(52), .y2 = loadWalls(53), .c = loadWalls(54)},
            New Wall With {.x1 = loadWalls(55), .y1 = loadWalls(56), .x2 = loadWalls(57), .y2 = loadWalls(58), .c = loadWalls(59)},
            New Wall With {.x1 = loadWalls(60), .y1 = loadWalls(61), .x2 = loadWalls(62), .y2 = loadWalls(63), .c = loadWalls(64)},
            New Wall With {.x1 = loadWalls(65), .y1 = loadWalls(66), .x2 = loadWalls(67), .y2 = loadWalls(68), .c = loadWalls(69)},
            New Wall With {.x1 = loadWalls(70), .y1 = loadWalls(71), .x2 = loadWalls(72), .y2 = loadWalls(73), .c = loadWalls(74)},
            New Wall With {.x1 = loadWalls(75), .y1 = loadWalls(76), .x2 = loadWalls(77), .y2 = loadWalls(78), .c = loadWalls(79)}
        }
        Return arr
    End Function
    Private Class CosSin
        Public cos(359) As Single
        Public sin(359) As Single
        Public Sub New()
            For i = 0 To 359
                cos(i) = CSng(Math.Cos(i * Math.PI / 180))
                sin(i) = CSng(Math.Sin(i * Math.PI / 180))
            Next
        End Sub
    End Class
    Private M As New CosSin
    Public Class player
        Public x As Integer
        Public y As Integer
        Public z As Integer
        Public a As Integer
        Public l As Integer
    End Class
    Private P As New player With {.x = 70, .y = -110, .z = 20, .a = 0, .l = 0}

    Private cols() As Color = {
    Color.FromArgb(255, 255, 0),
    Color.FromArgb(160, 160, 0),
    Color.FromArgb(0, 255, 0),
    Color.FromArgb(0, 160, 0),
    Color.FromArgb(0, 255, 255),
    Color.FromArgb(0, 160, 160),
    Color.FromArgb(160, 100, 0),
    Color.FromArgb(110, 50, 0),
    Color.FromArgb(0, 60, 130)
    }

    Private Frame As New DirectBitmap(SW, SH)

    Private Sub MovePlayer()
        If lookLeft Then
            P.a -= 4
            If P.a < 0 Then P.a += 360
        End If
        If lookRight Then
            P.a += 4
            If P.a > 359 Then P.a -= 360
        End If
        Dim dx = M.sin(P.a) * 10
        Dim dy = M.cos(P.a) * 10
        If moveForward Then
            P.x += dx
            P.y += dy
        End If
        If moveBackward Then
            P.x -= dx
            P.y -= dy
        End If
        If moveRight Then
            P.x += dy
            P.y -= dx
        End If
        If moveLeft Then
            P.x -= dy
            P.y += dx
        End If
        If lookUp Then P.l += 1
        If lookDown Then P.l -= 1
        If moveDown Then P.z += 4
        If moveUp Then P.z -= 4
    End Sub

    Private Sub DrawWall(x1 As Integer, x2 As Integer, b1 As Integer, b2 As Integer, t1 As Integer, t2 As Integer, c As Integer, s As Integer)
        Dim x As Integer : Dim y As Integer

        Dim dyb = b2 - b1
        Dim dyt = t2 - t1
        Dim dx = x2 - x1 : If dx = 0 Then dx = 1
        Dim xs = x1

        x1 = Math.Clamp(x1, 0, SW - 1)
        x2 = Math.Clamp(x2, 0, SW - 1)

        Dim gr = Graphics.FromImage(Frame.Bitmap)

        For x = x1 To x2
            Dim y1 As Integer = dyb * (x - xs + 0.5) / dx + b1
            Dim y2 As Integer = dyt * (x - xs + 0.5) / dx + t1

            y1 = Math.Clamp(y1, 0, SH)
            y2 = Math.Clamp(y2, 0, SH)

            If Sects(s).surface = 1 Then
                Sects(s).Surf(x) = y1
                Continue For
            ElseIf Sects(s).surface = 2 Then
                Sects(s).Surf(x) = y2
                Continue For
            ElseIf Sects(s).surface = -1 Then
                'gr.DrawLine(New Pen(cols(Sects(s).c1)), x, y1, x, Sects(s).Surf(x))
                For y = Sects(s).Surf(x) To y1 - 1
                    DrawPixel(x, y, Sects(s).c1)
                Next
            ElseIf Sects(s).surface = -2 Then
                'gr.DrawLine(New Pen(cols(Sects(s).c2)), x, Sects(s).Surf(x), x, y2)
                For y = y2 To Sects(s).Surf(x) - 1
                    DrawPixel(x, y, Sects(s).c2)
                Next
            End If
            'gr.DrawLine(New Pen(cols(c)), x, y1, x, y2)
            For y = y1 To y2 - 1
                DrawPixel(x, y, c, x2 - x1, y2 - y1, New Point(x1, y1))
            Next
        Next

    End Sub
    Public Function MakePolgon(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer, p5 As Integer, p6 As Integer, p7 As Integer, p8 As Integer) As PointF()
        Dim points() As PointF = {
        New PointF(p1, p2),
        New PointF(p3, p4),
        New PointF(p5, p6),
        New PointF(p7, p8)
        }
        Return points
    End Function

    Private Sub clipBehindPlayer(ByRef x1 As Integer, ByRef y1 As Integer, ByRef z1 As Integer, x2 As Integer, y2 As Integer, z2 As Integer)
        Dim da As Single = y1
        Dim db As Single = y2
        Dim d As Single = da - db
        If d = 0 Then d = 1
        Dim s As Single = da / (da - db)
        x1 += s * (x2 - x1)
        y1 += s * (y2 - y1) : If y1 = 0 Then y1 = 1
        z1 += s * (z2 - z1)
    End Sub

    Private Sub DrawFrame()
        Dim startTime = Date.Now.Millisecond
        Frame.FillImage(cols(8))
        Dim wx(4) As Integer : Dim wy(4) As Integer : Dim wz(4) As Integer : Dim CS = M.cos(P.a) : Dim SN = M.sin(P.a)

        For s = 0 To numSect - 2
            For w = 0 To numSect - s - 2
                If Sects(w).d < Sects(w + 1).d Then
                    Dim st As Sectors = Sects(w)
                    Sects(w) = Sects(w + 1)
                    Sects(w + 1) = st
                End If
            Next w
        Next s
        For s = 0 To numSect - 1
            Sects(s).d = 0

            If P.z < Sects(s).z1 Then : Sects(s).surface = 1
            ElseIf P.z > Sects(s).z2 Then : Sects(s).surface = 2
            Else : Sects(s).surface = 0 : End If
            For lop = 0 To 1

                For w = Sects(s).ws To Sects(s).we - 1


                    Dim x1 = Walls(w).x1 - P.x : Dim y1 = Walls(w).y1 - P.y
                    Dim x2 = Walls(w).x2 - P.x : Dim y2 = Walls(w).y2 - P.y

                    If lop = 0 Then
                        Dim swp = x1 : x1 = x2 : x2 = swp : swp = y1 : y1 = y2 : y2 = swp
                    End If

                    wx(0) = x1 * CS - y1 * SN
                    wx(1) = x2 * CS - y2 * SN
                    wx(2) = wx(0)
                    wx(3) = wx(1)

                    wy(0) = y1 * CS + x1 * SN
                    wy(1) = y2 * CS + x2 * SN
                    wy(2) = wy(0)
                    wy(3) = wy(1)
                    Sects(s).d += dist(0, 0, (wx(0) + wx(1)) / 2, (wy(0) + wy(1)) / 2)

                    wz(0) = Sects(s).z1 - P.z + (P.l * wy(0) / 32)
                    wz(1) = Sects(s).z1 - P.z + (P.l * wy(1) / 32)
                    wz(2) = wz(0) + Sects(s).z2
                    wz(3) = wz(1) + Sects(s).z2

                    If wy(0) < 1 AndAlso wy(1) < 1 Then Continue For

                    If wy(0) < 1 Then
                        clipBehindPlayer(wx(0), wy(0), wz(0), wx(1), wy(1), wz(1))
                        clipBehindPlayer(wx(2), wy(2), wz(2), wx(3), wy(3), wz(3))
                    End If

                    If wy(1) < 1 Then
                        clipBehindPlayer(wx(1), wy(1), wz(1), wx(0), wy(0), wz(0))
                        clipBehindPlayer(wx(3), wy(3), wz(3), wx(2), wy(2), wz(2))
                    End If


                    wx(0) = wx(0) * 270 / wy(0) + SW2 : wy(0) = wz(0) * 270 / wy(0) + SH2
                    wx(1) = wx(1) * 270 / wy(1) + SW2 : wy(1) = wz(1) * 270 / wy(1) + SH2
                    wx(2) = wx(2) * 270 / wy(2) + SW2 : wy(2) = wz(2) * 270 / wy(2) + SH2
                    wx(3) = wx(3) * 270 / wy(3) + SW2 : wy(3) = wz(3) * 270 / wy(3) + SH2

                    DrawWall(wx(0), wx(1), wy(0), wy(1), wy(2), wy(3), Walls(w).c, s)
                Next w
                Sects(s).d /= (Sects(s).we - Sects(s).ws)
                Sects(s).surface *= -1
            Next lop
        Next s
        DrawToScreen(Frame)
        Dim elapsedTime = Date.Now.Millisecond - startTime
        Label1.Text = elapsedTime & "ms"
        Label2.Text = CInt(1000 / If(Timer1.Interval < elapsedTime, elapsedTime, Timer1.Interval)) & "fps"
    End Sub

    Private Function dist(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As Single
        Return Math.Sqrt((x1 - x2) ^ 2 + (y1 - y2) ^ 2)
    End Function

    Public Sub DrawToScreen(bmp As DirectBitmap)
        PictureBox1.Image = bmp.Resize(SW * pixelScale, SH * pixelScale, InterpolationMode.NearestNeighbor).Bitmap
    End Sub

    Private Sub DrawPixel(x As Integer, y As Integer, c As Integer, Optional width As Integer = 0, Optional height As Integer = 0, Optional origin As Point = Nothing)
        Frame.SetPixel(x, y, cols(c))
    End Sub

    Private Sub Pro() Handles Timer1.Tick
        MovePlayer()
        DrawFrame()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.BackColor = cols(8)
        Label2.BackColor = cols(8)
        Label1.ForeColor = Color.White
        Label2.ForeColor = Color.White
        Timer1.Interval = FrameTimeCap
        PictureBox1.Location = New Point(0, 0)
        PictureBox1.Size = New Size(SW * pixelScale, SH * pixelScale)
        PictureBox1.SendToBack()
        Location = New Point(0, 0)
        FormBorderStyle = FormBorderStyle.None
        Height = SH * pixelScale : Width = SW * pixelScale
        Timer1.Start()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        If e.KeyCode = Keys.W Then
            moveForward = True
        ElseIf e.KeyCode = Keys.S Then
            moveBackward = True
        ElseIf e.KeyCode = Keys.A Then
            moveLeft = True
        ElseIf e.KeyCode = Keys.D Then
            moveRight = True
        ElseIf e.KeyCode = Keys.Up Then
            lookUp = True
        ElseIf e.KeyCode = Keys.Down Then
            lookDown = True
        ElseIf e.KeyCode = Keys.Left Then
            lookLeft = True
        ElseIf e.KeyCode = Keys.Right Then
            lookRight = True
        ElseIf e.KeyCode = Keys.ShiftKey Then
            moveUp = True
        ElseIf e.KeyCode = Keys.ControlKey Then
            moveDown = True
        ElseIf e.KeyCode = 18 Then ' Keys.Alt does not work, KeyCode 18 does
            altHeld = True
        ElseIf e.KeyCode = Keys.Return Then
            enterHeld = True
        End If
        If altHeld AndAlso enterHeld Then
            If FormBorderStyle = FormBorderStyle.None Then
                FormBorderStyle = FormBorderStyle.FixedSingle
                SW /= 2 : SH /= 2
                Frame = New DirectBitmap(SW, SH)
                Size = New Size(SW * pixelScale, SH * pixelScale)
            Else
                FormBorderStyle = FormBorderStyle.None
                Location = New Point(0, 0)
                SW = My.Computer.Screen.Bounds.Width
                SH = My.Computer.Screen.Bounds.Height
                Frame = New DirectBitmap(SW, SH)
                Size = New Size(SW * pixelScale, SH * pixelScale)
            End If
            altHeld = False
            enterHeld = False
        End If
        MovePlayer()
        MyBase.OnKeyDown(e)
    End Sub
    Protected Overrides Sub OnKeyPress(e As KeyPressEventArgs)
        If e.KeyChar = "-" Then
            SW /= 2 : SH /= 2 : pixelScale *= 2
            Frame = New DirectBitmap(SW, SH)
        ElseIf e.KeyChar = "+" Then
            If Not SW = My.Computer.Screen.Bounds.Width Then
                SW *= 2 : SH *= 2 : pixelScale /= 2
                Frame = New DirectBitmap(SW, SH)
            End If
        End If
        MyBase.OnKeyPress(e)
    End Sub
    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        If e.KeyCode = Keys.W Then
            moveForward = False
        ElseIf e.KeyCode = Keys.S Then
            moveBackward = False
        ElseIf e.KeyCode = Keys.A Then
            moveLeft = False
        ElseIf e.KeyCode = Keys.D Then
            moveRight = False
        ElseIf e.KeyCode = Keys.Up Then
            lookUp = False
        ElseIf e.KeyCode = Keys.Down Then
            lookDown = False
        ElseIf e.KeyCode = Keys.Left Then
            lookLeft = False
        ElseIf e.KeyCode = Keys.Right Then
            lookRight = False
        ElseIf e.KeyCode = Keys.ShiftKey Then
            moveUp = False
        ElseIf e.KeyCode = Keys.ControlKey Then
            moveDown = False
        ElseIf e.KeyCode = Keys.Alt Then
            altHeld = False
        ElseIf e.KeyCode = Keys.Enter Then
            enterHeld = False
        End If
        MyBase.OnKeyUp(e)
    End Sub
End Class
Imports System.Drawing.Drawing2D
Imports System.Threading
Imports ImageTools.Tools

Public Class Form1
    Private Const FrameTimeCap = 1 ' how long before a new frame is drawn (in ms)

    Private elapsedTime As Double = 0

    Private Const fov = 400.0F

    Private Const sf As Integer = 2

    Private Shared SW As Integer = My.Computer.Screen.Bounds.Width / sf
    Private Shared SH As Integer = My.Computer.Screen.Bounds.Height / sf

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

    Private numSect = 0
    Private numWall = 0

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

    Private Class Wall
        Public x1, y1 As Integer
        Public x2, y2 As Integer
        Public c As Integer
        Public wt, u, v As Integer
        Public shade As Integer
    End Class

    Private Walls As New List(Of Wall)

    Public Class Sectors
        Public ws, we As Integer
        Public z1, z2 As Integer
        Public d As Integer
        Public c1, c2 As Integer
        Public st, ss As Integer
        Public Surf(SW) As Integer
        Public surface As Integer
    End Class

    Private Sects As New List(Of Sectors)

    Sub LoadLevel()

        Walls = New List(Of Wall)
        Sects = New List(Of Sectors)

        Dim level = IO.File.ReadAllLines("level.h")

        numSect = level(0)
        Dim tmp As String()
        For s = 0 To numSect - 1
            tmp = level(1 + s).Split(" ")
            Sects.Add(New Sectors())
            Sects(s).ws = tmp(0)
            Sects(s).we = tmp(1)
            Sects(s).z1 = tmp(2)
            Sects(s).z2 = tmp(3)
            Sects(s).st = tmp(4)
            Sects(s).ss = tmp(5)
        Next
        numWall = level(1 + numSect)
        For w = 0 To numWall - 1
            tmp = level(2 + numSect + w).Split(" ")
            Walls.Add(New Wall())
            Walls(w).x1 = tmp(0)
            Walls(w).y1 = tmp(1)
            Walls(w).x2 = tmp(2)
            Walls(w).y2 = tmp(3)
            Walls(w).wt = tmp(4)
            Walls(w).u = tmp(5)
            Walls(w).v = tmp(6)
            Walls(w).shade = tmp(7)
        Next
        tmp = level(3 + numSect + numWall).Split(" ")
        P.x = tmp(0)
        P.y = tmp(1)
        P.z = tmp(2)
        P.a = tmp(3)
        P.l = tmp(4)
    End Sub

    Private Class CosSin
        Public cos(359) As Double
        Public sin(359) As Double

        Public Sub New()
            For i = 0 To 359
                cos(i) = Math.Cos(i * Math.PI / 180)
                sin(i) = Math.Sin(i * Math.PI / 180)
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

    Private texsheet As DirectBitmap = My.Resources.texsheet.ToDirectBitmap()
    Private textures As New List(Of DirectBitmap)

    Dim frame As New DirectBitmap(SW, SH)
    Dim scaledframe As New DirectBitmap(SW * sf, SH * sf)

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

    Private Sub floors()
        frame.FillImage(cols(8))
        Dim x, y As Integer
        Dim xo = SW2
        Dim yo = SH2

        Dim lookUpDown As Single = -P.l * 4
        If lookUpDown > SH Then lookUpDown = SH
        Dim moveUpDown As Single = P.z / 16
        If moveUpDown = 0 Then moveUpDown = 0.0001
        Dim ys = -yo, ye = -lookUpDown

        If moveUpDown < 0 Then
            ys = -lookUpDown
            ye = yo + lookUpDown
        End If

        For y = ys To ye
            For x = -xo To xo - 1
                Dim z = y + lookUpDown
                If z = 0 Then z = 0.0001
                Dim fx As Integer = x / z * moveUpDown
                Dim fy As Integer = fov / z * moveUpDown

                Dim rx = fx * M.sin(P.a) - fy * M.cos(P.a) + (P.y / 30.0)
                Dim ry = fx * M.cos(P.a) + fy * M.sin(P.a) - (P.x / 30.0)
                If rx < 0 Then rx = -rx + 1
                If ry < 0 Then ry = -ry + 1

                If rx <= 0 Or ry <= 0 Or rx >= 5 Or ry >= 5 Then Continue For

                If CInt(rx) Mod 2 = CInt(ry) Mod 2 Then : frame.SetPixel(x + xo, y + yo, Color.FromArgb(255, 0, 0))
                Else : frame.SetPixel(x + xo, y + yo, Color.FromArgb(0, 255, 0))
                End If
            Next
        Next

        PictureBox1.Image = frame.Bitmap
    End Sub

    Private Sub DrawWall(x1 As Integer, x2 As Integer, b1 As Integer, b2 As Integer, t1 As Integer, t2 As Integer, s As Integer, w As Integer, frontBack As Integer)
        Dim x As Integer : Dim y As Integer

        Dim wt = Walls(w).wt
        Dim ht = 0F, ht_step = textures(wt).Width / (x2 - x1) * Walls(w).u

        Dim dyb = b2 - b1
        Dim dyt = t2 - t1
        Dim dx = x2 - x1 : If dx = 0 Then dx = 1
        Dim xs = x1

        If x1 < 0 Then
            ht -= ht_step * x1
            x1 = 0
        End If
        If x2 < 0 Then x2 = 0
        If x1 > SW Then x1 = SW
        If x2 > SW Then x2 = SW

        For x = x1 To x2
            Dim y1 As Integer = dyb * (x - xs + 0.5) / dx + b1
            Dim y2 As Integer = dyt * (x - xs + 0.5) / dx + t1

            Dim vt = 0F, vt_step = textures(wt).Height / (y2 - y1) * Walls(w).v

            If y1 < 0 Then
                vt -= vt_step * y1
                y1 = 0
            End If
            If y2 < 0 Then y2 = 0
            If y1 > SH Then y1 = SH
            If y2 > SH Then y2 = SH

            If frontBack = 0 Then
                If Sects(s).surface = 1 Then Sects(s).Surf(x) = y1
                If Sects(s).surface = 2 Then Sects(s).Surf(x) = y2
                For y = y1 To y2 - 1
                    Dim pixel = textures(wt).GetPixel(Math.Clamp(ht Mod textures(wt).Width, 0, textures(w).Width - 1),
                                                      Math.Clamp(vt Mod textures(wt).Height, 0, textures(w).Height - 1))
                    Dim r = Math.Clamp(pixel.R - Walls(w).shade / 2, 0, 255)
                    Dim g = Math.Clamp(pixel.G - Walls(w).shade / 2, 0, 255)
                    Dim b = Math.Clamp(pixel.B - Walls(w).shade / 2, 0, 255)
                    pixel = Color.FromArgb(r, g, b)
                    frame.SetPixel(x, y, pixel)
                    vt += vt_step
                Next
                ht += ht_step
            ElseIf frontBack = 1 Then

                Dim xo = SW2
                Dim yo = SH2
                Dim xt = x - xo
                Dim wo As Integer
                Dim tile = Sects(s).ss * 7

                If Sects(s).surface = 1 Then
                    y2 = Sects(s).Surf(x)
                    wo = Sects(s).z1
                End If
                If Sects(s).surface = 2 Then
                    y1 = Sects(s).Surf(x)
                    wo = Sects(s).z2
                End If

                Dim lookUpDown As Single = -P.l * 6.2
                If lookUpDown > SH Then lookUpDown = SH
                Dim moveUpDown As Single = (P.z - wo) / 16
                If moveUpDown = 0 Then moveUpDown = 0.0001
                Dim ys = y1 - yo, ye = y2 - yo

                For y = ys To ye
                    Dim z = y + lookUpDown
                    If z = 0 Then z = 0.0001
                    Dim fx As Integer = xt / z * moveUpDown * tile
                    Dim fy As Integer = fov / z * moveUpDown * tile

                    Dim rx = fx * M.sin(P.a) - fy * M.cos(P.a) + (P.y / 60.0 * tile)
                    Dim ry = fx * M.cos(P.a) + fy * M.sin(P.a) - (P.x / 60.0 * tile)
                    If rx < 0 Then rx = -rx + 1
                    If ry < 0 Then ry = -ry + 1

                    Dim st = Sects(s).st
                    Dim pixel = textures(st).GetPixel(Math.Clamp(rx Mod textures(st).Width, 0, textures(st).Width - 1),
                                                      Math.Clamp(ry Mod textures(st).Height, 0, textures(st).Height - 1))
                    frame.SetPixel(xt + xo, y + yo, pixel)
                Next

            End If
        Next

    End Sub

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

        Dim startTime = Date.Now.Ticks

        scaledframe.dispose()
        frame.FillImage(cols(8))

        Dim wx(4), wy(4), wz(4) As Integer
        Dim CS = M.cos(P.a)
        Dim SN = M.sin(P.a)
        Dim cycles As Integer
        Dim x As Integer

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

            If P.z < Sects(s).z1 Then
                Sects(s).surface = 1 : cycles = 2
                For i = 0 To SW - 1
                    Sects(s).Surf(x) = SH
                Next
            ElseIf P.z > Sects(s).z2 Then
                Sects(s).surface = 2 : cycles = 2
                For i = 0 To SW - 1
                    Sects(s).Surf(x) = 0
                Next
            Else : Sects(s).surface = 0 : cycles = 1 : End If
            For lop = 0 To cycles - 1

                For w = Sects(s).ws To Sects(s).we - 1

                    Dim x1 = Walls(w).x1 - P.x : Dim y1 = Walls(w).y1 - P.y
                    Dim x2 = Walls(w).x2 - P.x : Dim y2 = Walls(w).y2 - P.y

                    If lop = 1 Then
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
                    wz(2) = Sects(s).z2 - P.z + (P.l * wy(0) / 32)
                    wz(3) = Sects(s).z2 - P.z + (P.l * wy(1) / 32)

                    If wy(0) < 1 AndAlso wy(1) < 1 Then Continue For

                    If wy(0) < 1 Then
                        clipBehindPlayer(wx(0), wy(0), wz(0), wx(1), wy(1), wz(1))
                        clipBehindPlayer(wx(2), wy(2), wz(2), wx(3), wy(3), wz(3))
                    End If

                    If wy(1) < 1 Then
                        clipBehindPlayer(wx(1), wy(1), wz(1), wx(0), wy(0), wz(0))
                        clipBehindPlayer(wx(3), wy(3), wz(3), wx(2), wy(2), wz(2))
                    End If

                    wx(0) = wx(0) * fov / wy(0) + SW2 : wy(0) = wz(0) * fov / wy(0) + SH2
                    wx(1) = wx(1) * fov / wy(1) + SW2 : wy(1) = wz(1) * fov / wy(1) + SH2
                    wx(2) = wx(2) * fov / wy(2) + SW2 : wy(2) = wz(2) * fov / wy(2) + SH2
                    wx(3) = wx(3) * fov / wy(3) + SW2 : wy(3) = wz(3) * fov / wy(3) + SH2

                    DrawWall(wx(0), wx(1), wy(0), wy(1), wy(2), wy(3), s, w, lop)
                Next w
                Sects(s).d /= (Sects(s).we - Sects(s).ws)
            Next lop
        Next s
        scaledframe = frame.Resize(SW * sf, SH * sf, InterpolationMode.NearestNeighbor)

        PictureBox1.Image = scaledframe.Bitmap
        elapsedTime = (Date.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond
        Thread.Sleep(FrameTimeCap)
    End Sub

    Private Sub UpdateInfo() Handles Timer2.Tick
        Label1.Text = elapsedTime & "ms"
        Label2.Text = CInt(1000 / If(FrameTimeCap < elapsedTime, elapsedTime, FrameTimeCap)) & "fps"
    End Sub

    Private Function dist(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As Single
        Return Math.Sqrt((x1 - x2) ^ 2 + (y1 - y2) ^ 2)
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For i As Integer = 0 To (texsheet.Width / texsheet.Height) - 1
            textures.Add(texsheet.Crop(texsheet.Height * i, 0, New Size(texsheet.Height, texsheet.Height)))
        Next

        Timer1.Interval = FrameTimeCap
        Timer2.Interval = 500
        Label1.BackColor = cols(8)
        Label2.BackColor = cols(8)
        Label1.ForeColor = Color.White
        Label2.ForeColor = Color.White
        PictureBox1.Location = New Point(0, 0)
        PictureBox1.Size = New Size(SW * sf, SH * sf)
        PictureBox1.SendToBack()
        Location = New Point(0, 0)
        FormBorderStyle = FormBorderStyle.None
        Height = SH * sf : Width = SW * sf

        Timer1.Start()
        Timer2.Start()
    End Sub

    Private Sub RenderLoop() Handles Timer1.Tick
        MovePlayer()
        DrawFrame()
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
        ElseIf e.KeyCode = Keys.Return Then
            LoadLevel()
        End If
        MovePlayer()
        MyBase.OnKeyDown(e)
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
        End If
        MyBase.OnKeyUp(e)
    End Sub

End Class
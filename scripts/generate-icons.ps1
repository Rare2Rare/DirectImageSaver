Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$assetRoot = Join-Path $repoRoot "assets\icons"
$extensionIconRoot = Join-Path $repoRoot "extension\icons"
$masterSvgPath = Join-Path $assetRoot "direct-image-saver.master.svg"
$icoPath = Join-Path $assetRoot "direct-image-saver.ico"
$previewPath = Join-Path $assetRoot "direct-image-saver-preview.png"
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$extensionSizes = @(16, 32, 48, 128)

New-Item -ItemType Directory -Force -Path $assetRoot, $extensionIconRoot | Out-Null

if (-not (Test-Path $masterSvgPath)) {
    throw "Master SVG not found at $masterSvgPath"
}

$colors = @{
    FrameStart = [System.Drawing.Color]::FromArgb(255, 13, 108, 120)
    FrameEnd = [System.Drawing.Color]::FromArgb(255, 6, 63, 80)
    PanelTint = [System.Drawing.Color]::FromArgb(86, 138, 234, 244)
    PanelShadow = [System.Drawing.Color]::FromArgb(20, 10, 41, 54)
    ArrowStroke = [System.Drawing.Color]::FromArgb(255, 5, 45, 57)
    ArrowStart = [System.Drawing.Color]::FromArgb(255, 184, 253, 255)
    ArrowMid = [System.Drawing.Color]::FromArgb(255, 67, 217, 235)
    ArrowEnd = [System.Drawing.Color]::FromArgb(255, 15, 157, 183)
    ArrowHighlight = [System.Drawing.Color]::FromArgb(210, 240, 255, 255)
    Speed = [System.Drawing.Color]::FromArgb(255, 36, 224, 235)
    Sun = [System.Drawing.Color]::FromArgb(145, 181, 252, 255)
    Landscape = [System.Drawing.Color]::FromArgb(48, 217, 251, 255)
}

function New-RoundedRectanglePath {
    param(
        [float]$X,
        [float]$Y,
        [float]$Width,
        [float]$Height,
        [float]$Radius
    )

    $diameter = $Radius * 2
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    $path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    $path.AddArc($X + $Width - $diameter, $Y + $Height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-ArrowPath {
    param([float]$Scale)

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $points = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(127 * $Scale, 76 * $Scale),
        [System.Drawing.PointF]::new(127 * $Scale, 76 * $Scale),
        [System.Drawing.PointF]::new(127 * $Scale, 129 * $Scale),
        [System.Drawing.PointF]::new(109.5 * $Scale, 129 * $Scale),
        [System.Drawing.PointF]::new(137 * $Scale, 165 * $Scale),
        [System.Drawing.PointF]::new(164.5 * $Scale, 129 * $Scale),
        [System.Drawing.PointF]::new(147 * $Scale, 129 * $Scale),
        [System.Drawing.PointF]::new(147 * $Scale, 76 * $Scale)
    )
    $path.AddPolygon($points)
    return $path
}

function New-SmallArrowPath {
    param([float]$Scale)

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $points = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(124 * $Scale, 83 * $Scale),
        [System.Drawing.PointF]::new(150 * $Scale, 83 * $Scale),
        [System.Drawing.PointF]::new(150 * $Scale, 119 * $Scale),
        [System.Drawing.PointF]::new(166 * $Scale, 119 * $Scale),
        [System.Drawing.PointF]::new(137 * $Scale, 159 * $Scale),
        [System.Drawing.PointF]::new(108 * $Scale, 119 * $Scale),
        [System.Drawing.PointF]::new(124 * $Scale, 119 * $Scale)
    )
    $path.AddPolygon($points)
    return $path
}

function Draw-Icon {
    param(
        [System.Drawing.Graphics]$Graphics,
        [int]$Size
    )

    $Graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $Graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $Graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $Graphics.Clear([System.Drawing.Color]::Transparent)

    $scale = $Size / 256.0
    $isSmall = $Size -le 20
    $showPanelDetails = $Size -ge 48

    $framePath = New-RoundedRectanglePath (38 * $scale) (50 * $scale) (164 * $scale) (120 * $scale) (16 * $scale)
    $frameBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(38 * $scale, 50 * $scale),
        [System.Drawing.PointF]::new(202 * $scale, 170 * $scale),
        $colors.FrameStart,
        $colors.FrameEnd)
    $Graphics.FillPath($frameBrush, $framePath)

    $innerPath = New-RoundedRectanglePath (56 * $scale) (68 * $scale) (128 * $scale) (84 * $scale) (10 * $scale)
    $innerBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(56 * $scale, 68 * $scale),
        [System.Drawing.PointF]::new(184 * $scale, 152 * $scale),
        $colors.PanelTint,
        $colors.PanelShadow)
    $Graphics.FillPath($innerBrush, $innerPath)

    if ($showPanelDetails) {
        $sunBrush = [System.Drawing.SolidBrush]::new($colors.Sun)
        $Graphics.FillEllipse($sunBrush, 79 * $scale, 81 * $scale, 22 * $scale, 22 * $scale)
        $sunBrush.Dispose()

        $landscapeBrush = [System.Drawing.SolidBrush]::new($colors.Landscape)
        $landscape = [System.Drawing.PointF[]]@(
            [System.Drawing.PointF]::new(67 * $scale, 139 * $scale),
            [System.Drawing.PointF]::new(103 * $scale, 107 * $scale),
            [System.Drawing.PointF]::new(127 * $scale, 128 * $scale),
            [System.Drawing.PointF]::new(175 * $scale, 89 * $scale),
            [System.Drawing.PointF]::new(175 * $scale, 152 * $scale),
            [System.Drawing.PointF]::new(67 * $scale, 152 * $scale)
        )
        $Graphics.FillPolygon($landscapeBrush, $landscape)
        $landscapeBrush.Dispose()
    }

    $speedPenWidth = if ($isSmall) { [Math]::Max(2, [Math]::Round(12 * $scale)) } else { [Math]::Max(3, [Math]::Round(10 * $scale)) }
    $speedPen = [System.Drawing.Pen]::new($colors.Speed, $speedPenWidth)
    $speedPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Square
    $speedPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Square
    $Graphics.DrawLine($speedPen, 44 * $scale, 58 * $scale, 74 * $scale, 28 * $scale)
    if (-not $isSmall) {
        $Graphics.DrawLine($speedPen, 62 * $scale, 76 * $scale, 88 * $scale, 50 * $scale)
    }
    $speedPen.Dispose()

    $arrowShadowPath = if ($isSmall) { New-SmallArrowPath $scale } else { New-ArrowPath $scale }
    $shadowMatrix = [System.Drawing.Drawing2D.Matrix]::new()
    $shadowMatrix.Translate(0, 2 * $scale)
    $arrowShadowPath.Transform($shadowMatrix)
    $shadowBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 5, 45, 57))
    $Graphics.FillPath($shadowBrush, $arrowShadowPath)
    $shadowBrush.Dispose()

    $arrowPath = if ($isSmall) { New-SmallArrowPath $scale } else { New-ArrowPath $scale }
    $arrowBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(111 * $scale, 76 * $scale),
        [System.Drawing.PointF]::new(168 * $scale, 178 * $scale),
        $colors.ArrowStart,
        $colors.ArrowEnd)
    $blend = [System.Drawing.Drawing2D.ColorBlend]::new(3)
    $blend.Colors = [System.Drawing.Color[]]@($colors.ArrowStart, $colors.ArrowMid, $colors.ArrowEnd)
    $blend.Positions = [float[]]@(0.0, 0.55, 1.0)
    $arrowBrush.InterpolationColors = $blend
    $Graphics.FillPath($arrowBrush, $arrowPath)

    if (-not $isSmall) {
        $highlightPen = [System.Drawing.Pen]::new($colors.ArrowHighlight, [Math]::Max(3, 9 * $scale))
        $highlightPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $highlightPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $Graphics.DrawLine($highlightPen, 137.5 * $scale, 80 * $scale, 137.5 * $scale, 135 * $scale)
        $highlightPen.Dispose()
    }

    $arrowBrush.Dispose()
    $arrowPath.Dispose()
    $arrowShadowPath.Dispose()
    $frameBrush.Dispose()
    $innerBrush.Dispose()
    $framePath.Dispose()
    $innerPath.Dispose()
}

function Save-Png {
    param(
        [int]$Size,
        [string]$OutputPath
    )

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            Draw-Icon -Graphics $graphics -Size $Size
        }
        finally {
            $graphics.Dispose()
        }

        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }
}

function Get-PngBytes {
    param([int]$Size)

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            Draw-Icon -Graphics $graphics -Size $Size
        }
        finally {
            $graphics.Dispose()
        }

        $memory = [System.IO.MemoryStream]::new()
        try {
            $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
            [byte[]]$pngBytes = $memory.ToArray()
            return ,$pngBytes
        }
        finally {
            $memory.Dispose()
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Save-Ico {
    param(
        [int[]]$Sizes,
        [string]$OutputPath
    )

    $images = @()
    foreach ($size in $Sizes) {
        $images += [pscustomobject]@{
            Size = $size
            Bytes = [byte[]](Get-PngBytes -Size $size)
        }
    }

    $stream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
    try {
        $writer = [System.IO.BinaryWriter]::new($stream)
        try {
            $writer.Write([UInt16]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]$images.Count)

            $offset = 6 + (16 * $images.Count)
            foreach ($image in $images) {
                $dimension = if ($image.Size -ge 256) { 0 } else { [byte]$image.Size }
                $writer.Write([byte]$dimension)
                $writer.Write([byte]$dimension)
                $writer.Write([byte]0)
                $writer.Write([byte]0)
                $writer.Write([UInt16]1)
                $writer.Write([UInt16]32)
                $writer.Write([UInt32]$image.Bytes.Length)
                $writer.Write([UInt32]$offset)
                $offset += $image.Bytes.Length
            }

            foreach ($image in $images) {
                $writer.Write($image.Bytes)
            }
        }
        finally {
            $writer.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Save-PreviewSheet {
    param([string]$OutputPath)

    $bitmap = [System.Drawing.Bitmap]::new(760, 270, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
            $graphics.Clear([System.Drawing.Color]::FromArgb(255, 245, 249, 250))

            $darkBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 17, 28, 36))
            $lightBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
            $labelBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 22, 40, 48))
            $font = [System.Drawing.Font]::new("Segoe UI", 12, [System.Drawing.FontStyle]::Regular)
            $smallFont = [System.Drawing.Font]::new("Segoe UI", 9, [System.Drawing.FontStyle]::Regular)
            $outlinePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(32, 0, 0, 0), 1)

            $graphics.FillRectangle($lightBrush, 30, 50, 330, 180)
            $graphics.FillRectangle($darkBrush, 400, 50, 330, 180)
            $graphics.DrawRectangle($outlinePen, 30, 50, 330, 180)
            $graphics.DrawRectangle($outlinePen, 400, 50, 330, 180)
            $graphics.DrawString("Light taskbar / explorer", $font, $labelBrush, 30, 18)
            $graphics.DrawString("Dark taskbar / browser UI", $font, $labelBrush, 400, 18)

            $sampleSizes = @(16, 24, 32, 48, 128)
            $xLight = 48
            $xDark = 418
            foreach ($sampleSize in $sampleSizes) {
                $bmpLight = [System.Drawing.Bitmap]::new($sampleSize, $sampleSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
                $bmpDark = [System.Drawing.Bitmap]::new($sampleSize, $sampleSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
                try {
                    $gLight = [System.Drawing.Graphics]::FromImage($bmpLight)
                    $gDark = [System.Drawing.Graphics]::FromImage($bmpDark)
                    try {
                        Draw-Icon -Graphics $gLight -Size $sampleSize
                        Draw-Icon -Graphics $gDark -Size $sampleSize
                    }
                    finally {
                        $gLight.Dispose()
                        $gDark.Dispose()
                    }

                    $graphics.DrawImage($bmpLight, $xLight, 92, $sampleSize, $sampleSize)
                    $graphics.DrawString("${sampleSize}px", $smallFont, $labelBrush, $xLight - 2, 160)

                    $graphics.DrawImage($bmpDark, $xDark, 92, $sampleSize, $sampleSize)
                    $graphics.DrawString("${sampleSize}px", $smallFont, $lightBrush, $xDark - 2, 160)
                }
                finally {
                    $bmpLight.Dispose()
                    $bmpDark.Dispose()
                }

                $xLight += [Math]::Max(54, $sampleSize + 18)
                $xDark += [Math]::Max(54, $sampleSize + 18)
            }

            $font.Dispose()
            $smallFont.Dispose()
            $outlinePen.Dispose()
            $darkBrush.Dispose()
            $lightBrush.Dispose()
            $labelBrush.Dispose()
        }
        finally {
            $graphics.Dispose()
        }

        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }
}

foreach ($size in $extensionSizes) {
    Save-Png -Size $size -OutputPath (Join-Path $extensionIconRoot "icon-$size.png")
}

Save-Ico -Sizes $sizes -OutputPath $icoPath
Save-PreviewSheet -OutputPath $previewPath

Write-Host "Generated ICO: $icoPath"
Write-Host "Generated extension icons in: $extensionIconRoot"
Write-Host "Generated preview: $previewPath"

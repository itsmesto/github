# Build from source

## Requirements

- Windows with the .NET Framework compiler (`csc.exe`) and the Windows Forms and System.Drawing framework assemblies.
- Microsoft C# dynamic COM support (`Microsoft.CSharp.dll`).
- Windows Image Acquisition (WIA) and a scanner driver. The HP M28a driver must already be installed for real scans.

No NuGet packages, Python runtime, network access, HP account, or admin installation is needed to build or run this utility on a compatible Windows PC.

## Compile

From the repository root in Windows PowerShell or Command Prompt:

```powershell
$compiler = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $compiler /nologo /target:winexe /out:release\HP_M28a_Scan.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll src\HP_M28a_Scan.cs
```

For 32-bit Windows, use `$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe` instead of the `Framework64` path.

The committed executable was compiled from `src/HP_M28a_Scan.cs` with the command above. It is unsigned.

## Verify scanner discovery

Running:

```powershell
release\HP_M28a_Scan.exe --check
```

checks only whether WIA enumerates an imaging device. It writes the result to `%TEMP%\HP_M28a_Scan_check.txt` and returns exit code 0 when a scanner is found. This does not perform a scan.

## PDF behavior

The application gets the image from WIA as a bitmap, encodes a JPEG locally, then writes a one-page PDF with an A4 media box. The image is fitted proportionally so the full captured frame remains visible. JPG output writes the JPEG directly.

## Reproducibility note

The app was compiled and scanner discovery was checked on the development PC. The PDF writer was validated by rendering a generated one-page PDF. The development PC's M28-M31 scanner returned a legible page through WIA. Hardware, cable, Windows build, and driver differences can change behavior on other PCs.

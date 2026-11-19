# HP M28a Scan Recovery

A small Windows desktop scanner for **HP LaserJet Pro MFP M28a / M28-M31 over USB**. It uses the Windows Image Acquisition (WIA) interface directly and saves scans as PDF (default) or JPG. It is intended as a practical workaround when HP Scan opens the scanner but leaves a blank preview or disables Save.

> **What this does and does not establish:** On the PC used to develop this tool, HP Scan detected the M28-M31 USB device and started the scan mechanism, but two attempts returned a blank preview. Windows listed the WIA scanner, its device status was OK, and the Windows Image Acquisition service was running. A direct WIA scan then returned a legible page. This supports an HP Scan/TWAIN acquisition-path problem on that PC; it does not prove that every intermittent scan problem has the same cause or rule out a loose USB cable or scanner hardware fault.

## Download and run

[Download HP_M28a_Scan.exe](https://github.com/itsmesto/github/raw/refs/heads/main/release/HP_M28a_Scan.exe)

1. Download the executable from the link above.
2. Connect and turn on the printer. For diagnosis, plug its USB cable directly into the PC rather than through a hub.
3. Lift the scanner cover. Place one page face down on the glass, align it with the corner guide, and close the cover. The M28a uses a flatbed scanner.
4. Run `HP_M28a_Scan.exe`.
5. Confirm **HP LaserJet MFP M28-M31 (USB)** appears in the Scanner list. Click **Refresh** if needed.
6. Choose a save folder and file name. PDF is selected by default; JPG is also available.
7. Click **Scan** and keep the cover closed until the app reports that the file was saved.

The default output folder is `Documents\Scans`. If a file with that name already exists, the app adds a number rather than overwriting it. Each scan is one page.

## What the app does

1. Uses the Windows Image Acquisition device manager to find scanners installed on this PC.
2. Connects to the selected WIA scanner and requests the image as a bitmap.
3. Converts the returned bitmap to JPEG locally.
4. For PDF output, places that image on a single A4 PDF page. For JPG output, saves the JPEG directly.
5. Writes the result to the folder you selected.

Scans stay on the PC. The app does not send documents to HP, GitHub, or a cloud service, and does not require an HP account. Errors are recorded locally at `%LOCALAPPDATA%\HP_M28a_Scan\scanner.log`.

## If scanning still fails

Follow the checks in [Troubleshooting](docs/TROUBLESHOOTING.md). In short: refresh the scanner list, check the USB cable and Windows WIA service, and compare the app’s result with a copy made from the printer. The copy test helps distinguish a Windows acquisition path problem from a physical scanner problem.

For a technical explanation of the blank-preview symptom and why this app uses WIA, see [Why WIA](docs/WHY_WIA.md). To rebuild the executable from source, see [Build](docs/BUILD.md).

## Compatibility and limits

- Developed and checked with an HP LaserJet Pro MFP M28a / M28-M31 connected by USB on Windows.
- Requires the scanner to be installed and exposed by Windows WIA. The app does not install HP drivers or repair a damaged USB cable.
- Designed for the M28a’s flatbed glass and single-page scans. It does not control an automatic document feeder.
- The shipped executable is unsigned. The source and build steps are included so users can inspect and rebuild it.
- Other HP models may expose different WIA behavior; this project does not claim universal compatibility.

## Files

- `release/HP_M28a_Scan.exe` — compiled Windows application.
- `src/HP_M28a_Scan.cs` — application source code.
- `docs/BUILD.md` — build requirements and command.
- `docs/TROUBLESHOOTING.md` — step-by-step diagnostic guide.
- `docs/WHY_WIA.md` — technical explanation of the workaround.

The repository intentionally contains no scanned documents or scanner logs. Do not commit private scans or log files.

## Reference

HP lists the M28a as a flatbed scanner and documents scanning through HP LaserJet software or TWAIN/WIA-compliant software: [HP M28a specifications](https://support.hp.com/us-en/product/product-specs/model/16647084).




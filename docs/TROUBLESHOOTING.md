# Troubleshooting

This guide applies to the HP LaserJet Pro MFP M28a / M28-M31 on Windows, especially when the scanner bar moves but the app gets no page image.

## Symptom: HP Scan opens, scans, then shows a blank preview

A moving scan bar means the scan request reached the printer and the scan mechanism ran. It does **not** confirm that the driver returned image data to the app. A blank preview with Save disabled means HP Scan does not have a usable page image to save.

1. Put one page face down on the glass and close the cover. The M28a is a flatbed model.
2. Close HP Scan and try `HP_M28a_Scan.exe`, which reads the scanner through Windows WIA directly.
3. If the WIA app saves the PDF, the scanner can return an image; the failure is likely in the HP Scan/TWAIN path or its current driver state. Use the WIA app as a workaround, then repair/reinstall the model's HP scanning software from HP Support if you need HP Scan back.
4. If both HP Scan and the WIA app fail, continue with the device and hardware checks below.

## Symptom: the WIA app does not list the printer

1. Make sure the printer is powered on and fully ready.
2. Connect USB directly to the computer. Avoid a hub or dock for the test. Try another USB port and, if available, a known-good USB data cable.
3. Click **Refresh** in the app.
4. In Windows Device Manager, look under **Imaging devices** or **Cameras** for `HP LaserJet MFP M28-M31 (USB)`. A warning icon, missing device, or repeated disconnects points to installation or USB enumeration.
5. Confirm the Windows **Windows Image Acquisition (WIA)** service is running. Open `services.msc`, find that service, and check its status. Do not change service settings unless you understand the PC's policy.
6. Restart the printer and PC, reconnect USB, then refresh the list again.

For current HP scanner-detection steps, see [HP: Scanner not found or connected](https://support.hp.com/us-en/document/ish_2041634-1713099-16).

## Symptom: the WIA app lists the printer but scan fails

1. Open `%LOCALAPPDATA%\HP_M28a_Scan\scanner.log` and note the most recent scan error. The log stays on this PC.
2. Disconnect and reconnect the USB cable at both ends. Test a different USB port and cable, with no hub.
3. Restart the printer and Windows, then try one page again.
4. Check that the paper lies flat on the glass and the lid closes. Clean the glass with a soft, lint-free cloth if the scan is streaked or unusually pale.
5. Make one copy directly from the printer using **Copy Black** with the same page on the glass:
   - If the copy is clear but Windows scans fail, the scanner mechanics work; focus on Windows WIA, the USB connection, and the HP driver path.
   - If the copy is blank or distorted too, the issue is likely in the scanner optics, carriage, or internal connection. Software cannot correct that; the unit may need service.

## Symptom: it works sometimes, then stops

Intermittent success can come from a marginal USB cable/port, a scanner driver or WIA handoff that is temporarily stuck, or a hardware fault. One successful WIA scan proves the device returned data on that attempt; it does not rule out an intermittent fault.

- Test with a direct USB cable and a different PC port.
- Restart printer and PC after a failed transfer.
- Compare results from HP Scan and this WIA app.
- Keep the timestamped app log when a failure occurs. Note whether the printer disappears from Device Manager or remains listed.
- If the direct copy test also fails intermittently, contact HP service or a printer technician.

## Error reporting

The app displays the immediate error and writes details to:

`%LOCALAPPDATA%\HP_M28a_Scan\scanner.log`

The log may contain device names, timestamps, file paths, and technical exception text. Review it before sharing publicly. Do not attach scanned documents unless you have removed private content.

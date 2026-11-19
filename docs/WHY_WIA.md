# Why use Windows WIA?

## How a normal software scan works

A desktop scan has several links: the scanner hardware reads the page; the USB connection carries the data; Windows exposes the scanner through a driver/interface; and the scanning application receives and saves the image. The scanner bar moving confirms only that the scan operation began. A usable preview confirms that the application received image data.

## What happened in this case

On the development PC:

- HP Scan identified `HP LaserJet MFP M28-M31 (USB)` and showed A4 in its settings.
- Scan operations ran, but the preview stayed blank and Save remained disabled.
- Windows listed an HP WIA imaging device with status OK, and its WIA service was running.
- A direct WIA transfer returned a 1700 x 2338 pixel page. The resulting PDF was rendered and checked.

That evidence points to an acquisition/driver handoff problem in HP Scan or its TWAIN path on that PC. It is an inference from one PC and one successful direct transfer, not proof of the root cause for all users. A USB cable, USB port, or scanner hardware problem can also be intermittent.

## What the replacement app does

The app uses Microsoft's Windows Image Acquisition (WIA) COM interface:

1. `WIA.DeviceManager` enumerates imaging devices.
2. It selects the installed M28-M31 scanner and connects to it.
3. It requests one bitmap from the flatbed scan item.
4. It converts the bitmap to JPEG in memory.
5. It saves a JPG or embeds that image into a one-page A4 PDF.

All work is local. The app does not upload or share scan data. A local diagnostic log records scanner discovery and errors to help investigate intermittent failures.

## Scope

This is a focused workaround for a Windows PC where the M28a is exposed through WIA. It does not patch HP Scan, replace the HP driver, alter Windows security policy, or repair physical scanner faults. The HP app may still need a driver repair if users specifically need HP Scan.

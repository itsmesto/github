using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

internal sealed class ScannerEntry
{
    public string Name;
    public object Info;
    public override string ToString() { return Name; }
}

internal static class Program
{
    private static readonly Guid BmpFormat = new Guid("B96B3CAB-0728-11D3-9D7B-0000F81EF32E");
    private static List<ScannerEntry> scanners = new List<ScannerEntry>();
    private static Form form;
    private static ComboBox scannerBox;
    private static ComboBox formatBox;
    private static TextBox folderBox;
    private static TextBox nameBox;
    private static Label status;
    private static Button scanButton;
    private static Button closeButton;
    private static string LogPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HP_M28a_Scan", "scanner.log"); } }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "--check")
            {
                List<ScannerEntry> found = FindScanners();
                string message = found.Count == 0 ? "No WIA scanner detected." : "Detected: " + String.Join(Environment.NewLine + "Detected: ", found.ConvertAll(x => x.Name).ToArray());
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "HP_M28a_Scan_check.txt"), message);
                return found.Count == 0 ? 2 : 0;
            }
            if (args.Length == 3 && args[0] == "--make-pdf")
            {
                MakePdfFromJpeg(args[1], args[2]);
                return 0;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BuildWindow();
            Application.Run(form);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("HP M28a Scan could not start.\r\n\r\n" + ex.Message, "HP M28a Scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static List<ScannerEntry> FindScanners()
    {
        Type type = Type.GetTypeFromProgID("WIA.DeviceManager");
        if (type == null) throw new InvalidOperationException("Windows Image Acquisition is unavailable on this PC.");
        dynamic manager = Activator.CreateInstance(type);
        var result = new List<ScannerEntry>();
        int count = Convert.ToInt32(manager.DeviceInfos.Count, CultureInfo.InvariantCulture);
        for (int i = 1; i <= count; i++)
        {
            dynamic info = manager.DeviceInfos.Item(i);
            if (Convert.ToInt32(info.Type, CultureInfo.InvariantCulture) == 1)
            {
                string deviceName = Convert.ToString(info.Properties.Item("Name").Value, CultureInfo.InvariantCulture);
                result.Add(new ScannerEntry { Name = deviceName, Info = (object)info });
            }
        }
        return result;
    }

    private static void BuildWindow()
    {
        form = new Form();
        form.Text = "HP M28a Scan";
        form.ClientSize = new Size(570, 260);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Font = new Font("Segoe UI", 9F);

        Label title = new Label { Text = "Scan directly through Windows", Font = new Font("Segoe UI", 13F, FontStyle.Bold), Location = new Point(18, 15), Size = new Size(530, 28) };
        form.Controls.Add(title);

        form.Controls.Add(new Label { Text = "Scanner", Location = new Point(20, 58), Size = new Size(95, 24) });
        scannerBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 55), Size = new Size(340, 25) };
        form.Controls.Add(scannerBox);
        Button refreshButton = new Button { Text = "Refresh", Location = new Point(470, 54), Size = new Size(82, 28) };
        form.Controls.Add(refreshButton);

        form.Controls.Add(new Label { Text = "Save folder", Location = new Point(20, 98), Size = new Size(95, 24) });
        folderBox = new TextBox { Location = new Point(120, 95), Size = new Size(340, 25), Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scans") };
        form.Controls.Add(folderBox);
        Button browseButton = new Button { Text = "Browse...", Location = new Point(470, 94), Size = new Size(82, 28) };
        form.Controls.Add(browseButton);

        form.Controls.Add(new Label { Text = "File name", Location = new Point(20, 138), Size = new Size(95, 24) });
        nameBox = new TextBox { Location = new Point(120, 135), Size = new Size(240, 25), Text = "Scan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) };
        form.Controls.Add(nameBox);
        form.Controls.Add(new Label { Text = "Format", Location = new Point(375, 138), Size = new Size(48, 24) });
        formatBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(422, 135), Size = new Size(130, 25) };
        formatBox.Items.Add("PDF"); formatBox.Items.Add("JPG"); formatBox.SelectedIndex = 0;
        form.Controls.Add(formatBox);

        status = new Label { Text = "Checking for scanner...", Location = new Point(20, 177), Size = new Size(532, 38) };
        form.Controls.Add(status);
        scanButton = new Button { Text = "Scan", Location = new Point(374, 220), Size = new Size(82, 30), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        form.Controls.Add(scanButton);
        closeButton = new Button { Text = "Close", Location = new Point(470, 220), Size = new Size(82, 30) };
        form.Controls.Add(closeButton);

        refreshButton.Click += delegate { RefreshScanners(); };
        browseButton.Click += delegate
        {
            using (var picker = new FolderBrowserDialog())
            {
                picker.Description = "Choose where scanned documents will be saved";
                picker.SelectedPath = folderBox.Text;
                if (picker.ShowDialog(form) == DialogResult.OK) folderBox.Text = picker.SelectedPath;
            }
        };
        scanButton.Click += delegate { ScanDocument(); };
        closeButton.Click += delegate { form.Close(); };
        form.Shown += delegate { RefreshScanners(); };
    }

private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " " + message + Environment.NewLine);
        }
        catch { }
    }

    private static void RefreshScanners()
    {
        try
        {
            scanners = FindScanners();
            Log("Scanner check: " + scanners.Count + " WIA device(s) detected.");
            scannerBox.Items.Clear();
            foreach (ScannerEntry scanner in scanners) scannerBox.Items.Add(scanner);
            if (scanners.Count > 0)
            {
                scannerBox.SelectedIndex = 0;
                status.Text = "Ready. Place the page face down on the glass and close the cover.";
            }
            else status.Text = "No WIA scanner found. Check power and USB, then click Refresh.";
        }
        catch (Exception ex) { status.Text = "Scanner check failed: " + ex.Message; }
    }

    private static string GetAvailablePath(string folder, string name, string extension)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        name = name.Trim();
        if (String.IsNullOrEmpty(name)) name = "Scan";
        string path = Path.Combine(folder, name + "." + extension);
        int suffix = 2;
        while (File.Exists(path)) path = Path.Combine(folder, name + " (" + suffix++ + ")." + extension);
        return path;
    }

    private static void ScanDocument()
    {
        if (scannerBox.SelectedItem == null)
        {
            MessageBox.Show(form, "No scanner is selected. Click Refresh and check the USB connection.", "Scanner not detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string scannerName = ((ScannerEntry)scannerBox.SelectedItem).Name;
        string folder = folderBox.Text.Trim();
        string fileName = nameBox.Text.Trim();
        string extension = formatBox.SelectedItem.ToString() == "PDF" ? "pdf" : "jpg";
        string output;
        try
        {
            Directory.CreateDirectory(folder);
            output = GetAvailablePath(folder, fileName, extension);
        }
        catch (Exception ex)
        {
            MessageBox.Show(form, "The save folder could not be used.\r\n\r\n" + ex.Message, "Save folder error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        scanButton.Enabled = false;
        closeButton.Enabled = false;
        status.Text = "Scanning... keep the cover closed.";
        Log("Scan started with " + scannerName + "; output format=" + extension + ".");
        Thread worker = new Thread(delegate() { ScanWorker(scannerName, folder, fileName, extension, output); });
        worker.IsBackground = true;
        worker.SetApartmentState(ApartmentState.STA);
        worker.Start();
    }

    private static void ScanWorker(string scannerName, string folder, string fileName, string extension, string output)
    {
        string bmpPath = Path.Combine(Path.GetTempPath(), "m28a-" + Guid.NewGuid().ToString("N") + ".bmp");
        try
        {
            ScannerEntry entry = FindScanners().Find(x => String.Equals(x.Name, scannerName, StringComparison.OrdinalIgnoreCase));
            if (entry == null) throw new InvalidOperationException("The scanner disconnected before the scan started. Click Refresh and try again.");
            dynamic info = entry.Info;
            dynamic device = info.Connect();
            dynamic item = device.Items.Item(1);
            dynamic imageFile = item.Transfer(BmpFormat.ToString("B").ToUpperInvariant());
            imageFile.SaveFile(bmpPath);
            using (Image image = Image.FromFile(bmpPath))
            using (var stream = new MemoryStream())
            {
                ImageCodecInfo codec = Array.Find(ImageCodecInfo.GetImageEncoders(), x => x.FormatID == ImageFormat.Jpeg.Guid);
                using (var parameters = new EncoderParameters(1))
                {
                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                    image.Save(stream, codec, parameters);
                }
                byte[] jpeg = stream.ToArray();
                if (extension == "pdf") MakePdf(jpeg, image.Width, image.Height, output);
                else File.WriteAllBytes(output, jpeg);
            }
            Log("Scan completed successfully: " + output);
            form.BeginInvoke((MethodInvoker)delegate
            {
                status.Text = "Saved: " + output;
                scanButton.Enabled = true;
                closeButton.Enabled = true;
                MessageBox.Show(form, "Scan complete.\r\n\r\n" + output, "Scan complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        catch (Exception ex)
        {
            Log("Scan failed: " + ex.ToString());
            form.BeginInvoke((MethodInvoker)delegate
            {
                status.Text = "Scan failed. Details are in the local diagnostic log.";
                scanButton.Enabled = true;
                closeButton.Enabled = true;
                MessageBox.Show(form, "The scan did not complete.\r\n\r\n" + ex.Message + "\r\n\r\nA diagnostic log was saved at:\r\n" + LogPath + "\r\n\r\nTry Refresh, then reconnect the USB cable directly to the PC.", "Scan failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }
        finally
        {
            try { if (File.Exists(bmpPath)) File.Delete(bmpPath); } catch { }
        }
    }
    private static void MakePdfFromJpeg(string jpegPath, string pdfPath)
    {
        using (Image image = Image.FromFile(jpegPath))
        using (var stream = new MemoryStream())
        {
            image.Save(stream, ImageFormat.Jpeg);
            MakePdf(stream.ToArray(), image.Width, image.Height, pdfPath);
        }
    }

    private static void MakePdf(byte[] jpeg, int imageWidth, int imageHeight, string path)
    {
        double pageWidth = 595.2756, pageHeight = 841.8898, margin = 28.3465;
        double scale = Math.Min((pageWidth - 2 * margin) / imageWidth, (pageHeight - 2 * margin) / imageHeight);
        double drawWidth = imageWidth * scale, drawHeight = imageHeight * scale;
        double left = (pageWidth - drawWidth) / 2, bottom = (pageHeight - drawHeight) / 2;
        string content = String.Format(CultureInfo.InvariantCulture, "q {0:0.####} 0 0 {1:0.####} {2:0.####} {3:0.####} cm /Im0 Do Q", drawWidth, drawHeight, left, bottom);
        byte[] contentBytes = Encoding.ASCII.GetBytes(content);
        var pdf = new List<byte>();
        var offsets = new List<long>();
        Action<string> text = s => pdf.AddRange(Encoding.ASCII.GetBytes(s));
        text("%PDF-1.4\n");
        offsets.Add(pdf.Count); text("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        offsets.Add(pdf.Count); text("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        offsets.Add(pdf.Count); text(String.Format(CultureInfo.InvariantCulture, "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0} {1}] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>\nendobj\n", pageWidth, pageHeight));
        offsets.Add(pdf.Count); text(String.Format(CultureInfo.InvariantCulture, "4 0 obj\n<< /Type /XObject /Subtype /Image /Width {0} /Height {1} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {2} >>\nstream\n", imageWidth, imageHeight, jpeg.Length));
        pdf.AddRange(jpeg); text("\nendstream\nendobj\n");
        offsets.Add(pdf.Count); text("5 0 obj\n<< /Length " + contentBytes.Length + " >>\nstream\n");
        pdf.AddRange(contentBytes); text("\nendstream\nendobj\n");
        long xref = pdf.Count;
        text("xref\n0 6\n0000000000 65535 f \n");
        foreach (long offset in offsets) text(offset.ToString("0000000000", CultureInfo.InvariantCulture) + " 00000 n \n");
        text("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n" + xref.ToString(CultureInfo.InvariantCulture) + "\n%%EOF\n");
        File.WriteAllBytes(path, pdf.ToArray());
    }
}






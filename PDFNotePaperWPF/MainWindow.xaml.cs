using System;
using System.Windows;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Windows.Threading;

namespace PDFNotePaperWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _targetFolderPath = null;
        string _backgroundPdfFilename = null;
        string[] _fileNames;

        public MainWindow()
        {
            InitializeComponent();
            lblBackgroundPdf.Content = Properties.Settings.Default.BackgroundFilename;
            _backgroundPdfFilename = Properties.Settings.Default.BackgroundFilename;
            lblTargetFolderPath.Content = Properties.Settings.Default.TargetFolderPath;
            _targetFolderPath = Properties.Settings.Default.TargetFolderPath;
        }

        private void btnBackground_Click(object sender, RoutedEventArgs e)
        {
            InitProgressBar();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            openFileDialog.Title = "Briefpapier auswählen...";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                _backgroundPdfFilename = openFileDialog.FileNames[0];
                lblBackgroundPdf.Content = _backgroundPdfFilename;
                Properties.Settings.Default.BackgroundFilename = _backgroundPdfFilename;
                Properties.Settings.Default.Save();
            }
        }

        private void btnSelectTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            InitProgressBar();
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            folderBrowser.Title = "Zielverzeichnis auswählen...";
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == true)
            {
                _targetFolderPath = System.IO.Path.GetDirectoryName(folderBrowser.FileName);
                lblTargetFolderPath.Content = _targetFolderPath;
                // ...
                Properties.Settings.Default.TargetFolderPath = _targetFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
        {


            InitProgressBar();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Zu konvertierende Dateien auswählen...";
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                if (!lbFiles.Items.IsEmpty)
                {
                    lbFiles.Items.Clear();
                }
                _fileNames = openFileDialog.FileNames;
                foreach (string filename in _fileNames)
                {
                    lbFiles.Items.Add(System.IO.Path.GetFileName(filename));
                }
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_fileNames == null || _fileNames.Length < 1)
                {
                    throw new NullReferenceException();
                }

                var i = 0;
                progressBar.Minimum = 0;
                progressBar.Maximum = _fileNames.Length;
                foreach (string filename in _fileNames)
                {
                    ++i;
                    DrawBackgroundPdfToPdf(_backgroundPdfFilename, filename);
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                    {                        
                        progressBar.Value = i;
                        progressBarStatus.Text = i + " / " + _fileNames.Length;
                    }));
                    
                    
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please select at least one file for conversion");
            }


        }
                
        public void DrawBackgroundPdfToPdf(string notePaperFilename, string documentFilename)
        {
            PdfDocument Document = PdfReader.Open(documentFilename, PdfDocumentOpenMode.Import);

            PdfDocument OutputPdf = new PdfDocument();

            foreach (PdfPage page in Document.Pages)
            {
                OutputPdf.AddPage(page);
            }

            var notePaperXImage = XImage.FromFile(notePaperFilename);

            foreach (PdfPage page in OutputPdf.Pages)
            {
                XGraphics graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                graphics.DrawImage(notePaperXImage, 1, 1);

            }

            try
            {
                if (string.IsNullOrEmpty(_targetFolderPath))
                    throw new ArgumentNullException();
                string resultFilename = _targetFolderPath + "\\" + System.IO.Path.GetFileName(documentFilename);
                OutputPdf.Save(resultFilename);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show("Target folder must not be empty");
            }

        }

        public void InitProgressBar()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                progressBar.Value = 0;
                progressBarStatus.Text = "";
            }));
        }


        


    }
}

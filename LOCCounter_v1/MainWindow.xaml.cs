using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Xml.Serialization;
using System.Xml;

namespace LOCCounter_v1
{
    public partial class MainWindow : Window
    {
        private static List<Files> sourceFiles = new List<Files>();
        int totalFileCount;
        private static int loc;
        private static bool turbo = false;
        private static FileExtensions extensions = new FileExtensions();
        BackgroundWorker worker = new BackgroundWorker();
        private static Dictionary<string, int> CodeBreakup = new Dictionary<string, int>();

        delegate void progressTextUpdater(string file);
        delegate void progressBarUpdater(int count);
        delegate void updateLOCCounter();
        delegate void updateLog(string message);

        public MainWindow()
        {
            InitializeComponent();
            PopulateExtensions();
        }

        private void btnDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtDir.Text = dialog.SelectedPath;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            worker.WorkerSupportsCancellation = true;
            if (btnStart.Content.ToString() == "Start")
            {
                btnStart.Content = "Stop";
                if (!String.IsNullOrEmpty(txtDir.Text))
                {
                    if (cbxTurbo.IsChecked == true)
                        turbo = true;

                    txtProgress.Foreground = Brushes.Black;
                    try
                    {
                        txtProgress.Text = "Analyzing input directory...";
                        LogToBox("Analyzing input directory");
                        generateListofFiles(new DirectoryInfo(txtDir.Text));
                    }
                    catch (Exception ex)
                    {
                        txtProgress.Text = "Failed: " + ex.Message;
                        LogToBox("Failed", ex);
                        txtProgress.Foreground = Brushes.Red;
                        sourceFiles = null;
                        btnStart.Content = "Start";
                    }

                    if (sourceFiles != null)
                    {
                        worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                        worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                        totalFileCount = sourceFiles.Count;
                        LogToBox("Total number of files: " + totalFileCount.ToString());
                        loc = 0;
                        txtProgress.Foreground = Brushes.Black;
                        worker.RunWorkerAsync();
                    }

                }
                else
                {
                    txtProgress.Text = "Select an input directory.";
                    LogToBox("Select an input directory");
                    txtProgress.Foreground = Brushes.Red;
                    btnStart.Content = "Start";
                }
            }
            else
            {
                LogToBox("Stopping...");
                worker.CancelAsync();
            }
        }

        private void generateListofFiles(DirectoryInfo source)
        {            
            foreach (DirectoryInfo dir in source.GetDirectories())
                generateListofFiles(dir);
            foreach (FileInfo file in source.GetFiles())
            {
                Files currfile = new Files();
                currfile.fullname = file.FullName;
                currfile.extension = file.Extension.Replace(".","");
                sourceFiles.Add(currfile);
            }
        }

        void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            int currentFileCount = 0;
            int currentCodeCount;
            updateLog updateLogDelegate = new updateLog(updateLogMethod);
            System.Windows.Threading.DispatcherOperation updateLogBox = txtLog.Dispatcher.BeginInvoke(updateLogDelegate, System.Windows.Threading.DispatcherPriority.Normal,"Reading files");
            foreach (Files file in sourceFiles)
            {
                currentCodeCount = 0;
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                if (isWhitelisted(file.extension.ToUpper()))
                {
                    if (!turbo)
                        Thread.Sleep(50);

                    currentFileCount++;
                    System.Windows.Threading.DispatcherOperation updateProgressText = tbxLoc.Dispatcher.BeginInvoke(new progressTextUpdater(progressTextUpdateMethod), System.Windows.Threading.DispatcherPriority.Normal, file.fullname);
                    System.Windows.Threading.DispatcherOperation updateProgressBar = progress.Dispatcher.BeginInvoke(new progressBarUpdater(progressBarUpdateMethod), System.Windows.Threading.DispatcherPriority.Normal, currentFileCount);

                    try
                    {
                        using (TextReader reader = new StreamReader(file.fullname))
                        {
                            while (reader.ReadLine() != null)
                            {
                                currentCodeCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        updateLogBox = txtLog.Dispatcher.BeginInvoke(updateLogDelegate, System.Windows.Threading.DispatcherPriority.Normal, "Exception: " + ex.Message);
                    }

                    loc += currentCodeCount;

                    if (CodeBreakup.ContainsKey(file.extension.ToLower()))
                    {
                        CodeBreakup[file.extension.ToLower()] += currentCodeCount;
                    }
                    else
                    {
                        CodeBreakup.Add(file.extension.ToLower(), currentCodeCount);
                    }

                    System.Windows.Threading.DispatcherOperation updateLocCounter = progress.Dispatcher.BeginInvoke(new updateLOCCounter(LocCounterUpdateMethod), System.Windows.Threading.DispatcherPriority.Normal);
                }
                else
                {
                    currentFileCount++;
                    updateLogBox = txtLog.Dispatcher.BeginInvoke(updateLogDelegate, System.Windows.Threading.DispatcherPriority.Normal, "Skipping file: " + file.fullname);
                }
            }

        }

        void worker_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                txtProgress.Text = "Stopped";
                LogToBox("Stopped");                
            }
            else if (e.Error != null)
            {
                txtProgress.Text = "Exception Thrown. " + e.Result;
                LogToBox("Failed", new Exception(e.Result.ToString()));                
            }
            else
            {
                txtProgress.Foreground = Brushes.Green;
                txtProgress.Text = "Finished!";
                LogToBox("Finished. Final count: " + loc.ToString() + " lines of code.");
                btnBreakup.Visibility = System.Windows.Visibility.Visible;
            }           

            //code to reset
            LogToBox(Environment.NewLine);            
            btnStart.Content = "Start";
            loc = 0;
            sourceFiles.Clear();
        }

        void progressTextUpdateMethod(string file)
        {
            txtProgress.Text = "Reading file " + file;
            //LogToBox("Reading file " + file);
        }

        void progressBarUpdateMethod(int count)
        {
            float progressValue = count/ (float)totalFileCount * 100;
            progress.Value = progressValue;
            
        }

        void LocCounterUpdateMethod()
        {
            if (loc > 10000)
            {
                tbxLoc.Foreground = new SolidColorBrush(Color.FromRgb(0x09, 0x41, 0x05));
            }
            else if (loc > 1000)
            {
                tbxLoc.Foreground = new SolidColorBrush(Color.FromRgb(0xCE, 0xAF, 0x16));
            }

            tbxLoc.Text = loc.ToString(); //update counter
        }

        void updateLogMethod(string message)
        {
            LogToBox(message);
        }

        void LogToBox(string message, Exception ex = null)
        {
            txtLog.AppendText(Environment.NewLine + message);
            if (ex != null)
                txtLog.AppendText(Environment.NewLine + "Exception: " + ex.Message);

            txtLog.ScrollToEnd();
        }

        private void btnWhitelist_Click(object sender, RoutedEventArgs e)
        {
            whitelist whitelistwindow = new whitelist(extensions);
            whitelistwindow.Show();
        }

        private void PopulateExtensions()
        {            
            string filename = "whitelist.xml";
            try
            {
                var deserializer = new System.Xml.Serialization.XmlSerializer(typeof(FileExtensions));
                FileStream fs = new FileStream(filename, FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);
                extensions = (FileExtensions)deserializer.Deserialize(reader);
                fs.Close();
                reader.Close();
                LogToBox("Whitelist loaded");
            }
            catch
            {
                extensions = null;
                btnWhitelist.Visibility = System.Windows.Visibility.Hidden;
                LogToBox("Failed to load whitelist. Won't ignore any files.");
            }
        }

        private bool isWhitelisted(string extension)
        {
            if (extensions == null)
            {
                return true;
            }
            else if (extensions.Script.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Program.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Other.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Custom.extensions.Contains(extension))
            {
                return true;
            }

            return false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            about aboutWindow = new about();
            aboutWindow.Show();            
        }

        private void btnBreakup_Click(object sender, RoutedEventArgs e)
        {
            Detail detailWindow = new Detail(CodeBreakup);
            detailWindow.Show();
        }       

    }

    public class Files
    {
        public string fullname;
        public string extension;
    }
}

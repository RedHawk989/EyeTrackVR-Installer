using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using IWshRuntimeLibrary;



namespace EyeTRackVR_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary
    /// 
    public static class Extensions
    {
        public static async Task DownloadFile(this HttpClient client, string address, string fileName)
        {
            using (var response = await client.GetAsync(address))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var file = System.IO.File.OpenWrite(fileName))
            {
                stream.CopyTo(file);
            }
        }
    }
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// >
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        //public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public string folderdir;
        public string inspath;
        public bool? mkshortcut;
        public void ChangePath_Click(object sender, RoutedEventArgs e) //open file browser
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select Install folder";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inspath = fbd.SelectedPath + "\\EyeTrackVR.zip";
                folderdir = fbd.SelectedPath + "\\EyeTrackVR";
                textBox1.Text = folderdir;
            }
        }


        private void mkUnchecked(object sender, RoutedEventArgs e)
        {
            mkshortcut = false;


        }

        private void mkCheck(object sender, RoutedEventArgs e)
        {
            mkshortcut = true;


        }


        private void Docs_Click(object sender, RoutedEventArgs e)
        {

            Process DocsProcess = new Process();
            DocsProcess.StartInfo.UseShellExecute = true;
            DocsProcess.StartInfo.FileName = "https://redhawk989.github.io/EyeTrackVR/";
            DocsProcess.Start();
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {

            Process GithubProcess = new Process();
            GithubProcess.StartInfo.UseShellExecute = true;
            GithubProcess.StartInfo.FileName = "https://github.com/RedHawk989/EyeTrackVR";
            GithubProcess.Start();
        }


        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            Process DiscordProcess = new Process();
            DiscordProcess.StartInfo.UseShellExecute = true;
            DiscordProcess.StartInfo.FileName = "https://discord.gg/kkXYbVykZX";
            DiscordProcess.Start();
        }

        public async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            //actually install stuff lmao
            InstallButton.Content = "Installing...";
            if (string.IsNullOrEmpty(folderdir))
            {
                folderdir = "C:\\Program Files\\EyeTrackVR";
            }

            if (string.IsNullOrEmpty(inspath))
            {
                inspath = "C:\\Program Files\\EyeTrackVR\\EyeTrackVR.zip";
            }


            System.IO.Directory.CreateDirectory(folderdir);

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData("https://raw.githubusercontent.com/RedHawk989/EyeTrackVR/main/Latest_Version.txt");  //get download link from repo

            string webData = System.Text.Encoding.UTF8.GetString(raw);
            webData = webData.Replace("\n", "").Replace("\r", "");


            InstallButton.Content = "Downloading...";
            using (var httpClient = new HttpClient()) // download
            {
                HttpClient httpClient1 = httpClient;
                await httpClient1.DownloadFile(webData, inspath);
            }


            InstallButton.Content = "Extracting...";
            ZipFile.ExtractToDirectory(inspath, folderdir); //extract zip

            InstallButton.Content = "Cleaning...";
            System.IO.File.Delete(inspath); // delete zip


       

            byte[] raw2 = wc.DownloadData("https://raw.githubusercontent.com/RedHawk989/EyeTrackVR/main/ver_num.txt");  //get download link from repo

            string webData2 = System.Text.Encoding.UTF8.GetString(raw2);
            webData2 = webData2.Replace("\n", "").Replace("\r", "");
            InstallButton.Content = "Making Shortcut...";


            // we need to check if create shortcut was checked




            if (!mkshortcut.HasValue || mkshortcut is true)
            {
                string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string exepath = folderdir + "\\RANSACApp-" + webData2 + "\\RANSACApp.exe";

                string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + System.IO.Path.DirectorySeparatorChar + "EyeTrackVR" + ".lnk";
                var shell = new WshShell();
                var shortcut = shell.CreateShortcut(link) as IWshShortcut;
                shortcut.TargetPath = exepath;
                shortcut.WorkingDirectory = folderdir;
                //shortcut...
                shortcut.Save(); // null
            }



            /* WshShell wsh = new WshShell();
             IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(
                 Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\EyeTrackVR.lnk") as IWshRuntimeLibrary.IWshShortcut;
             shortcut.Arguments = "";
             shortcut.TargetPath = exepath;
             // not sure about what this is for
             shortcut.WindowStyle = 1;
             shortcut.Description = "Shortcut for EyeTrackVR";
             shortcut.WorkingDirectory = folderdir;
             shortcut.IconLocation = "Images/logo.png";
             shortcut.Save();
            */









            textBox2.Text = "Successfully Installed!";
            InstallButton.Content = "Installed!";
        }

        private void Get_MouseDown(object sender, MouseButtonEventArgs e) //allow window to be moved on the top
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }





    }
}

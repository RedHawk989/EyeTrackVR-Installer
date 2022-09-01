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
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Net.Http;



namespace EyeTrackVR_Installer
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




    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            string rootPath = @AppDomain.CurrentDomain.BaseDirectory;
            string[] dirs = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly);

        }



        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryName, bool overwrite)
        {
            using (var archive = ZipFile.Open(sourceZipFilePath, ZipArchiveMode.Read))
            {
                if (!overwrite)
                {
                    archive.ExtractToDirectory(destinationDirectoryName);
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
                string destinationDirectoryFullPath = di.FullName;

                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = System.IO.Path.GetFullPath(System.IO.Path.Combine(destinationDirectoryFullPath, file.FullName));

                    if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                    }

                    if (file.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(completeFileName));

                        continue;
                    }
                    file.ExtractToFile(completeFileName, true);
                }
            }
        }




        private static void GrantAccess(string file)
        {
            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);

            }

            DirectoryInfo dInfo = new DirectoryInfo(file);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

        }


        private void CloseButton_Click(object sender, RoutedEventArgs e) //when X is clicked, close
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) //when - is clicked, minimize
        {
            this.WindowState = WindowState.Minimized;
        }
        const string lverurl = "https://raw.githubusercontent.com/RedHawk989/EyeTrackVR-Installer/master/Version-Data/Latest_Version.txt"; // install version data etc
        const string vnumurl = "https://raw.githubusercontent.com/RedHawk989/EyeTrackVR-Installer/master/Version-Data/Version_Num.txt";

        const string DocsLink = "https://redhawk989.github.io/EyeTrackVR/"; // top links string deff
        const string GitHubLink = "https://github.com/RedHawk989/EyeTrackVR";
        const string DiscordLink = "https://discord.gg/kkXYbVykZX";

        public void LinkSet(string url)
        {
            Process DiscordProcess = new Process();
            DiscordProcess.StartInfo.UseShellExecute = true;
            DiscordProcess.StartInfo.FileName = url;
            DiscordProcess.Start();
        }

        public string folderdir;
        public string inspath;
        public bool? mkshortcut;
        public bool? delinstaller;
        public void ChangePath_Click(object sender, RoutedEventArgs e) //open file browser
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select Install folder";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inspath = fbd.SelectedPath + "\\EyeTrackApp.zip"; //set selected path to that path and append /eyetrackvr to it
                folderdir = fbd.SelectedPath + "\\EyeTrackVR\\EyeTrackVR\\";
   
            }
        }


        public string HttpData(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(link).Result;
                HttpContent content = response.Content;
                string lver = content.ReadAsStringAsync().Result;
                return lver;
            }

        }


        private void Docs_Click(object sender, RoutedEventArgs e) //docs link at top
        {
            LinkSet(DocsLink);
        }
        private void Github_Click(object sender, RoutedEventArgs e) //github link at top
        {
            LinkSet(GitHubLink);
        }
        private void Discord_Click(object sender, RoutedEventArgs e) //discord link at top
        {
            LinkSet(DiscordLink);
        }

        public async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            //actually install stuff lmao
            textBox2.Text = "";
            InstallButton.Content = "Installing";



            string lver = HttpData(lverurl);
           // string vernum = HttpData(vnumurl);


            if (string.IsNullOrEmpty(folderdir)) //define default dirs
            {
                folderdir = "C:\\Program Files\\EyeTrackVR";
            }

            if (string.IsNullOrEmpty(inspath))
            {
                inspath = "C:\\Program Files\\EyeTrackVR\\EyeTrackApp.zip"; //name of zip folder once downloaded
            }

            System.IO.Directory.CreateDirectory(folderdir); //create install dir

            InstallButton.Content = "Downloading";
            using (var httpClient = new HttpClient()) // download zip
            {
                HttpClient httpClient1 = httpClient;
               // httpClient1.Timeout = new TimeSpan(0, 0, 300); // timeout 300 seconds (5min)
                await httpClient1.DownloadFile(lver, inspath);
            }


            InstallButton.Content = "Extracting";
            await Task.Delay(500); //give OS time. fixes odd bug where System.IO.InvalidDataException: 'Central Directory corrupt.' would be called
                                   // ZipFile.ExtractToDirectory(inspath, folderdir); //extract zip


            if (Directory.Exists(folderdir))
            {
                ExtractZipFileToDirectory(inspath, folderdir, true);
            }

            GrantAccess(folderdir); //make perms on install folder user, this keeps the eyetracking app from trowing no permission errors.


            InstallButton.Content = "Cleaning";
            System.IO.File.Delete(inspath); // delete zip



            if (CheckBox.IsChecked == true)         // if box is checked make a shortcut on desktop
            {
                InstallButton.Content = "Making Shortcut...";
                string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string exepath = folderdir + "\\EyeTrackApp\\eyetrackapp.exe";

                string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + System.IO.Path.DirectorySeparatorChar + "EyeTrackVR" + ".lnk";
                var shell = new WshShell();
                var shortcut = shell.CreateShortcut(link) as IWshShortcut;

                shortcut.TargetPath = exepath;
                shortcut.WorkingDirectory = folderdir; //where output files will be made from the eyetrack app
                shortcut.Save();
            }



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

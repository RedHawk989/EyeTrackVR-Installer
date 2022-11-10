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

        public static void Empty(this System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }


    public static class ZipArchiveExtension
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = System.IO.Path.Combine(destinationDirectoryName, file.FullName);
                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }



    public static class HttpClientUtils
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(FileName, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
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


        public void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryName, bool overwrite) /// WHY DOES THIS NOT WORK IGHKLSJDHGLKJSHDGLKJSHDGLKJSH HELPPPPPPPPP
        {
            try
            { 
                //Declare a temporary path to unzip your files
                string tempPath = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "tempUnzip");
                
                ZipFile.ExtractToDirectory(sourceZipFilePath, tempPath);

                //build an array of the unzipped directories:
                string[] folders = Directory.GetDirectories(tempPath);

                foreach (string folder in folders)
                {
                    DirectoryInfo d = new DirectoryInfo(folder);
                    //If the directory doesn't already exist in the destination folder, move it to the destination.
                    if (!Directory.Exists(System.IO.Path.Combine(destinationDirectoryName, d.Name)))
                    {
                        Directory.Move(d.FullName, System.IO.Path.Combine(destinationDirectoryName, d.Name));
                        continue;
                    }
                    //If directory does exist, iterate through the files updating duplicates.
                    else
                    {
                        string[] subFiles = Directory.GetFiles(d.FullName);
                        foreach (string subFile in subFiles)
                        {
                            FileInfo f = new FileInfo(subFile);
                            //Check if the file exists already, if so delete it and then move the new file to the extract folder
                            if (System.IO.File.Exists(System.IO.Path.Combine(destinationDirectoryName, d.Name, f.Name)))
                            {
                                System.IO.File.Delete(System.IO.Path.Combine(destinationDirectoryName, d.Name, f.Name));
                                System.IO.File.Move(f.FullName, System.IO.Path.Combine(destinationDirectoryName, d.Name, f.Name));
                            }
                            else
                            {
                                System.IO.File.Move(f.FullName, System.IO.Path.Combine(destinationDirectoryName, d.Name, f.Name));
                            }
                        }
                    }
                }
                //build an array of the unzipped files in the parent directory
                string[] files = Directory.GetFiles(tempPath);

                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);
                    //Check if the file exists already, if so delete it and then move the new file to the extract folder
                    if (System.IO.File.Exists(System.IO.Path.Combine(destinationDirectoryName, f.Name)))
                    {
                        System.IO.File.Delete(System.IO.Path.Combine(destinationDirectoryName, f.Name));
                        System.IO.File.Move(f.FullName, System.IO.Path.Combine(destinationDirectoryName, f.Name));
                    }
                    else
                    {
                        System.IO.File.Move(f.FullName, System.IO.Path.Combine(destinationDirectoryName, f.Name));
                    }
                }
                Directory.Delete(tempPath);
            }
            catch (Exception ex)
            {
                const string errmsg = "aua";
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
                inspath = fbd.SelectedPath + "\\EyeTrackVR\\EyeTrackApp.zip"; //set selected path to that path and append /eyetrackvr to it
                folderdir = fbd.SelectedPath + "\\EyeTrackVR";
                textBox1.Text = folderdir;
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
                inspath = "C:\\Program Files\\EyeTrackVR\\EyeTrackApp.zip"; // default name of zip folder once downloaded
            }

            System.IO.Directory.CreateDirectory(folderdir); //create install dir

            InstallButton.Content = "Downloading";
            //  using (var httpClient = new HttpClient()) // download zip
            // {
            //    HttpClient httpClient1 = httpClient;
            // httpClient1.Timeout = new TimeSpan(0, 0, 300); // timeout 300 seconds (5min)
            // await httpClient1.DownloadFile(lver, inspath);
            //     await httpClient1.DownloadFile("https://github.com/RedHawk989/EyeTrackVR/releases/download/EyeTrackApp-0.1.6/EyeTrackApp-0.1.6-win-amd-64.zip", inspath);
            //   textBox2.Text = inspath;

            // }

            // uri = new Uri(lver);
            using (var client = new System.Net.Http.HttpClient()) // WebClient
            {
                var uri = new Uri(lver);

                await client.DownloadFileTaskAsync(uri, "C:\\Program Files\\tempeyetrackapp.zip");
            }


            InstallButton.Content = "Extracting";
            await Task.Delay(300); //give OS time. fixes odd bug where System.IO.InvalidDataException: 'Central Directory corrupt.' would be called
                                   // ZipFile.ExtractToDirectory(inspath, folderdir); //extract zip


            if (Directory.Exists(folderdir + "\\EyeTrackApp"))
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(folderdir + "\\EyeTrackApp");

                directory.Empty();

            }
            if (Directory.Exists(folderdir))
            {
               
                ZipFile.ExtractToDirectory("C:\\Program Files\\tempeyetrackapp.zip", folderdir);
            }
                
                //ExtractZipFileToDirectory(inspath, folderdir, true);
               // ExtractZipFileToDirectory("C:\\Users\\beaul\\OneDrive\\Desktop\\EyeTrackVR-Installer\\EyeTrackVR", "C:\\Users\\beaul\\OneDrive\\Desktop\\EyeTrackVR - Installer\\EyeTrackVR\\EyeTrackApp.zip", true);
               // textBox2.Text = inspath;

            


            GrantAccess(folderdir); //make perms on install folder user, this keeps the eyetracking app from trowing no permission errors.


            InstallButton.Content = "Cleaning";
            System.IO.File.Delete("C:\\Program Files\\tempeyetrackapp.zip"); // delete zip



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

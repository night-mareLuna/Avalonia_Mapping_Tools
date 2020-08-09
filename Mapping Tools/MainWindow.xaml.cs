﻿using AutoUpdaterDotNET;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.Tools;
using Mapping_Tools.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.Exceptions;
using Mapping_Tools.Views.Standard;

namespace Mapping_Tools {

    public partial class MainWindow {
        public bool IsMaximized; //Check for window state
        public double WidthWin, HeightWin; //Set default sizes of window
        public ViewCollection Views;
        public ListenerManager ListenerManager;
        public bool SessionhasAdminRights;

        public static MainWindow AppWindow { get; set; }
        public static string AppCommon { get; set; }
        public static string AppDataPath { get; set; }
        public static string ExportPath { get; set; }
        public static Random MainRandom { get; set; }
        public static HttpClient HttpClient { get; set; }
        public static SnackbarMessageQueue MessageQueue { get; set; }

        public MainWindow() {
            // Initialize exception logging
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            InitializeComponent();

            try {
                AppWindow = this;
                AppCommon = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                AppDataPath = Path.Combine(AppCommon, "Mapping Tools");
                ExportPath = Path.Combine(AppDataPath, "Exports");
                MainRandom = new Random();
                HttpClient = new HttpClient();
                MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
                MainSnackbar.MessageQueue = MessageQueue;

                Setup();
                SettingsManager.LoadConfig();
                ListenerManager = new ListenerManager();
                IsMaximized = SettingsManager.Settings.MainWindowMaximized;
                WidthWin = SettingsManager.Settings.MainWindowWidth ?? Width;
                HeightWin = SettingsManager.Settings.MainWindowHeight ?? Height;
                IsMaximized = !IsMaximized;
                ToggleWin(this, null);
                WidthWin = SettingsManager.Settings.MainWindowWidth ?? Width;
                HeightWin = SettingsManager.Settings.MainWindowHeight ?? Height;
                SetCurrentView(typeof(StandardView)); // Generate Standard view model to show on startup

                SetCurrentMaps(SettingsManager.GetLatestCurrentMaps()); // Set currentmap to previously opened map
            } catch (Exception ex) {
                ex.Show();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            // Log the exception, display it, etc
            if (!(e.ExceptionObject is Exception exception)) return;
            var lines = new List<string> {exception.Message, exception.StackTrace, exception.Source};

            while (exception.InnerException != null) {
                exception = exception.InnerException;
                lines.Add("\nInner exception:");
                lines.Add(exception.Message);
                lines.Add(exception.StackTrace);
                lines.Add(exception.Source);
            }

            var path = Path.Combine(AppDataPath, "crash-log.txt");
            File.WriteAllLines(path, lines);
            MessageBox.Show($"The program encountered an unhandled exception. Look in crash-log.txt for more info:\n{path}", "Error");
        }

        private void Setup() {
            SessionhasAdminRights = IsUserAdministrator();

            try {
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
                AutoUpdater.Start("https://mappingtools.seira.moe/current/updater.json");
            }
            catch( Exception ex ) {
                Console.WriteLine(ex.Message);
            }

            try {
                Directory.CreateDirectory(AppDataPath);
                Directory.CreateDirectory(ExportPath);
            }
            catch( Exception ex ) {
                ex.Show();
            }

            Views = new ViewCollection(); // Make a ViewCollection object
            ToolsMenu.ItemsSource = ViewCollection.GetAllToolTypes().Where(o => o.GetCustomAttribute<HiddenToolAttribute>() == null).Select(o => {
                var name = ViewCollection.GetName(o);
                var item = new MenuItem {Header = "_" + name, ToolTip = $"Open {name}."};
                item.Click += ViewSelectMenuItemOnClick;
                return item;
            }).OrderBy(o => o.Header);
        }

        private void SetCurrentView(string name) {
            try {
                SetCurrentView(Views.GetView(name));
            } catch (ArgumentException ex) {
                ex.Show();
            }
        }

        private void SetCurrentView(Type type) {
            try {
                SetCurrentView(Views.GetView(type));
            } catch (ArgumentException ex) {
                ex.Show();
            }
        }

        public void SetCurrentView(object view) {
            if (view == null) return;

            var type = view.GetType();

            if (FindName("header") is TextBlock txt) {
                txt.Text = type.GetCustomAttribute<DontShowTitleAttribute>() == null ? $"Mapping Tools - {ViewCollection.GetName(type)}" : "Mapping Tools";
            }

            if (DataContext is MappingTool mt) {
                mt.Deactivate();
            }
            if (view is MappingTool nmt) {
                nmt.Activate();
            }

            DataContext = view;
            ViewChanged();
        }

        private void ViewSelectMenuItemOnClick(object sender, RoutedEventArgs e) {
            if (((MenuItem) sender).Header == null) return;

            var toolName = ((MenuItem) sender).Header.ToString().Substring(1);
            SetCurrentView(toolName);
        }

        private bool IsUserAdministrator() {
            bool isAdmin;
            try {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch( UnauthorizedAccessException ) {
                isAdmin = false;
            }
            catch( Exception ) {
                isAdmin = false;
            }
            return isAdmin;
        }

        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args) {
            try {
                dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
                args.UpdateInfo = new UpdateInfoEventArgs {
                    CurrentVersion = json.version,
                    ChangelogURL = json.changelog,
                    Mandatory = json.mandatory,
                    DownloadURL = json.url
                };
            }
            catch( Exception ex ) {
                Console.WriteLine(ex.Message);
            }
        }

        public object GetCurrentView() {
            return DataContext;
        }

        public void SetCurrentMaps(string[] paths) {
            currentMap.Text = string.Join("|", paths);
            SettingsManager.AddRecentMap(paths, DateTime.Now);
        }

        public void SetCurrentMapsString(string paths) {
            currentMap.Text = paths;
            SettingsManager.AddRecentMap(paths.Split('|'), DateTime.Now);
        }

        public string[] GetCurrentMaps() {
            return currentMap.Text.Split('|');
        }

        public string GetCurrentMapsString() {
            return string.Join("|", GetCurrentMaps());
        }

        private void OpenBeatmap(object sender, RoutedEventArgs e) {
            try {
                string[] paths = IOHelper.BeatmapFileDialog(true);
                if (paths.Length != 0) {
                    SetCurrentMaps(paths);
                }
            }
            catch (Exception ex) {
                ex.Show();
            }
        }

        private void OpenGetCurrentBeatmap(object sender, RoutedEventArgs e) {
            try {
                string path = IOHelper.GetCurrentBeatmap();
                if (path != "") {
                    SetCurrentMaps(new[] { path });
                }
            } catch (Exception ex) {
                ex.Show();
            }
        }

        private void SaveBackup(object sender, RoutedEventArgs e) {
            try {
                var paths = GetCurrentMaps();
                var result = BackupManager.SaveMapBackup(paths, true, "UB");  // UB stands for User Backup
                if (result)
                    Task.Factory.StartNew(() => MessageQueue.Enqueue($"Beatmap{(paths.Length == 1 ? "" : "s")} successfully copied!"));
            } catch (Exception ex) {
                ex.Show();
            }
        }

        private void LoadBackup(object sender, RoutedEventArgs e) {
            try {
                var paths = GetCurrentMaps();
                if (paths.Length > 1) {
                    throw new Exception($"Can't load backup into multiple beatmaps. You currently have {paths.Length} beatmaps selected.");
                }
                var backupPaths = IOHelper.BeatmapFileDialog(SettingsManager.GetBackupsPath(), false);
                if (backupPaths.Length == 1) {
                    try {
                        BackupManager.LoadMapBackup(backupPaths[0], paths[0], false);
                    }
                    catch (BeatmapIncompatibleException ex) {
                        ex.Show();
                        var result = MessageBox.Show("Do you want to load the backup anyways?", "Load backup",
                            MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes) {
                            BackupManager.LoadMapBackup(backupPaths[0], paths[0], true);
                        } else {
                            return;
                        }
                    }
                    Task.Factory.StartNew(() => MessageQueue.Enqueue("Backup successfully loaded!"));
                }
            } catch (Exception ex) {
                ex.Show();
            }
        }

        private void ViewChanged() { 
            if (!(FindName("ProjectMenu") is MenuItem projectMenu)) return;

            var isSavable = DataContext.GetType().GetInterfaces().Any(x =>
                              x.IsGenericType &&
                              x.GetGenericTypeDefinition() == typeof(ISavable<>));

            projectMenu.Visibility = Visibility.Collapsed;
            projectMenu.Items.Clear();

            if (isSavable) {
                projectMenu.Visibility = Visibility.Visible;

                projectMenu.Items.Add(GetSaveProjectMenuItem());
                projectMenu.Items.Add(GetLoadProjectMenuItem());
                projectMenu.Items.Add(GetNewProjectMenuItem());
            }

            if (DataContext is IHaveExtraProjectMenuItems havingExtraProjectMenuItems) {
                projectMenu.Visibility = Visibility.Visible;

                foreach (var menuItem in havingExtraProjectMenuItems.GetMenuItems()) {
                    projectMenu.Items.Add(menuItem);
                }
            }
        }

        private MenuItem GetSaveProjectMenuItem() {
            var menu = new MenuItem {
                Header = "_Save project", Icon = new PackIcon {Kind = PackIconKind.ContentSave},
                ToolTip = "Save tool settings to file."
            };
            menu.Click += SaveProject;

            return menu;
        }

        private MenuItem GetLoadProjectMenuItem() {
            var menu = new MenuItem {
                Header = "_Load project", Icon = new PackIcon {Kind = PackIconKind.Folder},
                ToolTip = "Load tool settings from file."
            };
            menu.Click += LoadProject;

            return menu;
        }

        private MenuItem GetNewProjectMenuItem() {
            var menu = new MenuItem {
                Header = "_New project", Icon = new PackIcon { Kind = PackIconKind.Rocket },
                ToolTip = "Load the default tool settings."
            };
            menu.Click += NewProject;

            return menu;
        }

        private void LoadProject(object sender, RoutedEventArgs e) {
            if( !ProjectManager.IsSavable(DataContext) )
                return;
            dynamic data = DataContext;
            ProjectManager.LoadProject(data, true);
        }

        private void SaveProject(object sender, RoutedEventArgs e) {
            if( !ProjectManager.IsSavable(DataContext) )
                return;
            dynamic data = DataContext;
            ProjectManager.SaveProject(data, true);
        }

        private void NewProject(object sender, RoutedEventArgs e) {
            if (!ProjectManager.IsSavable(DataContext))
                return;
            dynamic data = DataContext;
            ProjectManager.NewProject(data, true);
        }

        //Open backup folder in file explorer
        private void OpenBackups(object sender, RoutedEventArgs e) {
            try {
                System.Diagnostics.Process.Start(SettingsManager.GetBackupsPath());
            }
            catch( Exception ex ) {
                ex.Show();
            }
        }

        private void OpenConfig(object sender, RoutedEventArgs e) {
            try {
                System.Diagnostics.Process.Start(AppDataPath);
            } catch (Exception ex) {
                ex.Show();
            }
        }

        private void OpenWebsite(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://mappingtools.seira.moe/");
        }

        private void CoolSave(object sender, RoutedEventArgs e) {
            try {
                EditorReaderStuff.BetterSave();
            } 
            catch (Exception ex) {
                ex.Show();
            }
        }

        //Open project in browser
        private void OpenGitHub(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/OliBomby/Mapping_Tools");
        }

        //Open info screen
        private void OpenInfo(object sender, RoutedEventArgs e) {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            MessageBox.Show($"Mapping Tools {version}\n\nMade by:\nOliBomby\nPotoofu", "Info");
        }

        //Change top right icons on changed window state and set state variable
        private void Window_StateChanged(object sender, EventArgs e) {
            if (!(this.FindName("toggle_button") is Button bt)) return;

            switch( this.WindowState ) {
                case WindowState.Maximized:
                    bt.Content = new PackIcon { Kind = PackIconKind.WindowRestore };
                    IsMaximized = true;
                    window_border.BorderThickness = new Thickness(0);
                    break;

                case WindowState.Minimized:
                    break;

                case WindowState.Normal:
                    window_border.BorderThickness = new Thickness(1);
                    IsMaximized = false;
                    bt.Content = new PackIcon { Kind = PackIconKind.WindowMaximize };
                    break;
            }
        }

        //Clickevent for top right maximize/minimize button
        private void ToggleWin(object sender, RoutedEventArgs e) {
            if (!(this.FindName("toggle_button") is Button bt)) return;

            if( IsMaximized ) {
                this.WindowState = WindowState.Normal;
                Width = WidthWin;
                Height = HeightWin;
                IsMaximized = false;
                window_border.BorderThickness = new Thickness(1);
                bt.Content = new PackIcon { Kind = PackIconKind.WindowMaximize };
            }
            else {
                WidthWin = ActualWidth;
                HeightWin = ActualHeight;
                this.Left = SystemParameters.WorkArea.Left;
                this.Top = SystemParameters.WorkArea.Top;
                this.Height = SystemParameters.WorkArea.Height;
                this.Width = SystemParameters.WorkArea.Width;
                window_border.BorderThickness = new Thickness(0);
                //this.WindowState = WindowState.Maximized;
                IsMaximized = true;
                bt.Content = new PackIcon { Kind = PackIconKind.WindowRestore };
            }
        }

        //Minimize window on click
        private void MinimizeWin(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        //Close window
        private void CloseWin(object sender, RoutedEventArgs e) {
            Views.AutoSaveSettings();
            if (DataContext is MappingTool mt){ mt.Dispose(); }
            SettingsManager.UpdateSettings();
            SettingsManager.WriteToJson();
            this.Close();
        }

        //Enable drag control of window and set icons when docked
        private void DragWin(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) return;
            if (!(this.FindName("toggle_button") is Button bt)) return;

            if( WindowState == WindowState.Maximized ) {
                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                if( point.X <= RestoreBounds.Width / 2 )
                    Left = 0;
                else if( point.X >= RestoreBounds.Width )
                    Left = point.X - ( RestoreBounds.Width - ( this.ActualWidth - point.X ) );
                else
                    Left = point.X - ( RestoreBounds.Width / 2 );

                Top = point.Y - ( ( (FrameworkElement) sender ).ActualHeight / 2 );
                WindowState = WindowState.Normal;
                bt.Content = new PackIcon { Kind = PackIconKind.WindowMaximize };
            }
            if( e.LeftButton == MouseButtonState.Pressed )
                this.DragMove();
            //bt.Content = new PackIcon { Kind = PackIconKind.WindowRestore };
        }
    }
}
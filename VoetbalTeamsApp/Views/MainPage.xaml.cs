using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VoetbalTeamsApp.Models;
using System.Threading.Tasks;
using Windows.UI.Popups;
using VoetbalTeamsApp.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using VoetbalTeamsApp.Views;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VoetbalTeamsApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /* Startup */
        AppWindow leagueTableWindow;
        AppWindow MatchWindow;
        public static Dictionary<UIContext, AppWindow> AppWindows { get; set; }
            = new Dictionary<UIContext, AppWindow>();

        public PlayerListPageViewModel ViewModel = new PlayerListPageViewModel();
        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;

            CreateLeagueTableWindow(null,null);
            CreateMatchWindow(null,null);
        }

        private void Current_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            Application.Current.Exit();
        }
        private async void CreateLeagueTableWindow(object sender, RoutedEventArgs e)
        {
            if (leagueTableWindow == null)
            {
                //this.Frame.Navigate(typeof(LeagueTablePageViewModel));
                // Create new window
                leagueTableWindow = await AppWindow.TryCreateAsync();

                // Create Frame and navigate
                Frame appWindowContentFrame = new Frame();
                appWindowContentFrame.Navigate(typeof(LeagueTablePage));

                ElementCompositionPreview.SetAppWindowContent(leagueTableWindow, appWindowContentFrame);

                //Get a reference to the page instance and assign the
                // newly created Appwindow to the myappwindow property
                LeagueTablePage page = (LeagueTablePage)appWindowContentFrame.Content;
                page.MyAppWindow = leagueTableWindow;

                AppWindows.Add(appWindowContentFrame.UIContext, leagueTableWindow);
                leagueTableWindow.Title = "App Window " + AppWindows.Count.ToString();


                //when window is close release recources
                leagueTableWindow.Closed += delegate
                {
                    MainPage.AppWindows.Remove(appWindowContentFrame.UIContext);
                    appWindowContentFrame.Content = null;
                    leagueTableWindow = null;
                };
                leagueTableWindow.RequestMoveAdjacentToCurrentView();
            }

            // Show the window
            await leagueTableWindow.TryShowAsync();
            
        }

        private async void CreateMatchWindow(object sender, RoutedEventArgs e)
        {
            if (MatchWindow == null)
            {
                //this.Frame.Navigate(typeof(LeagueTablePageViewModel));
                // Create new window
                MatchWindow = await AppWindow.TryCreateAsync();

                // Create Frame and navigate
                Frame appWindowContentFrame = new Frame();
                appWindowContentFrame.Navigate(typeof(MatchPage));

                ElementCompositionPreview.SetAppWindowContent(MatchWindow, appWindowContentFrame);

                //Get a reference to the page instance and assign the
                // newly created Appwindow to the myappwindow property
                MatchPage page = (MatchPage)appWindowContentFrame.Content;
                page.MyAppWindow = MatchWindow;

                AppWindows.Add(appWindowContentFrame.UIContext, MatchWindow);
                MatchWindow.Title = "App Window " + AppWindows.Count.ToString();


                //when window is close release recources
                MatchWindow.Closed += delegate
                {
                    MainPage.AppWindows.Remove(appWindowContentFrame.UIContext);
                    appWindowContentFrame.Content = null;
                    MatchWindow = null;
                };
                MatchWindow.RequestMoveAdjacentToCurrentView();
            }

            // Show the window
            await MatchWindow.TryShowAsync();

        }

        private async void CloseChildWindows(object sender, RoutedEventArgs e)
        {
            while(AppWindows.Count > 0)
            {
                await AppWindows.Values.First().CloseAsync();
            }
            Window.Current.Activate();
            await ApplicationView.GetForCurrentView().TryConsolidateAsync();

        }
    }
}
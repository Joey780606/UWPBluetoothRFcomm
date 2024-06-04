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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/*
 * Author: Joey yang
 * Note:
 * 1. 
 *   a. DisplayHelpers.cs, Styles.xaml needs to add at first.
 *   b. MainPage.xaml some click function is empty.
 */
namespace BluetoothRfcomm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void StartWatcherButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StopWatcherButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PairButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UnpairButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}

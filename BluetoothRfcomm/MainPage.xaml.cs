using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
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
 * 2.
 *   a. Mainly I join DeviceWatcherHelper.cs
 *   b. Adjust some code.
 */
namespace BluetoothRfcomm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        private DeviceWatcherHelper deviceWatcherHelper;

        private ObservableCollection<DeviceInformationDisplay> resultCollection = new ObservableCollection<DeviceInformationDisplay>();

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            deviceWatcherHelper = new DeviceWatcherHelper(resultCollection, Dispatcher);
            deviceWatcherHelper.DeviceChanged += OnDeviceListChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            resultsListView.ItemsSource = resultCollection;

            selectorComboBox.ItemsSource = DeviceSelectorChoices.PairingSelectors;
            selectorComboBox.SelectedIndex = 0;
            Debug.WriteLine("Joey: OnNavigatedTo in");
        }

        private void OnDeviceListChanged(DeviceWatcher sender, string id)
        {
            // If the item being updated is currently "selected", then update the pairing buttons
            DeviceInformationDisplay selectedDeviceInfoDisp = (DeviceInformationDisplay)resultsListView.SelectedItem;
            if ((selectedDeviceInfoDisp != null) && (selectedDeviceInfoDisp.Id == id))
            {
                UpdatePairingButtons();
            }
        }

        private void UpdatePairingButtons()
        {
            DeviceInformationDisplay deviceInfoDisp = (DeviceInformationDisplay)resultsListView.SelectedItem;

            if (null != deviceInfoDisp &&
                deviceInfoDisp.DeviceInformation.Pairing.CanPair &&
                !deviceInfoDisp.DeviceInformation.Pairing.IsPaired)
            {
                pairButton.IsEnabled = true;
            }
            else
            {
                pairButton.IsEnabled = false;
            }

            if (null != deviceInfoDisp &&
                deviceInfoDisp.DeviceInformation.Pairing.IsPaired)
            {
                unpairButton.IsEnabled = true;
            }
            else
            {
                unpairButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Display a message to the user.
        /// This method may be called from any thread.
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

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

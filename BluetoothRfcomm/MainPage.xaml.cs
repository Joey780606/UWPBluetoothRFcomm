﻿using System;
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
 *   
 * 了解專案內容的知識
 * 1. ResultsListViewTemplate 是顯示Scan到的bluetooth裝置UI配置
 */
namespace BluetoothRfcomm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        private MainPage rootPage;

        private DeviceWatcherHelper deviceWatcherHelper;

        private ObservableCollection<DeviceInformationDisplay> resultCollection = new ObservableCollection<DeviceInformationDisplay>();

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            rootPage = MainPage.Current;
            deviceWatcherHelper = new DeviceWatcherHelper(resultCollection, Dispatcher); //Joey: Dispatcher傳過去的目的,是為了要做UI的變化
            deviceWatcherHelper.DeviceChanged += OnDeviceListChanged;  //DeviceWatcherHelper有幾個函式都會引發要不要pair的UI按鍵變化
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            resultsListView.ItemsSource = resultCollection; //resultsListView : UI, ListView

            selectorComboBox.ItemsSource = DeviceSelectorChoices.PairingSelectors;  //Joey: 只要存這個就好,這對我不太需要,因為只要Bluetooth類型
            selectorComboBox.SelectedIndex = 0;
            Debug.WriteLine("Joey: OnNavigatedTo in");  // It is work.
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)  // Joey: 離開此頁
        {
            deviceWatcherHelper.Reset();
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

        private void UpdatePairingButtons() //Joey 看點取的 DeviceInformationDisplay 資料,有無配對來改變按鍵畫面
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
            if (Dispatcher.HasThreadAccess) //Joey: 我猜是在前景
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type)); //Joey: 我猜是在後景
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)   //改變status bar的顏色
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
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock); //Joey: 這個不懂
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
            StartWatcher(); //UI
        }

        private void StopWatcherButton_Click(object sender, RoutedEventArgs e)
        {
            StopWatcher();  //UI
        }

        private async void PairButton_Click(object sender, RoutedEventArgs e)
        {
            // Gray out the pair button and results view while pairing is in progress.
            resultsListView.IsEnabled = false;
            pairButton.IsEnabled = false;
            rootPage.NotifyUser("Pairing started. Please wait...", NotifyType.StatusMessage);

            DeviceInformationDisplay deviceInfoDisp = resultsListView.SelectedItem as DeviceInformationDisplay;

            DevicePairingResult dpr = await deviceInfoDisp.DeviceInformation.Pairing.PairAsync();   //Joey: 真正進行配對的工作

            rootPage.NotifyUser(
                "Pairing result = " + dpr.Status.ToString(),
                dpr.Status == DevicePairingResultStatus.Paired ? NotifyType.StatusMessage : NotifyType.ErrorMessage);

            UpdatePairingButtons();
            resultsListView.IsEnabled = true;
        }

        private async void UnpairButton_Click(object sender, RoutedEventArgs e)
        {
            // Gray out the unpair button and results view while unpairing is in progress.
            resultsListView.IsEnabled = false;
            unpairButton.IsEnabled = false;
            rootPage.NotifyUser("Unpairing started. Please wait...", NotifyType.StatusMessage);

            DeviceInformationDisplay deviceInfoDisp = resultsListView.SelectedItem as DeviceInformationDisplay;

            DeviceUnpairingResult dupr = await deviceInfoDisp.DeviceInformation.Pairing.UnpairAsync();  // Joey: 真正進行Unpair的動作

            rootPage.NotifyUser(
                "Unpairing result = " + dupr.Status.ToString(),
                dupr.Status == DeviceUnpairingResultStatus.Unpaired ? NotifyType.StatusMessage : NotifyType.ErrorMessage);

            UpdatePairingButtons();
            resultsListView.IsEnabled = true;
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Joey: 結果列的其中一筆被點選時要做的事
            UpdatePairingButtons();
        }

        private void StartWatcher()
        {
            startWatcherButton.IsEnabled = false;
            resultCollection.Clear();

            // Get the device selector chosen by the UI then add additional constraints for devices that
            // can be paired or are already paired.
            DeviceSelectorInfo deviceSelectorInfo = (DeviceSelectorInfo)selectorComboBox.SelectedItem;  //Joey: 這個要指定
           // DeviceSelectorInfo deviceSelectorInfo = (DeviceSelectorInfo)DeviceSelectorChoices.Bluetooth;
            
            string selector = "(" + deviceSelectorInfo.Selector + ")" + " AND (System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True)";

            DeviceWatcher deviceWatcher;
            if (deviceSelectorInfo.Kind == DeviceInformationKind.Unknown)
            {
                // Kind will be determined by the selector
                deviceWatcher = DeviceInformation.CreateWatcher(
                    selector,
                    null // don't request additional properties for this sample
                    );
            }
            else
            {
                // Kind is specified in the selector info
                deviceWatcher = DeviceInformation.CreateWatcher(
                    selector,
                    null, // don't request additional properties for this sample
                    deviceSelectorInfo.Kind);
            }

            rootPage.NotifyUser("Starting Watcher...", NotifyType.StatusMessage);
            deviceWatcherHelper.StartWatcher(deviceWatcher);
            stopWatcherButton.IsEnabled = true;
        }

        private void StopWatcher()
        {
            stopWatcherButton.IsEnabled = false;

            deviceWatcherHelper.StopWatcher();

            startWatcherButton.IsEnabled = true;
        }
    }
}

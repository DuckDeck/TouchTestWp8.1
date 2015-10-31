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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TouchTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.SwitchShowTrace.IsOn = Config.isNeedShowTrace.Value;
            this.SwitchKeepTrace.IsOn = Config.isReserveTrace.Value;
            this.SwitchShowCoord.IsOn = Config.isShowCoord.Value;
            if (!Config.isNeedShowTrace.Value)
                this.SwitchKeepTrace.IsEnabled = false;
            this.SliderTraceThickness.Value = Config.TraceThickness.Value;
            this.RunPoiunt.Text = Config.supportMaxTouchs.Value.ToString();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Config.TraceThickness.Value = (float)this.SliderTraceThickness.Value;
            Config.isNeedShowTrace.Value = this.SwitchShowTrace.IsOn;
            Config.isReserveTrace.Value = this.SwitchKeepTrace.IsOn;
            Config.isShowCoord.Value = this.SwitchShowCoord.IsOn;
        }

        private void SwitchShowTrace_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.SwitchShowTrace.IsOn)
                this.SwitchKeepTrace.IsEnabled = true;
            else
            {
                this.SwitchKeepTrace.IsEnabled = false;
                this.SwitchKeepTrace.IsOn = false;
            }
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            Frame.GoBack();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wpf_simple_FFMPEG {
    /// <summary>
    /// Interaction logic for DimensionConfigScreen.xaml
    /// </summary>
    public partial class DimensionConfigScreen : Window {
        private readonly SimpleConfig simpleConfig;

        public DimensionConfigScreen(SimpleConfig simpleConfig) {
            if (simpleConfig is null) {
                throw new ArgumentNullException(nameof(simpleConfig));
            }

            InitializeComponent();
            this.simpleConfig = simpleConfig;

            simpleConfig.ScaleOptions.ForEach(item => lbExistingConfig.Items.Add(item));
        }

        private void ButtonRemoveSelected(object sender, RoutedEventArgs e) {
            simpleConfig.ScaleOptions.Remove(lbExistingConfig.SelectedItem as ScaleOptions);
            lbExistingConfig.Items.Remove(lbExistingConfig.SelectedItem);
        }

        private void ButtonAddNewOption(object sender, RoutedEventArgs e) {
            if(txtWidth.Text.Length == 0 || txtHeight.Text.Length == 0) {
                return;
            }

            var scaleObj = new ScaleOptions() { width = txtWidth.Text, height = txtHeight.Text };
            simpleConfig.ScaleOptions.Add(scaleObj);
            lbExistingConfig.Items.Add(scaleObj);

            txtWidth.Clear();
            txtHeight.Clear();
        }

        
        private void InputNumber_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !App.IsTextAllowed(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!App.IsTextAllowed(text)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }
    }
}

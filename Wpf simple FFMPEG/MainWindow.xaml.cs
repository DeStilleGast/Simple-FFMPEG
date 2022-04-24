using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wpf_simple_FFMPEG.external;

namespace Wpf_simple_FFMPEG {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {


        private readonly SimpleConfig applicationConfig;
        private List<Rect> currentBlurBlocks { get; set; } = new List<Rect>();
        
        public MainWindow() {
            InitializeComponent();
            
            tRangeTrimmer.Maximum = 30;
            tRangeTrimmer.UpperValue = tRangeTrimmer.Maximum;

            string json = File.ReadAllText("config.json");

            try {
                applicationConfig = JsonConvert.DeserializeObject<SimpleConfig>(json);

                applicationConfig.EncodingOptions.ForEach(item => cbPreset.Items.Add(item));
                applicationConfig.ScaleOptions.ForEach(item => cbScaleOptions.Items.Add(item));

                cbPreset.SelectedIndex = applicationConfig.appConfig.lastSelectedEncoding;
                cbScaleOptions.SelectedIndex = 0;
            } catch (Exception ex) {
                MessageBox.Show("config.json file is invalid !\nPlease delete or 'repair' config.json", "Invalid configuration", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void loadFromSettings() {
            cbPreset.Items.Clear();
            cbScaleOptions.Items.Clear();

            applicationConfig.EncodingOptions.ForEach(item => cbPreset.Items.Add(item));
            applicationConfig.ScaleOptions.ForEach(item => cbScaleOptions.Items.Add(item));

            cbPreset.SelectedIndex = applicationConfig.appConfig.lastSelectedEncoding;
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            applicationConfig.appConfig.lastSelectedEncoding = cbPreset.SelectedIndex;

            string json = JsonConvert.SerializeObject(applicationConfig, Formatting.Indented);
            File.WriteAllText("config.json", json);
        }

        private void btnGo_Click(object sender, RoutedEventArgs e) {
            startEncoding();
        }

        private void btnInputFileSelector_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select media file";
            if (openFileDialog.ShowDialog() == true) {
                txtInputFile.Text = openFileDialog.FileName;
                setupSuggestOutput();
            }
        }

        private void btnOutputFileSelector_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select media file";
            if (openFileDialog.ShowDialog() == true) {
                txtOutputFile.Text = openFileDialog.FileName;
            }
        }

        private void btnOutputFolderSelector_Click(object sender, RoutedEventArgs e) {
            MessageBoxResult messageBoxResult = MessageBox.Show("Would you like to select a default directory\n\n[Yes] Select directory\n[No] Remove default directoty\n[Cancel] Do nothing", "Select default output directory", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if(messageBoxResult == MessageBoxResult.No) {
                applicationConfig.appConfig.defaultOutputPath = null;
                MessageBox.Show($"Default output path has been reset, output files will be placed in the same directory as the original file !", "Default folder output", MessageBoxButton.OK, MessageBoxImage.Information);
            } else if(messageBoxResult == MessageBoxResult.Yes) {
                FolderPicker folderBrowserDialog = new FolderPicker();
                folderBrowserDialog.Title = "Select a default directory to save your video's in";
                if (folderBrowserDialog.ShowDialog() == true) {
                    applicationConfig.appConfig.defaultOutputPath = folderBrowserDialog.ResultPath;

                    MessageBox.Show($"Default output path has been set to: {applicationConfig.appConfig.defaultOutputPath}", "Default folder output", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
        }


        public async void setupSuggestOutput() {
            UpdateStatus("Preparing fields");

            FileInfo fileInfo = new FileInfo(txtInputFile.Text);
            txtOutputFile.Text = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf(".")) + ".compressed" + fileInfo.Extension;

            if (applicationConfig.appConfig.defaultOutputPath != null) {
                if (Directory.Exists(applicationConfig.appConfig.defaultOutputPath)) {
                    txtOutputFile.Text = Path.Combine(applicationConfig.appConfig.defaultOutputPath, Path.GetFileNameWithoutExtension(txtInputFile.Text) + ".compressed" + fileInfo.Extension);
                }
            }

            // -ss 00:00:03 -frames:v 1


            string ffprobePath = App.findTool("ffprobe.exe");
            if (ffprobePath != null) {

                Process ffprobe = Process.Start(new ProcessStartInfo(ffprobePath, $"-v error -print_format json -show_format -show_entries stream=r_frame_rate,duration,Width,Height -select_streams v \"{fileInfo.FullName}\"") {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                UpdateStatus("Waiting for ffprobe.exe");
                await ffprobe.WaitForExitAsync();

                string json = await ffprobe.StandardOutput.ReadToEndAsync();
                var jsonOutput = JsonConvert.DeserializeObject<FFProbeOutputs>(json);
                if (jsonOutput.Streams.Length > 0) {
                    var targetData = jsonOutput.Streams[0];
                    //string output = ffprobe.StandardOutput.ReadToEnd().Replace(".", ",");

                    if (double.TryParse(targetData.Duration.Replace(".", ","), out double result)) {
                        cbVideoTrimming.IsEnabled = true;
                        cbVideoTrimming.Content = "Video trimming";

                        tRangeTrimmer.Maximum = result;
                        tRangeTrimmer.UpperValue = Math.Min(tRangeTrimmer.UpperValue, tRangeTrimmer.Maximum);


                        lblVideoInfoDuration.Content = $"Video length: {result.ToTime()}";
                    } else {
                        cbVideoTrimming.IsEnabled = false;
                        cbVideoTrimming.IsChecked = false;

                        UpdateStatus("Could not extract video data, unsupported ?");
                        lblVideoInfoDuration.Content = $"Video length: ?";
                        return;
                    }

                    string[] splitFpsData = targetData.RFrameRate.Split('/');
                    string fpsPartA = splitFpsData[0].Replace(".", ",");
                    string fpsPartB = splitFpsData[1].Replace(".", ",");

                    if(double.TryParse(fpsPartA, out double fpsA) && double.TryParse(fpsPartB, out double fpsB)) {
                        double FPS = fpsA / fpsB;
                        lblVideoInfoFPS.Content = $"FPS: {FPS}";
                        txtNewFps.Text = FPS.ToString();
                    } else {
                        lblVideoInfoFPS.Content = $"FPS: ?";
                    }

                    lblVideoInfoDimension.Content = $"Video dimension: {targetData.Width}x{targetData.Height}";

                    UpdateStatus("Video data loaded");
                } else {
                    UpdateStatus("Could not extract video data, unsupported ?");
                }

            } else {
                cbVideoTrimming.IsEnabled = false;
                cbVideoTrimming.IsChecked = false;
                cbVideoTrimming.Content = "Missing ffprobe.exe";

                UpdateStatus("ffprobe.exe is missing, trimming disabled");
            }
        }

        private void txtInputFile_Drop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0) {
                txtInputFile.Text = files[0];
                setupSuggestOutput();
            }
        }

        private void txtInputFile_PreviewDragOver(object sender, DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }


        private void txtOutputFile_Drop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0) {
                txtOutputFile.Text = files[0];
            }
        }

        private void txtOutputFile_PreviewDragOver(object sender, DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        // trim command: -ss <start> -to <end>
        // get video length: ffprobe.exe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1


        public async void startEncoding() {
            string extraArguments = "";
            string filterComplex = "";

            string outputFile = txtOutputFile.Text;

            if (cbVideoTrimming.IsChecked == true) {
                string startTime = tRangeTrimmer.LowerValue.ToTime();
                string endTime = tRangeTrimmer.UpperValue.ToTime();

                extraArguments += $" -ss {startTime} -to {endTime} ";
            }


            if (cbBlurring.IsChecked == true && currentBlurBlocks.Count > 0) {
                string filterBuilder = "";

                // Create blur rectangles
                for (int i = 0; i < currentBlurBlocks.Count; i++) {
                    Rect rect = currentBlurBlocks[i];
                    string blurArg = $"[0:v]crop={(int)rect.Width}:{(int)rect.Height}:{(int)rect.Left}:{(int)rect.Top},avgblur=10[b{i}];";

                    filterBuilder += blurArg;
                }

                // Place blur rectangles on vid
                for (int i = 0; i < currentBlurBlocks.Count; i++) {
                    Rect rect = currentBlurBlocks[i];
                    string blurArg;
                    if (i == 0) {
                        blurArg = $"[0:v][b0]overlay={(int)rect.Left}:{(int)rect.Top}";
                        if (currentBlurBlocks.Count > 1) {
                            blurArg += "[ovr0];";
                        }

                    } else {
                        blurArg = $"[ovr{i - 1}][b{i}]overlay={(int)rect.Left}:{(int)rect.Top}";

                        if (i != currentBlurBlocks.Count -1) {
                            blurArg += $"[ovr{i}];";
                        }
                    }
                    filterBuilder += blurArg;
                }

                //extraArguments += $" -filter_complex \"{filterBuilder}\" ";
                filterComplex += filterBuilder;
            }

            if (cbVideoScaling.IsChecked == true) {
                if(filterComplex.Length > 0) {
                    filterComplex += "[scale];[scale]";
                }
                filterComplex += $"scale={(cbScaleOptions.SelectedItem as ScaleOptions).width}:{(cbScaleOptions.SelectedItem as ScaleOptions).height}";
            }

            // fps
            if (cbChangeFps.IsChecked == true) {
                if (filterComplex.Length > 0) {
                    filterComplex += "[fps];[fps]";
                }
                filterComplex += "fps=fps=" + txtNewFps.Text;
            }

            //extraArguments += " -filter_complex \"\" ";

            if (filterComplex.Length > 0) {
                extraArguments += $" -filter_complex \"{filterComplex}\" ";
            }

            if (cbPreset.SelectedItem == null) return;
            string selectedPreset = (cbPreset.SelectedItem as EncodingOptions).arguments;

            selectedPreset = selectedPreset
                .Replace("%extra%", extraArguments)
                .Replace("%in%", txtInputFile.Text)
                .Replace("%out%", outputFile);

            UpdateStatus("Running ffmpeg.exe...");
            using (var proc = Process.Start(new ProcessStartInfo(App.findTool("ffmpeg.exe"), selectedPreset) {
                UseShellExecute = true
            })) {
                await proc.WaitForExitAsync();
                UpdateStatus("ffmpeg.exe is done");

                PInvoke.FlashWindow(new WindowInteropHelper(this).EnsureHandle(), false);

                if (File.Exists(outputFile) && MessageBox.Show("Would you like open explorer with the video file ?", "ffmpeg.exe is finished", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    Process.Start("explorer.exe", $"/select,\"{outputFile}\"");
                }
            };
        }

        private void UpdateStatus(string status) {
            lblStatus.Content = $"Status: {status?.Trim() ?? "doing nothing"}";
        }

        private void OpenBlurScreenButton(object sender, RoutedEventArgs e) {
            UpdateStatus("Grabbing screenshot...");

            int secondInGame = 3;
            if(cbVideoTrimming.IsChecked == true) {
                secondInGame += (int)tRangeTrimmer.LowerValue;
            }


            if (GetVideoThumbnail(txtInputFile.Text, GetThumnailPath(), secondInGame)) {

                var imageSource = new BitmapImage();
                Stream fileStream = File.OpenRead(GetThumnailPath());
                imageSource.BeginInit();
                imageSource.StreamSource = fileStream;
                imageSource.EndInit();
                

                BlurSelector blurSelector = new BlurSelector(imageSource, currentBlurBlocks);

                blurSelector.ShowDialog();
                fileStream.Dispose();

                currentBlurBlocks = blurSelector.GetRects();
                lblBlurCount.Content = currentBlurBlocks.Count;

                blurSelector = null;
            } else {
                UpdateStatus("Could not generate screenshot !");
            }
        }

        public bool GetVideoThumbnail(string video, string saveThumbnailTo, int seconds) {
            string parameters = string.Format("-ss {0} -i \"{1}\" -f image2 -vframes 1 -y \"{2}\"", seconds, video, saveThumbnailTo);

            ProcessStartInfo processInfo = new ProcessStartInfo() {
                FileName = App.findTool("ffmpeg.exe"),
                Arguments = parameters,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            if (IsFileLocked(saveThumbnailTo)) {
                return false;
            }

            File.Delete(saveThumbnailTo);

            using (Process process = new Process()) {
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }

            return File.Exists(saveThumbnailTo);
        }

        private string GetThumnailPath() {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "thumbnail.png");
        }

        private bool IsFileLocked(string file) {
            if (!File.Exists(file)) return false;
            try {
                using (File.Open(file, FileMode.Open)) return false;
            } catch (IOException) {
                // file locked
                return true;
            }
        }

        private void aboutBoxWithCreditsButton(object sender, RoutedEventArgs e) {
            new AboutBox().ShowDialog();
        }

        private void btnOutputDirectorySelector_Click(object sender, RoutedEventArgs e) {

        }

        private void ButtonScaleSettings(object sender, RoutedEventArgs e) {
            new DimensionConfigScreen(applicationConfig).ShowDialog();

            loadFromSettings();
        }


        private void AddMoreFrameRateButton(object sender, RoutedEventArgs e) {
            if(double.TryParse(txtNewFps.Text, out double fps)) {
                txtNewFps.Text = (fps += 1).ToString();
            }
        }

        private void RemoveSomeFrameRateButton(object sender, RoutedEventArgs e) {
            if (double.TryParse(txtNewFps.Text, out double fps)) {
                if (fps > 0) {
                    txtNewFps.Text = (fps -= 1).ToString();
                }
            }
        }

        private void txtNewFps_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            e.Handled = !App.IsTextAllowed(e.Text);
        }
    }
}

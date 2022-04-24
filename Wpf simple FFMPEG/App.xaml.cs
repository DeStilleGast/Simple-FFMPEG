using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace Wpf_simple_FFMPEG {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        [System.STAThreadAttribute()]
        public static void Main() {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, a) => {
                string resourceName = new AssemblyName(a.Name).Name + ".dll";
                string resource = Array.Find(typeof(App).Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)) {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            if (!new FileInfo("config.json").Exists) {
                MessageBox.Show("Config is missing, default config has been generated", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                //using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("defaultConfig")) {
                //    using (var file = new FileStream("config.json", FileMode.Create, FileAccess.Write)) {
                //        resource.CopyTo(file);
                //    }
                //}
                File.WriteAllText("config.json", Wpf_simple_FFMPEG.Properties.Resources.defaultConfig);
            }

            if (!File.Exists(findTool("ffmpeg.exe"))) {
                MessageBox.Show("ffmpeg is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static string? findTool(string toolName) {
            string currentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), toolName);
            string binDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bin", toolName);
            string toolsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tools", toolName);

            if (File.Exists(currentDir)) {
                return currentDir;
            }

            if (File.Exists(binDir)) {
                return binDir;
            }

            if (File.Exists(toolsDir)) {
                return toolsDir;
            }

            return null;
        }

        private static readonly Regex _regex = new Regex("[^0-9,-]+"); //regex that matches disallowed text
        public static bool IsTextAllowed(string text) {
            return !_regex.IsMatch(text);
        }
    }
}

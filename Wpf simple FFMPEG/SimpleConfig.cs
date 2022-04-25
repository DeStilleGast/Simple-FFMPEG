using System;
using System.Collections.Generic;
using System.Windows;

namespace Wpf_simple_FFMPEG {
    public class SimpleConfig {

        public AppConfig appConfig = new AppConfig();
        public List<EncodingOptions> EncodingOptions = new List<EncodingOptions>();
        public List<ScaleOptions> ScaleOptions = new List<ScaleOptions>();
        public List<BlurPreset> blurPresets = new List<BlurPreset>();

    }

    public class AppConfig {
        public int lastSelectedEncoding = 0;
        public string defaultOutputPath = null;

    }

    public class EncodingOptions {
        public string name;
        public string arguments;

        public override string ToString() {
            return name;
        }
    }

    public class ScaleOptions {
        public string width, height;

        public override string ToString() {
            return $"{width} x {height}";
        }
    }

    public class BlurPreset {
        public string PresetName;
        public List<Rect> rects;
    }
}

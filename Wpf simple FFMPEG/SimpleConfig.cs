using System;
using System.Collections.Generic;

namespace Wpf_simple_FFMPEG {
    public class SimpleConfig {

        public AppConfig appConfig = new AppConfig();
        public List<EncodingOptions> EncodingOptions = new List<EncodingOptions>();
        public List<ScaleOptions> ScaleOptions = new List<ScaleOptions>();


        //public override string ToString() {
        //    return name;
        //}
    }

    public class AppConfig {
        public int lastSelectedEncoding = 0;
        public string defaultOutputPath = null;

    }

    public class EncodingOptions {
        public String name;
        public String arguments;

        public override string ToString() {
            return name;
        }
    }

    public class ScaleOptions {
        public String width, height;

        public override string ToString() {
            return $"{width} x {height}";
        }
    }
}

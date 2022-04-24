using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_simple_FFMPEG {
    class FFProbeOutputs {
        [JsonProperty("streams")]
        public FFProbeStream[] Streams { get; set; }
    }

    public partial class FFProbeStream {
        [JsonProperty("r_frame_rate")]
        public string RFrameRate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
        
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}

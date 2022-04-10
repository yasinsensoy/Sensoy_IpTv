using System.Globalization;

namespace Mpv.NET.Player
{
    public class OsdOverlay
    {
        public int Id { get; set; }

        public TextAlign Align { get; set; }

        public double FontSize { get; set; }

        public string Data { get; set; }

        public int ZOrder { get; set; }

        public string[] Show => new string[] { "osd-overlay", $"{Id}", "ass-events", $"{{\\an{(int)Align}{(FontSize == 0 ? "" : $"\\fs{FontSize.ToString(CultureInfo.InvariantCulture)}")}}}{Data}", "0", "0", $"{ZOrder}", "no", "no" };

        public string[] Hide => new string[] { "osd-overlay", $"{Id}", "none", "" };

        public OsdOverlay(int id, string data, TextAlign align = TextAlign.TopLeft, double fontsize = 0, int zorder = 0)
        {
            Id = id;
            Align = align;
            FontSize = fontsize;
            Data = data;
            ZOrder = zorder;
        }
    }
}
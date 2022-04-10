using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils.Svg;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraGrid.Views.Tile;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sensoy_IpTv
{
    public static class Tema
    {
        public static bool Mesgul = false;
        public static readonly Color MesgulRenk = Color.FromArgb(202, 101, 0);
        public static Dictionary<string, SvgColor> TemaRenkler;

        public static string AnaRenk => TemaRenkler.TryGetValue("Accent Paint", out SvgColor color) ? ColorTranslator.ToHtml(color.Value) : "";

        public static string FontRenk => TemaRenkler.TryGetValue("Brush", out SvgColor color) ? ColorTranslator.ToHtml(color.Value) : "";

        public static void Kaydet(string tema)
        {
            if (Kayit.ayar.PaletteName == tema)
                return;
            Kayit.ayar.PaletteName = tema == "" ? Skin.DefaultSkinPaletteName : tema;
            Kayit.Kaydet();
        }

        public static void DurumDegis(bool mesgul)
        {
            Mesgul = mesgul;
            LookAndFeelHelper.ForceDefaultLookAndFeelChanged();
        }

        public static void TileViewTema(ref TileView tileview, bool focused)
        {
            if (TemaRenkler != null)
            {
                if (TemaRenkler.TryGetValue("Accent Paint Dark", out SvgColor item)
                    && TemaRenkler.TryGetValue("Brush Minor", out SvgColor item1)
                    && TemaRenkler.TryGetValue("Paint High", out SvgColor item2)
                    && TemaRenkler.TryGetValue("Brush", out SvgColor item3))
                {
                    tileview.Appearance.ItemFocused.BackColor = item2.Value;
                    tileview.Appearance.ItemNormal.BackColor = item2.Value;
                    tileview.Appearance.ItemPressed.BackColor = item.Value;
                    tileview.Appearance.ItemPressed.ForeColor = Color.White;
                    tileview.Appearance.ItemSelected.BackColor = focused ? item.Value : item1.Value;
                    tileview.Appearance.ItemSelected.ForeColor = focused ? Color.White : item3.Value;
                }
            }
        }

        public static void TemaDuzenle(ref SkinPaletteDropDownButtonItem control)
        {
            GalleryDropDown gdd = (GalleryDropDown)control.DropDownControl;
            var gallery = gdd.Gallery;
            gallery.Groups[0].Caption = "Temalar";
            var item = gallery.GetAllItems().FirstOrDefault(i => i.Value.ToString() == Skin.DefaultSkinPaletteName);
            if (item != null)
                item.Caption = "Açık Mavi";
        }
    }
}

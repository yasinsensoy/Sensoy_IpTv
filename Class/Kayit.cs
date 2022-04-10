using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Sensoy_IpTv
{
    public static class Kayit
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Ayar));
        private static readonly DESCryptoServiceProvider crypto = new DESCryptoServiceProvider();
        private static readonly byte[] key = Encoding.ASCII.GetBytes("ys939604");
        private static readonly string Yol = Path.Combine(Application.StartupPath, "hesaplar");
        public static Ayar ayar = new Ayar();

        public static string Kaydet()
        {
            try
            {
                using (FileStream fs = File.Open(Yol, FileMode.Create))
                using (CryptoStream cryptoStream = new CryptoStream(fs, crypto.CreateEncryptor(key, key), CryptoStreamMode.Write))
                    serializer.Serialize(cryptoStream, ayar);
                return "Kaydedildi";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string Yukle()
        {
            if (File.Exists(Yol))
            {
                try
                {
                    using (FileStream fs = File.Open(Yol, FileMode.Open))
                    using (CryptoStream cryptoStream = new CryptoStream(fs, crypto.CreateDecryptor(key, key), CryptoStreamMode.Read))
                        ayar = (Ayar)serializer.Deserialize(cryptoStream);
                    return "Yüklendi";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
                return "Dosya bulunamadı";
        }
    }

    [Serializable()]
    public class Ayar
    {
        [XmlAttribute("pn")]
        public string PaletteName { get; set; } = "Koyu Mavi";

        [XmlAttribute("pv")]
        public int PlayerVolume { get; set; } = 100;

        public List<Hesap> Hesaplar { get; set; } = new List<Hesap>();

        public Ayar()
        { }
    }


    [Serializable()]
    public class Hesap
    {
        [XmlAttribute("had")]
        public string HesapAd { get; set; }

        [XmlAttribute("server")]
        public string SunucuUrl { get; set; }

        [XmlAttribute("kad")]
        public string KullaniciAd { get; set; }

        [XmlAttribute("sifre")]
        public string Sifre { get; set; }

        [XmlIgnore]
        public string Ad => $"{HesapAd} [{KullaniciAd}]";

        public void Yukle(Hesap hesap)
        {
            HesapAd = hesap.HesapAd;
            SunucuUrl = hesap.SunucuUrl;
            KullaniciAd = hesap.KullaniciAd;
            Sifre = hesap.Sifre;
        }

        public Hesap(string hesapad, string sunucuurl, string kad, string sifre)
        {
            HesapAd = hesapad;
            SunucuUrl = sunucuurl;
            KullaniciAd = kad;
            Sifre = sifre;
        }

        public Hesap()
        { }
    }
}
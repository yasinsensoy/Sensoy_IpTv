using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using TimeZoneConverter;

namespace Sensoy_IpTv
{
    public static class Komutlar
    {
        public static object Giris(Hesap hesap)
        {
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}");
            if (kaynak.StartsWith("Hata:"))
                return kaynak.Replace("Hata:", "");
            try
            {
                var json = (dynamic)JObject.Parse(kaynak);
                var user_info = json.user_info;
                if (user_info == null)
                    return "Xtream kodları desteklenmeyen sunucu.";
                bool auth = Convert.ToBoolean(user_info.auth.Value);
                if (!auth)
                    return "Kullanıcı adı yada şifre hatalı.";
                string durum = user_info.status == null ? "" : user_info.status.Value.Replace("Active", "Aktif").Replace("Disabled", "Aktif Değil").Replace("Expired", "Süresi Bitti").Replace("Banned", "Banlı");
                string timezone = json.server_info.timezone == null ? "" : json.server_info.timezone.Value;
                TZConvert.TryGetTimeZoneInfo(timezone, out TimeZoneInfo tz);
                int aktif = Convert.ToInt32(user_info.active_cons == null ? 0 : user_info.active_cons.Value);
                int max = Convert.ToInt32(user_info.max_connections == null ? 0 : user_info.max_connections.Value);
                DateTime suan = DateTime.MinValue, bittar = DateTime.MinValue, bastar = DateTime.MinValue;
                if (json.server_info.timestamp_now != null && json.server_info.timestamp_now.Value != null)
                {
                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(json.server_info.timestamp_now.Value));
                    suan = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                }
                if (user_info.exp_date != null && user_info.exp_date.Value != null)
                {
                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(user_info.exp_date.Value));
                    bittar = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                }
                if (user_info.created_at != null && user_info.created_at.Value != null)
                {
                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(user_info.created_at.Value));
                    bastar = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                }
                string[] destur = user_info.allowed_output_formats == null ? new string[] { } : Array.ConvertAll((object[])((JArray)user_info.allowed_output_formats).Select(j => ((dynamic)j).Value).ToArray(), x => x.ToString());
                return new IpTv(hesap, durum, aktif, max, timezone, suan, bastar, bittar, destur);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static List<object> GetKanallar(Hesap hesap, TimeZoneInfo tz, string id, string ext)
        {
            List<object> kanallar = new List<object>();
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action=get_live_streams&category_id={id}");
            try
            {
                var json = (dynamic)JArray.Parse(kaynak);
                foreach (var item in json)
                {
                    string ad = item.name == null ? "" : item.name.Value.ToString(); ;
                    string mediaid = item.stream_id == null ? "" : item.stream_id.Value.ToString();
                    string resim = item.stream_icon == null ? "" : item.stream_icon.Value.ToString();
                    DateTime etarih = DateTime.MinValue;
                    if (item.added != null && item.added.Value != null)
                    {
                        DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item.added.Value));
                        etarih = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                    }
                    kanallar.Add(new Medya(ad, mediaid, "", ext, resim, Tipler.canli, etarih, DateTime.MinValue, "", hesap));
                }
                kanallar = kanallar.OrderBy(a => ((Medya)a).Ad, new StrCmpLogicalComparer()).ToList();
            }
            catch (Exception)
            {
            }
            return kanallar;
        }

        public static object GetFilm(Hesap hesap, string id)
        {
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action=get_vod_info&vod_id={id}");
            try
            {
                var json = (dynamic)JObject.Parse(kaynak);
                return json.info;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<object> GetFilmler(Hesap hesap, TimeZoneInfo tz, string id)
        {
            List<object> filmler = new List<object>();
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action=get_vod_streams&category_id={id}");
            try
            {
                var json = (dynamic)JArray.Parse(kaynak);
                foreach (var item in json)
                {
                    string ad = item.name == null ? "" : item.name.Value.ToString();
                    string mediaid = item.stream_id == null ? "" : item.stream_id.Value.ToString();
                    string ext = item.container_extension == null ? "" : item.container_extension.Value.ToString();
                    string resim = item.stream_icon == null ? "" : item.stream_icon.Value.ToString();
                    DateTime etarih = DateTime.MinValue;
                    if (item.added != null && item.added.Value != null)
                    {
                        DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item.added.Value));
                        etarih = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                    }
                    filmler.Add(new Medya(ad, mediaid, "", ext, resim, Tipler.film, etarih, DateTime.MinValue, "", hesap));
                }
                filmler = filmler.OrderBy(a => ((Medya)a).Ad, new StrCmpLogicalComparer()).ToList();
            }
            catch (Exception)
            {
            }
            return filmler;
        }

        public static List<object> GetDiziler(Hesap hesap, string id)
        {
            List<object> diziler = new List<object>();
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action=get_series&category_id={id}");
            try
            {
                var json = (dynamic)JArray.Parse(kaynak);
                foreach (var item in json)
                {
                    string ad = item.name == null ? "" : item.name.Value.ToString().Trim();
                    string diziid = item.series_id == null ? "" : item.series_id.Value.ToString();
                    string resim = item.cover == null ? "" : item.cover.Value.ToString();
                    string ozet = item.plot == null ? "" : item.plot.Value.ToString();
                    DateTime tarih = DateTime.MinValue;
                    if (item.releaseDate != null)
                        DateTime.TryParse(item.releaseDate.Value.ToString(), out tarih);
                    diziler.Add(new Dizi(ad, diziid, resim, ozet, tarih));
                }
                diziler = diziler.OrderBy(a => ((Dizi)a).Ad, new StrCmpLogicalComparer()).ToList();
            }
            catch (Exception)
            {
            }
            return diziler;
        }

        public static List<object> GetSezonlar(Hesap hesap, TimeZoneInfo tz, string id)
        {
            List<object> sezonlar = new List<object>();
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action=get_series_info&series_id={id}");
            try
            {
                var json = (dynamic)JObject.Parse(kaynak);
                foreach (var e in json.episodes)
                {
                    dynamic list;
                    list = e.Type == JTokenType.Property ? ((JProperty)e).Values().ToArray() : ((JArray)e).ToArray();
                    dynamic i = ((object[])list).FirstOrDefault();
                    string season = i == null || i.season == null ? "" : i.season.Value.ToString();
                    if (season == "")
                        continue;
                    var item = (dynamic)((JArray)json.seasons).FirstOrDefault(a => ((dynamic)a).season_number.Value.ToString() == e.Name);
                    string ad = $"Sezon {season}";
                    string resim = item == null || item.cover == null ? "" : item.cover.Value.ToString();
                    string ozet = item == null || item.overview == null ? "" : item.overview.Value.ToString();
                    DateTime tarih = DateTime.MinValue;
                    if (item != null && item.air_date != null)
                        DateTime.TryParse(item.air_date.Value.ToString(), out tarih);
                    Sezon sezon = new Sezon(ad, resim, ozet, tarih);
                    foreach (var item1 in list)
                    {
                        ad = item1.episode_num == null ? (item1.title == null ? "" : item1.title.Value.ToString().Trim()) : $"Bölüm {item1.episode_num.Value}";
                        string mediaid = item1.id == null ? "" : item1.id.Value.ToString();
                        string sure = item1.info == null || item1.info.duration_secs == null ? "" : KalanSureDuzenle(TimeSpan.FromSeconds(Convert.ToDouble(item1.info.duration_secs.Value)), 3);
                        string ext = item1.container_extension == null ? "" : item1.container_extension.Value.ToString();
                        resim = item1.info == null || item1.info.movie_image == null ? "" : item1.info.movie_image.Value.ToString();
                        ozet = item1.info == null || item1.info.plot == null ? "" : item1.info.plot.Value.ToString();
                        DateTime etarih = tarih = DateTime.MinValue;
                        if (item1.added != null && item1.added.Value != null)
                        {
                            DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item1.added.Value));
                            etarih = (tz == null ? dto : TimeZoneInfo.ConvertTime(dto, tz)).DateTime;
                        }
                        if (item1.info != null && item1.info.releasedate != null)
                            DateTime.TryParse(item1.info.releasedate.Value.ToString(), out tarih);
                        sezon.AltOgeler.Add(new Medya(ad, mediaid, sure, ext, resim, Tipler.bolum, etarih, tarih, ozet, hesap));
                    }
                    sezon.AltOgeler = sezon.AltOgeler.OrderBy(a => ((Medya)a).Ad, new StrCmpLogicalComparer()).ToList();
                    sezonlar.Add(sezon);
                }
                sezonlar = sezonlar.OrderBy(a => ((Sezon)a).Ad, new StrCmpLogicalComparer()).ToList();
            }
            catch (Exception)
            {
            }
            return sezonlar;
        }

        public static Dictionary<string, string> GetKategori(Hesap hesap, string action)
        {
            Dictionary<string, string> deger = new Dictionary<string, string>();
            string kaynak = KaynakKoduAl($"{hesap.SunucuUrl}/player_api.php?username={hesap.KullaniciAd}&password={hesap.Sifre}&action={action}");
            try
            {
                var json = (dynamic)JArray.Parse(kaynak);
                foreach (var item in json)
                    deger.Add(item.category_id.Value.ToString(), item.category_name.Value.ToString().Trim());
                deger = deger.OrderBy(a => a.Value, new StrCmpLogicalComparer()).ToDictionary(t => t.Key, t => t.Value);
            }
            catch (Exception)
            {
            }
            return deger;
        }

        public static Kategori Kategoriler(Hesap hesap)
        {
            Dictionary<string, object[]> actions = new Dictionary<string, object[]>
            {
                { "Canlı", new object[] { "get_live_categories", Tipler.canlik } },
                { "Film", new object[] { "get_vod_categories", Tipler.filmk } },
                { "Dizi", new object[] { "get_series_categories", Tipler.dizik } }
            };
            Kategori kategori = new Kategori();
            foreach (var action in actions)
            {
                Tipler tip = (Tipler)action.Value[1];
                Klasor klasor = new Klasor(action.Key, Tipler.ana);
                foreach (var item in GetKategori(hesap, (string)action.Value[0]))
                    klasor.AltOgeler.Add(new Klasor(item.Value, tip, item.Key));
                if (klasor.AltOgeler.Count > 0)
                    kategori.Ogeler.Add(klasor);
            }
            return kategori;
        }

        public static string TagYakala(string veri, string baslangic, string bitis, int index = 1)
        {
            string[] a = veri.Split(new string[] { baslangic }, index + 1, StringSplitOptions.None);
            if (a.Length > 1 && a.Length > index)
                return a[index].Split(new string[] { bitis }, StringSplitOptions.None)[0];
            else
                return "";
        }

        public static string KalanSureDuzenle(TimeSpan sure, int sayac = 2)
        {
            string format = "";
            if (sure.Days >= 365) { format = Convert.ToInt32(Math.Floor(sure.Days / 365.0)) + " yıl"; sayac--; }
            if ((sure.Days % 365) >= 30 && sayac > 0) { format += (format == "" ? "" : " ") + Convert.ToInt32(Math.Floor(sure.Days % 365 / 30.0)) + " ay"; sayac--; }
            if ((sure.Days % 365 % 30) > 0 && sayac > 0) { format += (format == "" ? "" : " ") + sure.Days % 365 % 30 + " gün"; sayac--; }
            if (sure.Hours > 0 && sayac > 0) { format += (format == "" ? "" : " ") + sure.Hours + " saat"; sayac--; }
            if (sure.Minutes > 0 && sayac > 0) { format += (format == "" ? "" : " ") + sure.Minutes + " dak"; sayac--; }
            if (sure.Seconds > 0 && sayac > 0) format += (format == "" ? "" : " ") + sure.Seconds + " san";
            return format;
        }

        public static string KaynakKoduAl(string url, string method = WebRequestMethods.Http.Get, string postdegeri = "", string posttype = "", CookieCollection cookie = null, WebHeaderCollection header = null)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.CookieContainer = new CookieContainer();
                if (!(cookie is null))
                    request.CookieContainer.Add(cookie);
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62";
                request.Referer = url;
                request.Method = method;
                request.AllowAutoRedirect = false;
                request.Timeout = 10000;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (!(header is null))
                    request.Headers.Add(header);
                if (method == WebRequestMethods.Http.Post && postdegeri != "")
                {
                    byte[] p = Encoding.UTF8.GetBytes(postdegeri);
                    if (posttype == "")
                        posttype = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.ContentType = posttype;
                    request.ContentLength = p.Length;
                    using (Stream stream = request.GetRequestStream())
                        stream.Write(p, 0, p.Length);
                }
                string source = "";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (!(cookie is null))
                        cookie.Add(response.Cookies);
                    using (Stream rstream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(rstream))
                        source = reader.ReadToEnd();
                }
                return HttpUtility.HtmlDecode(source);
            }
            catch (Exception ex)
            {
                return $"Hata:{ex.Message}";
            }
        }
    }

    public class IpTv
    {
        public Hesap hesap { get; set; }
        public string Durum { get; set; }
        public string HataMesaj { get; set; }
        public string Bolge { get; set; }
        public int AktifKullaniciSayisi { get; set; }
        public int MaxKullaniciSayisi { get; set; }
        public DateTime SunucuTarih { get; set; }
        public DateTime BaslamaTarih { get; set; }
        public DateTime BitisTarih { get; set; }
        public string[] DesteklenenTurler { get; set; }
        public string AktifMax => string.IsNullOrEmpty(HataMesaj) ? $"{AktifKullaniciSayisi}/{MaxKullaniciSayisi}" : HataMesaj;
        public string KalanSure
        {
            get
            {
                if (!string.IsNullOrEmpty(HataMesaj))
                    return "";
                TZConvert.TryGetTimeZoneInfo(Bolge, out TimeZoneInfo tz);
                if (BitisTarih == DateTime.MinValue)
                    return "Sınırsız";
                if (SunucuTarih == DateTime.MinValue)
                {
                    if (string.IsNullOrEmpty(Bolge))
                        return "Bulunamadı";
                    else if (tz == null)
                        return "";
                    else
                        SunucuTarih = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
                }
                return Komutlar.KalanSureDuzenle(BitisTarih - SunucuTarih);
            }
        }

        public IpTv(Hesap m_hesap, string durum, int aktifks, int maxks, string timezone, DateTime suan, DateTime bastar, DateTime bittar, string[] desturler)
        {
            hesap = m_hesap;
            Durum = durum;
            AktifKullaniciSayisi = aktifks;
            MaxKullaniciSayisi = maxks;
            Bolge = timezone;
            SunucuTarih = suan;
            BaslamaTarih = bastar;
            BitisTarih = bittar;
            DesteklenenTurler = desturler;
        }

        public IpTv(Hesap m_hesap, string durum, string mesaj)
        {
            hesap = m_hesap;
            Durum = durum;
            HataMesaj = mesaj;
        }

        public void Yukle(IpTv iptv)
        {
            Durum = iptv.Durum;
            AktifKullaniciSayisi = iptv.AktifKullaniciSayisi;
            MaxKullaniciSayisi = iptv.MaxKullaniciSayisi;
            Bolge = iptv.Bolge;
            SunucuTarih = iptv.SunucuTarih;
            BaslamaTarih = iptv.BaslamaTarih;
            BitisTarih = iptv.BitisTarih;
            DesteklenenTurler = iptv.DesteklenenTurler;
        }
    }

    public class StrCmpLogicalComparer : Comparer<string>
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);

        public override int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }
}
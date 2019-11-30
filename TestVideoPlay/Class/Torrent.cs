using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestVideoPlay
{
    public enum TorrentQuality { Standart, HD, FHD, UHD }
    public class Torrent
    {
        public string Magnet { get; set; }
        public string Name { get; set; }
        public TorrentQuality Quality { get; set; }
        public int Seeders { get; set; }
        public int Leech { get; set; }
        public string Size { get; set; }
        //public Series Series { get; set; }
        public int seriesId;
        public int episodeId;
        public string URL { get; set; }
        //public Episode Episode { get; set; }
        public bool HasFinished { get; set; } = false;
        public bool IsSequential { get; set; } = false;
        public string FinishedAt { get; set; } = "-";

        public void AddTorrent()
        {
            Torrent tor = new Torrent();
            tor.Magnet = "magnet:?xt=urn:btih:3D78150CE51D1632419802D40AB9A2B26A10D726&dn=Arrow.S08E04.HDTV.x264-SVA%5Bettv%5D&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.pirateparty.gr%3A6969%2Fannounce&tr=udp%3A%2F%2Fexodus.desync.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.torrent.eu.org%3A451&tr=udp%3A%2F%2Ftracker.cyberia.is%3A6969%2Fannounce&tr=udp%3A%2F%2Fopen.demonii.si%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.tiny-vps.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.iamhansen.xyz%3A2000%2Fannounce&tr=udp%3A%2F%2Fexplodie.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fdenis.stalker.upeer.me%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.zer0day.to%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fcoppersurfer.tk%3A6969%2Fannounce";

        }

    }
}

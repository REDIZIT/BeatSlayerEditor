using Assets.AccountManagement;
using GameNet.Account;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountModel
{
    public class Account
    {
        public string nick;
        public string email;
        public string password;
        public string role;
        public AccountRole Role
        {
            get
            {
                return role == null || role == "" ? AccountRole.Player : (AccountRole)Enum.Parse(typeof(AccountRole), role);
            }
        }

        public TimeSpan playTime;

        public DateTime regTime;
        public DateTime activeTime;


        public int ratingPlace;
        public float score;

        public List<AccountMapInfo> playedMaps = new List<AccountMapInfo>();
        public List<AccountTrackRecord> records = new List<AccountTrackRecord>();

        public double RP
        {
            get
            {
                return replays.OrderByDescending(c => c.score).GroupBy(c => c.author + "-" + c.name).Select(c => c.First()).Sum(c => c.RP);
            }
        }
        public double TotalRP { get { return replays.Sum(c => c.RP); } }
        public List<Replay> replays = new List<Replay>();
    }

    public class Replay
    {
        public string player;
        // MapInfo
        public string author, name, nick;
        public int difficulty;

        public double RP;

        public float score;
        public int sliced, missed;
        [JsonIgnore] public float AllCubes { get { return sliced + missed; } }
        /// <summary>
        /// Accuracy in 1.0 (sliced / allCubes)
        /// </summary>
        [JsonIgnore] public float Accuracy { get { return AllCubes == 0 ? 0 : sliced / AllCubes; } }

        public Replay(string author, string name, string nick, int difficulty, float score, int sliced, int missed)
        {
            this.author = author;
            this.name = name;
            this.nick = nick;
            this.difficulty = difficulty;
            this.score = score;
            this.sliced = sliced;
            this.missed = missed;
        }
        public Replay(AccountTrackRecord record)
        {
            author = record.author;
            name = record.name;
            nick = record.nick;
            difficulty = 4; // USED DEFAULT VALUE
            score = record.score;
            sliced = record.sliced;
            missed = record.missed;
        }

        public Replay() { }
    }

    public class AccountTrackRecord
    {
        public string author, name, nick;

        public float score = 0, accuracy = 0; // Accuracy in 1.0
        public int missed = 0, sliced = 0;
    }

    public class LeaderboardItem
    {
        public string nick;
        public int place;
        public int playCount;
        public int slicedCount, missedCount;
        public double RP, score;
    }


    public class AccountMapInfo
    {
        public string author, name, nick;
        public int playTimes;
    }
}
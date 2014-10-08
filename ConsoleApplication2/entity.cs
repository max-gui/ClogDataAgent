using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    public class dvrData : DbContext
    {
        public DbSet<dvrInfo> dvrs { get; set; }
        public DbSet<userInfo> users { get; set; }
        public DbSet<userDeviceToken> deviceTokens { get; set; }
        public DbSet<dvrAlarmLog> alarmLogs { get; set; }
    }

    public class dvrInfo
    {
        [Key]
        public string dvrId { get; set; }
        public string dvrDomainName { get; set; }
        public string dvrName { get; set; }
        public virtual ICollection<userInfo> user { get; set; }
        public virtual ICollection<dvrAlarmLog> dvrAlarm { get; set; }
    }

    public class userInfo
    {
        [Key]
        public string userId { get; set; }
        public string password { get; set; }
        public virtual ICollection<userDeviceToken> deviceToken { get; set; }
        public virtual ICollection<dvrInfo> dvr { get; set; }
    }

    public class userDeviceToken
    {
        [Key]
        public int id { get; set; }
        public string deviceToken { get; set; }
        public virtual userInfo user { get; set; }
    }

    public class dvrAlarmLog
    {
        [Key]
        public int alarmId { get; set; }
        public int hddNumber { get; set; }
        public int channelNumber { get; set; }
        public errorType errorSet { get; set; }
        public string errorMessage { get; set; }
        public bool checkFlag { get; set; }
        public DateTime logTime { get; set; }
        public virtual dvrInfo dvr { get; set; }
        //[Timestamp]
        //public byte[] rowVersion { get; set; }
    }

    public enum errorType
    {
        hddFullError,
        signalLostError,
        emotionError,
        hddFormatError,
        hddIOError,
        cameraBlockError
    }
}

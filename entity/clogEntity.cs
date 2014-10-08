using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace entity
{
    public enum logLevel
    {
        INFO,
        ERROR
    }

    public enum hostIp
    {
        SH02SVR2626 = 1661274122,//"10.8.5.99",
        SH02SVR1860 = 1879377930,// "10.8.5.112",
        SH02SVR1199 = 1896155146,// "10.8.5.113",
        VMS05885 = 604309514,// "10.8.5.36",
        VMS05908 = 654641162,// "10.8.5.39",
        VMS05909 = 738527242,// "10.8.5.44"
        SH02SVR1636 = 419760138// "10.8.5.25"
    }

    public class clogEntity:DbContext
    {
        public DbSet<LogInfo> Logs { get; set; }
        public DbSet<TimeTable> Times { get; set; }
        public DbSet<RetryInfo> RetryInfos { get; set; }
        public DbSet<MoreInfo> MoreInfos { get; set; }
    }

    public class TimeTable
    {
        [Key]
        public int id { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime head { get; set; }
    }
    
    public class errorInfoBase
    {
        [Key]
        public string url { get; set; }
        public DateTime head { get; set; }
    }
    public class RetryInfo:errorInfoBase
    {
        //[Key]
        //public string url { get; set; }
        //public DateTime head { get; set; }
    }

    public class MoreInfo:errorInfoBase
    {
        //[Key]
        //public string url { get; set; }
        //public DateTime head { get; set; }
    }

    public class LogInfo
    {
        [Key]
        public int id { get; set; }
        public string AppId { get; set; }
        public logLevel Level { get; set; }
        public hostIp HostIp { get; set; }
        public ulong TimeStamp { get; set; }
        public string message { get; set; }
        public string Uid { get; set; }
        public string Platform { get; set; }
        public string Serviceversion { get; set; }
        public string Servicecode { get; set; }
        public string Servicetype { get; set; }
        public string Guid { get; set; }
        public string Logtype { get; set; }
        public string Bustype { get; set; }
        public string Orderid { get; set; }
        public string Title { get; set; }
        public DateTime head { get; set; }
    }
}

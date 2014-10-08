using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new dvrData())
            {
                // Create and save a new Blog 
                //Console.Write("Enter a name for a new Blog: ");

                var dvr = db.dvrs.Where(d => d.dvrId == "test1").FirstOrDefault();// new dvrInfo { dvrId = "test2", dvrDomainName = "116.226.77.223", dvrName = "test1" };
                //db.dvrs.Add(dvr);
                //var user = db.users.Where(u => u.userId.Equals("test")).FirstOrDefault();// new userInfo{userId="test",password="111"}
                //db.users.Add(user);
                //db.SaveChanges();
                dvr.dvrAlarm.Add(new dvrAlarmLog { channelNumber = 0, checkFlag = false, hddNumber = -1, errorMessage = "test1", errorSet = errorType.signalLostError, logTime = DateTime.Now });
                dvr.dvrAlarm.Add(new dvrAlarmLog { channelNumber = -1, checkFlag = false, hddNumber = 10, errorMessage = "test2", errorSet = errorType.hddFormatError, logTime = DateTime.Now });
                //var dvrList = new List<dvrInfo>();

                var dvr2 = db.dvrs.Where(d => d.dvrId == "test2").FirstOrDefault();
                dvr2.dvrAlarm.Add(new dvrAlarmLog { channelNumber = 0, checkFlag = false, hddNumber = -1, errorMessage = "test1", errorSet = errorType.signalLostError, logTime = DateTime.Now });
                dvr2.dvrAlarm.Add(new dvrAlarmLog { channelNumber = -1, checkFlag = false, hddNumber = 10, errorMessage = "test2", errorSet = errorType.hddFormatError, logTime = DateTime.Now });
                //var dvrList = new List<dvrInfo>();
                //dvrList.Add(dvr);
                //user.dvr = dvrList;
                db.SaveChanges();
            } 
        }
    }
}

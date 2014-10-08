using entity;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class LogCondition
    {
        public string[] HostList { get; set; }
        public List<string> LogLevelLs { get; set; }
        public List<string> appidLs { get; set; }
        public string[] TagValueArr { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var logCondition = Init();

            var getTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var t1 = DateTime.Now.Ticks;
                    
                    GetData(logCondition);
                    var t2 = DateTime.Now.Ticks;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("totle time = " + ((t2 - t1) / 10000000).ToString());
                    Thread.Sleep(1000);
                }
            });

            var tryCount = logCondition.appidLs.Count * logCondition.HostList.Count();
            var reTryTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        GetAgent(logCondition, tryCount, (db) =>
                        {
                            return db.RetryInfos;
                        });
                        //DoRetryToGetData(logCondition.LogLevelLs, logCondition.appidLs, logCondition.TagValueArr, tryCount);
                        Thread.Sleep(1000);
                    }
                });
            

            var moreGetTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GetAgent(logCondition, tryCount, (db) =>
                        {
                            return db.MoreInfos;
                        });

                    Thread.Sleep(1000);
                }
            });
            Task[] tasks = new Task[]{
                getTask,reTryTask,moreGetTask
            };

            Task.WaitAll(tasks);
        }

        private static void GetAgent<T>(LogCondition logCondition,
            int tryCount, Func<clogEntity ,DbSet<T>> getSource) where T : errorInfoBase
        {
            using (var db = new clogEntity())
            {
                var source = getSource(db);
                var tryTmp = source.Take(tryCount).ToList();
                //var tryTmp = source.Take(tryCount).ToList();
                if (tryTmp.Count() > 0)
                {
                    foreach (var moreGet in tryTmp)
                    {
                        var infoTmp = DoGetLogStrong(logCondition, moreGet.head, moreGet.url);

                        if (!infoTmp.LogErrorInfo.IsError)
                        {
                            db.Logs.AddRange(infoTmp.LogInfoList);//.AddRange(t.Result);

                            if (infoTmp.LogErrorInfo.HasMore)
                            {
                                //infoTmp.LogErrorInfo.ErrorMoreInfo.head = moreGet.head;
                                db.MoreInfos.Add(infoTmp.LogErrorInfo.ErrorMoreInfo);
                                //}
                            }
                            //db.MoreInfos
                            source.Remove(moreGet);
                            //db.MoreInfos.Remove(moreGet);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static LogCondition Init()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<clogEntity>());
            var hostList = new string[]{
                               "SH02SVR2626",//"10.8.5.99",
                               "SH02SVR1860",// "10.8.5.112",
                               "SH02SVR1199",// "10.8.5.113",
                               "VMS05885",// "10.8.5.36",
                               "VMS05908",// "10.8.5.39",
                               "VMS05909",// "10.8.5.44"
                               "SH02SVR1636"// "10.8.5.25"
                     };
            var logLevelArr = new string[]{
                "INFO","ERROR"
            };
            var logLevelLs = logLevelArr.ToList();



            var appidArr = new string[] { 
                "410471",
                "340101"
            };
            var appidLs = appidArr.ToList();
            var tagValueArr = new string[]{
                "paymentinfo",
                "paymentinfosoa",
                "paymentnotify",
                "paymentinfosoa2"
            };
            var tagValueLs = tagValueArr.ToList();

            return new LogCondition { 
                appidLs = appidLs,
                HostList = hostList, 
                LogLevelLs = logLevelLs, 
                TagValueArr = tagValueArr };
        }

        //private static void DoRetryToGetData(List<string> logLevelLs, List<string> appidLs, string[] tagValueArr, int tryCount)
        //{
        //    using (var db = new clogEntity())
        //    {
        //        var tryTmp = db.RetryInfos.Take(tryCount).ToList();
        //        if (tryTmp.Count() > 0)
        //        {
        //            foreach (var toTry in tryTmp)
        //            {
        //                var infoTmp = DoGetLog(logLevelLs, appidLs, tagValueArr, toTry.head, toTry.url);

        //                if (!infoTmp.LogErrorInfo.IsError)
        //                {
        //                    db.Logs.AddRange(infoTmp.LogInfoList);//.AddRange(t.Result);

        //                    if (infoTmp.LogErrorInfo.HasMore)
        //                    {
        //                        infoTmp.LogErrorInfo.ErrorMoreInfo.head = toTry.head;
        //                        db.MoreInfos.Add(infoTmp.LogErrorInfo.ErrorMoreInfo);
        //                        //}
        //                    } 
        //                    db.RetryInfos.Remove(toTry);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //    }
        //}

        private static void GetData(LogCondition logCondition)
        {
            #region prepare for time
            TimeTable endTmp = null;
            using (var db = new clogEntity())
            {
                if (db.Times.Count() < 1)
                {
                    endTmp = new TimeTable();
                    endTmp.start = new DateTime(2014, 10, 3);// headTime.end;
                    endTmp.end = endTmp.start.AddSeconds(30);
                    endTmp.head = new DateTime(2014, 10, 3);
                    //db.Times.Add(endTmp);
                    //db.SaveChanges();
                }
                else
                {
                    //var headTime =
                    //    (from ht in db.Times
                    //    group ht by ht.id into grp
                    //    select grp.OrderByDescending(g => g.end).FirstOrDefault()).First();

                    var headTime = db.Times.OrderByDescending(g => g.end).First();
                    endTmp = new TimeTable();
                    endTmp.start = headTime.end;
                    endTmp.end = headTime.end.AddSeconds(30);
                    endTmp.head = endTmp.start.Month != headTime.start.Month ? endTmp.start : headTime.head;
                    if (endTmp.end > DateTime.Now.AddMinutes(-1))
                    {
                        return;
                    }
                    
                    //db.Times.Add(endTmp);
                    //db.SaveChanges();
                }
            }
            #endregion
            //var from = endTmp.start;// new DateTime(2014, 8, 26);
            var newFrom = endTmp.start;
            var newTo = endTmp.end;

            if (newTo > DateTime.Now.AddMinutes(-5))
            {
                Thread.Sleep(new TimeSpan(0, 5, 0));
                return;
            }

            var urlLs = getUrlListByMinutes(logCondition.HostList, newFrom, newTo, logCondition.appidLs); //getUrlListByMinutes(hostList, logLevelLs, newFrom, newTo, appidLs, tagValueLs);
            //urlLs = getUrlListByMinutes(hostList, "2014-09-03 10:50:13", "2014-09-03 10:52:13", appidArr);
            Random rnd = new Random();

            List<Task<getTaskResult>> tasks = new List<Task<getTaskResult>>();
            // Execute the task 10 times.
            foreach (var url in urlLs)
            {

                tasks.Add(Task<getTaskResult>.Factory.StartNew(() =>
                {
                    //RetryInfo retryTmp = null;
                    var taskRes = DoGetLogStrong(logCondition, endTmp.head, url);

                    return taskRes;// new getTaskResult { LogInfoList = taskRes.LogInfoList, LogErrorInfo = new ErrorInfo { ErrorRetryInfo = retryTmp } };
                }));
            }

            var t1 = DateTime.Now.Ticks;

            Task.WaitAll(tasks.ToArray());

            var t2 = DateTime.Now.Ticks;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Task totle time = " + ((t2 - t1) / 10000000).ToString());

            using (var db = new clogEntity())
            {
                foreach (var t in tasks)
                {
                    db.Logs.AddRange(t.Result.LogInfoList);
                    //if(t.Result.retryInfo != null)
                    //{

                    if (t.Result.LogErrorInfo.IsError)
                    {
                        t.Result.LogErrorInfo.ErrorRetryInfo.head = endTmp.head;
                        db.RetryInfos.Add(t.Result.LogErrorInfo.ErrorRetryInfo);
                        //}
                    }
                    if (t.Result.LogErrorInfo.HasMore)
                    {
                        t.Result.LogErrorInfo.ErrorMoreInfo.head = endTmp.head;
                        db.MoreInfos.Add(t.Result.LogErrorInfo.ErrorMoreInfo);
                        //}
                    }
                }
                db.Times.Add(endTmp);
                db.SaveChanges();
            }
        }

        private static getTaskResult DoGetLogStrong(LogCondition logCondition, DateTime head, string url)
        {
            //var infoTmpList = new List<LogInfo>();
            long b;
            var errorInfoTmp = new ErrorInfo { IsError = false, HasMore = false };

            var rsObj = GetLogStrong(url, () =>
            {
                errorInfoTmp.IsError = true;
                errorInfoTmp.ErrorRetryInfo = new RetryInfo { url = url, head = head };
                //errorInfoTmp = error;
            }, out b);

            var infoTmpList = dataProcessorStrong(logCondition, head, b, rsObj, () =>
            {
                var urlTmp = string.Format(url + "&lastTimestamp={0}&lastScanRowKey={1}",
                                                        rsObj.lastTimestamp, rsObj.lastScanRowKey);

                errorInfoTmp.HasMore = true;
                errorInfoTmp.ErrorMoreInfo = new MoreInfo { url = urlTmp, head = head };
            });

            return new getTaskResult { LogInfoList = infoTmpList, LogErrorInfo = errorInfoTmp };// infoTmpList;
        }

        private static getTaskResult DoGetLog(LogCondition logCondition, DateTime head, string url)
        {
            //var infoTmpList = new List<LogInfo>();
            long b;
            var errorInfoTmp = new ErrorInfo { IsError = false, HasMore = false };

            var rsObj = GetLog(url, () =>
            {
                errorInfoTmp.IsError = true;
                errorInfoTmp.ErrorRetryInfo = new RetryInfo { url = url, head = head };
                //errorInfoTmp = error;
            }, out b);

            var infoTmpList = dataProcessor(logCondition, head, b, rsObj, () =>
                {
                    var urlTmp = string.Format(url + "&lastTimestamp={0}&lastScanRowKey={1}",
                                                            rsObj["lastTimestamp"], rsObj["lastScanRowKey"]);

                    errorInfoTmp.HasMore = true;
                    errorInfoTmp.ErrorMoreInfo = new MoreInfo { url = urlTmp, head = head };
                });

            return new getTaskResult { LogInfoList = infoTmpList, LogErrorInfo = errorInfoTmp };// infoTmpList;
        }

        private static List<LogInfo> dataProcessorStrong(LogCondition logCondition, DateTime head, long b, logData rsObj, Action act)
        {
            var infoTmpList = new List<LogInfo>();
            if (rsObj != null && rsObj.size > 0)
            {
                #region db
                //using (var db = new clogEntity())
                //{
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("size=" + rsObj.size);

                var logs = rsObj.logs;
                foreach (var log in logs)
                {
                    var appIdTmp = log.appId;// logTmp["appId"] as string;
                    var logLevelTmp = log.logLevel;// logTmp["logLevel"] as string;
                    var hostTmp = log.hostName;// logTmp["hostName"] as string;
                    //var logTyptTmp = logTmp["logType"] as string;
                    var logTimeTmp = log.timestamp;// logTmp["timestamp"] as string;
                    var logTitleTmp = log.title;// logTmp["title"] as string;
                    var logMessageTmp = log.message;// logTmp["message"] as string;

                    if (logCondition.appidLs.Contains(appIdTmp) && logCondition.LogLevelLs.Contains(logLevelTmp))
                    {
                        if (rsObj.size > 100)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(rsObj.size);

                            act();
                        }

                        var attrArrTmp = log.attributes;// logTmp["attributes"] as List<object>;

                        //attrArrTmp.AsEnumerable().Aggregate(infoTmpList, (r, n) =>
                        //    {
                        //        takeValueWhenMatch(logCondition,infoTmpList , infoTmp, n.key, n.value);
                        //        return r;
                        //    });

                        Dictionary<string, string> attrDic = 
                            attrArrTmp.AsEnumerable().
                                Aggregate(new Dictionary<string,string>(),(r, n) =>
                            {
                                r.Add(n.key.ToLower(), n.value);
                                return r;
                            });

                        //Func<string, string> helpTmp = (str) => attrDic.Keys.Contains(str) ? attrDic[str] : string.Empty;
                        Func<string, string> helpExTmp = (str) =>
                        {
                            string res = string.Empty;
                            try
                            {
                                res = attrDic[str];
                            }
                            catch (Exception)
                            {
                                res = string.Empty;
                            }

                            return res;
                        };
                        var typeTmp = helpExTmp("logtype");
                        if (logCondition.TagValueArr.Contains(typeTmp))
                        {
                            var infoTmp = new LogInfo();

                            //var infoTmp = new LogInfo();
                            infoTmp.AppId = appIdTmp;
                            infoTmp.HostIp = (hostIp)Enum.Parse(typeof(hostIp), hostTmp);
                            infoTmp.TimeStamp = ulong.Parse(logTimeTmp);
                            infoTmp.Title = logTitleTmp;
                            infoTmp.message = logMessageTmp;
                            infoTmp.head = head;
                            infoTmp.Level = (logLevel)Enum.Parse(typeof(logLevel), logLevelTmp);

                            infoTmp.Logtype = helpExTmp("logtype");// attrDic.Keys.Contains("logtype") ? attrDic["logtype"] : string.Empty;
                            infoTmp.Uid = helpExTmp("uid");// attrDic.Keys.Contains("uid") ? attrDic["uid"] : string.Empty;
                            infoTmp.Platform = helpExTmp("platform");// attrDic.Keys.Contains("platform") ? attrDic["platform"] : string.Empty;
                            infoTmp.Servicecode = helpExTmp("servicecode");// attrDic.Keys.Contains("servicecode") ? attrDic["servicecode"] : string.Empty;
                            infoTmp.Servicetype = helpExTmp("servicetype");// attrDic.Keys.Contains("servicetype") ? attrDic["servicetype"] : string.Empty;
                            infoTmp.Serviceversion = helpExTmp("serviceversion");// attrDic.Keys.Contains("serviceversion") ? attrDic["serviceversion"] : string.Empty;
                            infoTmp.Guid = helpExTmp("guid");// attrDic.Keys.Contains("guid") ? attrDic["Guid"] : string.Empty;
                            infoTmp.Bustype = helpExTmp("bustype");// attrDic.Keys.Contains("bustype") ? attrDic["bustype"] : string.Empty;
                            infoTmp.Orderid = helpExTmp("orderid");// attrDic.Keys.Contains("orderid") ? attrDic["orderid"] : string.Empty;

                            infoTmpList.Add(infoTmp);
                        }
                    }
                }
                #endregion
            }
            var c = DateTime.Now.Ticks;
            Console.WriteLine("self:" + (c - b) / 10000);

            return infoTmpList;
        }

        private static List<LogInfo> dataProcessor(LogCondition logCondition, DateTime head, long b, Dictionary<string, object> rsObj, Action act)
        {
            var infoTmpList = new List<LogInfo>();
            if (rsObj != null && int.Parse(rsObj["size"].ToString()) > 0)
            {
                #region db
                //using (var db = new clogEntity())
                //{
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(rsObj["size"]);

                var logs = rsObj["logs"] as List<object>;
                foreach (var log in logs)
                {
                    var logTmp = log as Dictionary<string, object>;
                    var appIdTmp = logTmp["appId"] as string;
                    var logLevelTmp = logTmp["logLevel"] as string;
                    var hostTmp = logTmp["hostName"] as string;
                    //var logTyptTmp = logTmp["logType"] as string;
                    var logTimeTmp = logTmp["timestamp"] as string;
                    var logTitleTmp = logTmp["title"] as string;
                    var logMessageTmp = logTmp["message"] as string;

                    if (logCondition.appidLs.Contains(appIdTmp) && logCondition.LogLevelLs.Contains(logLevelTmp))
                    {
                        if (int.Parse(rsObj["size"].ToString()) > 100)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(rsObj["size"]);

                            act();
                            
                        }

                        var infoTmp = new LogInfo();
                        infoTmp.AppId = appIdTmp;
                        infoTmp.HostIp = (hostIp)Enum.Parse(typeof(hostIp), hostTmp);
                        infoTmp.TimeStamp = ulong.Parse(logTimeTmp);
                        infoTmp.Title = logTitleTmp;
                        infoTmp.message = logMessageTmp;
                        infoTmp.head = head;

                        infoTmp.Level = (logLevel)Enum.Parse(typeof(logLevel), logLevelTmp);
                        var attrArrTmp = logTmp["attributes"] as List<object>;
                        foreach (var attr in attrArrTmp)
                        {
                            var attrTmp = attr as Dictionary<string, object>;
                            var keyTmp = attrTmp["key"] as string;
                            var valTmp = attrTmp["value"] as string;

                            var isMatch = takeValueWhenMatch(logCondition,infoTmpList, infoTmp, keyTmp, valTmp);
                            if (!isMatch)
                            { break; }
                        }

                        
                    }
                }

                //    db.SaveChanges();
                //}
                #endregion
            }
            //string id = dyn.Id;
            //string name = dyn.Name;
            //string dob = dyn.DateOfBirth;
            var c = DateTime.Now.Ticks;
            Console.WriteLine("self:" + (c - b) / 10000);

            return infoTmpList;
        }

        private static bool takeValueWhenMatch(LogCondition logCondition,List<LogInfo> infoTmpList, LogInfo infoTmp, string keyTmp, string valTmp)
        {
            var flag = true;
            switch (keyTmp.ToLower())
            {
                case "logtype":
                    infoTmp.Logtype = valTmp;
                    if (logCondition.TagValueArr.Contains(valTmp))
                    {
                        //okrsObj.PrintDump();
                        //db.Logs.Add(infoTmp);
                        infoTmpList.Add(infoTmp);
                    }
                    else
                    {
                        flag = false;
                    }
                    break;
                case "uid":
                    infoTmp.Uid = valTmp;
                    break;
                case "platform":
                    infoTmp.Platform = valTmp;
                    break;
                case "servicecode":
                    infoTmp.Servicecode = valTmp;
                    //Console.WriteLine("servicecode:" + valTmp);
                    break;
                case "servicetype":
                    infoTmp.Servicetype = valTmp;
                    //Console.WriteLine("servicetype:" + valTmp);
                    break;
                case "serviceversion":
                    infoTmp.Serviceversion = valTmp;
                    //Console.WriteLine("serviceversion:" + valTmp);
                    break;
                case "guid"://guid
                    infoTmp.Guid = valTmp;
                    break;
                case "bustype":
                    infoTmp.Bustype = valTmp;
                    break;
                case "orderid":
                    infoTmp.Orderid = valTmp;
                    break;
            }

            return flag;
        }

        private static logData GetLogStrong(string url, Action errorAct, out long b)
        {
            var hc = new HttpClient();
            hc.BaseAddress = new Uri("http://rest.logging.sh.ctriptravel.com");

            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            long secondS = 60;
            long timeS = secondS * 1000 * 10000;
            hc.Timeout = new TimeSpan(timeS);

            var res = hc.GetAsync(url);
            //Console.WriteLine(hc.BaseAddress + url);
            var a = DateTime.Now.Ticks;
            string rs = string.Empty;
            logData rsObj = null;
            var errorInfoTmp = new ErrorInfo { HasMore = false, ErrorMoreInfo = new MoreInfo(), IsError = false, ErrorRetryInfo = new RetryInfo() };
            b = DateTime.Now.Ticks;
            try
            {
                rs = res.Result.Content.ReadAsStringAsync().Result;
                //Console.WriteLine(rs);

                b = DateTime.Now.Ticks;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(url + System.Environment.NewLine + ":" + (b - a) / 10000);

                JsConfig.ConvertObjectTypesIntoStringDictionary = true;
                rsObj = rs.FromJson<logData>();
                //var dyn = DynamicJson.Deserialize(rs);

            }
            catch (Exception)
            {
                errorAct();
                //errorInfoTmp.IsError = true;
                //errorInfoTmp.ErrorRetryInfo.url = url;
                //return new List<LogInfo>();
            }

            //errorAct(errorInfoTmp);
            return rsObj;
        }

        private static Dictionary<string, object> GetLog(string url, Action errorAct, out long b)
        {
            var hc = new HttpClient();
            hc.BaseAddress = new Uri("http://rest.logging.sh.ctriptravel.com");

            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            long secondS = 60;
            long timeS = secondS * 1000 * 10000;
            hc.Timeout = new TimeSpan(timeS);

            var res = hc.GetAsync(url);
            //Console.WriteLine(hc.BaseAddress + url);
            var a = DateTime.Now.Ticks;
            string rs = string.Empty;
            Dictionary<string, object> rsObj = null;
            //var errorInfoTmp = new ErrorInfo {HasMore=false, ErrorMoreInfo = new MoreInfo(), IsError =false, ErrorRetryInfo = new RetryInfo() };
            b = DateTime.Now.Ticks;
            try
            {
                rs = res.Result.Content.ReadAsStringAsync().Result;
                //Console.WriteLine(rs);

                b = DateTime.Now.Ticks;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(url + System.Environment.NewLine + ":" + (b - a) / 10000);

                JsConfig.ConvertObjectTypesIntoStringDictionary = true;
                rsObj = (Dictionary<string, object>)rs.FromJson<object>();
                //var dyn = DynamicJson.Deserialize(rs);
                
            }
            catch (Exception)
            {

                errorAct();
                //errorInfoTmp.IsError = true;
                //errorInfoTmp.ErrorRetryInfo.url = url;
                //return new List<LogInfo>();
            }

            return rsObj;
        }

        private static List<string> getUrlListByMinutes(string[] hostList, string[] logLevelLs, DateTime newFrom, DateTime newTo, List<string> appidLs, string[] tagValueLs)
        {
            var urlTmp = string.Empty;

            var urlLs = new List<string>();
            foreach (var host in hostList)
            {
                foreach (var level in logLevelLs)
                {
                    foreach (var id in appidLs)
                    {
                        foreach (var value in tagValueLs)
                        {
                            urlTmp = string.Format("data/logs/{0}?fromDate={1}&toDate={2}&tagKey=logtype&tagValue={3}&logLevel={4}&hostName={5}",
                                        id, newFrom.ToString("yyyy-MM-dd HH:mm:ss"), newTo.ToString("yyyy-MM-dd HH:mm:ss"), value, level, host);
                            urlLs.Add(urlTmp);
                        }
                    }
                }
            }

            return urlLs;
        }

        private static List<string> getUrlListByMinutes(string[] hostList, DateTime newFrom, DateTime newTo, List<string> appidLs)
        {
            var urlTmp = string.Empty;

            var urlLs = new List<string>();
            foreach (var host in hostList)
            {
                
                    foreach (var id in appidLs)
                    {
                        
                            urlTmp = string.Format("data/logs/{0}?fromDate={1}&toDate={2}&hostName={3}",
                                        id, newFrom.ToString("yyyy-MM-dd HH:mm:ss"), newTo.ToString("yyyy-MM-dd HH:mm:ss"), host);
                            urlLs.Add(urlTmp);
                        
                    }
                
            }

            return urlLs;
        }

        private static List<string> getUrlListByMinutes(string[] hostList, string newFrom, string newTo, string[] appidLs)
        {
            var urlTmp = string.Empty;

            var urlLs = new List<string>();
            foreach (var host in hostList)
            {

                foreach (var id in appidLs)
                {
                    urlTmp = string.Format("data/logs/{0}?fromDate={1}&toDate={2}&hostName={3}",
                                id, newFrom, newTo, host);
                    urlLs.Add(urlTmp);

                }

            }

            return urlLs;
        }
    }

    public class getTaskResult
    {
        public List<LogInfo> LogInfoList { get; set; }
        public ErrorInfo LogErrorInfo { get; set; }
    }

    public class ErrorInfo
    {

        public bool IsError { get; set; }
        public bool HasMore { get; set; }
        public RetryInfo ErrorRetryInfo { get; set; }

        public MoreInfo ErrorMoreInfo { get; set; }

    }



    public class logData
    {
        public int size { get; set; }
        public string lastTimestamp { get; set; }
        public string lastScanRowKey { get; set; }
        public List<logInfoInData> logs { get; set; }
    }

    public class logInfoInData
    {
        public string appId { get; set; }
        public string logLevel { get; set; }
        public string hostName { get; set; }
        public string timestamp { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public List<logAttribute> attributes { get; set; }
    }

    public class logAttribute
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}

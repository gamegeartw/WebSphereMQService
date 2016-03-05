using System;
using IBM.WMQ;
using System.Xml;
using System.IO;

namespace MQService
{
    public class MQClient:IDisposable
    {
        public string MQManagerName { get; set; }
        public string MQPutQueueName { get; set; }
        public string MQGetQueueName { get; set; }
        public string IPAddress_Port { get; set; }
        public string SVRConnName { get; set; }
        public string Account { get; set; }
        public string password { get; set; }

        private string _statusMsg;
        public string statusMsg
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss") + " " + _statusMsg;
            }
            set
            {
                _statusMsg = value;
            }
        }
        private MQQueueManager manager;
        private static MQClient client;
        private static MQMessage msg;
        private MQClient()
        {
            msg = new MQMessage();
            using (var stream = File.Open(Path.Combine(Environment.CurrentDirectory, "data.xml"), FileMode.Open))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);
                Account = xmlDoc.SelectSingleNode("MQ/Account").InnerText;
                MQManagerName = xmlDoc.SelectSingleNode("MQ/MQManagerName").InnerText;
                MQPutQueueName = xmlDoc.SelectSingleNode("MQ/MQPutQueueName").InnerText;
                MQGetQueueName = xmlDoc.SelectSingleNode("MQ/MQGetQueueName").InnerText;
                IPAddress_Port = xmlDoc.SelectSingleNode("MQ/IPAddress_Port").InnerText;
                SVRConnName = xmlDoc.SelectSingleNode("MQ/SVRConnName").InnerText;
            }



        }
        public static MQClient GetInstence()
        {
            if (client == null)
            {
                client = new MQClient();
            }
            return client;
        }
        public bool OpenConnection()
        {
            bool flag = false;

            try
            {
                manager = new MQQueueManager(MQManagerName, this.SVRConnName, this.IPAddress_Port);
                if (manager.IsConnected)
                {
                    statusMsg = "成功建立連線";
                    flag = !flag;
                }
                else
                {
                    statusMsg = "無法建立連線";
                }

            }
            catch (Exception ex)
            {
                statusMsg = "無法建立連線，原因:" + ex.Message;
            }
            return flag;
        }

        public void CloseConnection()
        {
            manager.Close();
            var xmlDoc = new XmlDocument();
            using (var stream = File.Open(Path.Combine(Environment.CurrentDirectory, "data.xml"), FileMode.Open))
            {
                
                xmlDoc.Load(stream);
                xmlDoc.SelectSingleNode("MQ/Account").InnerText = Account;
                xmlDoc.SelectSingleNode("MQ/MQManagerName").InnerText = MQManagerName;
                xmlDoc.SelectSingleNode("MQ/MQPutQueueName").InnerText = MQPutQueueName;
                xmlDoc.SelectSingleNode("MQ/MQGetQueueName").InnerText = MQGetQueueName;
                xmlDoc.SelectSingleNode("MQ/IPAddress_Port").InnerText = IPAddress_Port;
                xmlDoc.SelectSingleNode("MQ/SVRConnName").InnerText = SVRConnName;
                
            }
            xmlDoc.Save(Path.Combine(Environment.CurrentDirectory, "data.xml"));
        }


        public void GetAndPutMsg(int WaitInterval)
        {
            ClearMsg();
            msg = GetQueue(WaitInterval);
            if (msg != null)
                putQueue(msg);
        }

        public MQMessage GetQueue(int WaitInterval)
        {
            ClearMsg();
            string result = string.Empty;
            int queueOptions = MQC.MQOO_INPUT_SHARED;
            MQGetMessageOptions gmo = new MQGetMessageOptions();

            gmo.Options = MQC.MQGMO_WAIT;
            gmo.WaitInterval = WaitInterval;

            try
            {
                var queue = manager.AccessQueue(MQGetQueueName, queueOptions);
                queue.Get(msg, gmo);
                result = "己取得佇列訊息";
            }
            catch (Exception ex)
            {
                if (ex.Message == "MQRC_UNKNOWN_OBJECT_NAME")
                {
                    result = "建立連線失敗";
                }
                result = "目前沒有任何佇列訊息";
                return null;
                //throw;
            }

            statusMsg = result;
            return msg;
        }

        private static void ClearMsg()
        {
            msg.MessageId = MQC.MQCI_NONE;
            msg.CorrelationId = MQC.MQCI_NONE;
            msg.ClearMessage();
        }

        public void putQueue(MQMessage msg)
        {
            try
            {
                int queueOptions = MQC.MQOO_OUTPUT;
                var queue = manager.AccessQueue(MQPutQueueName, queueOptions);
                MQPutMessageOptions mpo = new MQPutMessageOptions();
                queue.Put(msg, mpo);
                this.statusMsg = "訊息推播成功";
            }
            catch (Exception ex)
            {
                this.statusMsg = "訊息推播失敗，原因:" + ex.Message;
                throw;
            }

        }

        public void Dispose()
        {
            if (manager != null)
            {
                manager.Close();
                ClearMsg();
                msg = null;
                manager = null;
            }
        }



    }
}

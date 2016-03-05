using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MQService
{
    public partial class Form1 : Form
    {
        MQClient client;
        Boolean stopFlag = false;
        ModelMQData db = new ModelMQData();
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            client = MQClient.GetInstence();
            txtSVRConn.DataBindings.Add("Text", client, "SVRConnName");
            txtAccount.DataBindings.Add("Text", client, "Account");
            txtMQIP.DataBindings.Add("Text", client, "IPAddress_Port");
            txtMQManager.DataBindings.Add("Text", client, "MQManagerName");
            txtMQPutQueue.DataBindings.Add("Text", client, "MQPutQueueName");
            txtMQGetQueue.DataBindings.Add("Text", client, "MQGetQueueName");
            txtPassword.DataBindings.Add("Text", client, "password");
            dgDataView.DataSource = db.MQDatas.ToList();
            dgDataView.Columns[0].Visible = false;
            //DataRow dr = dt.NewRow();

        }
        delegate void MsgHandler(string msg);


        private void showMSG(string msg)
        {
            if (this.InvokeRequired)
            {
                MsgHandler _msg = new MsgHandler(showMSG);
                this.Invoke(_msg, msg);
            }
            else
            {
                txtMsg.Text += msg;
                txtMsg.Text += Environment.NewLine;
                MsgAutoScroll();
                dgDataView.DataSource = db.MQDatas.ToList();
            }
        }




        private void BtnStop_Click(object sender, EventArgs e)
        {
            //  throw new NotImplementedException();
            btnStop.Enabled = !btnStop.Enabled;
            showMSG(DateTime.Now.ToString("HH:mm:ss") + " 中止連線中...");
            stopFlag = false;
            ThreadPool.QueueUserWorkItem(x =>
            {
                client.CloseConnection();
                showMSG(DateTime.Now.ToString("HH:mm:ss") + " 已中止連線");
                this.Invoke(new Action(() => btnStart.Enabled = !btnStart.Enabled));
            });


        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (client.OpenConnection())
            {
                btnStart.Enabled = !btnStart.Enabled;
                btnStop.Enabled = !btnStop.Enabled;
                stopFlag = true;
                ThreadPool.QueueUserWorkItem(GetAndPutMsg, (int)(intNumControl.Value * 1000));
            }
            showMSG(client.statusMsg);
        }

        private void GetAndPutMsg(object state)
        {
            int waitInteveal = (int)state;
            while (stopFlag)
            {
                var msg = client.GetQueue(waitInteveal);
                if (msg != null)
                {
                    try
                    {
                        var mqData = new MQData();
                        mqData.Id = Guid.NewGuid();
                        mqData.CorrelationId = Encoding.UTF8.GetString(msg.CorrelationId).Trim();
                        mqData.GetDate = msg.PutDateTime.AddHours(8);
                        mqData.GroupID = Encoding.UTF8.GetString(msg.GroupId).Trim();
                        mqData.Msg = msg.ReadString(msg.MessageLength);
                        mqData.MsgID = Encoding.UTF8.GetString(msg.MessageId).Trim();
                        db.MQDatas.Add(mqData);
                        db.SaveChanges();
                        showMSG(client.statusMsg);
                        client.putQueue(msg);
                        showMSG(client.statusMsg);
                    }
                    catch (Exception)
                    {


                    }

                }
            }

        }

        private void getQueue(object state)
        {
            while (true)
            {
                client.GetQueue((int)(intNumControl.Value * 1000));
                showMSG(client.statusMsg);
            }
        }

        private void MsgAutoScroll()
        {
            txtMsg.SelectionStart = txtMsg.Text.Length;
            txtMsg.ScrollToCaret();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            client.Dispose();
            client = null;
            db.Dispose();
            db = null;
            base.OnFormClosing(e);
        }
    }
}

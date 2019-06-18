using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using HslCommunication.ModBus;
using HslCommunication;
using DevExpress.XtraEditors;

namespace Modbus
{
    public partial class Form1 : Form
    {
        TcpClient tcp = new TcpClient(); //定义连接

        //指令中间夹杂空格，不能缺失

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) //连接从站 
        {
            tcp.Connect("192.168.100.1", 8501); //设置IP与Port 8501为默认上位链路通讯端口


            if (tcp.Connected)
            {
                textBox1.Text = "连接成功";
            }

            else
            { textBox1.Text = "连接失败"; }

        }

        private void button2_Click(object sender, EventArgs e)//断开连接
        {

            tcp.Close();
            tcp = new TcpClient();//重新定义连接
            textBox1.Text = "连接断开";
        }

        private void button3_Click(object sender, EventArgs e) //写R
        {
            if (!tcp.Connected)
            {
                MessageBox.Show("请先连接网口");
                return;
            }
            string SendW = "";
            string RD = "RD R" + textBox4.Text + "\r";
            if (SendMessage(RD) == ("0\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0\0"))//如果该R值为0则发送置为指令
            {
                SendW = "ST R" + textBox4.Text + "\r";
            }
            if (SendMessage(RD) == ("1\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0\0"))//如果该R值为1则发送复位指令
            {
                SendW = "RS R" + textBox4.Text + "\r";
            }
            if (SendMessage(SendW) == ("OK\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0"))
            {
                textBox2.Text += "发送成功：" + SendW + "\r\n";
                if (SendMessage(RD) == ("1\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0\0"))
                {
                    label6.BackColor = Color.Green;
                    textBox3.Text += "接收成功：" + SendMessage(RD);
                }
                else
                {
                    label6.BackColor = Color.Black;
                    textBox3.Text += "接收成功：" + SendMessage(RD);
                }

            }
            else
            {
                textBox2.Text += "发送失败：" + SendW + "\r\n";
            }

        }
        private void test()
        {

            if (!tcp.Connected)
            {
                MessageBox.Show("请先连接网口");
                return;
            }

            string RD = "RD DM" + textBox5.Text + ".L\r";
            if (SendMessage(RD) == ("E0\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0") || SendMessage(RD) == ("E1\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0") || SendMessage(RD) == "未返回数据")
            {
                textBox2.Text += "发送失败：" + RD + "\r\n";
            }
            else
            {
                //textBox2.Text += "发送成功：" + RD + "\r\n";
                //textBox3.Text += "接收成功：" + SendMessage(RD);
                Console.WriteLine("发送成功：" + RD + "\r\n");
                Console.WriteLine("接收成功：" + SendMessage(RD));
            }

        }
        private void button4_Click(object sender, EventArgs e) //读DM
        {
            Action action = new Action(test);
            action.BeginInvoke(null, null);
        }
        private void Write()
        {

            if (!tcp.Connected)
            {
                MessageBox.Show("请先连接网口");
                return;
            }
            string RD = "WR DM" + textBox6.Text + ".L " + textBox7.Text + "\r";
            //string RD = "WRS DM" + textBox6.Text + ".D 1 " + textBox7.Text + "\r";
            if (SendMessage(RD) == ("OK\r\n" + "\0\0\0\0\0\0\0\0\0\0\0\0"))
            {
                //textBox2.Text += "发送成功：" + RD + "\r\n";
                //textBox3.Text += "接收成功：" + SendMessage(RD);
                Console.WriteLine("发送成功：" + RD + "\r\n");
                Console.WriteLine("接收成功：" + SendMessage(RD));
            }
            else
            {
                textBox2.Text += "发送失败：" + RD;

            }

        }
        private void button5_Click(object sender, EventArgs e)//写DM
        {
            Action action = new Action(Write);
            action.BeginInvoke(null, null);
        }


        /// <summary>
        /// 发送数据，返回结果
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        private string SendMessage(string mes)
        {
            NetworkStream DataSend = tcp.GetStream();//定义网络流
            string SendW = mes;
            var ByteSendW = Encoding.ASCII.GetBytes(SendW);//把发送数据转换为ASCII数组          
            DataSend.Write(ByteSendW, 0, ByteSendW.Length); //发送通讯数据
            byte[] data = new byte[16];//设定接收数据为16位数组，接收数据不足用0填充
            DataSend.Read(data, 0, data.Length);       //读取返回数据
            var ByteRD = "未返回数据";
            ByteRD = ASCII2String(data, 0, 100);
            ByteRD = Encoding.ASCII.GetString(data);//接收数据从ASCII数组转换为字符串
            return ByteRD;
        }
        public static string ASCII2String(byte[] data, int startIndex, int endIndex)
        {
            // 发现不一样 解析条码
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = startIndex; i < endIndex; i += 2)
            {
                if (data[i] == 0)
                {
                    break;
                }

                stringBuilder.Append(ASCII2String(data[i]));

                if (data[i + 1] == 0)
                {
                    break;
                }

                stringBuilder.Append(ASCII2String(data[i + 1]));
            }

            return stringBuilder.ToString();
        }
        public static string ASCII2String(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                return "";
                //throw new Exception("ASCII Code is not valid.");
            }
        }
        private ModbusTcpNet busTcpClient = new ModbusTcpNet("192.168.100.1", 8501, 0x01);
        private void button6_Click(object sender, EventArgs e)
        {
            int c = this.gridView1.RowCount;
            Console.WriteLine(c);
            for (int i = 0; i < c; i++)
            {
                DataRowView dr =(DataRowView) gridView1.GetRow(i);
              string a=  dr.Row["f001"].ToString();
                bool b = (bool)dr.Row["IS_CHECK"];
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("f001");
            dt.Columns.Add("f002");
            dt.Columns.Add("IS_CHECK", System.Type.GetType("System.Boolean"));

            dt.Rows.Add("1");
            dt.Rows.Add("2");
            dt.Rows.Add("3");
            dt.Rows.Add("4");
            dt.Rows.Add("5");
            gridControl1.DataSource = dt;
        }
        public int iCheckAll = 0;
        private void cb_checkall_CheckedChanged(object sender, EventArgs e)
        {

            DataTable dt = gridControl1.DataSource as DataTable;

            //优化 增加判断dt为null的条件
            if (dt != null)
            {
                if (cb_checkall.Checked == true)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        item["IS_CHECK"] = true;
                    }
                    iCheckAll = 1;
                }
                else
                {
                    if (iCheckAll == 0)
                    {
                        //表格中的数据没有全部选中时  设置全选框的的状态为FALSE  （觉得这个条件可以不要，可以试下哦O(∩_∩)O哈哈~）
                        DataRow[] drMM = dt.Select("IS_CHECK=0 OR IS_CHECK IS NULL");
                        if (drMM.Length > 0)
                        {
                            cb_checkall.Checked = false;
                        }
                    }
                    else if (iCheckAll == 1)
                    {

                        //表格中的数据是全选中状态时，取消全选时，设置表格中的标识为不选中的状态
                        foreach (DataRow item in dt.Rows)
                        {
                            item["IS_CHECK"] = false;
                        }
                    }


                }

            }
            else
            {
                //判断条件
                MessageBox.Show("没有可供选择的数据", "提示！");
                cb_checkall.Checked = false;
            }

        }
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            CheckEdit chkCheck = (sender as CheckEdit);
            DataRow dr = gridView1.GetFocusedDataRow();
            if (chkCheck.CheckState == CheckState.Checked)
            {
                dr["IS_CHECK"] = true;
            }
            else
            {
                dr["IS_CHECK"] = false;

            }

            //增加全部选择时，全选按钮应该勾选上
            DataTable dt = gridControl1.DataSource as DataTable;

            //判断如果GridView中按钮都全选了，把全选按钮也设置为选中状态 
            DataRow[] drTemp = dt.Select("IS_CHECK=0 OR IS_CHECK IS NULL");
            if (drTemp.Length > 0)
            {
                //没有全部选中
                iCheckAll = 0;
                cb_checkall.CheckState = CheckState.Unchecked;
            }
            else
            {
                iCheckAll = 1;
                cb_checkall.CheckState = CheckState.Checked;
            }



        }
    }
}


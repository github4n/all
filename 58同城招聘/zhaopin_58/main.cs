﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zhaopin_58
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false; // 设置线程之间可以操作
        }

        private void main_Load(object sender, EventArgs e)
        {

        }

        bool zanting = true;

        #region  主程序

        public void zhaopin()
        {

            try
            {

                string[] citys = textBox1.Text.Trim().Split(',');
                string[] keywords = textBox2.Text.Trim().Split(',');

                foreach (string city in citys)
                {
                    if (city == "")
                    {
                        MessageBox.Show("请选择城市！");
                        return;
                    }
                    foreach (string keyword in keywords)
                    {

                        if (keyword == "")
                        {
                            MessageBox.Show("请输入采集行业或者关键词！");
                            return;
                        }

                        for (int i = 1; i < 71; i++)
                        {
                            
                            string Url = "https://"+city+".58.com/job/pn"+i+"/?key="+keyword+ "&final=1&jump=1";
                            string html = method.GetUrl(Url);

                            textBox2.Text = Url;
                            // MatchCollection TitleMatchs = Regex.Matches(html, @"addition=""0""><a href=""([\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                            MatchCollection TitleMatchs = Regex.Matches(html, @"https://[a-z]+.58.com/[a-z]+/[0-9]+x.shtml", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                            ArrayList lists = new ArrayList();
                            //foreach (Match NextMatch in TitleMatchs)
                            //{

                            //    lists.Add(NextMatch.Groups[1].Value);
                            //}

                            //自己写的正则匹配是groups[0]
                            foreach (Match NextMatch in TitleMatchs)
                            {

                                lists.Add(NextMatch.Groups[0].Value);
                            }

                             if (lists.Count == 0)  //当前页没有网址数据跳过之后的网址采集，进行下个foreach采集

                                break;


                            foreach (string list in lists)

                            {
                                string strhtml = method.GetUrl(list);  //定义的GetRul方法 返回 reader.ReadToEnd()

                                string tellHtml = method.GetUrl("http://116.62.62.62/58.php?"+list);
                                
                                string rxg = @"<span class=""pos_title"">([\s\S]*?)</span>";
                                string rxg1 = @"content='\[([\s\S]*?)\]";    //公司
                                string rxg2 = @"<span class=""pos_area_item"" >([\s\S]*?)</span>";
                                string rxg4 = @"</span><span>([\s\S]*?)</span>";
                                string rxg5 = @"camp_indus"",""V"":""([\s\S]*?)""";
                                string rxg6 = @"linkman:'([\s\S]*?)'";
                                string rxg3 = @"更新:  <span>([\s\S]*?)</span>";
                                 string rxg7= @"<div class='nums'>([\s\S]*?)</div>";

                              

                                Match job = Regex.Match(strhtml, rxg);
                                Match company = Regex.Match(strhtml, rxg1);
                                MatchCollection areas = Regex.Matches(strhtml, rxg2);
                                Match addr = Regex.Match(strhtml, rxg4);
                                Match hangye = Regex.Match(strhtml, rxg5);
                                Match lxr = Regex.Match(strhtml, rxg6);
                                Match time = Regex.Match(strhtml, rxg3);

                                Match tell = Regex.Match(tellHtml, rxg7);

                                if (areas.Count > 0)
                                {
                                    ListViewItem lv1 = listView1.Items.Add(listView1.Items.Count.ToString());
                                    lv1.SubItems.Add(job.Groups[1].Value.Trim());
                                    lv1.SubItems.Add(company.Groups[1].Value.Trim().Replace("招聘信息", ""));
                                    lv1.SubItems.Add(areas[0].Groups[1].Value.Trim());

                                    if (areas.Count > 1)
                                    {
                                        lv1.SubItems.Add(areas[1].Groups[1].Value.Trim());
                                    }
                                    else
                                    {
                                        lv1.SubItems.Add("");
                                    }
                                    lv1.SubItems.Add(addr.Groups[1].Value.Trim());
                                    lv1.SubItems.Add(hangye.Groups[1].Value.Trim());
                                    lv1.SubItems.Add(lxr.Groups[1].Value.Trim());
                                    lv1.SubItems.Add(tell.Groups[1].Value.Trim());
                                    lv1.SubItems.Add(time.Groups[1].Value.Trim().Replace("<strong>","").Replace("</strong>",""));

                                }

                                else
                                {
                                    this.zanting = false;
                                    MessageBox.Show("请在浏览器打开的页面中完成验证码验证，然后回到软件界面点击继续采集按钮！");
                                    System.Diagnostics.Process.Start("https://suqian.58.com/yewu/33805288842284x.shtml");
                                }
                                  
                               


                                if (listView1.Items.Count - 1 > 1)
                                {
                                    listView1.EnsureVisible(listView1.Items.Count - 1);
                                }

                                while (this.zanting == false)
                                {
                                    Application.DoEvents();//如果loader是false表明正在加载,,则Application.DoEvents()意思就是处理其他消息。阻止当前的队列继续执行。
                                }
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(3000);   //内容获取间隔，可变量

                            }

                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                textBox1.Text = ex.ToString();
            }
        }









        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(zhaopin));
            thread.Start();
        }

        private void skinTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = e.Node.Name + ",";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.zanting = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.zanting = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            method.DataTableToExcel(method.listViewToDataTable(this.listView1), "Sheet1", true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://suqian.58.com/yewu/33805288842284x.shtml");
        }
    }
}

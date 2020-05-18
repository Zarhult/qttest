﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Client
{
    public partial class Form2 : Form
    {
        private TableLayoutPanel panel;
        private TabPage DataPage;
        public List<COVIDDataPoint> Result;
        public Form1 Parent;
        public NewDataPoint NewPoint;
        public UpdateDataPoint UpdatePoint;
        public ShowSpread SpreadVisualize;
        public List<String> X;
        public List<int> Y;
        public String GraphType;
        public Dictionary<List<COVIDDataPoint>, TableLayoutPanel> mapTable = new Dictionary<List<COVIDDataPoint>, TableLayoutPanel>();//J

        public Form2(Form1 ParentForm, List<COVIDDataPoint> data)//, Dictionary<List<COVIDDataPoint>, TableLayoutPanel> table)
        {
            InitializeComponent();
            Result = data;
            Parent = ParentForm;
            tabControl1.SelectTab("tabPage1");
            DataPage = tabControl1.SelectedTab;

            /*if (Form1.global.value.TryGetValue(data, out panel))
            {
                MessageBox.Show("Entered: key exists");
                
            }//J

            else
            {*/
                //MessageBox.Show("NOT Entered: key doesnt exist");
                //Initialize data view table
                panel = new TableLayoutPanel();
                panel.Location = new System.Drawing.Point(88, 100);
                panel.Name = "TableLayoutPanel1";
                panel.Size = new System.Drawing.Size(624, 279);
                panel.TabIndex = 0;
                panel.ColumnCount = 5;
                panel.RowCount = 1;
                panel.AutoScroll = true;

                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

                DataPage.Controls.Add(panel);
                panel.Controls.Add(new Label() { Text = "ID" }, 0, 0);
                panel.Controls.Add(new Label() { Text = "Date" }, 1, 0);
                panel.Controls.Add(new Label() { Text = "Country" }, 2, 0);
                panel.Controls.Add(new Label() { Text = "Sex" }, 3, 0);
                panel.Controls.Add(new Label() { Text = "Age" }, 4, 0);

                // Fill with data
                int row = 1;

                foreach (COVIDDataPoint point in data)
                {
                    panel.RowCount += 1;
                    panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

                    for (int i = 0; i <= 4; ++i)
                    {
                        switch (i)
                        {
                            case 0:
                                panel.Controls.Add(new Label() { Text = point.ID.ToString() }, i, row);
                                break;

                            case 1:
                                panel.Controls.Add(new Label() { Text = point.Date }, i, row);
                                break;

                            case 2:
                                panel.Controls.Add(new Label() { Text = point.Country }, i, row);
                                break;

                            case 3:
                                panel.Controls.Add(new Label() { Text = point.Sex }, i, row);
                                break;

                            case 4:
                                panel.Controls.Add(new Label() { Text = point.Age }, i, row);
                                break;
                        }
                    }
                    
                    ++row;
                }

              //  Form1.global.value.Add(data, panel);
                //MessageBox.Show("Added table");
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NewPoint = new NewDataPoint(this);
            NewPoint.Show();
        }

        public void CommunicateParent(string Msg)
        {
            Parent.SendMsg(Msg);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder csvExport = new StringBuilder();
            csvExport.AppendLine("ID,Date,Country,Sex,Age");
            string Line;
            foreach (COVIDDataPoint Point in Result)
            {
                Line = Point.ID.ToString() + "," + Point.Date + "," + Point.Country + "," + Point.Sex + "," + Point.Age;
                csvExport.AppendLine(Line);
            }
            string Path = "..\\..\\Export\\ExportedData.csv";
            if(File.Exists(Path))
            {
                File.Delete(Path);
            }
            File.AppendAllText(Path, csvExport.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool Found = false;
            string Data_ID = textBox1.Text;
            foreach (COVIDDataPoint point in Result)
            {
                if (point.ID.ToString() == Data_ID)
                    Found = true;
            }
            if(Found)
            {
                UpdatePoint = new UpdateDataPoint(this, Data_ID);
                UpdatePoint.Show();
            }
            else
            {
                MessageBox.Show("No Data Point Searched with that ID");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool Found = false;
            string Data_ID = textBox3.Text;
            foreach (COVIDDataPoint point in Result)
            {
                if (point.ID.ToString() == Data_ID)
                    Found = true;
            }
            if (Found)
            {
                string deleteMsg = "Delete Data with ID: " + Data_ID;
                CommunicateParent(deleteMsg);
            }
            else
            {
                MessageBox.Show("No Data Point Searched with that ID");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Result.Sort((x, y) => DateTime.Compare(DateTime.ParseExact(x.Date, "dd.MM.yyyy", null), DateTime.ParseExact(y.Date, "dd.MM.yyyy", null)));

            // Make a form popup to visualize spread using the sorted array
            SpreadVisualize = new ShowSpread(this, comboBox4.Text);
            SpreadVisualize.Show();
            SpreadVisualize.Visualize();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            double projection = Result.Count; //initialized with known number of cases 
            double numDays = 0;
            //check date
            if (dateCheck(textBox2.Text) && textBox2.Text != "" && futurecheck(textBox2.Text))
            {
                int begin = beginDate(Result); //earliest date
                int end = endDate(Result); //latest date
                double days = begin - end; // total number of days in Result data
                double rate = (Result.Count / days); //avg num of cases per day
                numDays = getFutureDays(textBox2.Text); //get number of days 
                projection = Math.Round(projection + (rate * numDays)); //get projection

                //display to the user
                textBox4.Text = "There are " + projection.ToString() + " projected cases on " + textBox2.Text +
                    " for the searched data. Note: the projection is just an estimate based on the data searched for and as such may not be accurate.";
            }
            else
            {
                MessageBox.Show("Invalid or no Date"); //error message
            }
        }
        public int endDate(List<COVIDDataPoint> data) //gets latest date in data; returns -1 if there is an error
        {
            int day = 1000;
            foreach (COVIDDataPoint point in data)
            {
                if (day > getDays(point.Date.ToString()))
                {
                    day = getDays(point.Date.ToString());
                }
            }
            if (day == 1000) //if for some reason could not get latest date 
            {
                return -1; //return error
            }

            return day;
        }

        public int beginDate(List<COVIDDataPoint> data) //gets earliest date in data; returns -1 if there is an error
        {
            int day = -1;
            foreach (COVIDDataPoint point in data)
            {
                if (day < getDays(point.Date.ToString()))
                {
                    day = getDays(point.Date.ToString());
                }
            }
            return day;
        }

        public bool dateCheck(String date)
        {
            if (date == "") return true;
            if (date.Length != 8) return false; //Make sure it is correct format
            if (date[0] > '1' || date[0] < '0') return false; //Tens place check
            if (date[0] == '0' && (date[1] > '9' || date[1] < '0')) return false; //Single digit dates
            if (date[0] == '1' && (date[1] > '2' || date[1] < '0')) return false; //Double digit dates
            if (date[2] != '/') return false; //Format
            if (date[3] > '3' || date[3] < '0') return false; //Tens place
            if (date[3] == '0' && (date[4] > '9' || date[4] < '0')) return false;
            if (date[3] == '1' && (date[4] > '9' || date[4] < '0')) return false;
            if (date[3] == '3' && (date[4] > '1' || date[4] < '0')) return false;
            if (date[5] != '/') return false;
            if (date[6] > '9' || date[6] < '0') return false;
            if ((date[7] > '9' || date[7] < '0')) return false;
            return true;
        }

        public bool futurecheck(String date) //check if date is after latest date in data set (Feb. 29, 2020)
        {
            if (date[6] < '2') return false;
            if (date[6] == '2' && date[7] == '0' && date[0] == '0' && date[1] < '3') return false;
            return true;
        }

        public int getDays(String date) //date in dataset where Jan 12, 2020 = 0; Jan 13, 2020 = 1; etc.
        {
            int num = 0;
            num = ((date[4] - '1') * 31) + ((date[0] - '1') * 10) + (date[1] - '2');
            return num;
        }

        public int getFutureDays(String date) //number of days from Feb 29, 2020 to given date
        {
            int num = 0;
            num = ((date[6] - '2') * 3650) + ((date[7] - '0') * 365) + ((date[3] - '0') * 10) + (date[4] - '0'); //add amount of days in year and day given

            if (date[0] == '0' && date[1] < '3') //calculate ammount of days in month given
            {
                num = num - (('3' - date[1]) * 30);
            }

            else if (date[0] == '1')
            {
                if (date[1] == '1')
                {
                    num = num + (9 * 30);
                }

                else if (date[1] == '2')
                {
                    num = num + (10 * 30);
                }

            }

            else
            {
                num = num + ((date[1] - '3') * 30);
            }

            return num;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            bool meanDiff = false;
            chart1.Series["Series1"].Points.Clear();
            GraphType = comboBox3.Text;
            int n;
            // X axis dataset
            List<String> Xaxis = new List<String>();
            List<int> Yaxis = new List<int>();
            if (comboBox1.Text == "Date")
            {
                foreach (COVIDDataPoint point in Result)
                {
                    if (point.Date != "" && point.Date != " ")
                        Xaxis.Add(point.Date);
                }
            }
            else if (comboBox1.Text == "Country")
            {
                foreach (COVIDDataPoint point in Result)
                {
                    if (point.Country != "" && point.Country != " ")
                        Xaxis.Add(point.Country);
                }
            }
            else if (comboBox1.Text == "Sex")
            {
                foreach (COVIDDataPoint point in Result)
                {
                    if (point.Sex != "" && point.Sex != " ")
                        Xaxis.Add(point.Sex);
                }
            }
            else if (comboBox1.Text == "Age")
            { 
                meanDiff = true;
                foreach (COVIDDataPoint point in Result)
                {
                    if (point.Age != "" && point.Age != " " && int.TryParse(point.Age, out n))
                        Xaxis.Add(point.Age);
                }
            }
            Yaxis = Yaxis_calculations(Xaxis);
            List<String> Unique = new List<String>();
            Unique = Xaxis.Distinct().ToList();
            for(int i = 0; i < Unique.Count; i++)
            {
                chart1.Series["Series1"].Points.AddXY(Unique[i], Yaxis[i]);
            }
            if (GraphType == "Line")
                chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            else if (GraphType == "Bar")
                chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            else if (GraphType == "ScatterPlot")
                chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            else if (GraphType == "Pie")
                chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            int max = 0;
            int pos = -1;
            int total = 0;
            for (int i = 0; i < Unique.Count; i++)
            {
                total += Yaxis[i];
                if(max < Yaxis[i])
                {
                    max = Yaxis[i];
                    pos = i;
                }
            }
            double mean = 0;
            textBox5.Text = Unique[pos];
            textBox6.Text = Xaxis[total / 2];
            if(meanDiff)
            {
                foreach(string age in Xaxis)
                {
                    mean += int.Parse(age);
                }
                mean = mean / Xaxis.Count;
                textBox7.Text = mean.ToString();
            }
            else
                textBox7.Text = Xaxis[total / 2];
        }

        List<int> Yaxis_calculations(List<String> Xaxis)
        {
            List<int> ret = new List<int>();
            int count = 0;
            List<String> Unique = new List<String>();
            Unique = Xaxis.Distinct().ToList();
            foreach(String element in Unique)
            {
                count = 0;
                foreach(String check in Xaxis)
                {
                    if (check == element)
                        count++;
                }
                ret.Add(count);
            }
            return ret;
        }
    }
}

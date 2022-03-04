using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using MetroFramework.Forms;
using System.Runtime.InteropServices;

namespace TaskManager4
{
    public partial class Form1 :  MetroForm
    {

        private float cpu;

        private float ram;

        private ulong installedMemory;

        public Form1()
        {
            InitializeComponent();
        }


        private List<Process> processes = null;

        private void GetProcesses()
        {
            processes.Clear();

            processes = Process.GetProcesses().ToList<Process>();
        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();

            double memSize = 0;

            foreach (Process p in processes)
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000);

                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                listView1.Items.Add(new ListViewItem(row));


                p.Close();
                p.Dispose();
            }

            Text = "Запущено процессов " + processes.Count.ToString();

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();

            RefreshProcessesList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();

            GetProcesses();

            RefreshProcessesList();

            toolStripComboBox1.SelectedIndex = 0;

            MEMORYSTATUSEX mEMORYSTATUSEX = new MEMORYSTATUSEX();

            if (GlobalMemoryStatusEx(mEMORYSTATUSEX))
            {
                installedMemory = mEMORYSTATUSEX.ullTotalPhys;
            }

            metroLabel10.Text = Convert.ToString(installedMemory / 1000000000) + " Гб";

            timer1.Interval = 1000;
            timer1.Start();
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
            string key = string.Empty;
            switch(toolStripComboBox1.SelectedItem.ToString())
            {
                case "Процессор":
                    key = "Win32_Processor";
                    break;

                case "Видеокарта":
                    key = "Win32_VideoController";
                    break;

                case "Сокет":
                    key = "Win32_IDEController";
                    break;

                case "Батарея":
                    key = "Win32_Battery";
                    break;

                case "Биос":
                    key = "Win32_BIOS";
                    break;

                case "Оперативная память":
                    key = "Win32_PhysicalMemory";
                    break;

                case "Кэш":
                    key = "Win32_CachMemory";
                    break;

                case "USB":
                    key = "Win32_USBController";
                    break;

                case "Диск":
                    key = "Win32_DiskDrive";
                    break;

                case "Логические диски":
                    key = "Win32_LogicalDisk";
                    break;

                case "Клавиатура":
                    key = "Win32_Keyboard";
                    break;

                case "Сеть":
                    key = "Win32_NetworkAdapter";
                    break;

                case "Пользователи":
                    key = "Win32_Account";
                    break;
            }
            GetHardWareInfo(key, listView2);
        }

        private void GetHardWareInfo(string key, ListView list)
        {
            list.Items.Clear();

            ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("SELECT * FROM " + key);

            try
            {
                foreach ( ManagementObject obj in searcher1.Get())
                {
                    ListViewGroup listViewGroup;

                    
                    
                        listViewGroup = list.Groups.Add(obj["Name"].ToString(), obj["Name"].ToString());
                    
                   

                    if(obj.Properties.Count == 0)
                    {
                        MessageBox.Show("Не удалось получить информацию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return; 
                    }

                    foreach(PropertyData data in obj.Properties)
                    {
                        ListViewItem item1 = new ListViewItem(listViewGroup);

                        if(list.Items.Count % 2 != 0)
                        {
                            item1.BackColor = Color.White;
                        }
                        else
                        {
                            item1.BackColor = Color.WhiteSmoke;
                        }
                        item1.Text = data.Name;

                        if (data.Value != null && !string.IsNullOrEmpty(data.Value.ToString()))
                        {
                            switch (data.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] stringData = data.Value as string[];

                                    string resStr1 = string.Empty;

                                    foreach (string s in stringData)
                                    {
                                        resStr1 += $"{s} ";
                                    }

                                    item1.SubItems.Add(resStr1);

                                    break;
                                case "System.UInt16[]":

                                    ushort[] ushortData = data.Value as ushort[];

                                    string resStr2 = string.Empty;

                                    foreach(ushort u in ushortData)
                                    {
                                        resStr2 += $"{Convert.ToString(u)}";

                                    }

                                    item1.SubItems.Add(resStr2);
                                    break;

                                default:
                                    item1.SubItems.Add(data.Value.ToString());
                                    break;
                            }

                            list.Items.Add(item1);

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 
            }

        }

      

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]//сама структура подписана аттрибутом [StructLayout]. Значение LayoutKind параметра Sequential используется для принудительного последовательного размещения членов в порядке их появления.

        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLength;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)] //Создания моста между управляемым кодом и неуправляемым
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] //Импортируем kernel32.dll
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private void timer1_Tick(object sender, EventArgs e)
        {
            cpu = performanceCPU.NextValue();
            ram = performanceRAM.NextValue();

            metroProgressBar1.Value = (int)cpu;
            metroProgressBar2.Value = (int)ram;

            metroLabel2.Text = Convert.ToString(Math.Round(cpu, 1)) + " %";
            metroLabel3.Text = Convert.ToString(Math.Round(ram, 1)) + " %";

            metroLabel8.Text = Convert.ToString(Math.Round((ram / 100 * installedMemory) / 1000000000, 1)) + " Гб";
            metroLabel9.Text = Convert.ToString(Math.Round((installedMemory - ram / 100 * installedMemory) / 1000000000, 1)) + " Гб";

            chart1.Series["ЦП"].Points.AddY(cpu);
            chart1.Series["ОЗУ"].Points.AddY(ram);

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void metroProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void metroLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}

using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmWcfRequest : Form
    {
        DynamicLibraryBase dl;
        SortedDictionary<string, string> streamfunction;
        public frmWcfRequest()
        {
            InitializeComponent();
        }

        private void frmWcfRequest_Load(object sender, EventArgs e)
        {
            comboBoxEQList.Items.Clear();
            InitialStreamFunction();
            foreach (KeyValuePair<string,string> sf in streamfunction)
            {
                comboBoxSFList.Items.Add(sf.Key);
            }

            comboBoxCarrierType.Items.Add("Full");
            comboBoxCarrierType.Items.Add("Empty");
        }
        private void comboBoxEQList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEQList.SelectedIndex == -1) return;
            string id = comboBoxEQList.SelectedItem.ToString();
            dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(id);
        }

        private void InitialStreamFunction()
        {
            streamfunction = new SortedDictionary<string, string>();
            streamfunction.Add("0-连线确认", "EAP_AliveCheckRequest");
            streamfunction.Add("0-询问模式", "EAP_EqpModeRequest");
            streamfunction.Add("1-请求Trace Data Spec", "EAP_EqpDefineInfo");
            streamfunction.Add("1-上料口请求上料", "EAP_LoadReuqest");
            streamfunction.Add("2-上料口上料完成", "EAP_LoadComplete");
            streamfunction.Add("3-下料口请求卸料", "EAP_UnloadRequest");
            streamfunction.Add("4-下料口卸料完成", "EAP_UnloadComplete");


        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxSFList.SelectedIndex == -1)
                {
                    MessageBox.Show("Stream Function Error!", "Error");
                    return;
                }

                if (comboBoxSFList.SelectedItem.ToString().Substring(0, 1) == "1")
                {
                    if (comboBoxEQList.SelectedIndex == -1)
                    {
                        MessageBox.Show("Equipment Error!", "Error");
                        return;
                    }
                }

                if (comboBoxSFList.SelectedItem.ToString().Substring(0, 1) == "2" || comboBoxSFList.SelectedItem.ToString().Substring(0, 1) == "3"
                    || comboBoxSFList.SelectedItem.ToString().Substring(0, 1) == "4")
                {
                    if (txtCarrierID.Text == string.Empty)
                    {
                        MessageBox.Show("Carrier Error!", "Error");
                        return;
                    }
                }
                string sf = comboBoxSFList.SelectedItem.ToString();
                EquipmentModel em = new EquipmentModel();
                if (sf.Substring(0, 1) != "0")
                {
                    string id = comboBoxEQList.SelectedItem.ToString();
                    if (id != string.Empty)
                    {
                        em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
                    }
                }
                switch (lblDescription.Text)
                {
                    case "EAP_AliveCheckRequest":
                        //new Environment.WCFReport().EAP_AliveCheckRequest(new AliveCheckRequest()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                        //    IPAddress = Environment.EAPEnvironment.commonLibrary.baseLib.wcfParaLibrary.LocalIP
                        //});
                        break;
                    case "EAP_EqpDefineInfo":
                        //new Environment.WCFReport().EAP_EqpDefineInfo(new EqpDefineInfo() { SubEqpID = em.EQName });
                        break;
                    case "EAP_EqpModeRequest":
                        //new Environment.WCFReport().EAP_EqpModeRequest(new EqpModeRequest() { MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName });
                        break;
                    case "EAP_LoadReuqest":
                        //new Environment.WCFReport().EAP_LoadReuqest(new LoadRequest()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                        //    SubEqpID = em.EQName,
                        //    PortID = "L01",
                        //    RequesType = comboBoxCarrierType.SelectedItem.ToString()
                        //});
                        break;
                    case "EAP_LoadComplete":
                        //new Environment.WCFReport().EAP_LoadComplete(new LoadComplete()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                        //    SubEqpID = em.EQName,
                        //    PortID = "L01",
                        //    CarriedID = txtCarrierID.Text.Trim()
                        //});
                        break;
                    case "EAP_UnloadRequest":
                        //new Environment.WCFReport().EAP_UnloadRequest(new UnloadRequest()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                        //    SubEqpID = em.EQName,
                        //    PortID = "U01",
                        //    CarriedID = txtCarrierID.Text.Trim(),
                        //    RequesType = comboBoxCarrierType.SelectedItem.ToString()
                        //});
                        break;
                    case "EAP_UnloadComplete":
                        //new Environment.WCFReport().EAP_UnloadComplete(new UnloadComplete()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                        //    SubEqpID = em.EQName,
                        //    PortID = "U01",
                        //    CarriedID = txtCarrierID.Text.Trim()
                        //});
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            { }
        }

        private void comboBoxSFList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSFList.SelectedIndex == -1) return;
            string id = comboBoxSFList.SelectedItem.ToString();
            lblDescription.Text = streamfunction[id];
            comboBoxCarrierType.Text = string.Empty;
            comboBoxEQList.Text = string.Empty;
            txtCarrierID.Text = string.Empty;
            if (id.Substring(0, 1) != "0")
            {
                comboBoxEQList.Items.Clear();
                foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    comboBoxEQList.Items.Add(em.EQName);
                }
                comboBoxEQList.Enabled = true;
                if (id.Substring(0, 1) == "1")
                {
                    txtCarrierID.Enabled = false;
                    comboBoxCarrierType.Enabled = true;
                }
                else if (id.Substring(0, 1) == "2")
                {
                    txtCarrierID.Enabled = true;
                    comboBoxCarrierType.Enabled = false;
                }
                else if (id.Substring(0, 1) == "3")
                {
                    txtCarrierID.Enabled = true;
                    comboBoxCarrierType.Enabled = true;
                }
                else if (id.Substring(0, 1) == "4")
                {
                    txtCarrierID.Enabled = true;
                    comboBoxCarrierType.Enabled = false;
                }
            }
            else
            {
                comboBoxEQList.Enabled = false;
                txtCarrierID.Enabled = false;
                comboBoxCarrierType.Enabled = false;
            }
        }

        private void frmCommonCommand_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}

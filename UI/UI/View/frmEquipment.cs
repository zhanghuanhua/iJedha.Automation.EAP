using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace iJedha.Automation.EAP.UI
{
    public partial class frmEquipment : Form
    {
        public frmEquipment()
        {
            InitializeComponent();
        }

        private void frmEquipment_Load(object sender, EventArgs e)
        {
            treeViewEQ.Nodes.Clear();
            TreeNode root = treeViewEQ.Nodes.Add("Equipment");
            foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                TreeNode node = root.Nodes.Add(em.EQID + " " + em.EQName);
                node.Tag = em.EQID;
                foreach (PortModel pm in em.List_Port.Values)
                {
                    TreeNode node1 = node.Nodes.Add(pm.PortID + " " + pm.Name.ToString());
                    node1.Tag = em.EQID + ":" + pm.PortID;
                }
            }
            root.ExpandAll();
        }

        private void treeViewEQ_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewEQ.SelectedNode == null)
            {
                return;
            }
            if (treeViewEQ.SelectedNode.Tag == null)
            {
                return;
            }
            string[] keys = treeViewEQ.SelectedNode.Tag.ToString().Split(':');
            if (keys.Count() == 1)
            {
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(keys[0]);
                PPTEQInfo.SelectedObjectsChanged += new EventHandler(PPTEQInfo_SelectedObjectsChanged);
                PPTEQInfo.SelectedObject = em;
            }
            if (keys.Count() == 2)
            {
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(keys[0]);
                PortModel pm = em.GetPortModelByPortID(keys[1]);
                PPTEQInfo.SelectedObjectsChanged += new EventHandler(PPTEQInfo_SelectedObjectsChanged);
                PPTEQInfo.SelectedObject = pm;
            }

        }

        private void frmEquipment_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        void PPTEQInfo_SelectedObjectsChanged(object sender, EventArgs e)
        {
            PPTEQInfo.Tag = PPTEQInfo.PropertySort;

            PPTEQInfo.PropertySort = PropertySort.CategorizedAlphabetical;

            PPTEQInfo.Paint += new PaintEventHandler(PPTEQInfo_Paint);
        }

        void PPTEQInfo_Paint(object sender, PaintEventArgs e)
        {

            var categorysinfo = PPTEQInfo.SelectedObject.GetType().GetField("categorys", BindingFlags.NonPublic | BindingFlags.Instance);

            if (categorysinfo != null)
            {
                var categorys = categorysinfo.GetValue(PPTEQInfo.SelectedObject) as List<String>;

                PPTEQInfo.CollapseAllGridItems();

                GridItemCollection currentPropEntries = PPTEQInfo.GetType().GetField("currentPropEntries", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PPTEQInfo) as GridItemCollection;
                var newarray = currentPropEntries.Cast<GridItem>().OrderBy((t) => categorys.IndexOf(t.Label)).ToArray();
                currentPropEntries.GetType().GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(currentPropEntries, newarray);
                PPTEQInfo.ExpandAllGridItems();
                PPTEQInfo.PropertySort = (PropertySort)PPTEQInfo.Tag;
            }
            PPTEQInfo.Paint -= new PaintEventHandler(PPTEQInfo_Paint);
        }

        private void PPTEQInfo_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string[] keys = treeViewEQ.SelectedNode.Tag.ToString().Split(':');
            if (keys.Count() == 1)
            {
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(treeViewEQ.SelectedNode.Tag.ToString());
                Environment.EAPEnvironment.commonLibrary.UpdateEquipmentLibrary(em);
            }
        }

        private void PPTEQInfo_Click(object sender, EventArgs e)
        {

        }
    }
}

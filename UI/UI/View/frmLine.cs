using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace iJedha.Automation.EAP.UI
{
    public partial class frmLine : Form
    {
        public frmLine()
        {
            InitializeComponent();
        }

        private void frmLine_Load(object sender, EventArgs e)
        {
            treeViewLine.Nodes.Clear();
            TreeNode root = treeViewLine.Nodes.Add("线体");
            TreeNode node = root.Nodes.Add(Environment.EAPEnvironment.commonLibrary.LineName);
            node.Tag = EAP.Environment.EAPEnvironment.commonLibrary.LineName;
            root.ExpandAll();
        }

        private void treeViewLine_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewLine.SelectedNode == null)
            {
                return;
            }
            if (treeViewLine.SelectedNode.Tag == null)
            {
                return;
            }
            string[] keys = treeViewLine.SelectedNode.Tag.ToString().Split(':');
            if (keys.Count() == 1)
            {
                LineModel lineMd = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetLineModelByID(EAP.Environment.EAPEnvironment.commonLibrary.LineName);
                PPTLineInfo.SelectedObjectsChanged += new EventHandler(PPTLineInfo_SelectedObjectsChanged);
                PPTLineInfo.SelectedObject = lineMd;
            }
        }

        private void frmLine_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        void PPTLineInfo_SelectedObjectsChanged(object sender, EventArgs e)
        {
            PPTLineInfo.Tag = PPTLineInfo.PropertySort;

            PPTLineInfo.PropertySort = PropertySort.CategorizedAlphabetical;

            PPTLineInfo.Paint += new PaintEventHandler(PPTLineInfo_Paint);
        }

        void PPTLineInfo_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                var categorysinfo = PPTLineInfo.SelectedObject.GetType().GetField("categorys", BindingFlags.NonPublic | BindingFlags.Instance);

                if (categorysinfo != null)
                {
                    var categorys = categorysinfo.GetValue(PPTLineInfo.SelectedObject) as List<String>;

                    PPTLineInfo.CollapseAllGridItems();

                    GridItemCollection currentPropEntries = PPTLineInfo.GetType().GetField("currentPropEntries", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PPTLineInfo) as GridItemCollection;
                    var newarray = currentPropEntries.Cast<GridItem>().OrderBy((t) => categorys.IndexOf(t.Label)).ToArray();
                    currentPropEntries.GetType().GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(currentPropEntries, newarray);
                    PPTLineInfo.ExpandAllGridItems();
                    PPTLineInfo.PropertySort = (PropertySort)PPTLineInfo.Tag;
                }
                PPTLineInfo.Paint -= new PaintEventHandler(PPTLineInfo_Paint);
            }
            catch (Exception ex)
            {

            }

        }

        private void PPTEQInfo_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string[] keys = treeViewLine.SelectedNode.Tag.ToString().Split(':');
            if (keys.Count() == 1)
            {
                LineModel lineModel = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetLineModelByID(EAP.Environment.EAPEnvironment.commonLibrary.LineName);
                Environment.EAPEnvironment.commonLibrary.UpdateEquipmentLibrary_Line(lineModel);
            }
        }

        private void PPTEQInfo_Click(object sender, EventArgs e)
        {

        }
    }
}

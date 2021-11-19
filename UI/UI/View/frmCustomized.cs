using iJedha.Automation.EAP.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace iJedha.Automation.EAP.UI
{
    public partial class frmCustomized : Form
    {
        public frmCustomized()
        {
            InitializeComponent();
        }

        private void frmCustomized_Load(object sender, EventArgs e)
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
                CustomizedLibrary customized = Environment.EAPEnvironment.commonLibrary.customizedLibrary;
                PPTCustomizedInfo.SelectedObjectsChanged += new EventHandler(PPTLineInfo_SelectedObjectsChanged);
                PPTCustomizedInfo.SelectedObject = customized;
            }
        }

        private void frmCustomized_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        void PPTLineInfo_SelectedObjectsChanged(object sender, EventArgs e)
        {
            PPTCustomizedInfo.Tag = PPTCustomizedInfo.PropertySort;

            PPTCustomizedInfo.PropertySort = PropertySort.CategorizedAlphabetical;

            PPTCustomizedInfo.Paint += new PaintEventHandler(PPTCustomizedInfo_Paint);
        }

        void PPTCustomizedInfo_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                var categorysinfo = PPTCustomizedInfo.SelectedObject.GetType().GetField("categorys", BindingFlags.NonPublic | BindingFlags.Instance);

                if (categorysinfo != null)
                {
                    var categorys = categorysinfo.GetValue(PPTCustomizedInfo.SelectedObject) as List<String>;

                    PPTCustomizedInfo.CollapseAllGridItems();

                    GridItemCollection currentPropEntries = PPTCustomizedInfo.GetType().GetField("currentPropEntries", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PPTCustomizedInfo) as GridItemCollection;
                    var newarray = currentPropEntries.Cast<GridItem>().OrderBy((t) => categorys.IndexOf(t.Label)).ToArray();
                    currentPropEntries.GetType().GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(currentPropEntries, newarray);
                    PPTCustomizedInfo.ExpandAllGridItems();
                    PPTCustomizedInfo.PropertySort = (PropertySort)PPTCustomizedInfo.Tag;
                }
                PPTCustomizedInfo.Paint -= new PaintEventHandler(PPTCustomizedInfo_Paint);
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
                CustomizedLibrary customized = Environment.EAPEnvironment.commonLibrary.customizedLibrary;
                Environment.EAPEnvironment.commonLibrary.UpdateCustomizedLibrary(customized);
            }
        }

      
    }
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Sap.Data.Hana;
using Sap.Data;
using SapHanaAddIn;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Core.CIM;
using Microsoft.Win32;
using System.Collections.Generic;

namespace HANATableViewer
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>

    public class Form1 : System.Windows.Forms.Form
    {
        public System.Windows.Forms.Label label2;
        public TextBox txtSQLStatement;
        public System.Windows.Forms.TextBox txtUserID;
        public System.Windows.Forms.Button btnExecute;
        public System.Windows.Forms.DataGrid dgResults;
        public System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.ComboBox comboBoxTables;
        public ComboBox cboQuickPicks;
        private Label label1;
        private Label label3;
        private Label label4;
        public ComboBox comboBoxSchemas;
        private Button btnAdd;
        private string spatialCol = "";
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            try
            {
                if (Globals.hanaConn == null || Globals.hanaConn.State == System.Data.ConnectionState.Closed)
                {
                    MessageBox.Show("Select an environment first.", "No environment is selected.");
                    return;
                }
                else
                {

                    //HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables", Globals.hanaConn);
                    HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                    
                   HanaDataReader dr = cmd.ExecuteReader();
                    comboBoxSchemas.Items.Clear();
                    while (dr.Read())
                    {
                        comboBoxSchemas.Items.Add(dr.GetString(0));
                    }
                    dr.Close();
                }
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                 ex.Errors[0].NativeError.ToString() + ")",
                 "Failed to initialize table drop down.");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (Globals.hanaConn != null && Globals.hanaConn.State == ConnectionState.Open)
            {
                Globals.hanaConn.Close();
            }
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            FrameworkApplication.State.Deactivate("condition_state_isconnected");
            IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
            if (wrapper2 != null)
            {
                wrapper2.Caption = "Connect";
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.txtSQLStatement = new System.Windows.Forms.TextBox();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.dgResults = new System.Windows.Forms.DataGrid();
            this.comboBoxTables = new System.Windows.Forms.ComboBox();
            this.cboQuickPicks = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxSchemas = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgResults)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(5, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "SQL Statement:";
            // 
            // txtSQLStatement
            // 
            this.txtSQLStatement.AcceptsReturn = true;
            this.txtSQLStatement.AcceptsTab = true;
            this.txtSQLStatement.Location = new System.Drawing.Point(7, 81);
            this.txtSQLStatement.Multiline = true;
            this.txtSQLStatement.Name = "txtSQLStatement";
            this.txtSQLStatement.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSQLStatement.Size = new System.Drawing.Size(684, 68);
            this.txtSQLStatement.TabIndex = 2;
            this.txtSQLStatement.Text = "SELECT * FROM ";
            this.txtSQLStatement.TextChanged += new System.EventHandler(this.txtSQLStatement_TextChanged);
            this.txtSQLStatement.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSQLStatement_KeyUp);
            // 
            // txtUserID
            // 
            this.txtUserID.Location = new System.Drawing.Point(0, 0);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(100, 20);
            this.txtUserID.TabIndex = 0;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(526, 155);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 4;
            this.btnExecute.Text = "&Execute";
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(607, 155);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "C&lose";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // dgResults
            // 
            this.dgResults.CaptionText = "Results";
            this.dgResults.DataMember = "";
            this.dgResults.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgResults.Location = new System.Drawing.Point(8, 184);
            this.dgResults.Name = "dgResults";
            this.dgResults.ReadOnly = true;
            this.dgResults.Size = new System.Drawing.Size(684, 320);
            this.dgResults.TabIndex = 6;
            // 
            // comboBoxTables
            // 
            this.comboBoxTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTables.Location = new System.Drawing.Point(474, 54);
            this.comboBoxTables.Name = "comboBoxTables";
            this.comboBoxTables.Size = new System.Drawing.Size(217, 21);
            this.comboBoxTables.Sorted = true;
            this.comboBoxTables.TabIndex = 1;
            this.comboBoxTables.SelectedIndexChanged += new System.EventHandler(this.comboBoxTables_SelectedIndexChanged);
            // 
            // cboQuickPicks
            // 
            this.cboQuickPicks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboQuickPicks.Location = new System.Drawing.Point(393, 155);
            this.cboQuickPicks.Name = "cboQuickPicks";
            this.cboQuickPicks.Size = new System.Drawing.Size(112, 21);
            this.cboQuickPicks.Sorted = true;
            this.cboQuickPicks.TabIndex = 3;
            this.cboQuickPicks.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(328, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Quick Pick:";
            this.label1.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(426, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Tables:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(218, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Schemas:";
            // 
            // comboBoxSchemas
            // 
            this.comboBoxSchemas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSchemas.Location = new System.Drawing.Point(278, 54);
            this.comboBoxSchemas.Name = "comboBoxSchemas";
            this.comboBoxSchemas.Size = new System.Drawing.Size(142, 21);
            this.comboBoxSchemas.Sorted = true;
            this.comboBoxSchemas.TabIndex = 11;
            this.comboBoxSchemas.SelectedIndexChanged += new System.EventHandler(this.comboBoxSchemas_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(13, 155);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(90, 23);
            this.btnAdd.TabIndex = 12;
            this.btnAdd.Text = "Add to TOC";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.btnExecute;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(703, 510);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.comboBoxSchemas);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboQuickPicks);
            this.Controls.Add(this.comboBoxTables);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgResults);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.txtSQLStatement);
            this.Controls.Add(this.label2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SAP HANA Data  Explorer";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.Run(new Form1());
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void btnExecute_Click(object sender, System.EventArgs e)
        {

            if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
            {
                MessageBox.Show("Connect to a database first.", "Not connected");
                return;
            }
            if (txtSQLStatement.Text.Trim().Length < 1)
            {
                MessageBox.Show("Please enter the command text.", "Empty command text");
                txtSQLStatement.SelectAll();
                txtSQLStatement.Focus();
                return;
            }

            string qtest = qtest = txtSQLStatement.Text; 
            if (spatialCol != "")
            {
                if(txtSQLStatement.Text.IndexOf(spatialCol) >0)
                {
                    qtest = txtSQLStatement.Text.Replace(spatialCol, spatialCol + ".st_aswkt()");
                }
                else
                {
                    qtest = txtSQLStatement.Text;
                }
            }
            HanaCommand cmd = new HanaCommand(qtest, Globals.hanaConn);

            HanaDataReader dr = null;
            try
            {
                
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                   dgResults.DataSource = null;

                   
                   ////cmd.CommandText = "select COLUMN_NAME from TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id != 29812";


                   ////HanaParameter parm1 = new HanaParameter();
                   //cmd.Prepare();

                   
                   //string colls = "";
                   //char[] charsToTrim = { ',', ' ' };
                   //if (dr != null)
                   //{
                   //    while (dr.Read())
                   //    {
                   //        colls = colls + dr.GetString(0) + ",";
                   //    }

                   //}
                   //colls = colls.TrimEnd(charsToTrim);
                   //dr.Close();




                   //cmd.CommandText = "SELECT " + colls + " FROM \"" + comboBoxSchemas.SelectedItem.ToString() + "\".\"" + comboBoxTables.SelectedItem.ToString() + "\"";
                   dr = cmd.ExecuteReader();
                   dgResults.DataSource = null;
                   dgResults.Refresh();
                   if (dr != null)
                   {
                       dgResults.DataSource = dr;
                       dr.Close();
                   }

                Mouse.OverrideCursor = null;
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                ex.Errors[0].NativeError.ToString() + ")",
                "Failed to execute SQL statement");
                if (dr != null)
                {
                    dr.Close();
                }
                
                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;
            txtSQLStatement.SelectAll();
            txtSQLStatement.Focus();
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            Globals.hanaConn.Close();
            this.Close();
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            //cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id != 29812";
            //HanaCommand cmd = new HanaCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id != 29812", Globals.hanaConn);
            HanaCommand cmd = new HanaCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "'", Globals.hanaConn);
            HanaDataReader dr = cmd.ExecuteReader();
            List<string> colls = new List<string>() ;
            while (dr.Read())
            {
                colls.Add(dr.GetString(0));

            }
            dr.Close();
            txtSQLStatement.Text = "SELECT TOP 1000 " + string.Join(", ",colls.ToArray() )+ " FROM \"" + comboBoxSchemas.SelectedItem.ToString()+ "\".\"" + comboBoxTables.SelectedItem.ToString() + "\"";
            spatialCol = "";
            cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id = 29812";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                spatialCol = dr.GetString(0);

            }
            dr.Close();
        }

        private void comboBoxSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables where schema_name = '" + comboBoxSchemas.SelectedItem.ToString() +"'", Globals.hanaConn);
            //HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
            //HanaCommand cmd = new HanaCommand(txtSQLStatement.Text, Globals.hanaConn);
            HanaDataReader dr = cmd.ExecuteReader();
            comboBoxTables.Items.Clear();
            while (dr.Read())
            {
                //comboBoxSchemas.Items.Add(dr.GetString(0) + '.' + dr.GetString(1));
                comboBoxTables.Items.Add(dr.GetString(1));
            }
            dr.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
             



            if (MapView.Active != null)
            {
                Task sssss = OpenEnterpriseGeodatabase();
                return;
            }
            else
            {
                MessageBox.Show("Please activate a map");
            }
            
        }
        public async Task OpenEnterpriseGeodatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {
                // Opening a Non-Versioned SQL Server instance.
                RegistryKey reg = (Registry.LocalMachine).OpenSubKey("Software");
                reg = reg.OpenSubKey("ODBC");
                reg = reg.OpenSubKey("ODBC.INI");
                reg = reg.OpenSubKey("ODBC Data Sources");
                string instance = "";
                foreach (string item in reg.GetValueNames())
                {
                    instance = item;
                    string vvv = reg.GetValue(item).ToString();
                }

                IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("cboEnv");

                //ArcGIS.Desktop.Framework.Contracts.ComboBox ddd = (ArcGIS.Desktop.Framework.Contracts.ComboBox)wrapper;
                ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;
                // = ddd.Text;
                ConnectionItem connitem = item2.Icon as ConnectionItem;
                string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                {
                    AuthenticationMode = AuthenticationMode.DBMS,

                    // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
                    Instance = cboEnv.cboBox.Text, //@"sapqe2hana",

                    // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
                    //Database = "LocalGovernment",

                    // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                    User = connitem.userid,//  "jluostarinen",
                    Password = tst2,
                    Version = "dbo.DEFAULT"
                };

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                {
                    Connector pCon = geodatabase.GetConnector();
                    pCon.ToString();
                    // Use the geodatabase
                }
                using (Database db = new Database(connectionProperties))
                {
                    string spatialquery = "";
                    //"Select * from 'JLUOSTARINEN'.'Points'", "MySelect"
                    //QueryDescription qds = db.GetQueryDescription("Select * from \"JLUOSTARINEN\".\"TESTCREATEFC\"", "MySelect");
                    if (spatialCol != "")
                    {
                        spatialquery = txtSQLStatement.Text.Insert(txtSQLStatement.Text.IndexOf("SELECT") + 7, spatialCol + ", ");
                    }
                    else
                    {
                        spatialquery = txtSQLStatement.Text;
                    }
                    QueryDescription qds = db.GetQueryDescription(txtSQLStatement.Text, "MySelect");// " select GEF_OBJECTID, GEF_OBJKEY, OBJNR, ERNAM, ERDAT, KTEXT, IDAT2, AENAM, ARTPR, GEF_SHAPE from JLUOSTARINEN.ORDERSWLINE", "MySelect");

                    //string workspaceConnectionString = db.GetConnectionString();
                    //dataConnection.WorkspaceConnectionString = workspaceConnectionString;
                    //dataConnection.WorkspaceFactory = WorkspaceFactory.OLEDB;
                    //dataConnection.DatasetType = esriDatasetType.esriDTFeatureClass;
                    //dataConnection.Dataset = "JLUOSTARINEN.ORDERSWLINE";
                    if (qds.IsSpatialQuery())
                    {
                        qds.SetObjectIDFields("OBJECTID");
                        Table pTab = db.OpenTable(qds);

                        var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                        var connt = new CIMWMTSServiceConnection
                        {
                            ServerConnection = serverConnection,
                        };
                        CIMStandardDataConnection dataConnection = new CIMStandardDataConnection();
                        CIMWMTSServiceConnection sd = new CIMWMTSServiceConnection();

                        FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, 0);
                        //MapView.Active.RedrawAsync(true);
                        pFL.Select(null, SelectionCombinationMethod.New);
                        MapView.Active.ZoomToSelected();
                        pFL.ClearSelection();
                        pFL.QueryExtent();
                    }
                    else
                    {
                        //Table ptab = (Table )LayerFactory.Instance.
                    }
                    
                   
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void txtSQLStatement_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txtSQLStatement_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}

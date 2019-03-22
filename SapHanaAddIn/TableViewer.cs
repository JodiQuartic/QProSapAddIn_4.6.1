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

namespace HANATableViewer
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>

    public class Form1 : System.Windows.Forms.Form
    {
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtSQLStatement;
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
            this.txtSQLStatement.AcceptsTab = true;
            this.txtSQLStatement.Location = new System.Drawing.Point(7, 81);
            this.txtSQLStatement.Multiline = true;
            this.txtSQLStatement.Name = "txtSQLStatement";
            this.txtSQLStatement.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSQLStatement.Size = new System.Drawing.Size(684, 68);
            this.txtSQLStatement.TabIndex = 2;
            this.txtSQLStatement.Text = "SELECT * FROM ";
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
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(328, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Quick Pick:";
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
            // Form1
            // 
            this.AcceptButton = this.btnExecute;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(703, 510);
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
            this.Text = "HANA Data  Explorer";
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

            try
            {
                dgResults.DataSource = null;

                HanaCommand cmd = new HanaCommand(txtSQLStatement.Text.Trim(), Globals.hanaConn);
                cmd.CommandText = "select COLUMN_NAME from TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id != 29812";
                
                
                //HanaParameter parm1 = new HanaParameter();
                cmd.Prepare();
                
                HanaDataReader dr = cmd.ExecuteReader();
                string colls = "";
                char[] charsToTrim = { ',', ' ' };
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        colls = colls + dr.GetString(0) + ",";
                    }
                    
                }
                colls = colls.TrimEnd(charsToTrim);
                dr.Close();




                cmd.CommandText = "SELECT " + colls + " FROM \"" + comboBoxSchemas.SelectedItem.ToString() + "\".\"" + comboBoxTables.SelectedItem.ToString() +"\"";
                dr = cmd.ExecuteReader();
                dgResults.DataSource = null;
                dgResults.Refresh();
                if (dr != null)
                {
                    dgResults.DataSource = dr;
                    dr.Close();
                }
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                ex.Errors[0].NativeError.ToString() + ")",
                "Failed to execute SQL statement");
            }

            txtSQLStatement.SelectAll();
            txtSQLStatement.Focus();
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSQLStatement.Text = "SELECT * FROM " + comboBoxTables.SelectedItem.ToString();
        }

        private void comboBoxSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables where schema_name = '" + comboBoxSchemas.SelectedItem.ToString() +"'", Globals.hanaConn);
            //HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);

            HanaDataReader dr = cmd.ExecuteReader();
            comboBoxTables.Items.Clear();
            while (dr.Read())
            {
                //comboBoxSchemas.Items.Add(dr.GetString(0) + '.' + dr.GetString(1));
                comboBoxTables.Items.Add(dr.GetString(1));
            }
            dr.Close();
        }
    }
}

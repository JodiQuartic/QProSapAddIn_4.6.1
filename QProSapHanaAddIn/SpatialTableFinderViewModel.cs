using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Data;
using System.ComponentModel;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Oracle.ManagedDataAccess.Client;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Geometry;
using System.Threading;

namespace QProSapAddIn
{
    internal class SpatialTableFinderViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID2 = "QProSapAddIn_SpatialTableFinder";

        //locks to update collections to avoid thread collisions
        private Object _aLock = new Object();

        protected SpatialTableFinderViewModel()
        {
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID2);
            if (pane == null)
            {
                return;
            }
            else
            {
                string dateInString = "02.01.2020";

                DateTime startDate = DateTime.Parse(dateInString);
                DateTime expiryDate = startDate.AddDays(29);
                if (DateTime.Now > expiryDate)
                {
                    MessageBox.Show("This Trial has expired.  Thank you for trying Q Pro for SAP. " + Environment.NewLine + Environment.NewLine + "We would appreciate any feedback you may have.  Please report bugs and/or enhancements to admin@quarticsolutions.com ." + Environment.NewLine + Environment.NewLine + "You can uninstall this addin by using ArcGIS Pro's AddIn Manager." + Environment.NewLine + Environment.NewLine + "If you are interested in purchasing Q Pro for SAP please contact Quartic Solutions at info@quarticsolutions.com or visit our website www.quarticsolutions.com");
                }
                else
                {
                    pane.Activate();
                    
                }

            }
        }

        protected override Task InitializeAsync()
        {
            // Initialize instance...
            return Task.FromResult(0);
        }
        protected override Task UninitializeAsync()
        {
            // Uninitialize instance...
            return Task.FromResult(0);
        }

        #region Tasks
        public async Task ExecuteSqlCallback2()
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    //string queryStr = QueryTxt.SelectString;
                    string queryStr = "select * from sde.st_GEOMETRY_COLUMNS ORDER BY TABLE_NAME ";

                    if (Globals.DBConn.State != ConnectionState.Open)
                        Globals.DBConn.Open();
                    if (Globals.DBConn == null || Globals.DBConn.State != ConnectionState.Open)
                    {
                        MessageBox.Show("There is no open database connection. Please connect to database first. ");
                        lock (_aLock)
                        {
                            BtnMapVis = false;
                        }
                        return;
                    }

                    DataTable dt = new DataTable();
                    int cnt = 0;
                    if (Globals.RdbmsType == "Hana")
                    {
                      
                    }
                    if (Globals.RdbmsType == "Oracle")
                    {
                        DataSet dataSet = new DataSet();
                        OracleCommand cmd = new OracleCommand(queryStr, Globals.DBConn);
                        using (OracleDataAdapter dataAdapter = new OracleDataAdapter())
                        {
                            dataAdapter.SelectCommand = cmd;
                            dataAdapter.Fill(dataSet);
                            dt = dataSet.Tables[0];
                            cnt = dt.Rows.Count;

                        }
                        if (cnt == 0)
                        {
                            lock (_aLock)
                            {
                                BtnMapVis = false;
                            }

                            MessageBox.Show("There are no rows in the selected table. ");

                            return;
                        }
                        else
                        {
                            lock (_aLock)
                            {
                                Results = dt.DefaultView;
                            }
                            lock (_aLock)
                            {
                                BtnMapVis = true;
                            }
                        }

                        if (Globals.DBConn.State != ConnectionState.Closed)
                            Globals.DBConn.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("The sql statement was not able to execute. " + ex.Message.ToString() + " Please review the sql statement.");
                lock (_aLock)
                {
                    BtnMapVis = false;
                }
                if (Globals.DBConn.State != ConnectionState.Closed)
                    Globals.DBConn.Close();
            }
        }
        public async Task AddToTOCCallback2(CancelableProgressorSource cpd)
        {
            try
            {
                await QueuedTask.Run(() =>
                {

                    if (MapView.Active == null)
                    {
                        MessageBox.Show("There is no active map view.  Please select a map and try again.");

                        return;
                    }

                    _currentSchema = _selrow[0].ToString();
                    _currentTable = _selrow[1].ToString();
                    _spatialCol.SelectString = _selrow[2].ToString();

                    //==================had to break code up more with using statements cause kept having thread probs

                    //create table name for the layer
                    Random r = new Random();
                    int n = r.Next();
                    string s = n.ToString();
                    s = s.Substring(s.Length - 4);
                    string lyrname = _currentTable + "_" + s;

                    //===========database values
                    //find spatial column - need to refresh incase the user has overwritten or manually typed in sql
                    if (Globals.DBConn.State != ConnectionState.Open)
                        Globals.DBConn.Open();
                    if (Globals.DBConn == null || Globals.DBConn.State != ConnectionState.Open)
                    {
                        MessageBox.Show("There is no open database connection. Please connect to database first. ");
                        lock (_aLock)
                        {
                            BtnMapVis = false;
                        }
                        return;
                    }
                    //OracleCommand cmd = new OracleCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + _currentTable + "' and data_type_id = 29812", Globals.DBConn);
                    OracleCommand cmd = new OracleCommand("select COLUMN_NAME from SYS.ALL_TAB_COLUMNS where OWNER like '" + _currentSchema + "'" + " and TABLE_NAME = " + "'" + _currentTable + "'" + " AND DATA_TYPE= " + "'" + "ST_GEOMETRY" + "'", Globals.DBConn);
                    OracleDataReader dr = cmd.ExecuteReader();
                    SpatialCol.SelectString = "";
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            lock (_aLock)
                            {
                                _spatialCol.SelectString = dr.GetString(0);
                            }
                        }
                    }
                    else
                    {
                        lock (_aLock)
                        {
                            _spatialCol.SelectString = "none";
                        }
                    }
                    dr.Close();

                    //find objectid
                    ObjidCol.SelectString = "";
                    cmd.CommandText = "select COLUMN_NAME from SYS.ALL_TAB_COLUMNS where OWNER like '" + _currentSchema + "' and table_name = '" + _currentTable + "' and COLUMN_NAME = 'OBJECTID'";
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                            lock (_aLock)
                            {
                                _objidCol.SelectString = dr.GetString(0);
                            }
                    }
                    else
                    {
                        _objidCol.SelectString = "";
                    }
                    dr.Close();
                    if (Globals.DBConn.State != ConnectionState.Closed)
                        Globals.DBConn.Close();

                    //open an Esri database connection type to sap
                    ComboBoxItem item2 = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    string serverport = connitem.server.ToString();
                    //for hana string inst = serverport.Substring(0, serverport.IndexOf(":"));
                    string inst = serverport;

                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Oracle)
                    {
                        AuthenticationMode = AuthenticationMode.DBMS,
                        Instance = inst,  //cboEnv.cboBox.Text, //@"sapqe2hana",
                        User = connitem.userid,
                        Password = tst2,
                        Version = "sde.DEFAULT" //hana "dbo.DEFAULT"
                    };

                    //variables needed from qds first
                    string db_sc = SpatialCol.SelectString;
                    string db_oc = ObjidCol.SelectString;

                    string qtest = _querytext.SelectString.Trim();
                    string oflds = "";
                    IReadOnlyList<Field> lst;
                    string esri_sc = "";
                    string esri_oc = "";
                    GeometryType st = GeometryType.Unknown;
                    SpatialReference sr = null;
                    string sri = "";

                    using (Database db = new Database(connectionProperties))
                    {
                        if (qtest.Length < 1)
                        {
                            _strMessage.SelectString = "Empty Sql.";
                            BtnMapVis = false;
                            return;
                        }
                        else
                        {
                            QueryDescription qds = db.GetQueryDescription(qtest, lyrname);
                            lst = qds.GetFields();
                            oflds = qds.GetObjectIDFields();
                            esri_sc = qds.GetShapeColumnName();
                            esri_oc = qds.GetObjectIDColumnName();
                            st = qds.GetShapeType();
                            sr = qds.GetSpatialReference();
                            sri = qds.GetSRID();
                        }
                    }

                    if (lst == null)   //table has no fields
                        return;

                    //now do work with variables from qds
                    int objfieldcount = oflds.Split(',').Length - 1;
                    string objForSet = "";
                    string fakeObjSql = "";
                    bool hasShape = false;
                    //bool hasSlashes = false;

                    //==============determine field to use for objectid - pro requires a pk - can't assume the field name OBJECTID in  is a valid field type to use for objectid according to Esri...

                    if (objfieldcount == 0)
                    {
                        if (db_oc != "")  //does db think there is a pk?
                        {
                            _strMessage.SelectString = "Tried to create key.";
                            ObjidCol.SelectString = db_oc;
                            objForSet = db_oc;
                        }
                        else   //it is possible (bug? that esri still can determine the objid eventhough we have already asked w objfieldcount method
                        {
                            _strMessage.SelectString = "Tried to create key.";
                            ObjidCol.SelectString = esri_oc;
                            objForSet = esri_oc;
                        }

                        //if (lst != null && lst.Count > 0)
                        //{
                        //    string oc = lst[0].Name;  //use this as a dummy field to put the partition statement in (as long as it is not named "OBJECTID" esri crashes)

                        //    fakeObjSql = ", row_number()" + " over(partition by " + oc + ")" + " as QID ";
                        //    objForSet = "QID";
                        //    ObjidCol.SelectString = "QID";
                        //    _strMessage.SelectString = "Tried to create key.";
                        //    cpd.Message = "Tried to create key.";
                        //}
                    }
                    if (objfieldcount == 1)
                    {
                        esri_oc = oflds.ToString().Split(',')[0];
                        objForSet = esri_oc;              // prepare for SetObjectidfeilds
                        ObjidCol.SelectString = objForSet;  //updateprop
                        cpd.Message = "Found valid key field.";
                    }
                    if (objfieldcount > 1)
                    {
                        //fix backslash slash bug
                        //string fldLst = "";
                        //int cnt = 0;

                        //foreach (Field fld in lst)
                        //{
                        //    //fix bug with slash in column namestring
                        //    if (fld.Name.IndexOfAny(new char[] { ' ', '/', '\\' }) != -1)
                        //    {
                        //        var f = '"' + fld.Name + '"';
                        //        if (cnt == 0)
                        //        {
                        //            fldLst = fldLst + f;  //first field don't put a comma
                        //        }
                        //        else
                        //        {
                        //            fldLst = fldLst + "," + f;
                        //        }
                        //        hasSlashes = true;
                        //    }
                        //    else
                        //    {
                        //        if (cnt == 0)
                        //        {
                        //            fldLst = fldLst + fld.Name;
                        //        }
                        //        else
                        //        {
                        //            fldLst = fldLst + "," + fld.Name;
                        //        }
                        //    }

                        //    cnt = cnt + 1;
                        //}
                        //objForSet = fldLst;   // prepare for SetObjectidfeilds

                        //just use first field in lst as objid
                        objForSet = oflds.ToString().Split(',')[0];
                        ObjidCol.SelectString = "multi";    //updateprop
                        cpd.Message = "found valid keys.";
                    }

                    //==================determine is shape field
                    if (esri_sc != null && esri_sc != "")
                    {
                        //determine shape type
                        //TODO 

                        //determine if it should be converted to .st_aswkt
                        //TODO

                        SpatialCol.SelectString = esri_sc;
                        hasShape = true;
                        cpd.Message = "Found valid shapefield";
                    }
                    else
                    {
                        // //if (db_sc != "none")   // (data_type_id = 29812)
                        //TODO - why does db think there is an ST_GEOMETRY and esri doesn't?
                        SpatialCol.SelectString = "none";
                        cpd.Message = "No valid shapefield";
                    }

                    //===========add objectid fake code to sql statement
                    string qtest1 = "";
                    string qtest2 = "";

                    //add conversion for objid to ssql
                    if (objForSet == "QID")
                    {
                        int inx = qtest.IndexOf("FROM") - 1;
                        qtest1 = qtest.Insert(inx, fakeObjSql);
                    }
                    else
                    {
                        qtest1 = qtest;
                    }

                    //=============add conversion for shape to sql
                    if (hasShape)
                    {
                        //hana qtest1.Replace(esri_sc, esri_sc + ".st_aswkt()" + " as " + esri_sc);
                        qtest1.Replace(esri_sc, esri_sc + ".st_astext()" + " as " + esri_sc);
                        qtest2 = qtest1;
                    }
                    else
                    {
                        qtest2 = qtest1;
                    }

                    //=====================add to map
                    if (Globals.DBConn.State != ConnectionState.Closed)
                        Globals.DBConn.Close();
                    using (Database db2 = new Database(connectionProperties))
                    {
                        QueryDescription qdsfinal = db2.GetQueryDescription(qtest2, lyrname);  //reapply with new sql statement

                        //set additional parameters to help arcgispro 
                        if (objfieldcount == 0 && db_oc != "none")
                        {
                            MessageBox.Show("ArcGIS Pro does not detect a key for this table. The Q Pro for SAP addin will attempt to use the SAP database key.");
                            //qdsfinal.SetObjectIDFields(objForSet);  
                        }
                        else if (objForSet != "")
                        {
                            if (objForSet == "QID")
                            {

                                MessageBox.Show("ArcGIS Pro does not detect a key for this table.. The Q Pro for SAP addin will attempt to create a temporary key.");
                                //THIS FAILS qdsfinal.SetObjectIDFields(objForSet);
                            }
                            else
                            {
                                qdsfinal.SetObjectIDFields(objForSet);
                            }
                        }

                        //qdsfinal.SetShapeType(GeometryType.Point);
                        //qdsfinal.SetSpatialReference;
                        //qdsfinal.SetSRID

                        Table pTab2 = db2.OpenTable(qdsfinal);

                        if (hasShape)
                        {
                            // Add a new layer to the map
                            FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab2.GetDataConnection(), MapView.Active.Map, layerName: lyrname);
                            pFL.Select(null, SelectionCombinationMethod.New);
                            MapView.Active.ZoomToSelected();
                            pFL.ClearSelection();

                            _strMessage.SelectString = "Layer added.";
                            cpd.Message = "Layer added successsful to map.";
                            MessageBox.Show("Layer added successsful to map.");
                            return;
                        }
                        else
                        {
                            StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab2.GetDataConnection(), MapView.Active.Map, tableName: lyrname);
                            _strMessage.SelectString = "Table added.";
                            cpd.Message = "Standalone table added successsfuly to map.";
                            MessageBox.Show("Standalone table added successsfuly to map.");
                        }

                    }
                    cpd.Dispose();

                });

            }
            catch (Exception ex)
            {
                lock (_aLock)
                { BtnMapVis = false; }

                MessageBox.Show("The table could not be added to ArcGIS Pro.  " + ex.Message.ToString());
                if (Globals.DBConn.State != ConnectionState.Closed)
                    Globals.DBConn.Close();

            }

        }

        private SelectionString _strMessage;
        public SelectionString StrMessage
        {
            get
            {
                if (_strMessage == null)
                {
                    _strMessage = new SelectionString();
                }
                return _strMessage;
            }
            set
            {
                _strMessage = value;
                NotifyPropertyChanged("StrMessage");
            }
        }

        private string _currentSchema;
        public string CurrentSchema
        {
            get { return _currentSchema; }
            set
            {
                _currentSchema = value;
                NotifyPropertyChanged("CurrentSchema");
            }
        }

        private string _currentTable;
        public string CurrentTable
        {
            get { return _currentTable; }
            set
            {
                _currentTable = value;
                NotifyPropertyChanged("CurrentTable");
            }
        }

        private DataView _results;
        public DataView Results
        {
            get
            {
                return _results;
            }
            private set
            {
                _results = value;
                NotifyPropertyChanged("Results");
            }
        }

        private SelectionString _querytext;
        public SelectionString QueryTxt
        {
            get
            {
                if (_querytext == null)
                {
                    _querytext = new SelectionString();
                }
                return _querytext;
            }
            set
            {
                _querytext = value;
                NotifyPropertyChanged("QueryTxt");
            }
        }

        private SelectionString _spatialCol;
        public SelectionString SpatialCol
        {
            get
            {
                if (_spatialCol == null)
                {
                    _spatialCol = new SelectionString();
                }
                return _spatialCol;
            }
            set
            {
                _spatialCol = value;
                NotifyPropertyChanged("SpatialCol");
            }
        }

        private SelectionString _objidCol;
        public SelectionString ObjidCol
        {
            get
            {
                if (_objidCol == null)
                {
                    _objidCol = new SelectionString();
                }
                return _objidCol;
            }
            set
            {
                _objidCol = value;
                NotifyPropertyChanged("ObjidCol");
            }
        }
        private DataRowView _selrow;
        public DataRowView SelRow
        {
            get
            {
                return _selrow;
            }
            set
            {
                _selrow = value;
                NotifyPropertyChanged("SelRow");
            }
        }

        private bool _btnMapVis = false;
        public bool BtnMapVis
        {
            get { return _btnMapVis; }
            set
            {
                SetProperty(ref _btnMapVis, value, () => BtnMapVis);
                NotifyPropertyChanged("BtnMapVis");
            }
        }
    }

    public class btnFindSpatial : Button
    {
        protected override void OnClick()
        {
            SpatialTableFinderViewModel.Show();

        }
    }

    #endregion



    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
   






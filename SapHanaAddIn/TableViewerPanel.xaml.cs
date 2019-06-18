﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SapHanaAddIn
{
    /// <summary>
    /// Interaction logic for TableViewerPanelView.xaml
    /// </summary>
    public partial class TableViewerPanelView : UserControl
    {
        public string _spatialcolumn = "";
        public string _objidcolumn = "";
        //public int _recCount = 0;
        public string _querystring = "";
        public string _tblname = "";
        public Boolean _hasrows = false;
        public Boolean _hasSql = false;

        public TableViewerPanelView()
        {
            InitializeComponent();
        }

        protected async void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            await QueuedTask.Run(() =>
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;


                    if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Connect to a database first.", "Not connected");
                        return;
                    }
                    string txtSQLStatement = txtQueryText.Text;
                    if (txtSQLStatement.Trim().Length < 1)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                        return;
                    }

                    //*
                    //**
                    string spatialCol = "";
                    if (lblSpatialCol.Content != null) { spatialCol = lblSpatialCol.Content.ToString(); }
                    string objidCol = "";
                    if (lblObjidCol.Content != null) { objidCol = lblObjidCol.Content.ToString(); }
                    string tableList = "";
                    if (cboTables.SelectedItem != null) { tableList = cboTables.SelectedItem.ToString(); }

                    string qtest = "";
                    if (txtQueryText.Text != null) { qtest = txtQueryText.Text; }

                    if (spatialCol != "")
                    {
                        if (txtSQLStatement.IndexOf(spatialCol) > 0)
                        {
                            qtest = txtSQLStatement.Replace(spatialCol, spatialCol + ".st_aswkt()" + " as " + spatialCol);
                        }
                        else
                        {
                            qtest = txtSQLStatement;
                        }
                    }

                    HanaCommand cmd = new HanaCommand(qtest, Globals.hanaConn);
                    HanaDataReader dr = null;
                    System.Windows.Forms.DataGrid dgResults = new System.Windows.Forms.DataGrid();

                    //build sql select statement for query
                    //cmd.CommandText = "SELECT " + colls + " FROM \"" + comboBoxSchemas.SelectedItem.ToString() + "\".\"" + comboBoxTables.SelectedItem.ToString() + "\"";
                    dr = cmd.ExecuteReader();
                    dgResults.DataSource = null;
                    dgResults.Refresh();

                    if (dr != null)
                    {
                        dgResults.DataSource = dr;
                        DataTable dt = new DataTable();
                        dt.Load(dr);
                        dgForResults.ItemsSource = dt.DefaultView;
                        dr.Close();

                        if (dgForResults.HasItems)
                        {

                            //dg column properties
                            foreach (DataGridColumn col in dgForResults.Columns)
                            {
                                if (col.Header.ToString() == "")
                                { col.Header = col.ToString(); }
                                if (col.ToString().Length > 0)
                                { col.Width = col.ToString().Length + 15; }
                            }
                        }
                        else
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table has no rows.", "Table is empty");
                            _hasrows = false;
                            _hasSql = false;

                            return;
                        }
                    }
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This data reader is empty", "Table is empty");
                        return;
                    }
                }
                catch (HanaException ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                    ex.Errors[0].NativeError.ToString() + ")",
                    "Failed to execute SQL statement");
                    Mouse.OverrideCursor = null;
                }
            });
            Mouse.OverrideCursor = null;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //Globals.hanaConn.Close();
            //Visibility = Visibility.Collapsed;

        }

        private void btnAddTOC_Click(object sender, RoutedEventArgs e)
        {

            if (MapView.Active != null)
            {
                _spatialcolumn = lblSpatialCol.Content.ToString();
                _objidcolumn = lblObjidCol.Content.ToString();
                //_recCount = lblrecCount.Content;
                _querystring = txtQueryText.Text;
                _tblname = cboTables.SelectedItem.ToString();

                //position - don't know why this doesn't work in xaml
                //dgForResults.Height = this.ActualHeight * .7;
                dgForResults.Width = 500;
                dpForGrid.Width = 500;
                dpMain.Width = 500;

                Task sssss = OpenEnterpriseGeodatabase();
                return;
            }
            else
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please activate a map");
            }
        }
        
        public async Task OpenEnterpriseGeodatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
           {
               try
               {
                   Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                   // Opening a Non-Versioned db instance.
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
                   ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;

                   ConnectionItem connitem = item2.Icon as ConnectionItem;
                   string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                   DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                   {
                       AuthenticationMode = AuthenticationMode.DBMS,

                        // Where testMachine is the machine where the instance is running and testInstance is the name of the db instance.
                        Instance = cboEnv.cboBox.Text, //@"sapqe2hana",

                        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                        User = connitem.userid,// 
                        Password = tst2,
                       Version = "dbo.DEFAULT"
                   };

                    //create a query layer
                    using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                   {
                       Connector pCon = geodatabase.GetConnector();
                       pCon.ToString();
                   }
                   using (Database db = new Database(connectionProperties))
                   {
                       string thequery = "";
                       string spatialCol = _spatialcolumn;
                       string objidCol = _objidcolumn;
                       string tblName = _tblname;
                       Boolean hasRows = _hasrows;
                        //int recCount = _recCount;

                        string txtSQLStatement = _querystring;// txtQueryText.Text;
                                                              //"Select * from 'xxx'.'Points'", "MySelect"
                                                              //QueryDescription qds = db.GetQueryDescription("Select * from \"xxx\".\"TESTCREATEFC\"", "MySelect");

                        //now append spatial column
                        if (spatialCol != "")
                       {
                           thequery = txtSQLStatement.Insert(txtSQLStatement.IndexOf("SELECT") + 16, spatialCol + ", ");
                       }
                       else
                       {
                           thequery = txtSQLStatement;
                       }

                        //create table name for the layer
                        Random r = new Random();
                       int n = r.Next();
                       string s = n.ToString();
                       s = s.Substring(s.Length - 4);
                       s = _tblname + "_" + s;

                       QueryDescription qds = db.GetQueryDescription(txtSQLStatement, "MySelect");// " select GEF_OBJECTID, GEF_OBJKEY, OBJNR, ERNAM, ERDAT, KTEXT, IDAT2, AENAM, ARTPR, GEF_SHAPE from JLUOSTARINEN.ORDERSWLINE", "MySelect");                                                                                                   

                        //must have a pk OBJECTID - required
                        if (objidCol == "OBJECTID")
                       {
                           qds.SetObjectIDFields(objidCol);
                       }
                       else
                       {
                           string oflds = qds.GetObjectIDFields();

                           if (oflds.Length > 0)
                           {
                               objidCol = oflds.ToString().Split(',')[0];
                               qds.SetObjectIDFields(objidCol);
                           }
                           else
                           {
                                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table does not have an OBJECTID field in the database.  A temporary one will be be created.");
                                //construct objectid as it is a required column in Pro
                                //row_number() over(partition by " + tempid + ") as OBJECTID";))
                                //string fakeObjCol = objidCol.Split('-')[1];
                                var lst = qds.GetFields();
                               if (lst != null)
                               {
                                   if (lst.Count > 0)
                                   {
                                       objidCol = lst[0].Name;
                                   }
                               }

                               string fakeObjSql = "row_number()" + " over(partition by " + objidCol + ")" + " as OBJID";
                               thequery = txtSQLStatement.Insert(txtSQLStatement.IndexOf("FROM") - 1, ", " + fakeObjSql);

                               qds.SetObjectIDFields(objidCol);
                           }
                       }

                       if (qds.IsSpatialQuery())
                       {
                           try
                           {
                               GeometryType gt = qds.GetShapeType();
                               qds.SetShapeType(gt);

                               ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                               var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                               var connt = new CIMWMTSServiceConnection
                               {
                                   ServerConnection = serverConnection,
                               };

                               FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, layerName: s);

                               ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Layer Successfully added to the Contents Tab: " + s);
                                //MapView.Active.RedrawAsync(true);
                                pFL.Select(null, SelectionCombinationMethod.New);
                               MapView.Active.ZoomToSelected();

                           }
                           catch (HanaException ex)
                           {
                               ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                               ex.Errors[0].NativeError.ToString() + ")",
                               "Failed to add table.  There is a problem with either the Geometry field type or the data within it.");
                               return;
                           }
                       }
                       else
                       {
                           ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                           var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                           var connt = new CIMWMTSServiceConnection
                           {
                               ServerConnection = serverConnection,
                           };

                           StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab.GetDataConnection(), MapView.Active.Map, tableName: s);
                           ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Table successfully added to the Contents Tab: " + s);
                       }

                   }
               }
               catch (HanaException ex)
               {
                   ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                    ex.Errors[0].NativeError.ToString() + ")",
                    "Failed to execute SQL statement");
                   Mouse.OverrideCursor = null;

               }
               Mouse.OverrideCursor = null;
           });
        }

        private static void ResetDataConnectionFeatureService(Layer dataConnectionLayer, string newConnectionString)
        {
            #region Reset the URL of a feature service layer 
            CIMStandardDataConnection dataConnection = dataConnectionLayer.GetDataConnection() as CIMStandardDataConnection;
            dataConnection.WorkspaceConnectionString = newConnectionString;
            dataConnectionLayer.SetDataConnection(dataConnection);
            #endregion
        }

        private void MenuItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    //Get the clicked MenuItem
                    //var menuItem = (MenuItem)sender;

                    //Get the ContextMenu to which the menuItem belongs
                    //var contextMenu = (ContextMenu)menuItem.Parent;

                    //Find the placementTarget - returns all rows
                    //var items = (DataGrid)contextMenu.PlacementTarget;

                    //Get the underlying item, that you cast to your object that is bound
                    //to the DataGrid (and has subject and state as property) - as long as it is selected first
                    //var theRow = (DataGrid)item.SelectedCells[0].Item;

                    //find the clicked row
                    // explicitly select a row when it is right-clicked
                    DataGridRow rowContainer = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dgForResults));

                    if (rowContainer == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Row is null");
                        return;
                    }
                    else
                    {
                        //make clicked row the selected row
                        (sender as DataGrid).SelectedIndex = rowContainer.GetIndex();
                        int rowIndex = rowContainer.GetIndex();

                        //get the spatial field column
                        if (_spatialcolumn != "" && _spatialcolumn != null)
                        {
                            string colValue = lblSpatialCol.Content.ToString();
                            System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);
                        }
                        else
                        { ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This row does not have a spatial column."); }
                    }
                });
            }
            catch (Exception ee)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ee.ToString());
                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;

        }

        /// /// Common UI related helper methods. 
        /// 
        public static class UIHelpers
        {

            #region find parent

            /// <summary>
            /// Finds a parent of a given item on the visual tree.
            /// </summary>
            /// <typeparam name="T">The type of the queried item.</typeparam>
            /// <param name="child">A direct or indirect child of the
            /// queried item.</param>
            /// <returns>The first parent item that matches the submitted
            /// type parameter. If not matching item can be found, a null
            /// reference is being returned.</returns>
            public static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
            {
                //get parent item
                DependencyObject parentObject = GetParentObject(child);

                //we've reached the end of the tree
                if (parentObject == null) return null;

                //check if the parent matches the type we're looking for
                T parent = parentObject as T;
                if (parent != null)
                {
                    return parent;
                }
                else
                {
                    //use recursion to proceed with next level
                    return TryFindParent<T>(parentObject);
                }
            }


            /// <summary>
            /// This method is an alternative to WPF's
            /// <see cref="VisualTreeHelper.GetParent"/> method, which also
            /// supports content elements. Do note, that for content element,
            /// this method falls back to the logical tree of the element.
            /// </summary>
            /// <param name="child">The item to be processed.</param>
            /// <returns>The submitted item's parent, if available. Otherwise
            /// null.</returns>
            public static DependencyObject GetParentObject(DependencyObject child)
            {
                if (child == null) return null;
                ContentElement contentElement = child as ContentElement;

                if (contentElement != null)
                {
                    DependencyObject parent = ContentOperations.GetParent(contentElement);
                    if (parent != null) return parent;

                    FrameworkContentElement fce = contentElement as FrameworkContentElement;
                    return fce != null ? fce.Parent : null;
                }

                //if it's not a ContentElement, rely on VisualTreeHelper
                return System.Windows.Media.VisualTreeHelper.GetParent(child);
            }

            #endregion

            /// <summary>
            /// Tries to locate a given item within the visual tree,
            /// starting with the dependency object at a given position. 
            /// </summary>
            /// <typeparam name="T">The type of the element to be found
            /// on the visual tree of the element at the given location.</typeparam>
            /// <param name="reference">The main element which is used to perform
            /// hit testing.</param>
            /// <param name="point">The position to be evaluated on the origin.</param>
            public static T TryFindFromPoint<T>(UIElement reference, Point point)
              where T : DependencyObject
            {
                DependencyObject element = reference.InputHitTest(point)
                                             as DependencyObject;
                if (element == null) return null;
                else if (element is T) return (T)element;
                else return TryFindParent<T>(element);
            }
        }

        public static T GetVisualChild<T>(System.Windows.Media.Visual parent) where T : System.Windows.Media.Visual
        {
            T child = default(T);
            int numVisuals = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                System.Windows.Media.Visual v = (System.Windows.Media.Visual)System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        //private void cboTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        //        {
        //            //set the current table property
        //            ComboBoxItem cmbi = ((sender as ComboBox).SelectedItem as ComboBoxItem);

        //            _tblname = cmbi.Content.ToString();
                

        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.ToString(),
        //         "Failed to set table");
        //    }
        //}

       
    }
}



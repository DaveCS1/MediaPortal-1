﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using MediaPortal.GUI.View;

namespace MediaPortal.Configuration.Sections
{
  public partial class BaseViewsFilter : Form
  {
    #region Variables

    private BaseViewsNew _main;
    private DataTable _datasetFilters;

    #endregion

    #region Properties

    public List<FilterDefinitionNew> Filter { get; set; }

    #endregion

    #region ctor

    public BaseViewsFilter(BaseViewsNew main)
    {
      InitializeComponent();

      _main = main;
    }

    #endregion

    #region Initialisation

    /// <summary>
    /// Set up the Datagrid column and the DataTable to which the grid is bound
    /// </summary>
    private void SetupGrid()
    {
      // Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable

      dgColField.Items.AddRange(_main.Selections);

      //Create the Data Table object which will then be used to hold
      //columns and rows
      _datasetFilters = new DataTable();

      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[4];
      arrColumnNames[0] = "Field";
      arrColumnNames[1] = "Operator";
      arrColumnNames[2] = "SelectionValue";
      arrColumnNames[3] = "AndOr";

      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        _datasetFilters.Columns.Add(dtCol);
      }

      // Set the Data Properties for the field to map to the data table
      dgColField.DataPropertyName = "Field";
      dgColOperator.DataPropertyName = "Operator";
      dgColSelectionValue.DataPropertyName = "SelectionValue";
      dgColAndOr.DataPropertyName = "AndOr";

      //Set the Data Grid Source as the Data Table created above
      dataGrid.AutoGenerateColumns = false;
      dataGrid.DataSource = _datasetFilters;
    }

    #endregion

    #region Events

    #region Form 

    /// <summary>
    /// The Form is Shown
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BaseViewsFilter_Shown(object sender, EventArgs e)
    {
      SetupGrid();

      foreach (FilterDefinitionNew filter in Filter)
      {
        _datasetFilters.Rows.Add(
            new object[]
            {
             filter.Where,
             filter.SqlOperator,
             filter.WhereValue,
             filter.AndOr,
            }
         );
      }
    }

    /// <summary>
    /// Handle Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btSave_Click(object sender, EventArgs e)
    {
      Filter.Clear();

      foreach (DataRow row in _datasetFilters.Rows)
      {
        FilterDefinitionNew filter = new FilterDefinitionNew();
        filter.Where = (string)row[0];
        filter.SqlOperator = (string)row[1];
        filter.WhereValue = (string)row[2];
        filter.AndOr = (string)row[3];
        Filter.Add(filter);
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    /// <summary>
    ///  Handle Cancel Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    #endregion

    #region Grid

    /// <summary>
    /// Handle the Keypress for the Filter Datagrid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_KeyDown(object sender, KeyEventArgs e)
    {
      int rowSelected = -1;
      if (dataGrid.CurrentRow != null)
      {
        rowSelected = dataGrid.CurrentRow.Index;
      }

      switch (e.KeyCode)
      {
        case System.Windows.Forms.Keys.Insert:
          DataRow row = _datasetFilters.NewRow();
          row[0] = row[1] = row[2] = row[3] = "";
          if (rowSelected == -1)
          {
            rowSelected = 0;
          }
          _datasetFilters.Rows.InsertAt(row, rowSelected + 1);
          e.Handled = true;
          break;
        case System.Windows.Forms.Keys.Delete:
          if (rowSelected > -1)
          {
            _datasetFilters.Rows.RemoveAt(rowSelected);
          }
          e.Handled = true;
          break;
      }
    }

    #endregion

    #endregion
  }
}

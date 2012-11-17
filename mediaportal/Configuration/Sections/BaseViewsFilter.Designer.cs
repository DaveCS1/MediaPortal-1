﻿namespace MediaPortal.Configuration.Sections
{
  partial class BaseViewsFilter
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.dgColField = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgColOperator = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgColSelectionValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgColAndOr = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.btSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.btCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.lblActionCodes);
      this.mpGroupBox1.Controls.Add(this.dataGrid);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(17, 22);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(743, 263);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Filter Definition";
      // 
      // dataGrid
      // 
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToResizeColumns = false;
      this.dataGrid.AllowUserToResizeRows = false;
      this.dataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgColField,
            this.dgColOperator,
            this.dgColSelectionValue,
            this.dgColAndOr});
      this.dataGrid.Location = new System.Drawing.Point(22, 30);
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(685, 188);
      this.dataGrid.TabIndex = 2;
      this.dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_KeyDown);
      // 
      // dgColField
      // 
      this.dgColField.HeaderText = "Field";
      this.dgColField.Name = "dgColField";
      this.dgColField.Width = 150;
      // 
      // dgColOperator
      // 
      this.dgColOperator.HeaderText = "Operator";
      this.dgColOperator.Items.AddRange(new object[] {
            "Equals",
            "Not Equals",
            "Contains",
            "Not Contains",
            "Greater Than",
            "Greater Equals",
            "Less Than",
            "Less Equals",
            "In",
            "Not In",
            "Starts",
            "Not Starts",
            "Ends",
            "Not Ends"});
      this.dgColOperator.Name = "dgColOperator";
      this.dgColOperator.Width = 80;
      // 
      // dgColSelectionValue
      // 
      this.dgColSelectionValue.HeaderText = "Selection Value";
      this.dgColSelectionValue.Name = "dgColSelectionValue";
      this.dgColSelectionValue.Width = 400;
      // 
      // dgColAndOr
      // 
      this.dgColAndOr.HeaderText = "A/O";
      this.dgColAndOr.Items.AddRange(new object[] {
            " ",
            "AND",
            "OR"});
      this.dgColAndOr.Name = "dgColAndOr";
      this.dgColAndOr.Width = 50;
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(42, 325);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(75, 23);
      this.btSave.TabIndex = 1;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // btCancel
      // 
      this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btCancel.Location = new System.Drawing.Point(150, 324);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(75, 23);
      this.btCancel.TabIndex = 2;
      this.btCancel.Text = "Cancel";
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
      // 
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(19, 221);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(430, 29);
      this.lblActionCodes.TabIndex = 13;
      this.lblActionCodes.Text = "Use the \"Ins\" and \"Del\" key to insert and delete lines.";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // BaseViewsFilter
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btCancel;
      this.ClientSize = new System.Drawing.Size(784, 379);
      this.ControlBox = false;
      this.Controls.Add(this.btCancel);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.mpGroupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "BaseViewsFilter";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Shown += new System.EventHandler(this.BaseViewsFilter_Shown);
      this.mpGroupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private UserInterface.Controls.MPGroupBox mpGroupBox1;
    private System.Windows.Forms.DataGridView dataGrid;
    private UserInterface.Controls.MPButton btSave;
    private UserInterface.Controls.MPButton btCancel;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColField;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColOperator;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgColSelectionValue;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColAndOr;
    private UserInterface.Controls.MPLabel lblActionCodes;
  }
}

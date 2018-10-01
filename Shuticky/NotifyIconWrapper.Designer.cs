namespace Shuticky
{
    partial class NotifyIconWrapper
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyIconWrapper));
            this.shutickyNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.shutickyNotifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_New = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_ShowAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_MinimizeAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Trash = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_ClearTrash = new System.Windows.Forms.ToolStripMenuItem();
            this.shutickyNotifyIconContextMenuStrip.SuspendLayout();
            // 
            // shutickyNotifyIcon
            // 
            this.shutickyNotifyIcon.ContextMenuStrip = this.shutickyNotifyIconContextMenuStrip;
            this.shutickyNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("shutickyNotifyIcon.Icon")));
            this.shutickyNotifyIcon.Text = "Shuticky";
            this.shutickyNotifyIcon.Visible = true;
            // 
            // shutickyNotifyIconContextMenuStrip
            // 
            this.shutickyNotifyIconContextMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.shutickyNotifyIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_New,
            this.toolStripSeparator2,
            this.toolStripMenuItem_ShowAll,
            this.toolStripMenuItem_MinimizeAll,
            this.toolStripSeparator1,
            this.toolStripMenuItem_Trash,
            this.toolStripMenuItem_ClearTrash,
            this.toolStripSeparator4,
            this.toolStripMenuItem_Exit,
            this.toolStripSeparator3,
            this.toolStripMenuItem_Help});
            this.shutickyNotifyIconContextMenuStrip.Name = "shutickyNotifyIconContextMenuStrip";
            this.shutickyNotifyIconContextMenuStrip.Size = new System.Drawing.Size(221, 244);
            // 
            // toolStripMenuItem_New
            // 
            this.toolStripMenuItem_New.Name = "toolStripMenuItem_New";
            this.toolStripMenuItem_New.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_New.Text = "新規付箋";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(217, 6);
            // 
            // toolStripMenuItem_ShowAll
            // 
            this.toolStripMenuItem_ShowAll.Name = "toolStripMenuItem_ShowAll";
            this.toolStripMenuItem_ShowAll.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_ShowAll.Text = "すべて表示";
            // 
            // toolStripMenuItem_MinimizeAll
            // 
            this.toolStripMenuItem_MinimizeAll.Name = "toolStripMenuItem_MinimizeAll";
            this.toolStripMenuItem_MinimizeAll.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_MinimizeAll.Text = "すべて最小化";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // toolStripMenuItem_Trash
            // 
            this.toolStripMenuItem_Trash.Name = "toolStripMenuItem_Trash";
            this.toolStripMenuItem_Trash.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_Trash.Text = "ゴミ箱";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(217, 6);
            // 
            // toolStripMenuItem_Exit
            // 
            this.toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            this.toolStripMenuItem_Exit.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_Exit.Text = "終了";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(217, 6);
            // 
            // toolStripMenuItem_Help
            // 
            this.toolStripMenuItem_Help.Name = "toolStripMenuItem_Help";
            this.toolStripMenuItem_Help.Size = new System.Drawing.Size(220, 36);
            this.toolStripMenuItem_Help.Text = "ヘルプ";
            // 
            // toolStripMenuItem_ClearTrash
            // 
            this.toolStripMenuItem_ClearTrash.Name = "toolStripMenuItem_ClearTrash";
            this.toolStripMenuItem_ClearTrash.Size = new System.Drawing.Size(246, 36);
            this.toolStripMenuItem_ClearTrash.Text = "ゴミ箱を空にする";
            this.shutickyNotifyIconContextMenuStrip.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon shutickyNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip shutickyNotifyIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_New;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Exit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ShowAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_MinimizeAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help;
        public System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Trash;//App.xaml.csからゴミ箱の中身を動的に追加したりするので
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ClearTrash;
    }
}

using System;
using System.ComponentModel;
using System.Windows;

namespace Shuticky
{
    //常駐アプリの作成方法の参考：https://garafu.blogspot.jp/2015/06/dev-tasktray-residentapplication.html

    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            this.InitializeComponent();

            //コンテキストメニューのイベントを設定
            this.toolStripMenuItem_New.Click += this.ContextMenuItem_New_Click;
            this.toolStripMenuItem_Exit.Click += this.ContextMenuItem_Exit_Click;
            this.toolStripMenuItem_ShowAll.Click += this.ContextMenuItem_ShowAll_Click;
            this.toolStripMenuItem_MinimizeAll.Click += this.ContextMenuItem_MinimizeAll_Click;
            this.toolStripMenuItem_Help.Click += this.ContextMenuItem_Help_Click;
            this.toolStripMenuItem_ClearTrash.Click += this.ContextMenuItem_ClearTrash_Click;
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public event EventHandler ContextMenuItem_Help_Clicked;
        private void ContextMenuItem_Help_Click(object sender, EventArgs e)
        {
            ContextMenuItem_Help_Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuItem_Exit_Clicked;
        private void ContextMenuItem_Exit_Click(object sender, EventArgs e)
        {
            ContextMenuItem_Exit_Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuItem_New_Clicked;
        private void ContextMenuItem_New_Click(object sender, EventArgs e)
        {
            ContextMenuItem_New_Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuItem_ShowAll_Clicked;
        private void ContextMenuItem_ShowAll_Click(object sender, EventArgs e)
        {
            ContextMenuItem_ShowAll_Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuItem_MinimizeAll_Clicked;
        private void ContextMenuItem_MinimizeAll_Click(object sender, EventArgs e)
        {
            ContextMenuItem_MinimizeAll_Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuItem_ClearTrash_Clicked;
        private void ContextMenuItem_ClearTrash_Click(object sender, EventArgs e)
        {
            ContextMenuItem_ClearTrash_Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}

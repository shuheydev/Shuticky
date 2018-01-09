using System;
using System.ComponentModel;
using System.Windows;

namespace 付箋アプリ
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
        }



        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public event EventHandler ContextMenuItem_Help_Clicked;
        private void ContextMenuItem_Help_Click(object sender, EventArgs e)
        {
            if (ContextMenuItem_Help_Clicked!=null)
            {
                ContextMenuItem_Help_Clicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler ContextMenuItem_Exit_Clicked;
        private void ContextMenuItem_Exit_Click(object sender, EventArgs e)
        {
            if(ContextMenuItem_Exit_Clicked!=null)
            {
                ContextMenuItem_Exit_Clicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler ContextMenuItem_New_Clicked;
        private void ContextMenuItem_New_Click(object sender, EventArgs e)
        {
           if(ContextMenuItem_New_Clicked!=null)
            {
                ContextMenuItem_New_Clicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler ContextMenuItem_ShowAll_Clicked;
        private void ContextMenuItem_ShowAll_Click(object sender, EventArgs e)
        {
            if(ContextMenuItem_ShowAll_Clicked!=null)
            {
                ContextMenuItem_ShowAll_Clicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler ContextMenuItem_MinimizeAll_Clicked;
        private void ContextMenuItem_MinimizeAll_Click(object sender, EventArgs e)
        {
            if(ContextMenuItem_MinimizeAll_Clicked!=null)
            {
                ContextMenuItem_MinimizeAll_Clicked(this, EventArgs.Empty);
            }
        }
    }
}
